using Discord;
using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes.Conversations.Commands;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using NadekoBot.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Conversations
{
    internal class Conversations : DiscordModule
    {
        private const string firestr = "🔥 ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็ด้้้้้็็็็็้้้้้็็็็็้้้้้้้้็็็็็้้้้้็็็็็้้้้ 🔥";
        private int tester = 0;
        public Conversations ()
        {
            commands.Add (new CopyCommand (this));
            commands.Add (new RequestsCommand (this));
        }

        public override string Prefix { get; } = String.Format (NadekoBot.Config.CommandPrefixes.Conversations,NadekoBot.Creds.BotId);

        public override void Install ( ModuleManager manager )
        {
            var rng = new Random ();

            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                //cgb.CreateCommand ("e")
                //    .Description ("Du, oder jemand anderes hast es geschafft.")
                //    .Parameter ("other",ParameterType.Unparsed)
                //    .Do (async e =>
                //     {
                //         var other = e.GetArg ("other");
                //         if (string.IsNullOrWhiteSpace (other))
                //             await e.Channel.SendMessage ($"{e.User.Name} hat es getan. 😒 🔫").ConfigureAwait (false);
                //         else
                //             await e.Channel.SendMessage ($"{other} hat es getan. 😒 🔫").ConfigureAwait (false);
                //     });

                cgb.CreateCommand ("comeatmebro")
                    .Description ("Come at me bro (ง’̀-‘́)ง \n**Benutzung**: comeatmebro {target}")
                    .Parameter ("target",ParameterType.Optional)
                    .Do (async e =>
                    {
                        var usr = e.Server.FindUsers (e.GetArg ("target")).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("(ง’̀-‘́)ง").ConfigureAwait (false);
                            return;
                        }
                        await e.Channel.SendMessage ($"{usr.Mention} (ง’̀-‘́)ง").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("\\o\\")
                        .Description ("MidnightBot antwortet mit /o/")
                        .Do (async e => await e.Channel.SendMessage (e.User.Mention + "/o/").ConfigureAwait (false));

                cgb.CreateCommand ("/o/")
                    .Description ("MidnightBot antwortet mit  \\o\\")
                    .Do (async e => await e.Channel.SendMessage (e.User.Mention + "\\o\\").ConfigureAwait (false));

                cgb.CreateCommand ("moveto")
                    .Description ("Schlägt vor die Unterhaltung in einem anderen Channel zu führen.\n**benutzung**: moveto #spam")
                    .Parameter ("target",ParameterType.Unparsed)
                    .Do (async e => await e.Channel.SendMessage ($"(👉 ͡° ͜ʖ ͡°)👉 {e.GetArg ("target")}"));

                cgb.CreateCommand ("..")
                        .Description ("Fügt ein neues Zitat mit Namen(ein Wort) und Nachricht (kein Limit).\n**Benutzung**: .. abc My message")
                        .Parameter ("keyword",ParameterType.Required)
                        .Parameter ("text",ParameterType.Unparsed)
                        .Do (async e =>
                        {
                            var text = e.GetArg ("text");
                            if (string.IsNullOrWhiteSpace (text))
                                return;
                            if (NadekoBot.IsOwner (e.User.Id))
                            {
                                await Task.Run (() =>
                                 Classes.DbHandler.Instance.InsertData (new DataModels.UserQuote ()
                                 {
                                     DateAdded = DateTime.Now,
                                     Keyword = e.GetArg ("keyword").ToLowerInvariant (),
                                     Text = text,
                                     UserName = e.User.Name,
                                 })).ConfigureAwait (false);
                            }
                            else if (e.User.Name.ToLowerInvariant () == e.GetArg ("keyword").ToLowerInvariant ())
                            {
                                await Task.Run (() =>
                                 Classes.DbHandler.Instance.InsertData (new DataModels.UserQuote ()
                                 {
                                     DateAdded = DateTime.Now,
                                     Keyword = e.GetArg ("keyword").ToLowerInvariant (),
                                     Text = text,
                                     UserName = e.User.Name,
                                 })).ConfigureAwait (false);
                            }
                            else
                            {
                                await e.Channel.SendMessage ("`Hinzufügen von Zitaten nur von jeweiliger Person erlaubt!`").ConfigureAwait (false);
                                return;
                            }

                            await e.Channel.SendMessage ("`Neues Zitat hinzugefügt.`").ConfigureAwait (false);
                        });

                cgb.CreateCommand ("...")
                    .Description ("Zeigt ein zufälliges Zitat eines Benutzers.\n**Benutzung**: .. abc")
                    .Parameter ("keyword",ParameterType.Required)
                    .Do (async e =>
                    {
                        var keyword = e.GetArg ("keyword")?.ToLowerInvariant ();
                        if (string.IsNullOrWhiteSpace (keyword))
                            return;

                        var quote =
                            Classes.DbHandler.Instance.GetRandom<DataModels.UserQuote> (
                                uqm => uqm.Keyword == keyword);

                        if (quote != null)
                            await e.Channel.SendMessage ($"📣 {quote.Text}").ConfigureAwait (false);
                        else
                            await e.Channel.SendMessage ("💢`Kein Zitat gefunden.`").ConfigureAwait (false);
                    });
            });

            manager.CreateCommands (NadekoBot.BotMention,cgb =>
            {
                var client = manager.Client;

                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand ("uptime")
                    .Description ("Zeigt wie lange Midnight-Bot schon läuft.")
                    .Do (async e =>
                    {
                        var time = (DateTime.Now - Process.GetCurrentProcess ().StartTime);
                        var str = string.Format ("Ich bin online seit: {0} Tagen, {1} Stunden, und {2} Minuten.",time.Days,time.Hours,time.Minutes);
                        await e.Channel.SendMessage (str).ConfigureAwait (false);
                    });

                cgb.CreateCommand ("die")
                    .Description ("Funktioniert nur für den Owner. Fährt den Bot herunter.")
                    .Do (async e =>
                    {
                        if (NadekoBot.IsOwner (e.User.Id))
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
                    .Description ("Antwortet nur dem Owner positiv.")
                    .Do (async e =>
                    {
                        if (NadekoBot.IsOwner (e.User.Id))
                            await e.Channel.SendMessage (e.User.Mention + ", Aber natürlich tue ich das. <3").ConfigureAwait (false);
                        else
                            await e.Channel.SendMessage (e.User.Mention + ", Sei doch nicht dumm. :P").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("how are you")
                    .Alias ("how are you?")
                    .Description ("Antwortet nur positiv, wenn der Owner online ist.")
                    .Do (async e =>
                    {
                        if (NadekoBot.IsOwner (e.User.Id))
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Mir geht es gut, solange du da bist.").ConfigureAwait (false);
                            return;
                        }
                        var kw = e.Server.GetUser (NadekoBot.Creds.OwnerIds[0]);
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
                    .Description ("Beleidigt @X Person.\n**Benutzung**: @MidnightBot insult @X.")
                    .Do (async e =>
                    {
                        var u = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();
                        if (u == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }

                        if (NadekoBot.IsOwner (u.Id))
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
                        else if (u.Id == NadekoBot.Client.CurrentUser.Id)
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Denkst du wirklich ich beleidige mich selber du Schwachkopf. :P")
                            .ConfigureAwait (false);
                            return;
                        }
                        else
                        {

                            tester = 0;
                            //var msgs = (await e.Channel.DownloadMessages (100)).Where (m => m.User.Id == NadekoBot.client.CurrentUser.Id);
                            foreach (var m in (await e.Channel.DownloadMessages (10)).Where (m => m.User.Id == e.User.Id))
                            {
                                if (tester == 0)
                                {
                                    await m.Delete ();
                                    tester++;
                                }
                            }
                            await e.Channel.SendMessage (u.Mention + NadekoBot.Locale.Insults[rng.Next (0,NadekoBot.Locale.Insults.Length)])
                            .ConfigureAwait (false);
                        }

                    });

                cgb.CreateCommand ("praise")
                    .Description ("Lobt @X Person.\n**Benutzung**: @MidnightBot praise @X.")
                    .Parameter ("mention",ParameterType.Required)
                    .Do (async e =>
                    {
                        var u = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();

                        if (u == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }

                        if (NadekoBot.IsOwner (u.Id))
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Ich brauche deine Erlaubnis nicht, um meinen geliebten Meister zu loben <3")
                            .ConfigureAwait (false);
                            return;
                        }
                        else if (u.Id == e.User.Id)
                        {
                            await e.Channel.SendMessage ("Eigenlob stinkt " + e.User.Mention)
                            .ConfigureAwait (false);
                            return;
                        }
                        else
                            await e.Channel.SendMessage (u.Mention + NadekoBot.Locale.Praises[rng.Next (0,NadekoBot.Locale.Praises.Length)])
                            .ConfigureAwait (false);
                    });

                cgb.CreateCommand ("pat")
                  .Description ("Streichle jemanden ^_^")
                  .Parameter ("user",ParameterType.Unparsed)
                  .Do (async e =>
                  {
                      var userStr = e.GetArg ("user");
                      if (string.IsNullOrWhiteSpace (userStr) || !e.Message.MentionedUsers.Any ())
                          return;
                      var user = e.Server.FindUsers (userStr).FirstOrDefault ();
                      if (user == null)
                          return;
                      try
                      {
                          await e.Channel.SendMessage (
                                    $"{user.Mention} " +
                                    $"{NadekoBot.Config.PatResponses[rng.Next (0,NadekoBot.Config.PatResponses.Length)]}")
                                    .ConfigureAwait (false);
                      }
                      catch
                      {
                          await e.Channel.SendMessage ("Fehler. Überprüfe data/config.json").ConfigureAwait (false);
                      }
                  });

                cgb.CreateCommand ("cry")
                  .Description ("Sag MidnightBot, dass er weinen soll. Du bist ein herzloses Monster, wenn du diesen Befehl benutzt.")
                  .Do (async e =>
                  {
                      try
                      {
                          await
                              e.Channel.SendMessage (
                                  $"(•̥́ _•ૅ｡)\n{NadekoBot.Config.CryResponses[rng.Next (0,NadekoBot.Config.CryResponses.Length)]}")
                                  .ConfigureAwait (false);
                      }
                      catch
                      {
                          await e.Channel.SendMessage ("Fehler. Überprüfe data/config.json").ConfigureAwait (false);
                      }
                  });

                cgb.CreateCommand ("disguise")
                  .Description ("Sagt MidnightBot, er soll sich verstecken.")
                  .Do (async e =>
                  {
                      try
                      {
                          await
                              e.Channel.SendMessage (
                                  $"{NadekoBot.Config.DisguiseResponses[rng.Next (0,NadekoBot.Config.DisguiseResponses.Length)]}")
                                  .ConfigureAwait (false);
                      }
                      catch
                      {
                          await e.Channel.SendMessage ("Fehler. Überprüfe data/config.json").ConfigureAwait (false);
                      }
                  });

                cgb.CreateCommand ("are you real")
                    .Description ("Unnütz.")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage (e.User.Mention + " Bald werde ich es sein.").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("are you there")
                    .Description ("Checkt, ob MidnightBot funktioniert")
                    .Alias ("!","?")
                    .Do (SayYes ());

                cgb.CreateCommand ("draw")
                    .Description ("MidnightBot sagt dir das du $draw schreiben sollst. Spiel Funktionen starten mit $")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("Sorry, ich spiele nicht, gib $draw ein für diese Funktion.").ConfigureAwait (false);
                    });
                cgb.CreateCommand ("fire")
                    .Description ("Zeigt eine unicode Feuer Nachricht. Optionaler Parameter [x] sagt ihm wie oft er das Feuer wiederholen soll.\n**Benutzung**: @MidnightBot fire [x]")
                    .Parameter ("times",ParameterType.Optional)
                    .Do (async e =>
                    {
                        var count = 1;
                        int.TryParse (e.Args[0],out count);
                        if (count == 0)
                            count = 1;
                        if (count < 1 || count > 12)
                        {
                            await e.Channel.SendMessage ("Nummer muss zwischen 0 und 12 sein.").ConfigureAwait (false);
                            return;
                        }

                        var str = "";
                        for (var i = 0; i < count; i++)
                        {
                            str += firestr;
                        }
                        await e.Channel.SendMessage (str).ConfigureAwait (false);
                    });

                cgb.CreateCommand ("rip")
                    .Description ("Zeigt ein Grab von jemanden mit einem Startjahr\n**Benutzung**: @MidnightBot rip @Someone 2000")
                    .Parameter ("user",ParameterType.Required)
                    .Parameter ("year",ParameterType.Optional)
                    .Do (async e =>
                    {
                        if (string.IsNullOrWhiteSpace (e.GetArg ("user")))
                            return;
                        var usr = e.Channel.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                        var text = "";
                        text = usr?.Name ?? e.GetArg ("user");
                        await e.Channel.SendFile ("ripzor_m8.png",
                                RipName (text,string.IsNullOrWhiteSpace (e.GetArg ("year"))
                                ? null 
                                : e.GetArg ("year")))
                                .ConfigureAwait (false);
                    });

                if (!NadekoBot.Config.DontJoinServers)
                {
                    cgb.CreateCommand ("j")
                        .Description ("Joint einem Server mit einem Code.")
                        .Parameter ("id",ParameterType.Required)
                        .Do (async e =>
                        {
                            var invite = await client.GetInvite (e.Args[0]).ConfigureAwait (false);
                            if (invite != null)
                            {
                                try
                                {
                                    await invite.Accept ();
                                }
                                catch
                                {
                                    await e.Channel.SendMessage ("Einladung nicht akzeptiert.").ConfigureAwait (false);
                                }
                                await e.Channel.SendMessage ("Ich bin drinnen!").ConfigureAwait (false);
                                return;
                            }
                            await e.Channel.SendMessage ("Ungültiger Code.").ConfigureAwait (false);
                        });
                }

                cgb.CreateCommand ("slm")
                    .Description ("Zeigt die Nachricht in der du in diesem Channel zuletzt erwähnt wurdest (checked die letzten 10k Nachrichten)")
                    .Do (async e =>
                    {

                        Message msg = null;
                        var msgs = (await e.Channel.DownloadMessages (100).ConfigureAwait (false))
                                    .Where (m => m.MentionedUsers.Contains (e.User))
                                    .OrderByDescending (m => m.Timestamp);
                        if (msgs.Any ())
                            msg = msgs.First ();
                        else
                        {
                            var attempt = 0;
                            Message lastMessage = null;
                            while (msg == null && attempt++ < 5)
                            {
                                var msgsarr = await e.Channel.DownloadMessages (100,lastMessage?.Id).ConfigureAwait (false);
                                msg = msgsarr
                                        .Where (m => m.MentionedUsers.Contains (e.User))
                                        .OrderByDescending (m => m.Timestamp)
                                        .FirstOrDefault ();
                                lastMessage = msgsarr.OrderBy (m => m.Timestamp).First ();
                            }
                        }
                        tester = 0;
                        //var msgs = (await e.Channel.DownloadMessages (100)).Where (m => m.User.Id == NadekoBot.client.CurrentUser.Id);
                        foreach (var m in (await e.Channel.DownloadMessages (10)).Where (m => m.User.Id == e.User.Id))
                        {
                            if (tester == 0)
                            {
                                await m.Delete ();
                                tester++;
                            }
                        }
                        if (msg != null)
                            await e.User.SendMessage ($"Die letzte Nachricht in der du erwähnt wurdest war um {msg.Timestamp}\n**Nachricht von {msg.User.Name}:** {msg.RawText}")
                            .ConfigureAwait (false);
                        else
                            await e.User.SendMessage ("Ich kann keine Nachricht finden, in der du erwähnt wirst.").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("bb")
                    .Description ("Sagt bye zu jemanden.\n **Benutztung**: @MidnightBot bb @X")
                    .Parameter ("ppl",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var str = "Bye";
                        foreach (var u in e.Message.MentionedUsers)
                        {
                            if (u.Id != NadekoBot.Client.CurrentUser.Id)
                                str += " " + u.Mention;
                        }
                        await e.Channel.SendMessage (str).ConfigureAwait (false);
                    });

                cgb.CreateCommand ("call")
                    .Description ("Nutzlos. Schreibt calling @X in den Chat.\n**Benutzung**: @MidnightBot call @X ")
                    .Parameter ("who",ParameterType.Required)
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("Calling " + e.Args[0] + "...").ConfigureAwait (false);
                    });
                cgb.CreateCommand ("hide")
                    .Description ("Versteckt MidnightBot!11!!")
                    .Do (async e =>
                    {
                        using (var ms = Resources.hidden.ToStream (ImageFormat.Png))
                        {
                            await client.CurrentUser.Edit (NadekoBot.Creds.Password,avatar: ms);
                        }
                        await e.Channel.SendMessage ("*versteckt sich*").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("unhide")
                    .Description ("MidnightBot kommt aus seinem Versteck!1!!1")
                    .Do (async e =>
                    {
                        using (var fs = new FileStream ("data/avatar.png",FileMode.Open))
                        {
                            await client.CurrentUser.Edit (NadekoBot.Creds.Password,avatar: fs);
                        }
                        await e.Channel.SendMessage ("*kommt aus seinem Versteck*").ConfigureAwait (false);
                    });

                cgb.CreateCommand ("dump")
                    .Description ("Dumped alle Einladungen die er findet in dump.txt.** Owner Only.**")
                    .Do (async e =>
                    {
                        if (!NadekoBot.IsOwner (e.User.Id))
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
                    .Description ("Versuche 'abalabahaha' zu bekommen")
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

                cgb.CreateCommand ("av").Alias ("avatar")
                    .Parameter ("mention",ParameterType.Required)
                    .Description ("Zeigt den Avatar einer erwähnten Person.\n **Benutzung**: ~av @X")
                    .Do (async e =>
                    {
                        var usr = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }
                        await e.Channel.SendMessage (await usr.AvatarUrl.ShortenUrl ()).ConfigureAwait (false);
                    });

                

            });
        }

        public Stream RipName ( string name,string year = null )
        {
            var bm = Resources.rip;

            var offset = name.Length * 5;

            var fontSize = 20;

            if (name.Length > 10)
            {
                fontSize -= (name.Length - 10) / 2;
            }

            //TODO use measure string
            var g = Graphics.FromImage (bm);
            g.DrawString (name,new Font ("Comic Sans MS",fontSize,FontStyle.Bold),Brushes.Black,100 - offset,200);
            g.DrawString ((year ?? "?") + " - " + DateTime.Now.Year,new Font ("Consolas",12,FontStyle.Bold),Brushes.Black,80,235);
            g.Flush ();
            g.Dispose ();

            return bm.ToStream (ImageFormat.Png);
        }

        private static Func<CommandEventArgs,Task> SayYes ()
            => async e => await e.Channel.SendMessage ("Ja. :)").ConfigureAwait (false);
    }
}