using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;

namespace MidnightBot.Modules.Administration.Commands
{
    internal class LogCommand : DiscordCommand
    {
        private string prettyCurrentTime => $"【{DateTime.Now:HH:mm:ss}】";

        public LogCommand(DiscordModule module) : base(module)
        {
            MidnightBot.OnReady += () =>
            {
            //MidnightBot.Client.MessageReceived += MsgRecivd;
            MidnightBot.Client.MessageDeleted += MsgDltd;
            MidnightBot.Client.MessageUpdated += MsgUpdtd;
            MidnightBot.Client.UserUpdated += UsrUpdtd;
            MidnightBot.Client.UserBanned += UsrBanned;
            MidnightBot.Client.UserLeft += UsrLeft;
            MidnightBot.Client.UserJoined += UsrJoined;
            MidnightBot.Client.UserUnbanned += UsrUnbanned;
            MidnightBot.Client.ChannelCreated += ChannelCreated;
            MidnightBot.Client.ChannelDestroyed += ChannelDestroyed;
            MidnightBot.Client.ChannelUpdated += ChannelUpdated;

                MidnightBot.Client.MessageReceived += async (s, e) =>
                {
                    if (e.Channel.IsPrivate || e.User.Id == MidnightBot.Client.CurrentUser.Id)
                        return;
                    if (!SpecificConfigurations.Default.Of(e.Server.Id).SendPrivateMessageOnMention)
                        return;
                    try
                    {
                        if (e.Channel.IsPrivate)
                            return;
                        var usr = e.Message.MentionedUsers.FirstOrDefault(u => u != e.User);
                        if (usr?.Status != UserStatus.Offline)
                            return;
                    //await e.Channel.SendMessage ($"Benutzer `{usr.Name}` ist offline. PM gesendet.").ConfigureAwait (false);
                    await usr.SendMessage(
                        $"Benutzer `{e.User.Name}` hat dich erwähnt auf dem Server " +
                        $"`{e.Server.Name}` während du offline warst.\n" +
                        $"`Nachricht:` {e.Message.Text}").ConfigureAwait(false);
                    }
                    catch { }
                };
            };
        }

        private async void ChannelUpdated ( object sender,ChannelUpdatedEventArgs e )
        {
            try
            {
                var config = SpecificConfigurations.Default.Of(e.Server.Id);
                var chId = config.LogServerChannel;
                if (chId == null || config.LogserverIgnoreChannels.Contains(e.After.Id))
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                if (e.Before.Name != e.After.Name)
                    await ch.SendMessage ($@"`{prettyCurrentTime}` **Channel Name geändert** `#{e.Before.Name}` (*{e.After.Id}*)
                                          `Neu:` {e.After.Name}").ConfigureAwait (false);
                else if (e.Before.Topic != e.After.Topic)
                    await ch.SendMessage ($@"`{prettyCurrentTime}` **Channel Topic geändert** `#{e.After.Name}` (*{e.After.Id}*)
                                          `Alt:` {e.Before.Topic}
                                          `Neu:` {e.After.Topic}").ConfigureAwait (false);
            }
            catch { }
        }

        private async void ChannelDestroyed ( object sender,ChannelEventArgs e )
        {
            try
            {
                var config = SpecificConfigurations.Default.Of(e.Server.Id);
                var chId = config.LogServerChannel;
                if (chId == null || config.LogserverIgnoreChannels.Contains(e.Channel.Id))
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"❗`{prettyCurrentTime}`❗`Channel Gelöscht:` #{e.Channel.Name} (*{e.Channel.Id}*)").ConfigureAwait (false);
            }
            catch { }
        }

        private async void ChannelCreated ( object sender,ChannelEventArgs e )
        {
            try
            {
                var config = SpecificConfigurations.Default.Of(e.Server.Id);
                var chId = config.LogServerChannel;
                if (chId == null || config.LogserverIgnoreChannels.Contains(e.Channel.Id))
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"`{prettyCurrentTime}`🆕`Channel Erstellt:` #{e.Channel.Mention} (*{e.Channel.Id}*)").ConfigureAwait (false);
            }
            catch { }
        }

        private async void UsrUnbanned ( object sender,UserEventArgs e )
        {
            try
            {
                var chId = SpecificConfigurations.Default.Of (e.Server.Id).LogServerChannel;
                if (chId == null)
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"`{prettyCurrentTime}`♻`Benutzer wurde entbannt:` **{e.User.Name}** ({e.User.Id})").ConfigureAwait (false);
            }
            catch { }
        }


