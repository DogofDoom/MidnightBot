using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;

namespace NadekoBot.Modules.Administration.Commands
{
    internal class RatelimitCommand : DiscordCommand
    {

        public static ConcurrentDictionary<ulong,ConcurrentDictionary<ulong,DateTime>> RatelimitingChannels = new ConcurrentDictionary<ulong,ConcurrentDictionary<ulong,DateTime>> ();

        private static readonly TimeSpan ratelimitTime = new TimeSpan (0,0,0,5);

        public RatelimitCommand ( DiscordModule module ) : base (module)
        {
            NadekoBot.Client.MessageReceived += async ( s,e ) =>
            {
                if (e.Channel.IsPrivate || e.User.Id == NadekoBot.Client.CurrentUser.Id)
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
                .Description ("Schaltet Slow Mode um. Wenn AN, Benutzer k�nnen nur alle 5 Sekunden eine Nachricht schicken.")
                .Parameter ("minutes",ParameterType.Optional)
                .AddCheck (SimpleCheckers.ManageMessages ())
                .Do (async e =>
                {
                    //var minutesStr = e.GetArg("minutes");
                    //if (string.IsNullOrWhiteSpace(minutesStr)) {
                    //    RatelimitingChannels.Remove(e.Channel.Id);
                    //    return;
                    //}
                    ConcurrentDictionary<ulong,DateTime> throwaway;
                    if (RatelimitingChannels.TryRemove (e.Channel.Id,out throwaway))
                    {
                        await e.Channel.SendMessage ("Slow Mode deaktiviert.").ConfigureAwait (false);
                        return;
                    }
                    if (RatelimitingChannels.TryAdd (e.Channel.Id,new ConcurrentDictionary<ulong,DateTime> ()))
                    {
                        await e.Channel.SendMessage ("Slow Mode aktivier. " +
                                                    "Benutzer k�nnen nicht mehr als 1 Nachricht, alle 5 Sekunden schicken.")
                                                    .ConfigureAwait (false);
                    }
                });
        }
    }
}