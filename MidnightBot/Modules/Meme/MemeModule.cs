using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;

namespace MidnightBot.Modules.Meme
{
    internal class MemeModule : DiscordModule
    {
        public string BotName { get; set; } = MidnightBot.BotName;
        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Searches;
        private static readonly Random rng = new Random ();

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "9gag")
                .Description ("Gets a random 9gag post")
                .Alias (Prefix + "9r")
                .Parameter ("anything",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var meme = await SearchHelper.GetRandomGagPost ().ConfigureAwait (false);
                    await e.Channel.SendMessage (meme).ConfigureAwait (false);
                });

                cgb.CreateCommand("feelsbadman")
                .Alias(new[] { "FeelsBadMan", })
                .Parameter("anything", ParameterType.Unparsed)
                .Description("FeelsBadMan")
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"https://forum-static.hummingbird.me/45789a861219d5ab0ada25c08bcdfd2324c58f882f95c.jpg").ConfigureAwait(false);
                });
                
                cgb.CreateCommand("feelsgoodman")
                .Alias(new[] { "FeelsGoodMan", })
                .Parameter("anything", ParameterType.Unparsed)
                .Description("FeelsGoodMan")
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"http://memesvault.com/wp-content/uploads/Feels-Good-Man-Frog-06.png").ConfigureAwait(false);
                });
            });
        }
    }
}