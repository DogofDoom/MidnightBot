using Discord.Commands;
using Discord.Modules;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Utility.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Utility
{
    internal class UtilityModule : DiscordModule
    {
        public UtilityModule ()
        {
            commands.Add (new Remind (this));
            commands.Add (new InfoCommands (this));
        }

        public override string Prefix => MidnightBot.Config.CommandPrefixes.Utility;

        public override void Install ( ModuleManager manager )
        {

            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                var client = manager.Client;

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "whoplays")
                    .Description ("Zeigt eine Liste von Benutzern die ein gewähltes Spiel spielen.")
                    .Parameter ("game",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var game = e.GetArg ("game")?.Trim ().ToUpperInvariant ();
                        if (string.IsNullOrWhiteSpace (game))
                            return;
                        var en = e.Server.Users
                            .Where (u => u.CurrentGame?.Name?.ToUpperInvariant () == game)
                                .Select (u => u.Name);

                        var arr = en as string[] ?? en.ToArray ();

                        int i = 0;
                        if (arr.Length == 0)
                            await e.Channel.SendMessage ("Niemand. (nicht 100% sicher)");
                        else
                            await e.Channel.SendMessage ("```xl\n" + string.Join ("\n",arr.GroupBy (item => (i++) / 3).Select (ig => string.Join ("",ig.Select (el => $"• {el,-35}")))) + "\n```");
                    });

                cgb.CreateCommand (Prefix + "inrole")
                    .Description ("Listet alle Benutzer von einer angegebenen Rolle, oder Rollen (getrennt mit einem ',') auf diesem Server.")
                    .Parameter ("roles",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.User.ServerPermissions.MentionEveryone)
                                return;
                            var arg = e.GetArg ("roles").Split (',').Select (r => r.Trim ());
                            string send = $"`Hier ist eine Liste alle Benutzer mit einer bestimmten Rolle:`";
                            foreach (var roleStr in arg.Where (str => !string.IsNullOrWhiteSpace (str)))
                            {
                                var role = e.Server.FindRoles (roleStr).FirstOrDefault ();
                                if (role == null)
                                    continue;
                                send += $"\n`{role.Name}`\n";
                                send += string.Join (", ",role.Members.Select (r => "**" + r.Name + "**#" + r.Discriminator));
                            }

                            while (send.Length > 2000)
                            {
                                var curstr = send.Substring (0,2000);
                                await
                                    e.Channel.Send (curstr.Substring (0,
                                        curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1)).ConfigureAwait (false);
                                send = curstr.Substring (curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1) +
                                       send.Substring (2000);
                            }
                            await e.Channel.Send (send).ConfigureAwait (false);
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "checkmyperms")
                    .Description ("Kontrolliere deine Berechtigungen auf diesem Server.")
                    .Do (async e =>
                    {
                        var output = "```\n";
                        foreach (var p in e.User.ServerPermissions.GetType ().GetProperties ().Where (p => !p.GetGetMethod ().GetParameters ().Any ()))
                        {
                            output += p.Name + ": " + p.GetValue (e.User.ServerPermissions,null).ToString () + "\n";
                        }
                        output += "```";
                        await e.User.SendMessage (output).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "stats")
                    .Description ("Zeigt ein paar Statisitken über MidnightBot.")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage (await MidnightStats.Instance.GetStats ());
                    });

                cgb.CreateCommand (Prefix + "dysyd")
                    .Description ("Zeigt ein paar Statisitken über MidnightBot.")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ((await MidnightStats.Instance.GetStats ()).Matrix ().TrimTo (1990)).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "userid").Alias (Prefix + "uid")
                    .Description ("Zeigt die ID eines Benutzers.")
                    .Parameter ("user",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var usr = e.User;
                        if (!string.IsNullOrWhiteSpace (e.GetArg ("user")))
                            usr = e.Channel.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                        if (usr == null)
                            return;
                        await e.Channel.SendMessage ($"Id des Users { usr.Name } ist { usr.Id }").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "channelid").Alias (Prefix + "cid")
                    .Description ("Zeigt ID des derzeitigen Channels")
                    .Do (async e => await e.Channel.SendMessage ("Die ID des derzeitigen Channels ist " + e.Channel.Id).ConfigureAwait (false));

                cgb.CreateCommand (Prefix + "serverid").Alias (Prefix + "sid")
                    .Description ("Zeigt ID des derzeitigen Servers.")
                    .Do (async e => await e.Channel.SendMessage ("Die ID des derzeitigen Servers ist " + e.Server.Id).ConfigureAwait (false));

                cgb.CreateCommand (Prefix + "roles")
                  .Description ("Listet alle Rollen auf diesem Server, oder die eines Benutzers wenn spezifiziert.")
                  .Parameter ("user",ParameterType.Unparsed)
                  .Do (async e =>
                  {
                      if (!string.IsNullOrWhiteSpace (e.GetArg ("user")))
                      {
                          var usr = e.Server.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                          if (usr == null)
                              return;

                          await e.Channel.SendMessage ($"`Liste der Rollen von **{usr.Name}**:` \n• " + string.Join ("\n• ",usr.Roles)).ConfigureAwait (false);
                          return;
                      }
                      await e.Channel.SendMessage ("`Liste der Rollen:` \n• " + string.Join ("\n• ",e.Server.Roles)).ConfigureAwait (false);
                  });
            });
        }
    }
}
