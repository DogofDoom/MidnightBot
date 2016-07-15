using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System.Linq;

namespace MidnightBot.Modules.Administration.Commands
{
    class SelfCommands : DiscordCommand
    {
        public string BotName { get; set; } = MidnightBot.BotName;

        public SelfCommands ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "leave")
                .Description ($"Lässt {BotName} den Server verlassen. Entweder Name, oder ID benötigt. |`.leave 123123123331`")
                .Parameter ("arg",ParameterType.Required)
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                {
                    var arg = e.GetArg ("arg").Trim ();
                    var server = MidnightBot.Client.Servers.FirstOrDefault (s => s.Id.ToString () == arg) ??
                                 MidnightBot.Client.FindServers (arg).FirstOrDefault ();
                    if (server == null)
                    {
                        await e.Channel.SendMessage ("Kann Server nicht finden.").ConfigureAwait (false);
                        return;
                    }
                    if (!server.IsOwner)
                    {
                        await server.Leave ().ConfigureAwait (false);
                    }
                    else
                    {
                        await server.Delete ().ConfigureAwait (false);
                    }
                    await MidnightBot.SendMessageToOwner ($"Server {server.Name} verlassen.").ConfigureAwait (false);
                });
        }
    }
}