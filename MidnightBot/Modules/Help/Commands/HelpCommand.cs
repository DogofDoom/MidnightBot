using Discord.Commands;
using MidnightBot.Extensions;
using MidnightBot.Modules;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Classes.Help.Commands
{
    internal class HelpCommand : DiscordCommand
    {
        public Func<CommandEventArgs,Task> HelpFunc () => async e =>
        {
            #region OldHelp
            
            string helpstr = "";
            var comToFind = e.GetArg ("module")?.ToLowerInvariant ();
            if (string.IsNullOrWhiteSpace (comToFind))
            {

                string lastCategory = "";
                foreach (var com in MidnightBot.Client.GetService<CommandService> ().AllCommands)
                {
                    if (com.Category != lastCategory)
                    {
                        helpstr += "\n`----`**`" + com.Category + "`**`----`\n";
                        lastCategory = com.Category;
                    }
                    helpstr += PrintCommandHelp (com);
                }
                helpstr += "\nBot Creator's server: https://discord.gg/0ehQwTK2RBhxEi0X";
                helpstr = helpstr.Replace (MidnightBot.BotMention,"@BotName");
                var curstr = "";
                while (helpstr.Length > 2000)
                {
                    curstr = helpstr.Substring (0,2000);
                    await e.User.SendMessage (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
                    helpstr = curstr.Substring (curstr.LastIndexOf ("\n") + 1) + helpstr.Substring (2000);
                    await Task.Delay (200).ConfigureAwait (false);
                }
                curstr = helpstr;
                await e.User.SendMessage (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
                return;
            }
            else
            {
                var cmds = MidnightBot.Client.GetService<CommandService> ().AllCommands
                                                    .Where (c => c.Category.ToLower () == e.GetArg ("module").Trim ().ToLower ());
                var cmdsArray = cmds as Command[] ?? cmds.ToArray ();
                if (!cmdsArray.Any ())
                {
                    await e.Channel.SendMessage ("Dieses Modul existiert nicht.").ConfigureAwait (false);
                    return;
                }
                string lastCategory = "";
                foreach (var com in MidnightBot.Client.GetService<CommandService> ().AllCommands.Where (c => c.Category.ToLower () == e.GetArg ("module").Trim ().ToLower ()))
                {
                    if (com.Category != lastCategory)
                    {
                        helpstr += "\n`----`**`" + com.Category + "`**`----`\n";
                        lastCategory = com.Category;
                    }
                    helpstr += PrintCommandHelp (com);
                }
                helpstr += "\nBot Creator's server: https://discord.gg/0ehQwTK2RBhxEi0X";
                helpstr = helpstr.Replace (MidnightBot.BotMention,"@BotName");
                var curstr = "";
                while (helpstr.Length > 2000)
                {
                    curstr = helpstr.Substring (0,2000);
                    await e.User.SendMessage (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
                    helpstr = curstr.Substring (curstr.LastIndexOf ("\n") + 1) + helpstr.Substring (2000);
                    await Task.Delay (200).ConfigureAwait (false);
                }
                curstr = helpstr;
                await e.User.SendMessage (curstr.Substring (0,curstr.LastIndexOf ("\n") + 1)).ConfigureAwait (false);
            }
            };

        #endregion OldHelp
        #region NewHelp
        public Func<CommandEventArgs,Task> NewHelpFunc () => async e =>
        {
            var comToFind = e.GetArg("command")?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(comToFind))
        {
            await e.User.Send(HelpString).ConfigureAwait(false);
            return;
        }
        await Task.Run(async () => {

            var com = MidnightBot.Client.GetService<CommandService>().AllCommands
                .FirstOrDefault(c => c.Text.ToLowerInvariant().Equals(comToFind) ||
                                        c.Aliases.Select(a => a.ToLowerInvariant()).Contains(comToFind));
            if (com != null)
                await e.Channel.SendMessage($"**__Hilfe für `{com.Text}`__ / __`{("" + com.Aliases.FirstOrDefault() + "" ?? "")}`__**\n**Beschreibung:** {com.Description.Replace("|", "\n**Benutzung:**")}").ConfigureAwait(false);
        }).ConfigureAwait(false);
    };
    
        public static string HelpString {
            get {
                var str = !string.IsNullOrWhiteSpace (MidnightBot.Creds.ClientId) && !MidnightBot.Config.DontJoinServers
                    ? String.Format("To add me to your server, use this link -> <https://discordapp.com/oauth2/authorize?client_id={0}&scope=bot&permissions=66186303>\n", MidnightBot.Creds.ClientId)
                    : "";
                return str + String.Format(MidnightBot.Config.HelpString, MidnightBot.Config.CommandPrefixes.Help);
            }
        }

        public static string DMHelpString => MidnightBot.Config.DMHelpString;

        public Action<CommandEventArgs> DoGitFunc () => e =>
        {
            string helpstr =
    $@"######For more information and how to setup your own MidnightBot, go to: **http://github.com/Midnight-Myth/MidnightBot/**
######You can donate on paypal: `nadekodiscordbot@gmail.com` or Bitcoin `17MZz1JAqME39akMLrVT4XBPffQJ2n1EPa`

#MidnightBot List Of Commands  
Version: `{MidnightStats.Instance.BotVersion}`";


            string lastCategory = "";
            foreach (var com in MidnightBot.Client.GetService<CommandService> ().AllCommands)
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
            helpstr = helpstr.Replace (MidnightBot.BotMention,"@BotName");
            helpstr = helpstr.Replace (" |"," | ").Replace ("**Benutzung**:"," | ").Replace ("**Beschreibung:**"," | ").Replace ("\n|"," |  \n");
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
                .Alias (Module.Prefix + "help",MidnightBot.BotMention + " help",MidnightBot.BotMention + " h","~h")
                .Description ("Hilfe-Befehl. | '-h !m q' or just '-h' ")
                .Parameter ("module",ParameterType.Unparsed)
                .Do (HelpFunc ());
            cgb.CreateCommand (Module.Prefix + "hh")
                .Description ("Hilfe-Befehl. | '-hh !m q' or just '-h' ")
                .Parameter ("command",ParameterType.Unparsed)
                .Do (NewHelpFunc ());
            cgb.CreateCommand (Module.Prefix + "hgit")
                .Description ("commandlist.md Datei erstellung. **Bot Owner Only!**")
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (DoGitFunc ());
            cgb.CreateCommand (Module.Prefix + "readme")
                .Alias (Module.Prefix + "guide")
                .Description ("Sendet eine readme und ein Guide verlinkt zum Channel.")
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                     await e.Channel.SendMessage (
 @"**Wiki with all info**: <https://github.com/Midnight-Myth/MidnightBot/wiki>

 **WINDOWS SETUP GUIDE**: <https://github.com/Midnight-Myth/MidnightBot/blob/master/ComprehensiveGuide.md>
  
 **LINUX SETUP GUIDE**: <https://github.com/Midnight-Myth/MidnightBot/blob/master/LinuxSetup.md>

**LIST OF COMMANDS**: <https://github.com/Midnight-Myth/MidnightBot/blob/master/commandlist.md>").ConfigureAwait (false));

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