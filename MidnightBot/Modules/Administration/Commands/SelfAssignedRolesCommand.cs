using Discord.Commands;
using Discord.Net;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Administration.Commands
{
    internal class SelfAssignedRolesCommand : DiscordCommand
    {
        public SelfAssignedRolesCommand ( DiscordModule module ) : base (module)
        {
        }
        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "asar")
                .Description ("Adds a role, or list of roles separated by whitespace" +
                             $"(use quotations for multiword roles) to the list of self-assignable roles. | ´{Prefix}asar Gamer´")
                .Parameter ("roles",ParameterType.Multiple)
                .AddCheck (SimpleCheckers.CanManageRoles)
                .Do (async e =>
                {
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    var msg = new StringBuilder ();
                    foreach (var arg in e.Args)
                    {
                        var role = e.Server.FindRoles (arg.Trim ()).FirstOrDefault ();
                        if (role == null)
                            msg.AppendLine ($":anger:Role **{arg}** not found.");
                        else
                        {
                            if (config.ListOfSelfAssignableRoles.Contains (role.Id))
                            {
                                msg.AppendLine ($":anger:Role **{role.Name}** is already in the list.");
                                continue;
                            }
                            config.ListOfSelfAssignableRoles.Add (role.Id);
                            msg.AppendLine ($":ok:Role **{role.Name}** added to the list.");
                        }
                    }
                    await e.Channel.SendMessage (msg.ToString ()).ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "rsar")
                .Description ($"Removes a specified role from the list of self-assignable roles. | `{Prefix}rsar`")
                .Parameter ("role",ParameterType.Unparsed)
                .AddCheck (SimpleCheckers.CanManageRoles)
                .Do (async e =>
                {
                    var roleName = e.GetArg ("role")?.Trim ();
                    if (string.IsNullOrWhiteSpace (roleName))
                        return;
                    var role = e.Server.FindRoles (roleName).FirstOrDefault ();
                    if (role == null)
                    {
                        await e.Channel.SendMessage (":anger:That role does not exist.").ConfigureAwait (false);
                        return;
                    }
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    if (!config.ListOfSelfAssignableRoles.Contains (role.Id))
                    {
                        await e.Channel.SendMessage (":anger:That role is not self-assignable.").ConfigureAwait (false);
                        return;
                    }
                    config.ListOfSelfAssignableRoles.Remove (role.Id);
                    await e.Channel.SendMessage ($":ok:**{role.Name}** has been removed from the list of self-assignable roles").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "lsar")
                .Description ("$Lists all self-assignable roles. | `{Prefix}lsar`")
                .Parameter ("roles",ParameterType.Multiple)
                .Do (async e =>
                {
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    var msg = new StringBuilder ($"There are `{config.ListOfSelfAssignableRoles.Count}` self assignable roles:\n");
                    var toRemove = new HashSet<ulong> ();
                    foreach (var roleId in config.ListOfSelfAssignableRoles.OrderBy(r => r.ToString()))
                    {
                        var role = e.Server.GetRole (roleId);
                        if (role == null)
                        {
                            msg.Append ($"`{roleId} not found. Cleaned up.`, ");
                            toRemove.Add (roleId);
                        }
                        else
                        {
                            msg.Append ($"**{role.Name}**, ");
                        }
                    }
                    foreach (var id in toRemove)
                    {
                        config.ListOfSelfAssignableRoles.Remove (id);
                    }
                    await e.Channel.SendMessage (msg.ToString ()).ConfigureAwait (false);
                });

            cgb.CreateCommand(Module.Prefix + "togglexclsar").Alias(Module.Prefix + "tesar")
                .Description($"Ändert ob die Self-Assigned Roles exklusiv, oder nicht exklusiv sind. | `{Prefix}tesar`")
                .AddCheck(SimpleCheckers.CanManageRoles)
                .Do(async e =>
                {
                    var config = SpecificConfigurations.Default.Of(e.Server.Id);
                    config.ExclusiveSelfAssignedRoles = !config.ExclusiveSelfAssignedRoles;
                    string exl = config.ExclusiveSelfAssignedRoles ? "exklusiv" : "nichtt exklusiv";
                    await e.Channel.SendMessage("Self Assigned Roles sind jetzt " + exl);
                });

            cgb.CreateCommand (Module.Prefix + "iam")
                .Description ("Adds a role to you that you choose. " +
                             "Role must be on a list of self-assignable roles." +
                             $" | ´{Prefix}iam Gamer´")
                .Parameter ("role",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var roleName = e.GetArg ("role")?.Trim ();
                    if (string.IsNullOrWhiteSpace (roleName))
                        return;
                    var role = e.Server.FindRoles (roleName).FirstOrDefault ();
                    if (role == null)
                    {
                        await e.Channel.SendMessage (":anger:Dieser Rang nicht.").ConfigureAwait (false);
                        return;
                    }
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    if (!config.ListOfSelfAssignableRoles.Contains (role.Id))
                    {
                        await e.Channel.SendMessage (":anger:Dieser Rang ist nicht selbst zuweisebar.").ConfigureAwait (false);
                        return;
                    }
                    if (e.User.HasRole (role))
                    {
                        await e.Channel.SendMessage ($":anger:Du hast bereits den {role.Name} Rang.").ConfigureAwait (false);
                        return;
                    }
                    var sameRoles = e.User.Roles.Where(r => config.ListOfSelfAssignableRoles.Contains(r.Id));
                    if (config.ExclusiveSelfAssignedRoles && sameRoles.Any())
                    {
                        await e.Channel.SendMessage($":anger:Du hast bereits den {sameRoles.FirstOrDefault().Name} Rang.").ConfigureAwait(false);
                        return;
                    }
                    try
                    {
                        await e.User.AddRoles(role).ConfigureAwait(false);
                    }
                    catch (HttpException ex) when (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                    }
                    catch (Exception)
                    {
                        await e.Channel.SendMessage($":anger:`Ich kann dir diese Rolle nicht hinzufügen. Ich kann Ownern, oder Rängen die in der Hierarchy über mir sind, keine Ränge zuweisen.`").ConfigureAwait (false);
                        return;
                    }
                    var msg = await e.Channel.SendMessage ($":ok:Du hast jetzt den {role.Name} Rang.").ConfigureAwait (false);
                    await Task.Delay(3000).ConfigureAwait(false);
                    await msg.Delete().ConfigureAwait(false);
                    try
                    {
                        await e.Message.Delete ().ConfigureAwait (false);
                    }
                    catch { }
                });

            cgb.CreateCommand (Module.Prefix + "iamnot")
                .Alias (Module.Prefix + "iamn")
                .Description ("Removes a role to you that you choose. " +
                              "Role must be on a list of self-assignable roles." +
                              $" | ´{Prefix}iamn Gamer´")
                .Parameter ("role",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var roleName = e.GetArg ("role")?.Trim ();
                    if (string.IsNullOrWhiteSpace (roleName))
                        return;
                    var role = e.Server.FindRoles (roleName).FirstOrDefault ();
                    if (role == null)
                    {
                        await e.Channel.SendMessage (":anger:That role does not exist.").ConfigureAwait (false);
                        return;
                    }
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    if (!config.ListOfSelfAssignableRoles.Contains (role.Id))
                    {
                        await e.Channel.SendMessage (":anger:That role is not self-assignable.").ConfigureAwait (false);
                        return;
                    }
                    if (!e.User.HasRole (role))
                    {
                        await e.Channel.SendMessage ($":anger:You don't have {role.Name} role.").ConfigureAwait (false);
                        return;
                    }
                    await e.User.RemoveRoles (role).ConfigureAwait (false);
                    var msg = await e.Channel.SendMessage ($":ok:Successfuly removed {role.Name} role from you.").ConfigureAwait (false);
                    await Task.Delay(3000).ConfigureAwait(false);
                    await msg.Delete().ConfigureAwait(false);
                    try
                    {
                        await e.Message.Delete().ConfigureAwait(false);
                    }
                    catch { }
                });
        }
    }
}