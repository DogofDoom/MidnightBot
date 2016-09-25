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
                var user = args.User;
                var channel = args.Channel;
                var server = args.Server;
                var message = args.Message;

                if (user == null || channel == null || server == null || message == null)
                    return;

                if (channel.IsPrivate || user.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                try
                {
                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering (channel,out serverPerms))
                        return;

                    

                    if (filterRegex.IsMatch (message.RawText))
                    {
                        await args.Message.Delete ().ConfigureAwait (false);
                        IncidentsHandler.Add (server.Id,channel.Id,$"Benutzer [{user.Name}/{user.Id}] hat einen " +
                                                             $"EINLADUNGS LINK im [{channel.Name}/{channel.Id}] Channel gepostet.\n" +
                                                             $"`Ganze Nachricht:` {args.Message.Text}");
                        if (serverPerms.Verbose)
                            await channel.SendMessage ($"{user.Mention} Invite Links sind " +
                                                           $"in diesem Channel nicht erlaubt.")
                                                           .ConfigureAwait (false);
                    }
                }
                catch { }
            };

            MidnightBot.Client.MessageUpdated += async (sender, args) =>
            {
                var user = args.User;
                var channel = args.Channel;
                var server = args.Server;
                var after = args.After;

                if (user == null || channel == null || server == null || after == null)
                    return;

                if (channel.IsPrivate || user.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                try
                {
                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering(channel, out serverPerms))
                        return;

                    if (filterRegex.IsMatch(after.RawText))
                    {
                        await args.After.Delete().ConfigureAwait(false);
                        IncidentsHandler.Add(server.Id, channel.Id, $"Benutzer [{user.Name}/{user.Id}] hat einen " +
                                                             $"EINLADUNGS LINK im [{channel.Name}/{channel.Id}] Channel gepostet.\n" +
                                                             $"`Ganze Nachricht:` {after.Text}");
                        if (serverPerms.Verbose)
                            await channel.SendMessage($"{user.Mention} Invite Links sind " +
                                                           $"in diesem Channel nicht erlaubt.")
                                                           .ConfigureAwait(false);
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
            cgb.CreateCommand (Module.Prefix + "chnlfilterinv")
                .Alias (Module.Prefix + "cfi")
                .Description ("Aktiviert, oder deaktiviert automatische Löschung von Einladungen in diesem Channel." +
                             "Falls kein Channel gewählt, derzeitige Channel. Benutze ALL um alle derzeitig existierenden Channel zu wählen." +
                             $" | `{Prefix}cfi enable #general-chat`")
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
                            await PermissionsHandler.SetChannelFilterInvitesPermission(chan, state).ConfigureAwait(false);
                            await e.Channel.SendMessage ($"Invite Filter wurde **{(state ? "aktiviert" : "deaktiviert")}** für Channel **{chan.Name}**.")
                            .ConfigureAwait (false);
                            return;
                        }
                        //all channels

                        foreach (var curChannel in e.Server.TextChannels)
                        {
                            await PermissionsHandler.SetChannelFilterInvitesPermission(curChannel, state).ConfigureAwait(false);
                        }
                        await e.Channel.SendMessage ($"Invite Filter wurde **{(state ? "aktiviert" : "deaktiviert")}** für **ALL** Channel.")
                        .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage ($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                    }
                });

            cgb.CreateCommand (Module.Prefix + "srvrfilterinv")
                .Alias (Module.Prefix + "sfi")
                .Description ($"Aktiviert, oder deaktiviert automatische Löschung von Einladungenauf diesem Server. | `{Prefix}sfi disable`")
                .Parameter ("bool")
                .Do (async e =>
                {
                    try
                    {
                        var state = PermissionHelper.ValidateBool (e.GetArg ("bool"));
                        await PermissionsHandler.SetServerFilterInvitesPermission(e.Server, state).ConfigureAwait(false);
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