using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Extensions;
using MidnightBot.Modules.Gambling.Commands;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;
using System.Text;

namespace MidnightBot.Modules.Gambling
{
    internal class GamblingModule : DiscordModule
    {
        public GamblingModule ()
        {
            commands.Add (new DrawCommand (this));
            commands.Add (new FlipCoinCommand (this));
            commands.Add (new DiceRollCommand (this));
            commands.Add(new AnimalRacing(this));
        }
        public string BotName { get; set; } = MidnightBot.BotName;
        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Gambling;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
           {
               cgb.AddCheck (PermissionChecker.Instance);

               commands.ForEach (com => com.Init (cgb));

               cgb.CreateCommand (Prefix + "raffle")
                   .Description ($"Schreibt den Namen und die ID eines zufälligen Benutzers aus der Online Liste einer (optionalen) Rolle. | `{Prefix}raffle` oder `{Prefix}raffle RoleName`")
                   .Parameter ("role",ParameterType.Optional)
                   .Do (async e =>
                   {
                       var arg = string.IsNullOrWhiteSpace (e.GetArg ("role")) ? "@everyone" : e.GetArg ("role");
                       var role = e.Server.FindRoles (arg).FirstOrDefault ();
                       if (role == null)
                       {
                           await e.Channel.SendMessage ("💢 Rolle nicht gefunden.").ConfigureAwait (false);
                           return;
                       }
                       var members = role.Members.Where (u => u.Status == UserStatus.Online); // only online
                       var membersArray = members as User[] ?? members.ToArray ();
                       var usr = membersArray[new Random ().Next (0,membersArray.Length)];
                       await e.Channel.SendMessage ($"**Gezogener Benutzer:** {usr.Name} (Id: {usr.Id})").ConfigureAwait (false);
                   });

               cgb.CreateCommand (Prefix + "$$")
                   .Description (string.Format ("Überprüft, wieviele {0} du hast.",MidnightBot.Config.CurrencyName))
                   .Parameter ("all",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var usr = e.Message.MentionedUsers.FirstOrDefault () ?? e.User;
                        var pts = GetUserFlowers (usr.Id);
                        var str = $"{usr.Name} hat {pts} {MidnightBot.Config.CurrencyName} {MidnightBot.Config.CurrencySign}";
                        await e.Channel.SendMessage (str).ConfigureAwait (false);
                    });

               cgb.CreateCommand (Prefix + "award")
                   .Description (string.Format ($"Gibt jemanden eine bestimmte Anzahl an {MidnightBot.Config.CurrencyName}. **Bot Owner Only!** | `{Prefix}award 5 @Benutzer`"))
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Parameter ("amount",ParameterType.Required)
                   .Parameter ("receiver",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       var amountStr = e.GetArg("amount")?.Trim();
                       long amount;
                       if (!long.TryParse(amountStr, out amount) || amount <= 0)
                           return;
                           var mentionedUser = e.Message.MentionedUsers.FirstOrDefault(u =>
                           u.Id != MidnightBot.Client.CurrentUser.Id);
                           if (mentionedUser == null)
                               return;

                           await FlowersHandler.AddFlowersAsync(mentionedUser, $"Awarded by bot owner. ({e.User.Name}/{e.User.Id})", (int)amount).ConfigureAwait(false);

                           await e.Channel.SendMessage($"{e.User.Mention} erfolgreich {amount}  {MidnightBot.Config.CurrencyName} zu {mentionedUser.Mention} hinzugefügt!").ConfigureAwait(false);
                   });

