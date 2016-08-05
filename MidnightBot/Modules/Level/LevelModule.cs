using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;

using MidnightBot.Modules.Level.Classes;
using MidnightBot.Modules.Level.Commands;
using MidnightBot.Extensions;

namespace MidnightBot.Modules.Level
{
    internal class LevelModule : DiscordModule
    {

        public LevelModule ()
        {
            commands.Add(new RankCommand(this));
        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Level;

        public override void Install (ModuleManager manager)
        {
            manager.CreateCommands ("", cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach(cmd => cmd.Init(cgb));
            });

            MessageHandler handler = new MessageHandler();

            manager.MessageReceived += handler.messageReceived;
            manager.MessageDeleted += handler.messageDeleted;
            manager.MessageUpdated += handler.messageUpdated;

            JoinHandler joinHandler = new JoinHandler();

            manager.UserJoined += joinHandler.serverJoined;
        }

    }
}
