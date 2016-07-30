using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Gambling.Commands
{
    class AnimalRacing : DiscordCommand
    {
        public static ConcurrentDictionary<ulong, AnimalRace> AnimalRaces = new ConcurrentDictionary<ulong, AnimalRace>();

        public AnimalRacing(DiscordModule module) : base(module)
        {
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Prefix + "race")
                .Description($"Startet ein neues Tier-Rennen. | `{Prefix}race`")
                .Do(e =>
                {
                    var ar = new AnimalRace(e.Server.Id, e.Channel);
                    if (ar.Fail)
                    {
                        return;
                    }
                });

            cgb.CreateCommand(Prefix + "joinrace")
                .Alias(Prefix + "jr")
                .Description($"Tritt einem Rennen bei. Du kannst eine Anzahl an {MidnightBot.Config.CurrencyName} zum Wetten setzen (Optional). Du bekommst deine Wette*(Teilnehmer-1) zurück, wenn du gewinnst. | `{Prefix}jr` oder `{Prefix}jr 5`")
                .Parameter("amount", ParameterType.Optional)
                .Do(async e =>
                {
                    int amount;
                    if (!int.TryParse(e.GetArg("amount"), out amount) || amount < 0)
                        amount = 0;

                    var userFlowers = GamblingModule.GetUserFlowers(e.User.Id);

                    if (userFlowers < amount)
                    {
                        await e.Channel.SendMessage($"{e.User.Mention} Du hast nicht genug {MidnightBot.Config.CurrencyName}. Du hast nur {userFlowers}{MidnightBot.Config.CurrencySign}.").ConfigureAwait(false);
                        return;
                    }

                    if (amount > 0)
                        await FlowersHandler.RemoveFlowers(e.User, "BetRace", (int)amount, true).ConfigureAwait(false);

                    AnimalRace ar;

                    if (!AnimalRaces.TryGetValue(e.Server.Id, out ar))
                    {
                        await e.Channel.SendMessage("Es existiert kein Rennen auf diesem Server.");
                        return;
                    }
                    await ar.JoinRace(e.User, amount);
                });
        }

        public class AnimalRace
        {
            private ConcurrentQueue<string> animals = new ConcurrentQueue<string>(MidnightBot.Config.RaceAnimals.Shuffle());

            public bool Fail { get; internal set; }

            public List<Participant> participants = new List<Participant>();
            private ulong serverId;
            private int messagesSinceGameStarted = 0;

            public Channel raceChannel { get; set; }
            public bool Started { get; private set; } = false;

            public AnimalRace(ulong serverId, Channel ch)
            {
                this.serverId = serverId;
                this.raceChannel = ch;
                if (!AnimalRaces.TryAdd(serverId, this))
                {
                    Fail = true;
                    return;
                }
                var cancelSource = new CancellationTokenSource();
                var token = cancelSource.Token;
                var fullgame = CheckForFullGameAsync(token);
                Task.Run(async () =>
                {
                    try
                    {
                        await raceChannel.SendMessage($"🏁`Rennen startet in 20 Sekunden oder wenn der Raum voll ist. Gib {MidnightBot.Config.CommandPrefixes.Gambling}jr ein, um dem Rennen beizutreten.`");
                        var t = await Task.WhenAny(Task.Delay(20000, token), fullgame);
                        Started = true;
                        cancelSource.Cancel();
                        if (t == fullgame)
                        {
                            await raceChannel.SendMessage("🏁`Rennen ist voll, wird gestartet!`");
                        }
                        else if (participants.Count > 1)
                        {
                            await raceChannel.SendMessage("🏁`Spiel startet mit " + participants.Count + " Teilnehmern.`");
                        }
                        else
                        {
                            await raceChannel.SendMessage("🏁`Rennen konnte nicht starten da es nicht genug Teilnehmer gab.`");
                            var p = participants.FirstOrDefault();
                            if (p != null)
                                await FlowersHandler.AddFlowersAsync(p.User, "BetRace", p.AmountBet, true).ConfigureAwait(false);
                            End();
                            return;
                        }
                        await Task.Run(StartRace);
                        End();
                    }
                    catch { }
                });
            }

            private void End()
            {
                AnimalRace throwaway;
                AnimalRaces.TryRemove(serverId, out throwaway);
            }

            private async Task StartRace()
            {
                var rng = new Random();
                Participant winner = null;
                Message msg = null;
                Message msg2 = null;
                int place = 1;
                try
                {
                    MidnightBot.Client.MessageReceived += Client_MessageReceived;

                    while (!participants.All(p => p.Total >= 60))
                    {
                        //update the state
                        participants.ForEach(p =>
                        {
                            p.Total += 1 + rng.Next(0, 10);
                            if (p.Total > 60)
                            {
                                p.Total = 60;
                                if (winner == null)
                                {
                                    winner = p;
                                }
                                if (p.Place == 0)
                                    p.Place = place++;
                            }
                        });
                        //draw the state
                            var names = $@"**Teilnehmer**
{String.Join("\n", participants.Select(p => $"{p.User} | {p.Animal}"))}
";
                        if (msg2 == null || messagesSinceGameStarted >= 10) // also resend the message if channel was spammed
                        {
                            if (msg2 != null)
                                try { await msg2.Delete(); } catch { }
                            msg2 = await raceChannel.SendMessage(names);
                        }
                        var text = $@"|🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚|
{String.Join("\n", participants.Select(p => $"{(int)(p.Total / 60f * 100),-2}%|{p.ToString()}"))}
|🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚|";
                        if (msg == null || messagesSinceGameStarted >= 10) // also resend the message if channel was spammed
                        {
                            if (msg != null)
                                try { await msg.Delete(); } catch { }
                            msg = await raceChannel.SendMessage(text);
                            messagesSinceGameStarted = 0;
                        }
                        else
                            await msg.Edit(text);

                        await Task.Delay(2500);
                    }
                }
                finally
                {
                    MidnightBot.Client.MessageReceived -= Client_MessageReceived;
                }

                    if (winner.AmountBet > 0)
                {
                    var wonAmount = winner.AmountBet * (participants.Count - 1);
                    await FlowersHandler.AddFlowersAsync(winner.User, "Won a Race", wonAmount).ConfigureAwait(false);
                    await raceChannel.SendMessage($"🏁 {winner.User.Mention} as {winner.Animal} **Hat das Rennen gewonnen und bekommt daher {wonAmount}{MidnightBot.Config.CurrencySign}!**");
                }
                else
                {
                    await raceChannel.SendMessage($"🏁 {winner.User.Mention} als {winner.Animal} **hat das Rennen gewonnen!**");
                }
            }

            private void Client_MessageReceived(object sender, MessageEventArgs e)
            {
                if (e.Message.IsAuthor || e.Channel.IsPrivate || e.Channel != raceChannel)
                    return;
                messagesSinceGameStarted++;
            }

            private async Task CheckForFullGameAsync(CancellationToken cancelToken)
            {
                while (animals.Count > 0)
                {
                    await Task.Delay(100, cancelToken);
                }
            }

            public async Task<bool> JoinRace(User u, int amount = 0)
            {
                var animal = "";
                if (!animals.TryDequeue(out animal))
                {
                    await raceChannel.SendMessage($"{u.Mention} `Es gibt kein laufendes Rennen auf diesem Server.`");
                    return false;
                }
                var p = new Participant(u, animal, amount);
                if (participants.Contains(p))
                {
                    await raceChannel.SendMessage($"{u.Mention} `Du nimmst bereits an diesem Rennen teil.`");
                    return false;
                }
                if (Started)
                {
                    await raceChannel.SendMessage($"{u.Mention} `Rennen hat bereits gestartet.`");
                    return false;
                }
                participants.Add(p);
                await raceChannel.SendMessage($"{u.Mention} **trat dem Rennen als {p.Animal} bei" + (amount > 0 ? $" und wettet um {amount} {MidnightBot.Config.CurrencyName.SnPl(amount)}!**" : "**"));
                return true;
            }
        }

        public class Participant
        {
            public User User { get; set; }
            public string Animal { get; set; }
            public int AmountBet { get; set; }

            public float Coeff { get; set; }
            public int Total { get; set; }

            public int Place { get; set; } = 0;

            public Participant(User u, string a, int amount)
            {
                this.User = u;
                this.Animal = a;
                this.AmountBet = amount;
            }

            public override int GetHashCode()
            {
                return User.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var p = obj as Participant;
                return p == null ?
                    false :
                    p.User == User;
            }

            public override string ToString()
            {
                var str = new string('‣', Total) + Animal;
                if (Place == 0)
                    return str;
                if (Place == 1)
                {
                    return str + $"🏆 {User}";
                }
                else if (Place == 2)
                {
                    return str + $"`2nd` {User}";
                }
                else if (Place == 3)
                {
                    return str + $"`3rd` {User}";
                }
                else
                {
                    return str + $"`{Place}th` {User}";
                }
            }
        }
    }
}