               cgb.CreateCommand(Prefix + "dailymoney")
                   .Description($"Tägliches Geld (20 Euro, wird um 0 Uhr zurückgesetzt.) | `{Prefix}dailymoney`")
                   .Do(async e =>
                   {
                       DateTime today = DateTime.Today;
                       var uid = (long)e.User.Id;
                       var sid = (long)e.Server.Id;

                       var user = DbHandler.Instance.FindOne<DailyMoney>(dm => dm.UserId == uid && dm.ServerId == sid);
                       if (user == null)
                       {
                           var data = new DailyMoney
                           {
                               UserId = (long)e.User.Id,
                               LastTimeGotten = today.AddDays(-1),
                               ServerId = (long)e.Server.Id
                           };
                           DbHandler.Instance.InsertData(data);
                           user = DbHandler.Instance.FindOne<DailyMoney>(dm => dm.UserId == uid && dm.ServerId == sid);
                       }
                       if (user.LastTimeGotten.Date.DayOfYear < today.Date.DayOfYear)
                       {
                           var data = DbHandler.Instance.FindAll<DailyMoney>(d => d.ServerId == sid && d.UserId == uid);
                           DbHandler.Instance.UpdateAll<DailyMoney>(data.Select(i => { i.LastTimeGotten = today; return i; }));
                           await FlowersHandler.AddFlowersAsync(e.User, $"Daily Reward. ({e.User.Name}/{e.User.Id})", 20).ConfigureAwait(false);
                           await e.Channel.SendMessage($"{e.User.Mention} hat sich seinen täglichen Anteil  von 20 {MidnightBot.Config.CurrencyName} abgeholt.");
                           return;
                       }
                       else
                       {
                           await e.Channel.SendMessage("Du hast deinen täglichen Anteil heute bereits abgeholt.");
                       }
                   });

