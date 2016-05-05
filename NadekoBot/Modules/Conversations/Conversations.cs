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