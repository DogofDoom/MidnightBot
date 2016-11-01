using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;
using System.Text;

namespace MidnightBot.Modules.Permissions.Commands
{
    internal class FilterWords : DiscordCommand
    {
        public FilterWords(DiscordModule module) : base(module)
        {
            MidnightBot.Client.MessageReceived += async (sender, args) =>
            {
                var noFilter = false;
                var user = args.User;
                var channel = args.Channel;
                var server = args.Server;
                var message = args.Message;

                if (user == null || channel == null || server == null || message == null)
                    return;

                if (channel.IsPrivate || user.Id == MidnightBot.Client.CurrentUser.Id) return;
                try
                {
                    var userRoles = args.User.Roles;
                    foreach (Role role in userRoles)
                    {
                        if (MidnightBot.Config.NoFilterRoles.Contains(role.Id))
                        {
                            noFilter = true;
                        }
                    }

                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering(channel, out serverPerms) || user.ServerPermissions.ManageMessages || noFilter == true) return;

                    var wordsInMessage = message.RawText.ToLowerInvariant().Split(' ');
                    if (serverPerms.Words.Any(w => wordsInMessage.Contains(w)))
                    {
                        string bufferString = "";
                        var sb = new StringBuilder();
                        foreach (var w in wordsInMessage)
                        {
                            foreach (var word in serverPerms.Words)
                            {
                                if (w == word)
                                {
                                    foreach (char c in w)
                                    {
                                        bufferString += "*";
                                    }
                                    sb.Append(bufferString + " ");
                                }
                                else
                                {
                                    sb.Append(w + " ");
                                }
                            }
                        }

                        await args.Message.Delete().ConfigureAwait(false);
                        IncidentsHandler.Add(server.Id, channel.Id, $"Benutzer [{user.Name}/{user.Id}] schrieb ein " +
                                                             $"gebanntes Wort im Channel [{channel.Name}/{channel.Id}].\n" +
                                                             $"`Ganze Nachricht:` {message.Text}");
                        if (serverPerms.Verbose)
                            await channel.SendMessage($"**{user.Nickname}**: ```{sb.ToString()}```")
                                                           .ConfigureAwait(false);
                    }
                }
                catch { }
            };

