using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;

namespace MidnightBot.Modules.Administration.Commands
{
    class AutoAssignRole : DiscordCommand
    {
        public AutoAssignRole ( DiscordModule module ) : base (module)
        {
            MidnightBot.Client.UserJoined += ( s,e ) =>
            {
                try
                {
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);

                    var role = e.Server.Roles.Where (r => r.Id == config.AutoAssignedRole).FirstOrDefault ();

                    if (role == null)
                        return;

                    e.User.AddRoles (role);
                }
                catch (Exception ex)
                {
                    Console.WriteLine ($"aar exception. {ex}");
                }
            };
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "autoassignrole")
                .Alias (Module.Prefix + "aar")
                .Description ($"Fügt automatisch jedem Benutzer der dem Server joint eine Rolle zu. Gib `{Prefix}aar` ein um zu deaktivieren, `{Prefix}aar Rollen Name` um zu aktivieren.")
                .Parameter ("role",ParameterType.Unparsed)
                .AddCheck (new SimpleCheckers.ManageRoles ())
                .Do (async e =>
                {
                    if (!e.Server.CurrentUser.ServerPermissions.ManageRoles)
                    {
                        await e.Channel.SendMessage ("Ich habe keine Berechtigungen um Rollen zu bearbeiten.").ConfigureAwait (false);
                        return;
                    }
                    var r = e.GetArg ("role")?.Trim ();

                    var config = SpecificConfigurations.Default.Of (e.Server.Id);

                    if (string.IsNullOrWhiteSpace (r)) //if role is not specified, disable
                    {
                        config.AutoAssignedRole = 0;

                        await e.Channel.SendMessage ("`Auto Assign Role beim betreten des Servers ist jetzt deaktiviert.`").ConfigureAwait (false);
                        return;
                    }
                    var role = e.Server.FindRoles (r).FirstOrDefault ();

                    if (role == null)
                    {
                        await e.Channel.SendMessage ("💢 `Rolle nicht gefunden.`").ConfigureAwait (false);
                        return;
                    }

                    config.AutoAssignedRole = role.Id;
                    await e.Channel.SendMessage ("`Auto Assigned Role ist gesetzt.`").ConfigureAwait (false);

                });
        }
    }
}