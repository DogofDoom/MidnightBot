using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;

namespace MidnightBot.Modules.Permissions.Commands
{
    internal class FilterWords : DiscordCommand
    {
        public FilterWords(DiscordModule module) : base(module)
        {
            MidnightBot.Client.MessageReceived += async (sender, args) =>
            {
                if (args.Channel.IsPrivate || args.User.Id == MidnightBot.Client.CurrentUser.Id) return;
                try
                {
                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering(args.Channel, out serverPerms)|| args.User.ServerPermissions.ManageMessages) return;

                    var wordsInMessage = args.Message.RawText.ToLowerInvariant().Split(' ');
                    if (serverPerms.Words.Any(w => wordsInMessage.Contains(w)))
                    {
                        await args.Message.Delete().ConfigureAwait (false);
                        IncidentsHandler.Add(args.Server.Id,args.Channel.Id,$"Benutzer [{args.User.Name}/{args.User.Id}] schrieb ein " +
                                                             $"gebanntes Wort im Channel [{args.Channel.Name}/{args.Channel.Id}].\n" +
                                                             $"`Ganze Nachricht:` {args.Message.Text}");
                        if (serverPerms.Verbose)
                            await args.Channel.SendMessage($"{args.User.Mention} Ein, oder mehrere Wörter " +
                                                           $"in diesem Satz sind hier nicht erlaubt.")
                                                           .ConfigureAwait (false);
                    }
                }
                catch { }
            };

            MidnightBot.Client.MessageUpdated += async (sender, args) =>
            {
                if (args.Channel.IsPrivate || args.User.Id == MidnightBot.Client.CurrentUser.Id) return;
                try
                {
                    Classes.ServerPermissions serverPerms;
                    if (!IsChannelOrServerFiltering(args.Channel, out serverPerms) || args.User.ServerPermissions.ManageMessages) return;

                    var wordsInMessage = args.After.RawText.ToLowerInvariant().Split(' ');
                    if (serverPerms.Words.Any(w => wordsInMessage.Contains(w)))
                    {
                        await args.After.Delete().ConfigureAwait(false);
                        IncidentsHandler.Add(args.Server.Id, args.Channel.Id, $"Benutzer [{args.User.Name}/{args.User.Id}] schrieb ein " +
                                                             $"gebanntes Wort im Channel [{args.Channel.Name}/{args.Channel.Id}].\n" +
                                                             $"`Ganze Nachricht:` {args.After.Text}");
                        if (serverPerms.Verbose)
                            await args.Channel.SendMessage($"{args.User.Mention} Ein, oder mehrere Wörter " +
                                                           $"in diesem Satz sind hier nicht erlaubt.")
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
