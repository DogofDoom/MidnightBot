using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Extensions;
using MidnightBot.Modules.Conversations.Commands;
using MidnightBot.DataModels;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MidnightBot.Modules.Conversations
{
    internal class Conversations : DiscordModule
    {
        private const string firestr = "🔥 ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้ 🔥";
        private int tester = 0;
        public Conversations ()
        {
            commands.Add (new RipCommand (this));
        }
        public string BotName { get; set; } = MidnightBot.BotName;

        public override string Prefix { get; } = String.Format (MidnightBot.Config.CommandPrefixes.Conversations,MidnightBot.Creds.BotId);

        public override void Install ( ModuleManager manager )
        {
            var rng = new Random ();

            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                cgb.CreateCommand ("..")
                    .Description ("Fügt ein neues Zitat mit Keyword (einzelnes Wort) und Nachricht (kein Limit). | `.. abc My message`")
                    .Parameter ("keyword",ParameterType.Required)
                    .Parameter ("text",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var text = e.GetArg ("text");
                        if (string.IsNullOrWhiteSpace (text))
                            return;
                        await Task.Run (() =>
                             Classes.DbHandler.Instance.Connection.Insert(new DataModels.UserQuote()
                             {
                                 DateAdded = DateTime.Now,
                                 Keyword = e.GetArg ("keyword").ToLowerInvariant (),
                                 Text = text,
                                 UserName = e.User.Name,
                             })).ConfigureAwait (false);

                        await e.Channel.SendMessage ("`Neues Zitat hinzugefügt.`").ConfigureAwait (false);
                    });


                cgb.CreateCommand ("...")
                    .Description ("Zeigt ein zufälliges Zitat eines Benutzers. | `... abc`")
                    .Parameter ("keyword",ParameterType.Required)
                    .Do (async e =>
                    {
                        var keyword = e.GetArg ("keyword")?.ToLowerInvariant ();
                        if (string.IsNullOrWhiteSpace (keyword))
                            return;

                        var quote =
                            Classes.DbHandler.Instance.GetRandom<UserQuote> (
                                uqm => uqm.Keyword == keyword);

                        if (quote != null)
                            await e.Channel.SendMessage ($"📣 {quote.Text}").ConfigureAwait (false);
                        else
                            await e.Channel.SendMessage ("💢`Kein Zitat gefunden.`").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("..qdel")
                    .Alias ("..quotedelete")
                    .Description ("Löscht alle Zitate mit angegebenen Keyword. Du musst entweder der Bot-Besitzer oder der Ersteller des Quotes sein um es zu löschen. | `..qdel abc`")
                    .Parameter ("quote",ParameterType.Required)
                    .Do (async e =>
                    {
                        var text = e.GetArg ("quote")?.Trim ();
                        if (string.IsNullOrWhiteSpace (text))
                            return;
                        await Task.Run (() =>
                        {
                            if (MidnightBot.IsOwner (e.User.Id))
                                Classes.DbHandler.Instance.DeleteWhere<UserQuote> (uq => uq.Keyword == text);
                            else
                                Classes.DbHandler.Instance.DeleteWhere<UserQuote> (uq => uq.Keyword == text && uq.UserName == e.User.Name || uq.Keyword == e.User.Name.ToLowerInvariant());
                        }).ConfigureAwait (false);

                        await e.Channel.SendMessage ("`Erledigt.`").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("..qdelothers")
                    .Alias ("..quotedeleteothers")
                    .Description ("Löscht alle Zitate mit eigenem Namen als Keyword, welche von anderen geaddet wurden.  | `..qdelothers`")
                    .Do (async e =>
                    {
                        var text = e.User.Name.ToLowerInvariant ();
                        await Task.Run (() =>
                        {
                            Classes.DbHandler.Instance.DeleteWhere<UserQuote> (uq => uq.Keyword == text && uq.UserName != e.User.Name);
                        }).ConfigureAwait (false);

                        await e.Channel.SendMessage ("`Erledigt.`").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("..qshow")
                    .Description ("Zeigt alle Zitate mit angegebenen Keyword. | `..qshow abc`")
                    .Parameter ("quote",ParameterType.Required)
                    .Do (async e =>
                    {
                        var text = e.GetArg ("quote")?.Trim ();
                        var allQuotes = "";
                        var number = 1;
                        if (string.IsNullOrWhiteSpace (text))
                            return;
                        await Task.Run (() =>
                        {
                            var quoteList = Classes.DbHandler.Instance.FindAll<UserQuote> (uq => uq.Keyword == text);
                            foreach(UserQuote quote in quoteList)
                            {
                                allQuotes += $"{number}. `{quote.Text}` **Hinzugefügt von: {quote.UserName}**\n";
                                number++;
                            }
                        }).ConfigureAwait (false);

                        await e.Channel.SendMessage (allQuotes).ConfigureAwait (false);
                    });
            });

            manager.CreateCommands (MidnightBot.BotMention,cgb =>
            {
                var client = manager.Client;

                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand ("die")
                    .Description ($"Funktioniert nur für den Owner. Fährt den Bot herunter. | `@{BotName} die`")
                    .Do (async e =>
                    {
                        if (MidnightBot.IsOwner (e.User.Id))
                        {
                            await e.Channel.SendMessage (e.User.Mention + ", Alles klar Chef.").ConfigureAwait (false);
                            await Task.Delay (5000).ConfigureAwait (false);
                            Environment.Exit (0);
                        }
                        else
                            await e.Channel.SendMessage (e.User.Mention + ", Nein.");
                    });

                var randServerSw = new Stopwatch ();
                randServerSw.Start ();

                cgb.CreateCommand ("do you love me")
                    .Alias ("do you love me?")
                    .Description ($"Antwortet nur dem Owner positiv. | `@{BotName} do you love me`")
                    .Do (async e =>
                    {
                        if (MidnightBot.IsOwner (e.User.Id))
                            await e.Channel.SendMessage (e.User.Mention + ", Aber natürlich tue ich das. <3").ConfigureAwait (false);
                        else
                            await e.Channel.SendMessage (e.User.Mention + ", Sei doch nicht dumm. :P").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("how are you")
                    .Alias ("how are you?")
                    .Description ($"Antwortet nur positiv, wenn der Owner online ist. | `@{BotName} do you love me`")
                    .Do (async e =>
                    {
                        if (MidnightBot.IsOwner (e.User.Id))
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Mir geht es gut, solange du da bist.").ConfigureAwait (false);
                            return;
                        }
                        var kw = e.Server.GetUser (MidnightBot.Creds.OwnerIds[0]);
                        if (kw != null && kw.Status == UserStatus.Online)
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Mir geht es gut, solange " + kw.Mention + " hier mit mir ist.").ConfigureAwait (false);
                        }
                        else
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Ich bin traurig. Mein Meister ist nicht hier.").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand ("insult")
                    .Parameter ("mention",ParameterType.Required)
                    .Description ($"Beleidigt @X Person. | @{BotName} insult @X.")
                    .Do (async e =>
                    {
                        var u = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();
                        if (u == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }

                        if (MidnightBot.IsOwner (u.Id))
                        {
                            await e.Channel.SendMessage ("Ich würde nie meinen Meister beleidigen. <3").ConfigureAwait (false);
                            return;
                        }
                        else if (u.Id == e.User.Id)
                        {
                            await e.Channel.SendMessage ("Man muss schon doof sein, wenn man sich selber beleidigen will.")
                            .ConfigureAwait (false);
                            return;
                        }
                        else if (u.Id == MidnightBot.Client.CurrentUser.Id)
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Denkst du wirklich ich beleidige mich selbst, du Schwachkopf. :P")
                            .ConfigureAwait (false);
                            return;
                        }
                        else
                        {

                            tester = 0;
                            //var msgs = (await e.Channel.DownloadMessages (100)).Where (m => m.User.Id == MidnightBot.client.CurrentUser.Id);
                            foreach (var m in (await e.Channel.DownloadMessages (10)).Where (m => m.User.Id == e.User.Id))
                            {
                                if (tester == 0)
                                {
                                    await m.Delete ();
                                    tester++;
                                }
                            }
                            await e.Channel.SendMessage (u.Mention + MidnightBot.Locale.Insults[rng.Next (0,MidnightBot.Locale.Insults.Length)])
                            .ConfigureAwait (false);
                        }

                    });

                cgb.CreateCommand ("praise")
                    .Description ($"Lobt @X Person. | @{BotName} praise @X.")
                    .Parameter ("mention",ParameterType.Required)
                    .Do (async e =>
                    {
                        var u = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();

                        if (u == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }

                        if (MidnightBot.IsOwner (u.Id))
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Ich brauche deine Erlaubnis nicht, um meinen Meister zu loben <3")
                            .ConfigureAwait (false);
                            return;
                        }
                        else if (u.Id == e.User.Id)
                        {
                            await e.Channel.SendMessage ($"Eigenlob stinkt {e.User.Mention}")
                            .ConfigureAwait (false);
                            return;
                        }
                        else
                            await e.Channel.SendMessage (u.Mention + MidnightBot.Locale.Praises[rng.Next (0,MidnightBot.Locale.Praises.Length)])
                            .ConfigureAwait (false);
                    });

                cgb.CreateCommand ("fire")
                    .Description ($"Zeigt eine unicode Feuer Nachricht. Optionaler Parameter [x] sagt ihm wie oft er das Feuer wiederholen soll. | `@{BotName} fire [x]`")
                    .Parameter ("times",ParameterType.Optional)
                    .Do (async e =>
                    {
                        int count;
                        if (string.IsNullOrWhiteSpace(e.Args[0]))
                            count = 1;
                        else
                            int.TryParse(e.Args[0], out count);
                        if (count < 1 || count > 12)
                        {
                            await e.Channel.SendMessage ("Nummer muss zwischen 1 und 12 sein.").ConfigureAwait (false);
                            return;
                        }

                        var str = new StringBuilder();
                        for (var i = 0; i < count; i++)
                        {
                            str.Append(firestr);
                        }
                        await e.Channel.SendMessage(str.ToString()).ConfigureAwait(false);
                    });

                cgb.CreateCommand ("dump")
                    .Description ($"Dumped alle Einladungen die er findet in dump.txt.** Owner Only.** | `@{BotName} dump`")
                    .Do (async e =>
                    {
                        if (!MidnightBot.IsOwner (e.User.Id))
                            return;
                        var i = 0;
                        var j = 0;
                        var invites = "";
                        foreach (var s in client.Servers)
                        {
                            try
                            {
                                var invite = await s.CreateInvite (0).ConfigureAwait (false);
                                invites += invite.Url + "\n";
                                i++;
                            }
                            catch
                            {
                                j++;
                                continue;
                            }
                        }
                        File.WriteAllText ("dump.txt",invites);
                        await e.Channel.SendMessage ($"Ich habe Einladungen für {i} Server bekommen und konnte für {j} Server keine Einladungen finden.")
                                       .ConfigureAwait (false);
                    });

                cgb.CreateCommand ("ab")
                    .Description ($"Versuche 'abalabahaha' zu bekommen.| `@{BotName} ab`")
                    .Do (async e =>
                    {
                        string[] strings = { "ba","la","ha" };
                        var construct = "@a";
                        var cnt = rng.Next (4,7);
                        while (cnt-- > 0)
                        {
                            construct += strings[rng.Next (0,strings.Length)];
                        }
                        await e.Channel.SendMessage (construct).ConfigureAwait (false);
                    });
            });
        }
        
        private static Func<CommandEventArgs,Task> SayYes ()
            => async e => await e.Channel.SendMessage ("Ja. :)").ConfigureAwait (false);
    }
}