               cgb.CreateCommand (Prefix + "take")
                   .Description (string.Format ($"Entfernt eine bestimmte Anzahl an {MidnightBot.Config.CurrencyName} von jemanden. **Bot Owner Only!** | `{Prefix}take 1 \"@someguy\"`"))
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Parameter ("amount",ParameterType.Required)
                   .Parameter ("rektperson",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       var amountStr = e.GetArg ("amount")?.Trim ();
                       long amount;
                       if (!long.TryParse (amountStr,out amount) || amount < 0)
                           return;

                       var mentionedUser = e.Message.MentionedUsers.FirstOrDefault (u =>
                       u.Id != MidnightBot.Client.CurrentUser.Id);
                       if (mentionedUser == null)
                           return;

                       await FlowersHandler.RemoveFlowers (mentionedUser,$"Taken by bot owner.({e.User.Name}/{e.User.Id})",(int)amount).ConfigureAwait(false);

                       await e.Channel.SendMessage ($"{e.User.Mention} erfolgreich {amount} {MidnightBot.Config.CurrencyName} von {mentionedUser.Mention} entfernt!").ConfigureAwait (false);
                   });

               cgb.CreateCommand (Prefix + "give")
                        .Description ($"Gibt jemanden eine Anzahl {MidnightBot.Config.CurrencyName}. |`{Prefix}give 1 \"@SomeGuy\"`")
                        .Parameter ("amount",ParameterType.Required)
                        .Parameter ("receiver",ParameterType.Unparsed)
                        .Do (async e =>
                        {
                            var amountStr = e.GetArg ("amount")?.Trim ();
                            long amount;
                            if (!long.TryParse(amountStr, out amount) || amount <= 0)
                                return;

                            var mentionedUser = e.Message.MentionedUsers.FirstOrDefault (u =>
                            u.Id != MidnightBot.Client.CurrentUser.Id &&
                            u.Id != e.User.Id);
                            if (mentionedUser == null)
                                return;

                            var userFlowers = GetUserFlowers (e.User.Id);

                            if (userFlowers < amount)
                            {
                                await e.Channel.SendMessage ($"{e.User.Mention} Du hast nicht genug {MidnightBot.Config.CurrencyName}. Du hast nur {userFlowers} {MidnightBot.Config.CurrencySign}.").ConfigureAwait (false);
                                return;
                            }

                            await FlowersHandler.RemoveFlowers(e.User, "Gift", (int)amount, true).ConfigureAwait(false);
                            await FlowersHandler.AddFlowersAsync (mentionedUser,"Gift",(int)amount).ConfigureAwait (false);

                            await e.Channel.SendMessage ($"{e.User.Mention} erfolgreich {amount}{MidnightBot.Config.CurrencyName} gesendet an {mentionedUser.Mention}!").ConfigureAwait (false);

                        });

               cgb.CreateCommand(Prefix + "betroll")
                    .Alias(Prefix + "br")
                    .Description($"Wettet einen bestimmten Betrag an {MidnightBot.Config.CurrencyName} und wirft einen Würfel. Bei über 66 Punkten: x2 {MidnightBot.Config.CurrencyName}, über 90 Punkte: x3 und 100 x10. | `{Prefix}br 5`")
                    .Parameter("amount",ParameterType.Required)
                    .Do(async e =>
                    {
                        var amountstr = e.GetArg("amount").Trim();
                        int amount;

                        if (!int.TryParse(amountstr, out amount) || amount < 1)
                            return;

                        var userFlowers = GetUserFlowers(e.User.Id);

                        if (userFlowers < amount)
                        {
                            await e.Channel.SendMessage($"{e.User.Mention} Du hast nicht genug {MidnightBot.Config.CurrencyName}. Du hast nur {userFlowers}{MidnightBot.Config.CurrencySign}.").ConfigureAwait(false);
                            return;
                        }

                        await FlowersHandler.RemoveFlowers(e.User, "Betroll Gamble", (int)amount, true).ConfigureAwait(false);

                        var rng = new Random().Next(0, 101);
                        var str = $"{e.User.Mention} `Du hast folgende Punktzahl geworfen: {rng}` ";
                        if (rng < 67)
                        {
                            str += "Viel Glück beim nächsten Mal.";
                        }
                        else if (rng < 90)
                        {
                            str += $"Glückwunsch! Du hast {amount * 2}{MidnightBot.Config.CurrencySign} für das erzielen von mehr als 66 Punkten gewonnen.";
                            await FlowersHandler.AddFlowersAsync(e.User, "Betroll Gamble", amount * 2, true).ConfigureAwait(false);
                        }
                        else if (rng < 100)
                        {
                            str += $"Glückwunsch! Du hast {amount * 3}{MidnightBot.Config.CurrencySign} für das erzielen von mehr als 90 Punkten gewonnen.";
                            await FlowersHandler.AddFlowersAsync(e.User, "Betroll Gamble", amount * 3, true).ConfigureAwait(false);
                        }
                        else {
                            str += $"👑 Glückwunsch! Du hast {amount * 10}{MidnightBot.Config.CurrencySign} für das erzielen von **100** Punkten gewonnen. 👑";
                            await FlowersHandler.AddFlowersAsync(e.User, "Betroll Gamble", amount * 10, true).ConfigureAwait(false);
                        }

                        await e.Channel.SendMessage(str).ConfigureAwait(false);

                    });

               cgb.CreateCommand (Prefix + "leaderboard")
                    .Alias (Prefix + "lb")
                    .Description($"Displays bot currency leaderboard | {Prefix}lb")
                    .Do (async e =>
                    {
                        var richestTemp = DbHandler.Instance.GetTopRichest ();
                        var richest = richestTemp as CurrencyState[] ?? richestTemp.ToArray ();
                        if (richest.Length == 0)
                            return;
                        await e.Channel.SendMessage
                        (
                            richest.Aggregate (new StringBuilder (
    $@"```xl
┏━━━━━━━━━━━━━━━━━━━━━┳━━━━━━━┓
┃        Id           ┃  $$$  ┃
"),
                            ( cur,cs ) => cur.AppendLine (
    $@"┣━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━┫
┃{(e.Server.Users.Where(u => u.Id == (ulong)cs.UserId).FirstOrDefault()?.Name.TrimTo(18, true) ?? cs.UserId.ToString()),-20} ┃ {cs.Value,5} ┃")
                                    ).ToString() + "┗━━━━━━━━━━━━━━━━━━━━━┻━━━━━━━┛```").ConfigureAwait(false);
                    });
           });
        }

        public static long GetUserFlowers ( ulong userId ) =>
        DbHandler.Instance.GetStateByUserId ((long)userId)?.Value ?? 0;

    }
}