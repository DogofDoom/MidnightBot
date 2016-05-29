using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes.JSONModels;
using MidnightBot.Extensions;
using MidnightBot.Modules.Games.Commands;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Permissions.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Permissions
{
    internal class PermissionModule : DiscordModule
    {
        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Permissions;

        public PermissionModule ()
        {
            commands.Add (new FilterInvitesCommand (this));
            commands.Add (new FilterWords (this));
        }

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "permrole")
                    .Alias (Prefix + "pr")
                    .Description ("Setzt eine Rolle, welche die Berechtigungen bearbeiten kann. Ohne Angabe wird die derzeitige Rolle gezeigt. Standard 'Pony'.")
                    .Parameter ("role",ParameterType.Unparsed)
                     .Do (async e =>
                     {
                         if (string.IsNullOrWhiteSpace (e.GetArg ("role")))
                         {
                             await e.Channel.SendMessage ($"Derzeitige Berechtigungs-Rolle ist `{PermissionsHandler.GetServerPermissionsRoleName (e.Server)}`").ConfigureAwait (false);
                             return;
                         }

                         var arg = e.GetArg ("role");
                         Discord.Role role = null;
                         try
                         {
                             role = PermissionHelper.ValidateRole (e.Server,arg);
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine (ex.Message);
                             await e.Channel.SendMessage ($"Rolle `{arg}` existiert nicht. Erstelle zuerst eine Rolle mit diesem Namen.").ConfigureAwait (false);
                             return;
                         }
                         PermissionsHandler.SetPermissionsRole (e.Server,role.Name);
                         await e.Channel.SendMessage ($"Rolle `{role.Name}` ist nun benötigt um die Berechtigungen zu bearbeiten.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "rpc")
                    .Alias (Prefix + "rolepermissionscopy")
                    .Description ($"Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einer Rolle zu einer anderen.\n**Benutzung**:`{Prefix}rpc Some Role ~ Some other role`")
                    .Parameter ("from_to",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("from_to")?.Trim ();
                        if (string.IsNullOrWhiteSpace (arg) || !arg.Contains ('~'))
                            return;
                        var args = arg.Split ('~').Select (a => a.Trim ()).ToArray ();
                        if (args.Length > 2)
                        {
                            await e.Channel.SendMessage ("💢Ungültige Anzahl von '~'s in den Argumenten.");
                            return;
                        }
                        try
                        {
                            var fromRole = PermissionHelper.ValidateRole (e.Server,args[0]);
                            var toRole = PermissionHelper.ValidateRole (e.Server,args[1]);

                            PermissionsHandler.CopyRolePermissions (fromRole,toRole);
                            await e.Channel.SendMessage ($"CBerechtigungseinstellungen von **{fromRole.Name}** zu **{toRole.Name}** kopiert.");
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢{ex.Message}");
                        }
                    });
                cgb.CreateCommand (Prefix + "cpc")
                    .Alias (Prefix + "channelpermissionscopy")
                    .Description ($"Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Channel zu einem anderen.\n**Benutzung**:`{Prefix}cpc Some Channel ~ Some other channel`")
                    .Parameter ("from_to",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("from_to")?.Trim ();
                        if (string.IsNullOrWhiteSpace (arg) || !arg.Contains ('~'))
                            return;
                        var args = arg.Split ('~').Select (a => a.Trim ()).ToArray ();
                        if (args.Length > 2)
                        {
                            await e.Channel.SendMessage ("💢Ungültige Anzahl von '~'s in den Argumenten.");
                            return;
                        }
                        try
                        {
                            var fromChannel = PermissionHelper.ValidateChannel (e.Server,args[0]);
                            var toChannel = PermissionHelper.ValidateChannel (e.Server,args[1]);

                            PermissionsHandler.CopyChannelPermissions (fromChannel,toChannel);
                            await e.Channel.SendMessage ($"Berechtigungseinstellungen von **{fromChannel.Name}** zu **{toChannel.Name}** kopiert.");
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢{ex.Message}");
                        }
                    });
                cgb.CreateCommand (Prefix + "upc")
                    .Alias (Prefix + "userpermissionscopy")
                    .Description ($"Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Benutzer, zu einem anderen.\n**Benutzung**:`{Prefix}upc @SomeUser ~ @SomeOtherUser`")
                    .Parameter ("from_to",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("from_to")?.Trim ();
                        if (string.IsNullOrWhiteSpace (arg) || !arg.Contains ('~'))
                            return;
                        var args = arg.Split ('~').Select (a => a.Trim ()).ToArray ();
                        if (args.Length > 2)
                        {
                            await e.Channel.SendMessage ("💢Ungültige Anzahl von '~'s in den Argumenten.");
                            return;
                        }
                        try
                        {
                            var fromUser = PermissionHelper.ValidateUser (e.Server,args[0]);
                            var toUser = PermissionHelper.ValidateUser (e.Server,args[1]);

                            PermissionsHandler.CopyUserPermissions (fromUser,toUser);
                            await e.Channel.SendMessage ($"Berechtigungseinstellungen von **{fromUser.ToString ()}** zu * *{toUser.ToString ()}** kopiert.");
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢{ex.Message}");
                        }
                    });

                cgb.CreateCommand (Prefix + "verbose")
                    .Alias (Prefix + "v")
                    .Description ("Ändert ob das blocken/entblocken eines Modules/Befehls angezeigt wird.\n**Benutzung**: ;verbose true")
                    .Parameter ("arg",ParameterType.Required)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("arg");
                        var val = PermissionHelper.ValidateBool (arg);
                        PermissionsHandler.SetVerbosity (e.Server,val);
                        await e.Channel.SendMessage ($"Verbosity wurde gesetzt auf {val}.").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "serverperms")
                    .Alias (Prefix + "sp")
                    .Description ("Zeigt gebannte Berechtigungen für diesen Server.")
                    .Do (async e =>
                    {
                        var perms = PermissionsHandler.GetServerPermissions (e.Server);
                        if (string.IsNullOrWhiteSpace (perms?.ToString ()))
                            await e.Channel.SendMessage ("Keine Berechtigungen für diesen Server vorhanden.").ConfigureAwait (false);
                        await e.Channel.SendMessage (perms.ToString ()).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "roleperms")
                    .Alias (Prefix + "rp")
                    .Description ("Zeigt gebannt Berechtigungen für eine bestimmte Rolle. Kein Argument bedeutet für alle.\n**Benutzung**: ;rp AwesomeRole")
                    .Parameter ("role",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("role");
                        var role = e.Server.EveryoneRole;
                        if (!string.IsNullOrWhiteSpace (arg))
                            try
                            {
                                role = PermissionHelper.ValidateRole (e.Server,arg);
                            }
                            catch (Exception ex)
                            {
                                await e.Channel.SendMessage ("💢 Fehler: " + ex.Message).ConfigureAwait (false);
                                return;
                            }

                        var perms = PermissionsHandler.GetRolePermissionsById (e.Server,role.Id);

                        if (string.IsNullOrWhiteSpace (perms?.ToString ()))
                            await e.Channel.SendMessage ($"Keine Berechtigungen gesetzt für **{role.Name}** Rolle.").ConfigureAwait (false);
                        await e.Channel.SendMessage (perms.ToString ()).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "channelperms")
                    .Alias (Prefix + "cp")
                    .Description ("Zeigt gebannte Berechtigungen für einen bestimmten Channel. Kein Argument für derzeitigen Channel.\n**Benutzung**: ;cp #dev")
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg = e.GetArg ("channel");
                        var channel = e.Channel;
                        if (!string.IsNullOrWhiteSpace (arg))
                            try
                            {
                                channel = PermissionHelper.ValidateChannel (e.Server,arg);
                            }
                            catch (Exception ex)
                            {
                                await e.Channel.SendMessage ("💢 Fehler: " + ex.Message).ConfigureAwait (false);
                                return;
                            }

                        var perms = PermissionsHandler.GetChannelPermissionsById (e.Server,channel.Id);
                        if (string.IsNullOrWhiteSpace (perms?.ToString ()))
                            await e.Channel.SendMessage ($"Keine Berechtigungen gesetzt für **{channel.Name}** Channel.").ConfigureAwait (false);
                        await e.Channel.SendMessage (perms.ToString ()).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "userperms")
                    .Alias (Prefix + "up")
                    .Description ("Zeigt gebannte Berechtigungen für einen bestimmten Benutzer. Keine Argumente für sich selber.\n**Benutzung**: ;up Kwoth")
                    .Parameter ("user",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var user = e.User;
                        if (!string.IsNullOrWhiteSpace (e.GetArg ("user")))
                            try
                            {
                                user = PermissionHelper.ValidateUser (e.Server,e.GetArg ("user"));
                            }
                            catch (Exception ex)
                            {
                                await e.Channel.SendMessage ("💢 Fehler: " + ex.Message).ConfigureAwait (false);
                                return;
                            }

                        var perms = PermissionsHandler.GetUserPermissionsById (e.Server,user.Id);
                        if (string.IsNullOrWhiteSpace (perms?.ToString ()))
                            await e.Channel.SendMessage ($"Keine Berechtigungen gesetzt für Benutzer **{user.Name}**.").ConfigureAwait (false);
                        await e.Channel.SendMessage (perms.ToString ()).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "sm").Alias (Prefix + "servermodule")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Description ("Setzt die Berechtigung eines Moduls auf Serverlevel.\n**Benutzung**: ;sm [module_name] enable")
                    .Do (async e =>
                    {
                        try
                        {
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            PermissionsHandler.SetServerModulePermission (e.Server,module,state);
                            await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktiviert")}** auf diesem Server.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "sc").Alias (Prefix + "servercommand")
                    .Parameter ("command",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Description ("Setzt die Berechtigung eines Befehls auf Serverlevel.\n**Benutzung**: ;sc [command_name] disable")
                    .Do (async e =>
                    {
                        try
                        {
                            var command = PermissionHelper.ValidateCommand (e.GetArg ("command"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            PermissionsHandler.SetServerCommandPermission (e.Server,command,state);
                            await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktiviert" : "deaktiviert")}** auf diesem Server.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schied gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "rm").Alias (Prefix + "rolemodule")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("role",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Moduls auf Rollenlevel.\n**Benutzung**: ;rm [module_name] enable [role_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            if (e.GetArg ("role")?.ToLower () == "all")
                            {
                                foreach (var role in e.Server.Roles)
                                {
                                    PermissionsHandler.SetRoleModulePermission (role,module,state);
                                }
                                await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktivert")}** für **ALLE** Rollen.").ConfigureAwait (false);
                            }
                            else
                            {
                                var role = PermissionHelper.ValidateRole (e.Server,e.GetArg ("role"));

                                PermissionsHandler.SetRoleModulePermission (role,module,state);
                                await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktivert")}** für die **{role.Name}** Rolle.").ConfigureAwait (false);
                            }
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "rc").Alias (Prefix + "rolecommand")
                    .Parameter ("command",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("role",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Befehls auf Rollenlevel.\n**Benutzung**: ;rc [command_name] disable [role_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var command = PermissionHelper.ValidateCommand (e.GetArg ("command"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            if (e.GetArg ("role")?.ToLower () == "all")
                            {
                                foreach (var role in e.Server.Roles)
                                {
                                    PermissionsHandler.SetRoleCommandPermission (role,command,state);
                                }
                                await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktivert" : "deaktivert")}** für **ALLE** Rollen.").ConfigureAwait (false);
                            }
                            else
                            {
                                var role = PermissionHelper.ValidateRole (e.Server,e.GetArg ("role"));

                                PermissionsHandler.SetRoleCommandPermission (role,command,state);
                                await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktivert" : "deaktiviert")}** für die **{role.Name}** Rolle.").ConfigureAwait (false);
                            }
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "cm").Alias (Prefix + "channelmodule")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Moduls auf Channellevel.\n**Benutzung**: ;cm [module_name] enable [channel_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            var channelArg = e.GetArg ("channel");
                            if (channelArg?.ToLower () == "all")
                            {
                                foreach (var channel in e.Server.TextChannels)
                                {
                                    PermissionsHandler.SetChannelModulePermission (channel,module,state);
                                }
                                await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktiviert")}** auf **ALLEN** Channels.").ConfigureAwait (false);
                            }
                            else if (string.IsNullOrWhiteSpace (channelArg))
                            {
                                PermissionsHandler.SetChannelModulePermission (e.Channel,module,state);
                                await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktiviert")}** im **{e.Channel.Name}** Channel.").ConfigureAwait (false);
                            }
                            else
                            {
                                var channel = PermissionHelper.ValidateChannel (e.Server,channelArg);

                                PermissionsHandler.SetChannelModulePermission (channel,module,state);
                                await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktiviert")}** für den **{channel.Name}** Channel.").ConfigureAwait (false);
                            }
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "cc").Alias (Prefix + "channelcommand")
                    .Parameter ("command",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Befehls auf Channellevel.\n**Benutzung**: ;cc [command_name] enable [channel_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var command = PermissionHelper.ValidateCommand (e.GetArg ("command"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            if (e.GetArg ("channel")?.ToLower () == "all")
                            {
                                foreach (var channel in e.Server.TextChannels)
                                {
                                    PermissionsHandler.SetChannelCommandPermission (channel,command,state);
                                }
                                await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktiviert" : "deaktivert")}** auf **ALLEN** Channeln.").ConfigureAwait (false);
                            }
                            else
                            {
                                var channel = PermissionHelper.ValidateChannel (e.Server,e.GetArg ("channel"));

                                PermissionsHandler.SetChannelCommandPermission (channel,command,state);
                                await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktiviert" : "deaktiviert")}** für den **{channel.Name}** Channel.").ConfigureAwait (false);
                            }
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "um").Alias (Prefix + "usermodule")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("user",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Moduls auf Benutzerlevel.\n**Benutzung**: ;um [module_name] enable [user_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var user = PermissionHelper.ValidateUser (e.Server,e.GetArg ("user"));

                            PermissionsHandler.SetUserModulePermission (user,module,state);
                            await e.Channel.SendMessage ($"Modul **{module}** wurde **{(state ? "aktiviert" : "deaktiviert")}** für Benutzer **{user.Name}**.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "uc").Alias (Prefix + "usercommand")
                    .Parameter ("command",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("user",ParameterType.Unparsed)
                    .Description ("Setzt die Berechtigung eines Befehls auf Benutzerlevel.\n**Benutzung**: ;uc [command_name] enable [user_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var command = PermissionHelper.ValidateCommand (e.GetArg ("command"));
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var user = PermissionHelper.ValidateUser (e.Server,e.GetArg ("user"));

                            PermissionsHandler.SetUserCommandPermission (user,command,state);
                            await e.Channel.SendMessage ($"Befehl **{command}** wurde **{(state ? "aktiviert" : "deaktiviert")}** für Benutzer **{user.Name}**.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "asm").Alias (Prefix + "allservermodules")
                    .Parameter ("bool",ParameterType.Required)
                    .Description ("Setzt die Berechtigung aller Module auf Serverlevel.\n**Benutzung**: ;asm [enable/disable]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));

                            foreach (var module in MidnightBot.Client.GetService<ModuleService> ().Modules)
                            {
                                PermissionsHandler.SetServerModulePermission (e.Server,module.Name,state);
                            }
                            await e.Channel.SendMessage ($"Alle Module wurden **{(state ? "aktivert" : "deaktivert")}** auf diesem Server.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "asc").Alias (Prefix + "allservercommands")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Description ("Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Serverlevel.\n**Benutzung**: ;asc [module_name] [enable/disable]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));

                            foreach (var command in MidnightBot.Client.GetService<CommandService> ().AllCommands.Where (c => c.Category == module))
                            {
                                PermissionsHandler.SetServerCommandPermission (e.Server,command.Text,state);
                            }
                            await e.Channel.SendMessage ($"Alle Befehle des **{module}** Moduls wurden **{(state ? "aktiviert" : "deaktiviert")}** auf diesem Server.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "acm").Alias (Prefix + "allchannelmodules")
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Description ("Setzt Berechtigungen für alle Module auf Channellevel.\n**Benutzung**: ;acm [enable/disable] [channel_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var chArg = e.GetArg ("channel");
                            var channel = string.IsNullOrWhiteSpace (chArg) ? e.Channel : PermissionHelper.ValidateChannel (e.Server,chArg);
                            foreach (var module in MidnightBot.Client.GetService<ModuleService> ().Modules)
                            {
                                PermissionsHandler.SetChannelModulePermission (channel,module.Name,state);
                            }

                            await e.Channel.SendMessage ($"Alle Module wurden **{(state ? "aktivert" : "deaktiviert")}** im **{channel.Name}** Channel.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "acc").Alias (Prefix + "allchannelcommands")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Description ("Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Channellevel.\n**Benutzung**: ;acc [module_name] [enable/disable] [channel_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var channel = PermissionHelper.ValidateChannel (e.Server,e.GetArg ("channel"));
                            foreach (var command in MidnightBot.Client.GetService<CommandService> ().AllCommands.Where (c => c.Category == module))
                            {
                                PermissionsHandler.SetChannelCommandPermission (channel,command.Text,state);
                            }
                            await e.Channel.SendMessage ($"Alle Befehle des **{module}** Moduls wurden **{(state ? "aktiviert" : "deaktiviert")}** im **{channel.Name}** Channel.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "arm").Alias (Prefix + "allrolemodules")
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("role",ParameterType.Unparsed)
                    .Description ("Setzt Berechtigung von allen Modulen auf Rollenlevel.\n**Benutzung**: ;arm [enable/disable] [role_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var role = PermissionHelper.ValidateRole (e.Server,e.GetArg ("role"));
                            foreach (var module in MidnightBot.Client.GetService<ModuleService> ().Modules)
                            {
                                PermissionsHandler.SetRoleModulePermission (role,module.Name,state);
                            }

                            await e.Channel.SendMessage ($"Alle Module wurden **{(state ? "aktiviert" : "deaktiviert")}** für die **{role.Name}** Rolle.").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "arc").Alias (Prefix + "allrolecommands")
                    .Parameter ("module",ParameterType.Required)
                    .Parameter ("bool",ParameterType.Required)
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Description ("Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Rollenlevel.\n**Benutzung**: ;arc [module_name] [enable/disable] [role_name]")
                    .Do (async e =>
                    {
                        try
                        {
                            var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                            var module = PermissionHelper.ValidateModule (e.GetArg ("module"));
                            var role = PermissionHelper.ValidateRole (e.Server,e.GetArg ("channel"));
                            foreach (var command in MidnightBot.Client.GetService<CommandService> ().AllCommands.Where (c => c.Category == module))
                            {
                                PermissionsHandler.SetRoleCommandPermission (role,command.Text,state);
                            }
                            await e.Channel.SendMessage ($"Alle Befehle des **{module}** Moduls wurden **{(state ? "aktiviert" : "deaktiviert")}** für die **{role.Name}** Rolle..").ConfigureAwait (false);
                        }
                        catch (ArgumentException exArg)
                        {
                            await e.Channel.SendMessage (exArg.Message).ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ("Etwas ist schief gelaufen - " + ex.Message).ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "ubl")
                    .Description ("Blacklists einen Benutzer.\n**Benutzung**: ;ubl [user_mention]")
                    .Parameter ("user",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.Message.MentionedUsers.Any ())
                                return;
                            var usr = e.Message.MentionedUsers.First ();
                            MidnightBot.Config.UserBlacklist.Add (usr.Id);
                            ConfigHandler.SaveConfig ();
                            await e.Channel.SendMessage ($"`Benutzer {usr.Name}` erfolgreich geblacklisted.").ConfigureAwait (false);
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "uubl")
                   .Description ($"Unblacklisted einen erwähnten Benutzer.\n**Benutzung**: {Prefix}uubl [user_mention]")
                   .Parameter ("user",ParameterType.Unparsed)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       await Task.Run (async () =>
                       {
                           if (!e.Message.MentionedUsers.Any ())
                               return;
                           var usr = e.Message.MentionedUsers.First ();
                           if (MidnightBot.Config.UserBlacklist.Contains (usr.Id))
                           {
                               MidnightBot.Config.UserBlacklist.Remove (usr.Id);
                               ConfigHandler.SaveConfig ();
                               await e.Channel.SendMessage ($"`Benutzer {usr.Name} erfolgreich aus der Blacklist entfernt`").ConfigureAwait (false);
                           }
                           else
                           {
                               await e.Channel.SendMessage ($"`{usr.Name} war nicht auf der Blacklist`").ConfigureAwait (false);
                           }
                       }).ConfigureAwait (false);
                   });



                cgb.CreateCommand (Prefix + "cbl")
                    .Description ("Blacklists einen erwähnten Channel (#general zum Beispiel).\n**Benutzung**: ;ubl [channel_mention]")
                    .Parameter ("channel",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.Message.MentionedChannels.Any ())
                                return;
                            var ch = e.Message.MentionedChannels.First ();
                            MidnightBot.Config.UserBlacklist.Add (ch.Id);
                            ConfigHandler.SaveConfig ();
                            await e.Channel.SendMessage ($"`Channel {ch.Name}` erfolgreich geblacklisted.").ConfigureAwait (false);
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "cubl")
                    .Description ("Unblacklists einen erwähnten Channel (#general zum Beispiel).\n**Benutzung**: ;cubl [channel_mention]")
                    .Parameter ("channel",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.Message.MentionedChannels.Any ())
                                return;
                            var ch = e.Message.MentionedChannels.First ();
                            MidnightBot.Config.UserBlacklist.Remove (ch.Id);
                            ConfigHandler.SaveConfig ();
                            await e.Channel.SendMessage ($"`Channel {ch.Name} erfolgreich von der Blacklist entfernt.`").ConfigureAwait (false);
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "sbl")
                    .Description ("Blacklists einen Server per Name, oder ID (#general zum Beispiel).\n**Benutzung**: ;usl [servername/serverid]")
                    .Parameter ("server",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            var arg = e.GetArg ("server")?.Trim ();
                            if (string.IsNullOrWhiteSpace (arg))
                                return;
                            var server = MidnightBot.Client.Servers.FirstOrDefault (s => s.Id.ToString () == arg) ??
                            MidnightBot.Client.FindServers (arg.Trim ()).FirstOrDefault ();
                            if (server == null)
                            {
                                await e.Channel.SendMessage ("Kann den Server nicht finden.").ConfigureAwait (false);
                                return;
                            }
                            var serverId = server.Id;
                            MidnightBot.Config.ServerBlacklist.Add (serverId);
                            ConfigHandler.SaveConfig ();
                            //cleanup trivias and typeracing
                            Modules.Games.Commands.Trivia.TriviaGame trivia;
                            TriviaCommands.RunningTrivias.TryRemove (serverId,out trivia);
                            TypingGame typeracer;
                            SpeedTyping.RunningContests.TryRemove (serverId,out typeracer);

                            await e.Channel.SendMessage ($"`Server {server.Name}` erfolgreich geblacklisted.").ConfigureAwait (false);
                            
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "subl")
                    .Description ("Unblacklists einen erwähnten Server (#general zum Beispiel).\n**Benutzung**: ;cubl [channel_mention]")
                    .Parameter ("server",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            var arg = e.GetArg ("server")?.Trim ();
                            if (string.IsNullOrWhiteSpace (arg))
                                return;
                            ulong serverId;
                            if (!ulong.TryParse (arg,out serverId))
                            {
                                await e.Channel.SendMessage ("Id ungültig.").ConfigureAwait (false);
                                return;
                            }
                            var server = MidnightBot.Client.Servers.FirstOrDefault (s => s.Id.ToString () == arg) ??
                            MidnightBot.Client.FindServers (arg.Trim ()).FirstOrDefault ();
                            MidnightBot.Config.ServerBlacklist.Remove (serverId);
                            ConfigHandler.SaveConfig ();
                            if (server == null)
                            {
                                await e.Channel.SendMessage ("Server nicht gefunden!").ConfigureAwait (false);
                            }
                            else
                            {
                                await e.Channel.SendMessage ($"`Server {server.Name}` erfolgreich von Blacklist entfernt.").ConfigureAwait (false);
                            }
                        }).ConfigureAwait (false);
                    });
            });
        }
    }
}