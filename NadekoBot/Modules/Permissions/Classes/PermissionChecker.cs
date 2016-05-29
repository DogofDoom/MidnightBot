using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using MidnightBot.Classes.JSONModels;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Permissions.Classes
{

    internal class PermissionChecker : IPermissionChecker
    {
        public static PermissionChecker Instance { get; } = new PermissionChecker ();

        private ConcurrentDictionary<User,DateTime> timeBlackList { get; } = new ConcurrentDictionary<User,DateTime> ();

        static PermissionChecker () { }
        private PermissionChecker ()
        {
            Task.Run (async () =>
            {
                while (true)
                {
                    //blacklist is cleared every 1.75 seconds. That is the most time anyone will be blocked
                    await Task.Delay (1750).ConfigureAwait (false);
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

            if (timeBlackList.ContainsKey (user))
                return false;



            timeBlackList.TryAdd (user,DateTime.Now);

            if (!channel.IsPrivate && !channel.Server.CurrentUser.GetPermissions (channel).SendMessages)
            {
                return false;
            }
            //{
            //    user.SendMessage($"I ignored your command in {channel.Server.Name}/#{channel.Name} because i don't have permissions to write to it. Please use `;acm channel_name 0` in that server instead of muting me.").GetAwaiter().GetResult();
            //}

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
                    if (user.Server.Owner.Id == user.Id || (role != null && user.HasRole (role)))
                        return true;
                    ServerPermissions perms;
                    PermissionsHandler.PermissionsDict.TryGetValue (user.Server.Id,out perms);
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
                    ServerPermissions perms;
                    if (PermissionsHandler.PermissionsDict.TryGetValue (user.Server.Id,out perms) && perms.Verbose)
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
    }
}