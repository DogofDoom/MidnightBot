using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;

namespace MidnightBot.Modules.Administration.Commands
{
    internal class RatelimitCommand : DiscordCommand
    {

        public static ConcurrentDictionary<ulong,ConcurrentDictionary<ulong,DateTime>> RatelimitingChannels = new ConcurrentDictionary<ulong,ConcurrentDictionary<ulong,DateTime>> ();

        private static readonly TimeSpan ratelimitTime = new TimeSpan (0,0,0,5);

        public RatelimitCommand ( DiscordModule module ) : base (module)
        {
            MidnightBot.Client.MessageReceived += async ( s,e ) =>
            {
                if (e.Channel.IsPrivate || e.User.Id == MidnightBot.Client.CurrentUser.Id || MidnightBot.IsOwner(e.User.Id))
                    return;
                ConcurrentDictionary<ulong,DateTime> userTimePair;
                if (!RatelimitingChannels.TryGetValue (e.Channel.Id,out userTimePair))
                    return;
                DateTime lastMessageTime;
                if (userTimePair.TryGetValue (e.User.Id,out lastMessageTime))
                {
                    if (DateTime.Now - lastMessageTime < ratelimitTime)
                    {
                        try
                        {
                            await e.Message.Delete ().ConfigureAwait (false);
                        }
                        catch { }
                        return;
                    }
                }
                userTimePair.AddOrUpdate (e.User.Id,id => DateTime.Now,( id,dt ) => DateTime.Now);
            };
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "slowmode")
                .Description ($"Schaltet Slow Mode um. Wenn AN, Benutzer können nur alle 5 Sekunden eine Nachricht schicken. | `{Prefix}slowmode`")
                .AddCheck (SimpleCheckers.ManageMessages ())
                .Do (async e =>
                {
                    ConcurrentDictionary<ulong,DateTime> throwaway;
                    if (RatelimitingChannels.TryRemove (e.Channel.Id,out throwaway))
                    {
                        await e.Channel.SendMessage ("Slow Mode deaktiviert.").ConfigureAwait (false);
                        return;
                    }
                    if (RatelimitingChannels.TryAdd (e.Channel.Id,new ConcurrentDictionary<ulong,DateTime> ()))
                    {
                        await e.Channel.SendMessage ("Slow Mode aktiviert. " +
                                                    "Benutzer können nicht mehr als 1 Nachricht, alle 5 Sekunden schicken.")
                                                    .ConfigureAwait (false);
                    }
                });
        }
    }
}