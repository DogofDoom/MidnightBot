using Discord.Modules;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Programming.Commands;

namespace MidnightBot.Modules.Programming
{
    class ProgrammingModule : DiscordModule
    {
        public override string Prefix => MidnightBot.Config.CommandPrefixes.Programming;

        public ProgrammingModule ()
        {
            commands.Add(new HaskellRepl ( this));
        }

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(PermissionChecker.Instance);
                commands.ForEach(c => c.Init(cgb));
            });
        }
    }
}