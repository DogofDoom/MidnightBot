using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

/* Voltana's legacy
public class AsyncLazy<T> : Lazy<Task<T>> 
{ 
    public AsyncLazy(Func<T> valueFactory) : 
        base(() => Task.Factory.StartNew(valueFactory)) { }

    public AsyncLazy(Func<Task<T>> taskFactory) : 
        base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap()) { } 

    public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); } 
}
*/

namespace MidnightBot.Modules.Administration.Commands
{
    internal class ServerGreetCommand : DiscordCommand
    {

        public static ConcurrentDictionary<ulong,AnnounceControls> AnnouncementsDictionary;

        public static long Greeted = 0;

        public ServerGreetCommand ( DiscordModule module ) : base (module)
        {
            AnnouncementsDictionary = new ConcurrentDictionary<ulong,AnnounceControls> ();

            MidnightBot.Client.UserJoined += UserJoined;
            MidnightBot.Client.UserLeft += UserLeft;

            var data = Classes.DbHandler.Instance.GetAllRows<DataModels.Announcement> ();

            if (!data.Any ())
                return;
            foreach (var obj in data)
                AnnouncementsDictionary.TryAdd ((ulong)obj.ServerId,new AnnounceControls (obj));
        }

        private async void UserLeft ( object sender,UserEventArgs e )
        {
            try
            {
                if (!AnnouncementsDictionary.ContainsKey (e.Server.Id) ||
                    !AnnouncementsDictionary[e.Server.Id].Bye)
                    return;

                var controls = AnnouncementsDictionary[e.Server.Id];
                var channel = MidnightBot.Client.GetChannel (controls.ByeChannel);
                var msg = controls.ByeText.Replace ("%user%","**" + e.User.Name + "**").Trim ();
                if (string.IsNullOrEmpty (msg))
                    return;

                if (controls.ByePM)
                {
                    Greeted++;
                    try
                    {
                        await e.User.SendMessage ($"`Verabschiedungs-Nachricht von {e.Server?.Name}`\n" + msg).ConfigureAwait (false);
                    }
                    catch { }
                }
                else
                {
                    if (channel == null)
                        return;
                    Greeted++;
                    var toDelete = await channel.SendMessage (msg).ConfigureAwait (false);
                    if (e.Server.CurrentUser.GetPermissions (channel).ManageMessages && controls.DeleteGreetMessages)
                    {
                        await Task.Delay (30000).ConfigureAwait (false); // 5 minutes
                        await toDelete.Delete ().ConfigureAwait (false);
                    }
                }
            }
            catch { }
        }

        private async void UserJoined ( object sender,Discord.UserEventArgs e )
        {
            try
            {
                if (!AnnouncementsDictionary.ContainsKey (e.Server.Id) ||
                    !AnnouncementsDictionary[e.Server.Id].Greet)
                    return;

                var controls = AnnouncementsDictionary[e.Server.Id];
                var channel = MidnightBot.Client.GetChannel (controls.GreetChannel);

                var msg = controls.GreetText.Replace ("%user%",e.User.Mention).Trim ();
                msg = msg.Replace ("%name%",e.User.Name);
                if (string.IsNullOrEmpty (msg))
                    return;
                if (controls.GreetPM)
                {
                    Greeted++;
                    await e.User.SendMessage ($"`Willkommensnachricht von {e.Server.Name}`\n" + msg).ConfigureAwait (false);
                }
                else
                {
                    if (channel == null)
                        return;
                    Greeted++;
                    var toDelete = await channel.SendMessage (msg).ConfigureAwait (false);
                    if (e.Server.CurrentUser.GetPermissions (channel).ManageMessages && controls.DeleteGreetMessages)
                    {
                        await Task.Delay (30000).ConfigureAwait (false); // 5 minutes
                        await toDelete.Delete ().ConfigureAwait (false);
                    }
                }
            }
            catch { }
        }

        public class AnnounceControls
        {
            private DataModels.Announcement _model { get; }

            public bool Greet
            {
                get { return _model.Greet; }
                set { _model.Greet = value; Save (); }
            }

            public ulong GreetChannel
            {
                get { return (ulong)_model.GreetChannelId; }
                set { _model.GreetChannelId = (long)value; Save (); }
            }

            public bool GreetPM
            {
                get { return _model.GreetPM; }
                set { _model.GreetPM = value; Save (); }
            }

            public bool ByePM
            {
                get { return _model.ByePM; }
                set { _model.ByePM = value; Save (); }
            }

            public string GreetText
            {
                get { return _model.GreetText; }
                set { _model.GreetText = value; Save (); }
            }

            public bool Bye
            {
                get { return _model.Bye; }
                set { _model.Bye = value; Save (); }
            }
            public ulong ByeChannel
            {
                get { return (ulong)_model.ByeChannelId; }
                set { _model.ByeChannelId = (long)value; Save (); }
            }

            public string ByeText
            {
                get { return _model.ByeText; }
                set { _model.ByeText = value; Save (); }
            }

            public ulong ServerId
            {
                get { return (ulong)_model.ServerId; }
                set { _model.ServerId = (long)value; }
            }

            public bool DeleteGreetMessages {
                get {
                    return _model.DeleteGreetMessages;
                }
                set {
                    _model.DeleteGreetMessages = value; Save();
                }
            }

