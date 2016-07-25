using MidnightBot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Collections.Concurrent;
using Discord;
using MidnightBot.Extensions;
using System.Threading;

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
                .Description("Startet ein neues Tier-Rennen.")
                .Do(e => {
                    var ar = new AnimalRace(e.Server.Id, e.Channel);
                    if (ar.Fail)
                    {
                        return;
                    }
                });

            cgb.CreateCommand(Prefix + "joinrace")
                .Alias(Prefix + "jr")
                .Description("Tritt einem neuem Rennen bei")
                .Do(async e => {
                    AnimalRace ar;

                    if (!AnimalRaces.TryGetValue(e.Server.Id, out ar))
                    {
                        await e.Channel.SendMessage("Es existiert kein Rennen auf diesem Server.");
                    }
                    await ar.JoinRace(e.User);
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
                    await raceChannel.SendMessage($"🏁`Rennen startet in 20 Sekunden oder wenn der Raum voll ist. Gib $jr ein, um dem Rennen beizutreten.`");
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
                        End();
                        return;
                    }
                    await Task.Run(StartRace);
                    End();
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
                await raceChannel.SendMessage($"🏁 {winner.User.Mention} als {winner.Animal} `hat das Rennen gewonnen!`");
            }

            private void Client_MessageReceived(object sender, MessageEventArgs e)
            {
                if (e.Message.IsAuthor)
                    return;
                Console.WriteLine("Nachricht erhalten " + messagesSinceGameStarted);
                messagesSinceGameStarted++;
            }

            private async Task CheckForFullGameAsync(CancellationToken cancelToken)
            {
                while (animals.Count > 0)
                {
                    await Task.Delay(100, cancelToken);
                }
            }

            public async Task<bool> JoinRace(User u)
            {
                var animal = "";
                if (!animals.TryDequeue(out animal))
                {
                    await raceChannel.SendMessage($"{u.Mention} `Es gibt kein laufenedes Rennen auf diesem Server.`");
                    return false;
                }
                var p = new Participant(u, animal);
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
                await raceChannel.SendMessage($"{u.Mention} **trat dem Rennen als {p.Animal} bei**");
                return true;
            }
        }

        public class Participant
        {
            public User User { get; set; }
            public string Animal { get; set; }

            public float Coeff { get; set; }
            public int Total { get; set; }

            public int Place { get; set; } = 0;

            public Participant(User u, string a)
            {
                this.User = u;
                this.Animal = a;
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
                    return str + "🏆";
                }
                else if (Place == 2)
                {
                    return str + "`2nd`";
                }
                else if (Place == 3)
                {
                    return str + "`3rd`";
                }
                else
                {
                    return str + $"`{Place}th`";
                }
            }
        }
    }
}