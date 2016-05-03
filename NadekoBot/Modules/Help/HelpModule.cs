using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes.Help.Commands;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System.Linq;

namespace NadekoBot.Modules.Help
{
    internal class HelpModule : DiscordModule
    {

        public HelpModule ()
        {
            commands.Add (new HelpCommand (this));
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Help;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);
                commands.ForEach (com => com.Init (cgb));

                cgb.CreateCommand (Prefix + "modules")
                    .Alias (".modules")
                    .Description ("Listet alle Module des Bots.")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("`Liste der Module:` \n• " + string.Join ("\n• ",NadekoBot.Client.GetService<ModuleService> ().Modules.Select (m => m.Name)))
                        .ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "commands")
                    .Alias (".commands")
                    .Description ("Listet alle Befehle eines bestimmten Moduls.")
                    .Parameter ("module",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var cmds = NadekoBot.Client.GetService<CommandService> ().AllCommands
                                                    .Where (c => c.Category.ToLower () == e.GetArg ("module").Trim ().ToLower ());
                        var cmdsArray = cmds as Command[] ?? cmds.ToArray ();
                        if (!cmdsArray.Any ())
                        {
                            await e.Channel.SendMessage ("Dieses Modul existiert nicht.").ConfigureAwait (false);
                            return;
                        }
                        await e.Channel.SendMessage ("`Liste von Befehlen:` \n• " + string.Join ("\n• ",cmdsArray.Select (c => c.Text)))
                        .ConfigureAwait (false);
                    });
            });
        }
    }
}