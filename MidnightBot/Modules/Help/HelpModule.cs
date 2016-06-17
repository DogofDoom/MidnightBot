using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes.Help.Commands;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;
using System.Linq;

namespace MidnightBot.Modules.Help
{
    internal class HelpModule : DiscordModule
    {

        public HelpModule ()
        {
            commands.Add (new HelpCommand (this));
        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Help;

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
                        await e.Channel.SendMessage ("`Liste der Module:` \n• " + string.Join ("\n• ",MidnightBot.Client.GetService<ModuleService> ().Modules.Select (m => m.Name)) + $"\n`Gib den Befehl \"{Prefix}commands module_name\" ein um eine Liste der Befehle dieses Moduls zu bekommen.`")
                        .ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "commands")
                    .Alias (".commands")
                    .Description ("Listet alle Befehle eines bestimmten Moduls.")
                    .Parameter ("module",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var cmds = MidnightBot.Client.GetService<CommandService> ().AllCommands
                                                    .Where (c => c.Category.ToLower () == e.GetArg ("module").Trim ().ToLower ());
                        var cmdsArray = cmds as Command[] ?? cmds.ToArray ();
                        if (!cmdsArray.Any ())
                        {
                            await e.Channel.SendMessage ("Dieses Modul existiert nicht.").ConfigureAwait (false);
                            return;
                        }
                        var i = 0;
                        await e.Channel.SendMessage ("`Liste der Befehle:`\n```xl\n" + string.Join ("\n",cmdsArray.GroupBy (item => (i++) / 3).Select (ig => string.Join ("",ig.Select (el => $"{el.Text,-22}")))) + $"\n``` `Gib den Befehl \"{Prefix}h command_name\" ein, um Hilfe für einen spezifischen Befehl zu bekommen.`").ConfigureAwait (false);
                    });
            });
        }
    }
}