using Discord.Commands;
using NadekoBot.Extensions;
using NadekoBot.Modules;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Classes.Help.Commands
{
    internal class HelpCommand : DiscordCommand
    {
        public Func<CommandEventArgs,Task> HelpFunc () => async e =>
        {
                #region OldHelp

                string helpstr = "";

                string lastCategory = "";
                foreach (var com in NadekoBot.Client.GetService<CommandService> ().AllCommands)
                {
                    if (com.Category != lastCategory)
                    {
                        helpstr += "\n`----`**`" + com.Category + "`**`----`\n";
                        lastCategory = com.Category;
                    }
                    helpstr += PrintCommandHelp (com);
                }
                helpstr += "\nBot Creator's server: https://discord.gg/0ehQwTK2RBhxEi0X";
                helpstr = helpstr.Replace (NadekoBot.BotMention,"@BotName");
                var curstr = "";
                while (helpstr.Length > 1000)
                {
                    curstr = helpstr.Substring (0,1000);
                    await e.User.Send (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
                    helpstr = curstr.Substring (curstr.LastIndexOf ("\n") + 1) + helpstr.Substring (1000);
                    await Task.Delay (200).ConfigureAwait (false);
                }
                curstr = helpstr.Substring (0,1000);
                await e.User.Send (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
                helpstr = curstr.Substring (curstr.LastIndexOf ("\n") + 1) + helpstr.Substring (1000);
                await Task.Delay (200).ConfigureAwait (false);

            };

            #endregion OldHelp
            #region NewHelp
        /*
            var comToFind = e.GetArg("command")?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(comToFind))
        {
            await e.User.Send(HelpString).ConfigureAwait(false);
            return;
        }
        await Task.Run(async () => {

            var com = NadekoBot.Client.GetService<CommandService>().AllCommands
                .FirstOrDefault(c => c.Text.ToLowerInvariant().Equals(comToFind) ||
                                        c.Aliases.Select(a => a.ToLowerInvariant()).Contains(comToFind));
            if (com != null)
                await e.Channel.SendMessage($"`Help for '{com.Text}':` **{com.Description}**").ConfigureAwait(false);
        }).ConfigureAwait(false);
    };
    */
        public static string HelpString => NadekoBot.IsBot ? $"Um {NadekoBot.Client.CurrentUser.Name} zu deinem Server einzuladen, gehe hierhin: <>\n" : "" +
                                       $"Du kannst `{NadekoBot.Config.CommandPrefixes.Help}modules` benutzencommand um eine Liste aller Module zu sehen.\n" +
                                       $"Du kannst `{NadekoBot.Config.CommandPrefixes.Help}commands ModuleName`" +
                                       $" (zum Beispiel `{NadekoBot.Config.CommandPrefixes.Help}commands Administration`) benutzen um eine Liste aller Befehle des Modules zu sehen.\n" +
                                       $"Für die Hilfe bei einem bestimmten Befehl, benutze `{NadekoBot.Config.CommandPrefixes.Help}h \"Command name\"` (zum Beispiel `-h \"! q\"`)";

        public static string DMHelpString => NadekoBot.Config.DMHelpString;

        public Action<CommandEventArgs> DoGitFunc () => e =>
        {
            string helpstr =
    $@"######For more information and how to setup your own NadekoBot, go to: **http://github.com/Kwoth/NadekoBot/**
######You can donate on paypal: `nadekodiscordbot@gmail.com` or Bitcoin `17MZz1JAqME39akMLrVT4XBPffQJ2n1EPa`

#NadekoBot List Of Commands  
Version: `{NadekoStats.Instance.BotVersion}`";


            string lastCategory = "";
            foreach (var com in NadekoBot.Client.GetService<CommandService> ().AllCommands)
            {
                if (com.Category != lastCategory)
                {
                    helpstr += "\n### " + com.Category + "  \n";
                    helpstr += "Befehl und Alternativen | Beschreibung | Benutzung\n";
                    helpstr += "----------------|--------------|-------\n";
                    lastCategory = com.Category;
                }
                helpstr += PrintCommandHelp (com);
            }
            helpstr = helpstr.Replace (NadekoBot.BotMention,"@BotName");
            helpstr = helpstr.Replace ("\n**Benutzung**:"," | ").Replace ("**Benutzung**:"," | ").Replace ("**Beschreibung:**"," | ").Replace ("\n|"," |  \n");
#if DEBUG
            File.WriteAllText ("../../../commandlist.md",helpstr);
#else
        File.WriteAllText("commandlist.md", helpstr);
#endif
        };

        #endregion NewHelp 
        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "h")
                .Alias (Module.Prefix + "help",NadekoBot.BotMention + " help",NadekoBot.BotMention + " h","~h")
                .Description ("Hilfe-Befehl.\n**Usage**: '-h !m q' or just '-h' ")
                .Parameter ("command",ParameterType.Unparsed)
                .Do (HelpFunc ());
            cgb.CreateCommand (Module.Prefix + "hgit")
                .Description ("OWNER ONLY commandlist.md Datei erstellung. **Owner Only!**")
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (DoGitFunc ());
            cgb.CreateCommand (Module.Prefix + "readme")
                .Alias (Module.Prefix + "guide")
                .Description ("Sendet eine readme und ein Guide verlinkt zum Channel.")
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                     await e.Channel.SendMessage (
 @"**FULL README**: <https://github.com/Kwoth/NadekoBot/blob/master/README.md>

**GUIDE ONLY**: <https://github.com/Kwoth/NadekoBot/blob/master/ComprehensiveGuide.md>

**LIST OF COMMANDS**: <https://github.com/Kwoth/NadekoBot/blob/master/commandlist.md>").ConfigureAwait (false));

            cgb.CreateCommand (Module.Prefix + "donate")
                .Alias ("~donate")
                .Description ("Informationen um das Projekt zu unterstützen!")
                .Do (async e =>
                {
                    await e.Channel.SendMessage (
@"Habt Spaß"
                    ).ConfigureAwait (false);
                });
        }

        private static string PrintCommandHelp ( Command com )
        {
            var str = "`" + com.Text + "`";
            str = com.Aliases.Aggregate (str,( current,a ) => current + (", `" + a + "`"));
            str += " **Beschreibung:** " + com.Description + "\n";
            return str;
        }

        public HelpCommand ( DiscordModule module ) : base (module) { }
    }
}