        private async void UsrJoined ( object sender,UserEventArgs e )
        {
            try
            {
                var chId = SpecificConfigurations.Default.Of (e.Server.Id).LogServerChannel;
                if (chId == null)
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"`{prettyCurrentTime}`✅`User joined:` **{e.User.Name}** ({e.User.Id})").ConfigureAwait (false);
            }
            catch { }
        }

        private async void UsrLeft ( object sender,UserEventArgs e )
        {
            try
            {
                var chId = SpecificConfigurations.Default.Of (e.Server.Id).LogServerChannel;
                if (chId == null)
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"`{prettyCurrentTime}`❗`Benutzer verließ den Server:` **{e.User.Name}** ({e.User.Id})").ConfigureAwait (false);
            }
            catch { }
        }

        private async void UsrBanned(object sender, UserEventArgs e)
        {
            try
            {
                var chId = SpecificConfigurations.Default.Of (e.Server.Id).LogServerChannel;
                if (chId == null)
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage ($"❗`{prettyCurrentTime}`❌`User gebannt:` **{e.User.Name}** ({e.User.Id})").ConfigureAwait (false);
            }
            catch { }
        }

        //private async void MsgRecivd(object sender, MessageEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Server == null || e.Channel.IsPrivate || e.User.Id == MidnightBot.Client.CurrentUser.Id)
        //            return;
        //        var config = SpecificConfigurations.Default.Of(e.Server.Id);
        //        var chId = config.LogServerChannel;
        //        if (chId == null || e.Channel.Id == chId || config.LogserverIgnoreChannels.Contains(e.Channel.Id))
        //            return;
        //        Channel ch;
        //        if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
        //            return;
        //        if (!string.IsNullOrWhiteSpace(e.Message.Text))
        //        {
        //            await ch.SendMessage(
        //            $@"🕔`{prettyCurrentTime}` **Neue Nachricht** `#{e.Channel.Name}`
        //            👤`{e.User?.ToString() ?? ("NULL")}` {e.Message.Text.Unmention()}").ConfigureAwait(false);
        //        }
        //        else
        //        {
        //            await ch.SendMessage(
        //            $@"🕔`{prettyCurrentTime}` **Datei hochgeladen** `#{e.Channel.Name}`
        //            👤`{e.User?.ToString() ?? ("NULL")}` {e.Message.Attachments.FirstOrDefault()?.ProxyUrl}").ConfigureAwait(false);
        //        }
        //    }
        //    catch { }
        //}
        private async void MsgDltd(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Server == null || e.Channel.IsPrivate || e.User?.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                var config = SpecificConfigurations.Default.Of(e.Server.Id);
                var chId = config.LogServerChannel;
                if (chId == null || e.Channel.Id == chId || config.LogserverIgnoreChannels.Contains(e.Channel.Id))
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                if (!string.IsNullOrWhiteSpace(e.Message.Text))
                {
                    await ch.SendMessage(
                    $@"🕔`{prettyCurrentTime}` **Nachricht gelöscht** 🚮 `#{e.Channel.Name}`
                    👤`{e.User?.ToString() ?? ("NULL")}` {e.Message.Text.Unmention()}").ConfigureAwait(false);
                }
                else
                {
                    await ch.SendMessage(
                    $@"🕔`{prettyCurrentTime}` **Datei gelöscht** `#{e.Channel.Name}`
                    👤`{e.User?.ToString() ?? ("NULL")}` {e.Message.Attachments.FirstOrDefault()?.ProxyUrl}").ConfigureAwait(false);
                }
            }
            catch { }
        }
        private async void MsgUpdtd(object sender, MessageUpdatedEventArgs e)
        {
            try
            {
                if (e.Server == null || e.Channel.IsPrivate || e.User?.Id == MidnightBot.Client.CurrentUser.Id)
                    return;
                if (e.Before.RawText == e.After.RawText)
                    return;
                var config = SpecificConfigurations.Default.Of(e.Server.Id);
                var chId = config.LogServerChannel;
                if (chId == null || e.Channel.Id == chId || config.LogserverIgnoreChannels.Contains(e.Channel.Id))
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                await ch.SendMessage 
                    (
                        $@"🕔`{prettyCurrentTime}` **Nachricht** 📝 `#{e.Channel.Name}`
                        👤`{e.User?.ToString () ?? ("NULL")}`
                        `Alt:` {e.Before.Text.Unmention ()}
                        `Neu:` {e.After.Text.Unmention ()}"
                    ).ConfigureAwait (false);
            }
            catch { }
        }
        private async void UsrUpdtd(object sender, UserUpdatedEventArgs e)
        {
            var config = SpecificConfigurations.Default.Of (e.Server.Id);
            try
            {
                var chId = config.LogPresenceChannel;
                if (chId != null)
                {
                    Channel ch;
                    if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) != null)
                    {
                        if (e.Before.Status != e.After.Status)
                        {
                            await ch.SendMessage ($"`{prettyCurrentTime}`**{e.Before.Name}** ist jetzt **{e.After.Status}**.").ConfigureAwait (false);
                        }
                    }
                }
            }
            catch { }

            try
            {
                ulong notifyChBeforeId;
                ulong notifyChAfterId;
                Channel notifyChBefore = null;
                Channel notifyChAfter = null;
                var beforeVch = e.Before.VoiceChannel;
                var afterVch = e.After.VoiceChannel;
                var notifyLeave = false;
                var notifyJoin = false;
                if ((beforeVch != null || afterVch != null) && (beforeVch != afterVch)) // this means we need to notify for sure.
                {
                    if (beforeVch != null && config.VoiceChannelLog.TryGetValue (beforeVch.Id,out notifyChBeforeId) && (notifyChBefore = e.Before.Server.TextChannels.FirstOrDefault (tc => tc.Id == notifyChBeforeId)) != null)
                    {
                        notifyLeave = true;
                    }
                    if (afterVch != null && config.VoiceChannelLog.TryGetValue (afterVch.Id,out notifyChAfterId) && (notifyChAfter = e.After.Server.TextChannels.FirstOrDefault (tc => tc.Id == notifyChAfterId)) != null)
                    {
                        notifyJoin = true;
                    }
                    if ((notifyLeave && notifyJoin) && (notifyChAfter == notifyChBefore))
                    {
                        await notifyChAfter.SendMessage ($"🎼`{prettyCurrentTime}` {e.Before.Name} ging vom **{beforeVch.Mention}** zum **{afterVch.Mention}** Voice Channel.").ConfigureAwait (false);
                    }
                    else if (notifyJoin)
                    {
                        await notifyChAfter.SendMessage ($"🎼`{prettyCurrentTime}` {e.Before.Name} betrat den **{afterVch.Mention}** Voice Channel.").ConfigureAwait (false);
                    }
                    else if (notifyLeave)
                    {
                        await notifyChBefore.SendMessage ($"🎼`{prettyCurrentTime}` {e.Before.Name} verlies den **{beforeVch.Mention}** Voice Channel.").ConfigureAwait (false);
                    }
                }
            }
            catch { }

            try
            {
                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                if (chId == null)
                    return;
                Channel ch;
                if ((ch = e.Server.TextChannels.Where (tc => tc.Id == chId).FirstOrDefault ()) == null)
                    return;
                string str = $"🕔`{prettyCurrentTime}`";
                if (e.Before.Name != e.After.Name)
                    str += $"**Name geändert**👤`{e.Before?.ToString ()}`\n\t\t`Neu:`{e.After.ToString ()}`";
                else if (e.Before.Nickname != e.After.Nickname)
                    str += $"**Nickname geändert**👤`{e.Before?.ToString ()}`\n\t\t`Alt:` {e.Before.Nickname}#{e.Before.Discriminator}\n\t\t`Neu:` {e.After.Nickname}#{e.After.Discriminator}";
                else if (e.Before.AvatarUrl != e.After.AvatarUrl)
                    str += $"**Neuer Avatar**👤`{e.Before?.ToString ()}`\n\t {await e.Before.AvatarUrl.ShortenUrl ()} `=>` {await e.After.AvatarUrl.ShortenUrl ()}";
                else if (!e.Before.Roles.SequenceEqual (e.After.Roles))
                {
                    if (e.Before.Roles.Count () < e.After.Roles.Count ())
                    {
                        var diffRoles = e.After.Roles.Where (r => !e.Before.Roles.Contains (r)).Select (r => "`" + r.Name + "`");
                        str += $"**Benutzer Rollen geändert ⚔➕**👤`{e.Before?.ToString ()}`\n\tHat nun die Rolle {string.Join (", ",diffRoles)}.";
                    }
                    else if (e.Before.Roles.Count () > e.After.Roles.Count ())
                    {
                        var diffRoles = e.Before.Roles.Where (r => !e.After.Roles.Contains (r)).Select (r => "`" + r.Name + "`");
                        str += $"**Benutzer Rollen geändert ⚔➖**👤`{e.Before?.ToString ()}`\n\tHat nicht länger die Rolle {string.Join (", ",diffRoles)}.";
                    }
                    else
                    {
                        Console.WriteLine ("SEQUENCE NOT EQUAL BUT NO DIFF ROLES - REPORT TO KWOTH on #NADEKOLOG server");
                        return;
                    }
                }
                else
                    return;
                await ch.SendMessage (str).ConfigureAwait (false);
            }
            catch { }
        }

        internal override void Init(CommandGroupBuilder cgb)
        {

            cgb.CreateCommand (Module.Prefix + "spmom")
                .Description ($"Toggles whether mentions of other offline users on your server will send a pm to them. | `{Prefix}spmom`")
                .AddCheck (SimpleCheckers.ManageServer ())
                .Do (async e =>
                {
                    var specificConfig = SpecificConfigurations.Default.Of (e.Server.Id);
                    specificConfig.SendPrivateMessageOnMention =
                    !specificConfig.SendPrivateMessageOnMention;
                        if (specificConfig.SendPrivateMessageOnMention)
                            await e.Channel.SendMessage (":ok: I will send private messages " +
                            "to mentioned offline users.");
                        else
                            await e.Channel.SendMessage (":ok: I won't send private messages " +
                            "to mentioned offline users anymore.").ConfigureAwait (false);
                });

            cgb.CreateCommand(Module.Prefix + "logserver")
                  .Description($"Toggles logging in this channel. Logs every message sent/deleted/edited on the server. **Bot Owner Only!** | `{Prefix}logserver`")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .AddCheck (SimpleCheckers.ManageServer ())
                  .Do(async e =>
                  {
                      var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                      if (chId == null)
                      {
                          SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel = e.Channel.Id;
                          await e.Channel.SendMessage($"❗**ICH WERDE SERVER AKTIVITÄTEN IN DIESEM CHANNEL LOGGEN**❗").ConfigureAwait(false);
                          return;
                      }
                      Channel ch;
                      if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) == null)
                          return;

                      SpecificConfigurations.Default.Of (e.Server.Id).LogServerChannel = null;
                      await e.Channel.SendMessage($"❗**LOGGING BEENDET IM CHANNEL {ch.Mention}**❗").ConfigureAwait(false);
                  });

            cgb.CreateCommand(Prefix + "logignore")
                .Description($"Toggles whether the {Prefix}logserver command ignores this channel. Useful if you have hidden admin channel and public log channel. | `{Prefix}logignore`")
                .AddCheck(SimpleCheckers.OwnerOnly())
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    var config = SpecificConfigurations.Default.Of(e.Server.Id);
                    if (config.LogserverIgnoreChannels.Remove(e.Channel.Id))
                    {
                        await e.Channel.SendMessage($"`{Prefix}logserver will stop ignoring this channel.`");
                    }
                    else
                    {
                        config.LogserverIgnoreChannels.Add(e.Channel.Id);
                        await e.Channel.SendMessage($"`{Prefix}logserver will ignore this channel.`");
                    }
                });

            cgb.CreateCommand(Module.Prefix + "userpresence")
                  .Description($"Starts logging to this channel when someone from the server goes online/offline/idle. **Bot Owner Only!** | `{Prefix}userpresence`")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .AddCheck (SimpleCheckers.ManageServer ())
                  .Do(async e =>
                  {
                      var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogPresenceChannel;
                      if (chId == null)
                      {
                          SpecificConfigurations.Default.Of (e.Server.Id).LogPresenceChannel = e.Channel.Id;
                          await e.Channel.SendMessage($"**User presence notifications enabled.**").ConfigureAwait (false);
                          return;
                      }

                      SpecificConfigurations.Default.Of (e.Server.Id).LogPresenceChannel = null;
                      await e.Channel.SendMessage($"**User presence notifications disabled.**").ConfigureAwait (false);
                  });

            cgb.CreateCommand(Module.Prefix + "voicepresence")
                  .Description($"Toggles logging to this channel whenever someone joins or leaves a voice channel you are in right now. **Bot Owner Only!** | `{Prefix}voicerpresence`")
                  .Parameter("all", ParameterType.Optional)
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .AddCheck (SimpleCheckers.ManageServer ())
                  .Do(async e => 
                  {
                      var config = SpecificConfigurations.Default.Of (e.Server.Id);
                      if (e.GetArg("all")?.ToLower() == "all")
                      {
                          foreach (var voiceChannel in e.Server.VoiceChannels) {
                              config.VoiceChannelLog.TryAdd (voiceChannel.Id,e.Channel.Id);
                          }
                          await e.Channel.SendMessage("Started logging user presence for **ALL** voice channels!").ConfigureAwait (false);
                          return;
                      }

                      if (e.User.VoiceChannel == null)
                      {
                          await e.Channel.SendMessage("💢 You are not in a voice channel right now. If you are, please rejoin it.").ConfigureAwait (false);
                          return;
                      }
                      ulong throwaway;
                      if (!config.VoiceChannelLog.TryRemove (e.User.VoiceChannel.Id,out throwaway))
                      {
                          config.VoiceChannelLog.TryAdd (e.User.VoiceChannel.Id,e.Channel.Id);
                          await e.Channel.SendMessage($"`Logging user updates for` {e.User.VoiceChannel.Mention} `voice channel.`").ConfigureAwait (false);
                      } else
                          await e.Channel.SendMessage($"`Stopped logging user updates for` {e.User.VoiceChannel.Mention} `voice channel.`").ConfigureAwait (false);
                  });
        }
    }
}
