using Discord.Commands;
using MidnightBot.Modules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MidnightBot.Classes.Conversations.Commands
{
    internal class CopyCommand : DiscordCommand
    {
        private readonly HashSet<ulong> CopiedUsers = new HashSet<ulong> ();
        public string BotName { get; set; } = MidnightBot.BotName;

        public CopyCommand ( DiscordModule module ) : base (module)
        {
            MidnightBot.Client.MessageReceived += Client_MessageReceived;
        }

        private async void Client_MessageReceived ( object sender,Discord.MessageEventArgs e )
        {
            try
            {
                if (e.User.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                if (string.IsNullOrWhiteSpace (e.Message.Text))
                    return;
                if (CopiedUsers.Contains (e.User.Id))
                {
                    await e.Channel.SendMessage (e.Message.Text).ConfigureAwait (false);
                }
            }
            catch { }
        }

        public Func<CommandEventArgs,Task> DoFunc () => async e =>
        {
            if (CopiedUsers.Contains (e.User.Id))
                return;
            if (MidnightBot.IsOwner (e.User.Id))
            {

                CopiedUsers.Add (e.User.Id);
                await e.Channel.SendMessage (" Ich mache dir nun alles nach.").ConfigureAwait (false);
            }
            else
                await e.Channel.SendMessage ("Nein").ConfigureAwait (false);
        };

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand ("copyme")
                .Alias ("cm")
                .Description ($"{BotName} macht alles nach, was du schreibst. Deaktivieren mit cs")
                .Do (DoFunc ());

            cgb.CreateCommand ("cs")
                .Alias ("copystop")
                .Description ($"{BotName} kopiert dich nicht mehr.")
                .Do (StopCopy ());
        }

        private Func<CommandEventArgs,Task> StopCopy () => async e =>
        {
            if (!CopiedUsers.Contains (e.User.Id))
                return;

            CopiedUsers.Remove (e.User.Id);
            await e.Channel.SendMessage ("Ich mach dir nicht mehr nach.").ConfigureAwait (false);
        };
    }
}