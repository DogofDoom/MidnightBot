using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Administration.Commands
{
    internal class VoiceNotificationCommand : DiscordCommand
    {

        //voicechannel/text channel
        private readonly ConcurrentDictionary<Channel,Channel> subscribers = new ConcurrentDictionary<Channel,Channel> ();

        public Func<CommandEventArgs,Task> DoFunc () => async e =>
        {
            var arg = e.GetArg ("voice_name");
            if (string.IsNullOrWhiteSpace ("voice_name"))
                return;
            var voiceChannel = e.Server.FindChannels (arg,ChannelType.Voice).FirstOrDefault ();
            if (voiceChannel == null)
                return;
            if (subscribers.ContainsKey (voiceChannel))
            {
                await e.Channel.SendMessage ("`Voice Channel Benachrichtigungen deaktiviert.`").ConfigureAwait (false);
                return;
            }
            if (subscribers.TryAdd (voiceChannel,e.Channel))
            {
                await e.Channel.SendMessage ("`Voice Channel Benachrichtigungen aktiviert.`").ConfigureAwait (false);
            }
        };

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "voicenotif")
                  .Description ("Aktiviert Benachrichtigung wer den Voice-Channel gejoined/verlassen hat. |.voicenotif Karaoke club")
                  .Parameter ("voice_name",ParameterType.Unparsed)
                  .Do (DoFunc ());
        }

        public VoiceNotificationCommand ( DiscordModule module ) : base (module) { }
    }
}