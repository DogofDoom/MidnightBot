using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Text.RegularExpressions;

namespace MidnightBot.Modules.Permissions.Commands
{
    internal class FilterInvitesCommand : DiscordCommand
    {
        private readonly Regex filterRegex = new Regex (@"(?:discord(?:\.gg|app\.com\/invite)\/(?<id>([\w]{16}|(?:[\w]+-?){3})))");


        public FilterInvitesCommand ( DiscordModule module ) : base (module)
        {
            MidnightBot.Client.MessageReceived += async ( sender,args ) =>
            {
                if (args.Channel.IsPrivate || args.User.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                try
                {
                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering (args.Channel,out serverPerms))
                        return;

                    if (filterRegex.IsMatch (args.Message.RawText))
                    {
                        await args.Message.Delete ().ConfigureAwait (false);
                        IncidentsHandler.Add (args.Server.Id,$"Benutzer [{args.User.Name}/{args.User.Id}] hat einen " +
                                                             $"EINLADUNGS LINK im [{args.Channel.Name}/{args.Channel.Id}] Channel gepostet. " +
                                                             $"Ganze Nachricht: [[{args.Message.Text}]]");
                        if (serverPerms.Verbose)
                            await args.Channel.SendMessage ($"{args.User.Mention} Invite Links sind " +
                                                           $"in diesem Channel nicht erlaubt.")
                                                           .ConfigureAwait (false);
                    }
                }
                catch { }
            };
        }

        private static bool IsChannelOrServerFiltering ( Channel channel,out Classes.ServerPermissions serverPerms )
        {
            if (!PermissionsHandler.PermissionsDict.TryGetValue (channel.Server.Id,out serverPerms))
                return false;

            if (serverPerms.Permissions.FilterInvites)
                return true;

            Classes.Permissions perms;
            return serverPerms.ChannelPermissions.TryGetValue (channel.Id,out perms) && perms.FilterInvites;
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "cfi")
                .Alias (Module.Prefix + "channelfilterinvites")
                .Description ("Aktiviert, oder deaktiviert automatische Löschung von Einladungen in diesem Channel." +
                             "Falls kein Channel gewählt, derzeitige Channel. Benutze ALL um alle derzeitig existierenden Channel zu wählen." +
                             "\n**Benutzung**: ;cfi enable #general-chat")
                .Parameter ("bool")
                .Parameter ("channel",ParameterType.Optional)
                .Do (async e =>
                {
                    try
                    {
                        var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                        var chanStr = e.GetArg ("channel");

                        if (chanStr?.ToLowerInvariant ().Trim () != "all")
                        {

                            var chan = string.IsNullOrWhiteSpace (chanStr)
                                ? e.Channel
                                : PermissionHelper.ValidateChannel (e.Server,chanStr);
                            PermissionsHandler.SetChannelFilterInvitesPermission (chan,state);
                            await e.Channel.SendMessage ($"Invite Filter wurde **{(state ? "aktiviert" : "deaktiviert")}** für Channel **{chan.Name}**.")
                            .ConfigureAwait (false);
                            return;
                        }
                        //all channels

                        foreach (var curChannel in e.Server.TextChannels)
                        {
                            PermissionsHandler.SetChannelFilterInvitesPermission (curChannel,state);
                        }
                        await e.Channel.SendMessage ($"Invite Filter wurde **{(state ? "aktiviert" : "deaktiviert")}** für **ALL** Channel.")
                        .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage ($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                    }
                });

            cgb.CreateCommand (Module.Prefix + "sfi")
                .Alias (Module.Prefix + "serverfilterinvites")
                .Description ("Aktiviert, oder deaktiviert automatische Löschung von Einladungenauf diesem Server.\n**Benutzung**: ;sfi disable")
                .Parameter ("bool")
                .Do (async e =>
                {
                    try
                    {
                        var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                        PermissionsHandler.SetServerFilterInvitesPermission (e.Server,state);
                        await e.Channel.SendMessage ($"Invite Filter wurde **{(state ? "aktiviert" : "deaktiviert")}** für diesen Server.")
                        .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage ($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                    }
                });
        }
    }
}