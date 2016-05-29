﻿using Discord;
using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChPermOverride = Discord.ChannelPermissionOverrides;

namespace NadekoBot.Modules.Administration.Commands
{
    internal class VoicePlusTextCommand : DiscordCommand
    {
        Regex channelNameRegex = new Regex (@"[^a-zA-Z0-9 -]",RegexOptions.Compiled);
        public VoicePlusTextCommand ( DiscordModule module ) : base (module)
        {
            // changing servers may cause bugs
            NadekoBot.Client.UserUpdated += async ( sender,e ) =>
            {
                try
                {
                    if (e.Server == null)
                        return;
                    var config = SpecificConfigurations.Default.Of (e.Server.Id);
                    if (e.Before.VoiceChannel == e.After.VoiceChannel)
                        return;
                    if (!config.VoicePlusTextEnabled)
                        return;
                    var serverPerms = e.Server.GetUser (NadekoBot.Client.CurrentUser.Id)?.ServerPermissions;
                    if (serverPerms == null)
                        return;
                    if (!serverPerms.Value.ManageChannels || !serverPerms.Value.ManageRoles)
                    {

                        try
                        {
                            await e.Server.Owner.SendMessage (
                            "I don't have manage server and/or Manage Channels permission," +
                            $" so I cannot run voice+text on **{e.Server.Name}** server.").ConfigureAwait (false);
                        }
                        catch { } // meh
                        config.VoicePlusTextEnabled = false;
                        return;
                    }


                    var beforeVch = e.Before.VoiceChannel;
                    if (beforeVch != null)
                    {
                        var textChannel =
                            e.Server.FindChannels (GetChannelName (beforeVch.Name),ChannelType.Text).FirstOrDefault ();
                        if (textChannel != null)
                            await textChannel.AddPermissionsRule (e.Before,
                                new ChPermOverride (readMessages: PermValue.Deny,
                                                   sendMessages: PermValue.Deny)).ConfigureAwait (false);
                    }
                    var afterVch = e.After.VoiceChannel;
                    if (afterVch != null)
                    {
                        var textChannel = e.Server.FindChannels (
                                                    GetChannelName (afterVch.Name),
                                                    ChannelType.Text)
                                                    .FirstOrDefault ();
                        if (textChannel == null)
                        {
                            textChannel = (await e.Server.CreateChannel (GetChannelName (afterVch.Name),ChannelType.Text).ConfigureAwait (false));
                            await textChannel.AddPermissionsRule (e.Server.EveryoneRole,
                                new ChPermOverride (readMessages: PermValue.Deny,
                                                   sendMessages: PermValue.Deny)).ConfigureAwait (false);
                        }
                        await textChannel.AddPermissionsRule (e.After,
                            new ChPermOverride (readMessages: PermValue.Allow,
                                               sendMessages: PermValue.Allow)).ConfigureAwait (false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine (ex);
                }
            };
        }

        private string GetChannelName ( string voiceName ) =>
            channelNameRegex.Replace (voiceName,"").Trim ().Replace (" ","-").TrimTo (90,true) + "-voice";

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "cleanv+t")
                .Description ("Löscht alle Text-Channel die auf `-voice` enden für die keine Voicechannels gefunden werden. **Benutzung auf eigene Gefahr.**")
                .AddCheck (SimpleCheckers.CanManageRoles)
                .AddCheck (SimpleCheckers.ManageChannels ())
                .Do (async e =>
                {
                    if (!e.Server.CurrentUser.ServerPermissions.ManageChannels)
                    {
                        await e.Channel.SendMessage("`Ich habe nicht ausreichend Rechte, um dies zu tun.`");
                        return;
                    }

                    var allTxtChannels = e.Server.TextChannels.Where (c => c.Name.EndsWith ("-voice"));
                    var validTxtChannelNames = e.Server.VoiceChannels.Select (c => GetChannelName (c.Name));

                    var invalidTxtChannels = allTxtChannels.Where (c => !validTxtChannelNames.Contains (c.Name));

                    foreach (var c in invalidTxtChannels)
                    {
                        try
                        {
                            await c.Delete();
                        }
                        catch { }
                        await Task.Delay(500);
                    }

                    await e.Channel.SendMessage ("`Erledigt.`");
                });

            cgb.CreateCommand (Module.Prefix + "v+t")
                .Alias (Module.Prefix + "voice+text")
                .Description ("Erstellt einen Text-Channel für jeden Voice-Channel, welchen nur User im dazugehörigen Voice-Channel sehen können." +
                             "Als Server-Owner sieht du alle Channel, zu jeder Zeit.")
                .AddCheck (SimpleCheckers.ManageChannels ())
                .AddCheck (SimpleCheckers.CanManageRoles)
                .Do (async e =>
                {
                    try
                    {
                        var config = SpecificConfigurations.Default.Of (e.Server.Id);
                        if (config.VoicePlusTextEnabled == true)
                        {
                            config.VoicePlusTextEnabled = false;
                            foreach (var textChannel in e.Server.TextChannels.Where (c => c.Name.EndsWith ("-voice")))
                            {
                                try
                                {
                                    await textChannel.Delete ().ConfigureAwait (false);
                                }
                                catch
                                {
                                    await e.Channel.SendMessage (
                                            ":anger: Fehler: Nicht genug Rechte(Bot).")
                                            .ConfigureAwait (false);
                                    return;
                                }
                            }
                            await e.Channel.SendMessage ("Erfolgreich voice + text Feature entfernt.").ConfigureAwait (false);
                            return;
                        }
                        config.VoicePlusTextEnabled = true;
                        await e.Channel.SendMessage ("Erfolgreich voice + text Feature aktiviert. " +
                                                    "**Sei sicher das der Bot 'manage roles' und 'manage channels' Rechte besitzt.**")
                                                    .ConfigureAwait (false);

                    }
                    catch (Exception ex)
                    {
                        await e.Channel.SendMessage (ex.ToString ()).ConfigureAwait (false);
                    }
                });
        }
    }
}