            MidnightBot.Client.MessageUpdated += async (sender, args) =>
            {
                var noFilter = false;
                var user = args.User;
                var channel = args.Channel;
                var server = args.Server;
                var after = args.After;

                if (user == null || channel == null || server == null || after == null)
                    return;

                if (channel.IsPrivate || user.Id == MidnightBot.Client.CurrentUser.Id) return;
                try
                {
                    var userRoles = args.User.Roles;
                    foreach (Role role in userRoles)
                    {
                        if (MidnightBot.Config.NoFilterRoles.Contains(role.Id))
                        {
                            noFilter = true;
                        }
                    }

                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering(channel, out serverPerms) || user.ServerPermissions.ManageMessages || noFilter == true) return;

                    var wordsInMessage = after.RawText.ToLowerInvariant().Split(' ');
                    if (serverPerms.Words.Any(w => wordsInMessage.Contains(w)))
                    {
                        string bufferString = "";
                        var sb = new StringBuilder();
                        foreach (var w in wordsInMessage)
                        {
                            foreach (var word in serverPerms.Words)
                            {
                                if (w == word)
                                {
                                    foreach (char c in w)
                                    {
                                        bufferString += "*";
                                    }
                                    sb.Append(bufferString + " ");
                                }
                                else
                                {
                                    sb.Append(w + " ");
                                }
                            }
                        }

                        await args.After.Delete().ConfigureAwait(false);
                        IncidentsHandler.Add(server.Id, channel.Id, $"Benutzer [{user.Name}/{user.Id}] schrieb ein " +
                                                             $"gebanntes Wort im Channel [{channel.Name}/{channel.Id}].\n" +
                                                             $"`Ganze Nachricht:` {after.Text}");
                        if (serverPerms.Verbose)
                            await channel.SendMessage($"**{user.Nickname}**: ```{sb.ToString()}```")
                                                           .ConfigureAwait(false);
                    }
                }
                catch { }
            };
        }

        private static bool IsChannelOrServerFiltering(Channel channel,out Classes.ServerPermissions serverPerms )
        {
            if (!PermissionsHandler.PermissionsDict.TryGetValue(channel.Server.Id, out serverPerms)) return false;

            if (serverPerms.Permissions.FilterWords)
                return true;

            Classes.Permissions perms;
            return serverPerms.ChannelPermissions.TryGetValue(channel.Id, out perms) && perms.FilterWords;
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand (Module.Prefix + "chnlfilterwords")
                .Alias (Module.Prefix + "cfw")
                .Description("Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf diesem Channel, die gebannte Wörter beinhalten." +
                             "Wenn kein Channel ausgewählt, dieser hier. Benutze ALL um auf alle derzeit existierenden Channel zu aktivieren." +
                             $" | `{Prefix}cfw enable #general-chat`")
                .Parameter("bool")
                .Parameter("channel", ParameterType.Optional)
                .Do(async e =>
                {
                    try
                    {
                        var state = PermissionHelper.ValidateBool(e.GetArg("bool"));
                        var chanStr = e.GetArg("channel")?.ToLowerInvariant().Trim();

                        if (chanStr != "all")
                        {
                            var chan = string.IsNullOrWhiteSpace(chanStr)
                                ? e.Channel
                                : PermissionHelper.ValidateChannel(e.Server, chanStr);
                            await PermissionsHandler.SetChannelWordPermission(chan, state).ConfigureAwait(false);
                            await e.Channel.SendMessage($"Wort Filterung wurde **{(state ? "aktiviert" : "deaktiviert")}** für Channel **{chan.Name}**.")
                            .ConfigureAwait (false);
                            return;
                        }
                        //all channels

                        foreach (var curChannel in e.Server.TextChannels)
                        {
                            await PermissionsHandler.SetChannelWordPermission(curChannel, state).ConfigureAwait(false);
                        }
                        await e.Channel.SendMessage($"Wort Filterung wurde **{(state ? "aktiviert" : "deaktivert")}** für **ALLE** Channel.")
                        .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                    }
                });

            cgb.CreateCommand (Module.Prefix + "addfilterword")
               .Alias (Module.Prefix + "afw")
               .Description("Fügt ein neues Wort zur Liste der gefilterten Wörter hinzu." +
                            $" | `{Prefix}afw poop`")
               .Parameter("word", ParameterType.Unparsed)
               .Do(async e => 
               {
                   try
                   {
                       var word = e.GetArg("word");
                       if (string.IsNullOrWhiteSpace(word))
                           return;
                       await PermissionsHandler.AddFilteredWord(e.Server, word.ToLowerInvariant().Trim()).ConfigureAwait(false);
                       await e.Channel.SendMessage($"Neues Wort erfolgreich zum Filter hinzugefügt.")
                       .ConfigureAwait (false);

                   }
                   catch (Exception ex)
                   {
                       await e.Channel.SendMessage($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                   }
               });

            cgb.CreateCommand (Module.Prefix + "rmvfilterword")
               .Alias (Module.Prefix + "rfw")
               .Description("Entfernt ein Wort von der Liste der gefilterten Wörter." +
                            $" | `{Prefix}rfw poop`")
               .Parameter("word", ParameterType.Unparsed)
               .Do(async e =>
               {
                   try
                   {
                       var word = e.GetArg("word");
                       if (string.IsNullOrWhiteSpace(word))
                           return;
                       await PermissionsHandler.RemoveFilteredWord(e.Server, word.ToLowerInvariant().Trim()).ConfigureAwait(false);
                       await e.Channel.SendMessage($"Wort erfolgreich von Liste entfernt.")
                       .ConfigureAwait (false);

                   }
                   catch (Exception ex)
                   {
                       await e.Channel.SendMessage($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                   }
               });

            cgb.CreateCommand (Module.Prefix + "lstfilterwords")
               .Alias (Module.Prefix + "lfw")
               .Description("Zeigt Liste der gefilterten Wörter." +
                            $" | `{Prefix}lfw`")
               .Do(async e => 
               {
                   try
                   {
                       Classes.ServerPermissions serverPerms;
                       if (!PermissionsHandler.PermissionsDict.TryGetValue(e.Server.Id, out serverPerms))
                           return;
                       await e.Channel.SendMessage($"Es gibt `{serverPerms.Words.Count}` gefilterte Wörter.\n" +
                           string.Join("\n", serverPerms.Words)).ConfigureAwait (false);
                   }
                   catch (Exception ex)
                   {
                       await e.Channel.SendMessage($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                   }
               });

            cgb.CreateCommand (Module.Prefix + "srvrfilterwords")
                .Alias (Module.Prefix + "sfw")
                .Description($"Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf dem Server, die verbotene Wörter enthalten. | `{Prefix}sfi disable`")
                .Parameter("bool")
                .Do(async e =>
                {
                    try
                    {
                        var state = PermissionHelper.ValidateBool(e.GetArg("bool"));
                        await PermissionsHandler.SetServerWordPermission(e.Server, state).ConfigureAwait(false);
                        await e.Channel.SendMessage($"Wort Filterung wurde **{(state ? "aktiviert" : "deaktiviert")}** auf diesem Server.")
                        .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage($"💢 Fehler: {ex.Message}").ConfigureAwait (false);
                    }
                });
        }
    }
}
