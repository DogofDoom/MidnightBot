using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Extensions;
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
                   .Description ("Schreibt den Namen und die ID eines zufälligen Benutzers aus der Online Liste einer (optionalen) Rolle.")
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
                   .Description (string.Format ($"Gibt jemanden eine bestimmte Anzahl an {MidnightBot.Config.CurrencyName}. **Owner only!**\n**Benutzung**: $award 5 @Benutzer"))
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Parameter ("amount",ParameterType.Required)
                   .Parameter ("receiver",ParameterType.Unparsed)
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

                       await FlowersHandler.AddFlowersAsync (mentionedUser,$"Awarded by bot owner. ({e.User.Name}/{e.User.Id})",(int)amount).ConfigureAwait (false);

                       await e.Channel.SendMessage ($"{e.User.Mention} erfolgreich {amount}  {MidnightBot.Config.CurrencyName} zu {mentionedUser.Mention} hinzugefügt!").ConfigureAwait (false);
                   });

               cgb.CreateCommand (Prefix + "take")
                   .Description (string.Format ($"Entfernt eine bestimmte Anzahl an {MidnightBot.Config.CurrencyName} von jemanden. **Owner only!**\n**Benutzung**: $take 5 @Benutzer"))
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

                       FlowersHandler.RemoveFlowers (mentionedUser,$"Taken by bot owner.({e.User.Name}/{e.User.Id})",(int)amount);

                       await e.Channel.SendMessage ($"{e.User.Mention} erfolgreich {amount} {MidnightBot.Config.CurrencyName} von {mentionedUser.Mention} entfernt!").ConfigureAwait (false);
                   });

               cgb.CreateCommand (Prefix + "give")
                        .Description (string.Format ($"Gibt jemanden eine Anzahl {MidnightBot.Config.CurrencyName}.\n**Benutzung**: $give 5 @Benutzer"))
                        .Parameter ("amount",ParameterType.Required)
                        .Parameter ("receiver",ParameterType.Unparsed)
                        .Do (async e =>
                        {
                            var amountStr = e.GetArg ("amount")?.Trim ();
                            long amount;
                            if (!long.TryParse (amountStr,out amount) || amount < 0)
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

                            FlowersHandler.RemoveFlowers (e.User,"Gift",(int)amount);
                            await FlowersHandler.AddFlowersAsync (mentionedUser,"Gift",(int)amount).ConfigureAwait (false);

                            await e.Channel.SendMessage ($"{e.User.Mention} erfolgreich {amount}{MidnightBot.Config.CurrencyName} gesendet an {mentionedUser.Mention}!").ConfigureAwait (false);

                        });
               cgb.CreateCommand (Prefix + "leaderboard")
                    .Alias (Prefix + "lb")
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
┏━━━━━━━━━━━━━━━━━━━┳━━━━━━━┓
┃        Id         ┃  $$$  ┃
"),
                            ( cur,cs ) => cur.AppendLine (
$@"┣━━━━━━━━━━━━━━━━━━━╋━━━━━━━┫
┃{cs.UserId,-18} ┃ {cs.Value,5} ┃")
                                ).ToString () + "┗━━━━━━━━━━━━━━━━━━━┻━━━━━━━┛```");
                    });
           });
        }

        private static long GetUserFlowers ( ulong userId ) =>
        DbHandler.Instance.GetStateByUserId ((long)userId)?.Value ?? 0;

    }
}