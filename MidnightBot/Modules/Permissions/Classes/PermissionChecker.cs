using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using MidnightBot.Classes.JSONModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Permissions.Classes
{

    internal class PermissionChecker : IPermissionChecker
    {
        public static PermissionChecker Instance { get; } = new PermissionChecker ();

        //key - sid:command
        //value - userid
        private ConcurrentDictionary<string, ulong> commandCooldowns = new ConcurrentDictionary<string, ulong>();
        private HashSet<ulong> timeBlackList { get; } = new HashSet<ulong>();

        static PermissionChecker () { }
        private PermissionChecker ()
        {
            Task.Run (async () =>
            {
                while (true)
                {
                    //blacklist is cleared every 1.00 second. That is the most time anyone will be blocked
                    await Task.Delay(1000).ConfigureAwait(false);
                    timeBlackList.Clear ();
                }
            });
        }

        public bool CanRun ( Command command,User user,Channel channel,out string error )
        {
            error = String.Empty;

            if (!MidnightBot.Ready)
                return false;

            if (channel.IsPrivate)
                return true;

            if (ConfigHandler.IsUserBlacklisted (user.Id) ||
                (!channel.IsPrivate &&
                 (ConfigHandler.IsServerBlacklisted (channel.Server.Id) || ConfigHandler.IsChannelBlacklisted (channel.Id))))
            {
                return false;
            }

            if (timeBlackList.Contains(user.Id))
                return false;

            if (!channel.IsPrivate && !channel.Server.CurrentUser.GetPermissions(channel).SendMessages)
            {
                return false;
            }

            timeBlackList.Add(user.Id);

            ServerPermissions perms;
            PermissionsHandler.PermissionsDict.TryGetValue(user.Server.Id, out perms);

            AddUserCooldown(user.Server.Id, user.Id, command.Text.ToLower());
            if (!MidnightBot.IsOwner(user.Id) && commandCooldowns.Keys.Contains(user.Server.Id+":"+command.Text.ToLower()))
            {
                if(perms?.Verbose == true)
                    error = $"{user.Mention} Du hast einen Cooldown auf diesem Befehl.";
                return false;
            }

            try
            {
                //is it a permission command?
                // if it is, check if the user has the correct role
                // if yes return true, if no return false
                if (command.Category == "Permissions")
                {
                    Discord.Role role = null;
                    try
                    {
                        role = PermissionHelper.ValidateRole (user.Server,
                        PermissionsHandler.GetServerPermissionsRoleName (user.Server));
                    }
                    catch { }
                    if (user.Server.Owner.Id == user.Id || (role != null && user.HasRole (role)) || MidnightBot.IsOwner (user.Id))
                        return true;
                    throw new Exception ($"Du hast nicht die benötigte Rolle (**{(perms?.PermissionsControllerRole ?? "Admin")}**) um die Berechtigungen zu ändern.");
                }

                var permissionType = PermissionsHandler.GetPermissionBanType (command,user,channel);

                string msg;

                if (permissionType == PermissionsHandler.PermissionBanType.ServerBanModule &&
                command.Category.ToLower () == "nsfw")
                    msg = $"**{command.Category}** Modul wurde gebannt von diesem **server**.\nNSFW Modul ist standardmäßig deaktiviert. Der Server-Owner kann `;sm nsfw enable` eingeben, um es zu aktivieren.";
                else
                    switch (permissionType)
                    {
                        case PermissionsHandler.PermissionBanType.None:
                            return true;
                        case PermissionsHandler.PermissionBanType.ServerBanCommand:
                            msg = $"Befehl **{command.Text}** wurde gebannt von diesem **Server**.";
                            break;
                        case PermissionsHandler.PermissionBanType.ServerBanModule:
                            msg = $"Modul **{command.Category}** wurde gebannt von diesem **Server**.";
                            break;
                        case PermissionsHandler.PermissionBanType.ChannelBanCommand:
                            msg = $"Befehl **{command.Text}** wurde gebannt von diesem **Channel**.";
                            break;
                        case PermissionsHandler.PermissionBanType.ChannelBanModule:
                            msg = $"Modul **{command.Category}** wurde gebannt von diesem **Channel**.";
                            break;
                        case PermissionsHandler.PermissionBanType.RoleBanCommand:
                            msg = $"Du hast nicht die benötigte **Rolle** welche dich zur Benutzung vom **{command.Text}** Befehl berechtigt.";
                            break;
                        case PermissionsHandler.PermissionBanType.RoleBanModule:
                            msg = $"Du hast keine **Rolle** welche dich zur Benutzung vom **{command.Category}** Modul berechtigt.";
                            break;
                        case PermissionsHandler.PermissionBanType.UserBanCommand:
                            msg = $"{user.Mention}, Du wurdest gebannt von der Benutzung des **{command.Text}** Befehls.";
                            break;
                        case PermissionsHandler.PermissionBanType.UserBanModule:
                            msg = $"{user.Mention}, Du wurdest gebannt von der Benutzung des **{command.Category}** Modules.";
                            break;
                        default:
                            return true;
                    }
                if (PermissionsHandler.PermissionsDict[user.Server.Id].Verbose) //if verbose - print errors
                    error = msg;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine ($"Exception in canrun: {ex}");
                try
                {
                    if (perms != null && perms.Verbose)
                        //if verbose - print errors
                        error = ex.Message;
                }
                catch (Exception ex2)
                {
                    Console.WriteLine ($"SERIOUS PERMISSION ERROR {ex2}\n\nUser:{user} Server: {user?.Server?.Name}/{user?.Server?.Id}");
                }
                return false;
            }
        }
        public void AddUserCooldown(ulong serverId, ulong userId, string commandName) {
            commandCooldowns.TryAdd(commandName, userId);
            var tosave = serverId + ":" + commandName;
            Task.Run(async () =>
            {
                ServerPermissions perms;
                PermissionsHandler.PermissionsDict.TryGetValue(serverId, out perms);
                int cd;
                if (!perms.CommandCooldowns.TryGetValue(commandName,out cd)) {
                    return;
                }
                if (commandCooldowns.TryAdd(tosave, userId))
                {
                    await Task.Delay(cd * 1000);
                    ulong throwaway;
                    commandCooldowns.TryRemove(tosave, out throwaway);
                }

            });
        }
    }
}