            public AnnounceControls ( DataModels.Announcement model )
            {
                this._model = model;
            }

            public AnnounceControls ( ulong serverId )
            {
                this._model = new DataModels.Announcement ();
                ServerId = serverId;
            }

            internal bool ToggleBye ( ulong id )
            {
                if (Bye)
                {
                    return Bye = false;
                }
                else
                {
                    ByeChannel = id;
                    return Bye = true;
                }
            }

            internal bool ToggleGreet ( ulong id )
            {
                if (Greet)
                {
                    return Greet = false;
                }
                else
                {
                    GreetChannel = id;
                    return Greet = true;
                }
            }

            internal bool ToggleDelete () => DeleteGreetMessages = !DeleteGreetMessages;
            internal bool ToggleGreetPM () => GreetPM = !GreetPM;
            internal bool ToggleByePM () => ByePM = !ByePM;

            private void Save ()
            {
                Classes.DbHandler.Instance.Save (_model);
            }
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand(Module.Prefix + "grdel")
                .Description($"Aktiviert oder deaktiviert automatische Löschung von Willkommens- und Verabschiedungsnachrichten. | `{Prefix}grdel`")
                .Do(async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer) return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (ann.ToggleDelete ())
                        await e.Channel.SendMessage("`Automatische Löschung von Willkommens- und Verabschiedungsnachrichten wurde aktiviert.`").ConfigureAwait(false);
                    else
                        await e.Channel.SendMessage("`Automatische Löschung von Willkommens- und Verabschiedungsnachrichten wurde deaktiviert.`").ConfigureAwait(false);
                });

            cgb.CreateCommand (Module.Prefix + "greet")
                .Description ($"Aktiviert oder deaktiviert Benachrichtigungen auf dem derzeitigen Channel wenn jemand dem Server beitritt. | `{Prefix}greet`")
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (ann.ToggleGreet (e.Channel.Id))
                        await e.Channel.SendMessage ("Begrüßung aktiviert auf diesem Channel.").ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Begrüßung deaktiviert.").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "greetmsg")
                .Description ($"Setzt einen neuen Gruß. Gib %user% ein, wenn du den neuen Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. | `{Prefix}greetmsg Welcome to the server, %user%.´")
                .Parameter ("msg",ParameterType.Unparsed)
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (string.IsNullOrWhiteSpace (e.GetArg ("msg")))
                    {
                        await e.Channel.SendMessage ("`Derzeitige Gruß Nachricht:` " + ann.GreetText);
                        return;
                    }
                    ann.GreetText = e.GetArg ("msg");
                    await e.Channel.SendMessage ("Neuer Gruß gesetzt.");
                    if (!ann.Greet)
                        await e.Channel.SendMessage ("Aktiviere Begrüßungen mit `.greet`").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "bye")
                .Description ($"Aktiviert, oder deaktiviert Benachrichtigungen, wenn ein Benutzer den Server verlässt. | `{Prefix}bye`")
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (ann.ToggleBye (e.Channel.Id))
                        await e.Channel.SendMessage ("Verabschiedung aktiviert.").ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Verabschiedung deaktiviert.").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "byemsg")
                .Description ($"Setzt eine neue Verabschiedung. Gib %user% ein, wenn du den Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. | `{Prefix}byemsg %user% has left the server.´")
                .Parameter ("msg",ParameterType.Unparsed)
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (string.IsNullOrWhiteSpace (e.GetArg ("msg")))
                    {
                        await e.Channel.SendMessage ("`Derzeitige Verabschiedungs Nachricht:` " + ann.ByeText);
                        return;
                    }

                    ann.ByeText = e.GetArg ("msg");
                    await e.Channel.SendMessage ("Neue Verabschiedung gesetzt.");
                    if (!ann.Bye)
                        await e.Channel.SendMessage ("Verabschiedung aktivieren mit `.bye`.").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "byepm")
                .Description ($"Stellt ein ob die Verabschiedung im Channel, oder per PN geschickt wird. | `{Prefix}byepm`")
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));

                    if (ann.ToggleByePM ())
                        await e.Channel.SendMessage ("Verabschiedung wird per PN geschickt.\n ⚠ Falls der Bot und der User keine gleichen Channel haben, wird dies nicht funktionieren.").ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Verabschiedungen werden im Channel geschrieben.").ConfigureAwait (false);
                    if (!ann.Bye)
                        await e.Channel.SendMessage ("Verabschiedung aktivieren mit `.bye`, setzen der Verabschiedung mit `.byemsg`").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "greetpm")
                .Description ($"Stellt ein ob der Gruß im Channel, oder per PN geschickt wird. | `{Prefix}greetpm`")
                .Do (async e =>
                {
                    if (!e.User.ServerPermissions.ManageServer)return;
                    var ann = AnnouncementsDictionary.GetOrAdd (e.Server.Id,new AnnounceControls (e.Server.Id));
                    
                                        if (ann.ToggleGreetPM ())
                        await e.Channel.SendMessage ("Gruß wird per PN geschickt.").ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Gruß wird im Channel geschickt.").ConfigureAwait (false);
                    if (!ann.Greet)
                        await e.Channel.SendMessage ("Gruß aktivieren mit `.greet`, Gruß setzen mit `.greetmsg`").ConfigureAwait (false);
                });
        }
    }
}