using MidnightBot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MidnightBot.Modules.Permissions.Classes;

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
                .Description ($"Lässt {BotName} den Server verlassen. Entweder Name, oder ID benötigt.\n**Benutzung**:.leave NSFW")
                .Parameter ("arg",ParameterType.Required)
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                {
                    var arg = e.GetArg ("arg")?.Trim ();
                    var server = MidnightBot.Client.Servers.FirstOrDefault (s => s.Id.ToString () == arg) ??
                                 MidnightBot.Client.FindServers (arg.Trim ()).FirstOrDefault ();
                    if (server == null)
                    {
                        await e.Channel.SendMessage ("Kann Server nicht finden.").ConfigureAwait (false);
                        return;
                    }
                    if (!server.IsOwner)
                    {
                        await server.Leave ();
                    }
                    else
                    {
                        await server.Delete ();
                    }
                    await MidnightBot.SendMessageToOwner ($"Server {server.Name} verlassen.");
                });
        }
    }
}