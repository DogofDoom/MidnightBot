using Discord;
using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NadekoBot.Modules.Administration.Commands
{
    class CrossServerTextChannel : DiscordCommand
    {
        public CrossServerTextChannel(DiscordModule module) : base(module)
        {
            NadekoBot.Client.MessageReceived += async (s, e) =>
            {
                try
                {
                    if (e.User.Id == NadekoBot.Client.CurrentUser.Id) return;
                    foreach (var subscriber in Subscribers)
                    {
                        var set = subscriber.Value;
                        if (!set.Contains(e.Channel))
                            continue;
                        foreach (var chan in set.Except(new[] { e.Channel }))
                        {
                            await chan.SendMessage(GetText(e.Server, e.Channel, e.User, e.Message)).ConfigureAwait (false);
                        }
                    }
                }
                catch { }
            };
            NadekoBot.Client.MessageUpdated += async (s, e) => 
            {
                try
                {
                    if (e.After?.User?.Id == null || e.After.User.Id == NadekoBot.Client.CurrentUser.Id) return;
                    foreach (var subscriber in Subscribers)
                    {
                        var set = subscriber.Value;
                        if (!set.Contains(e.Channel))
                            continue;
                        foreach (var chan in set.Except(new[] { e.Channel }))
                        {
                            var msg = chan.Messages
                                .FirstOrDefault(m =>
                                    m.RawText == GetText(e.Server, e.Channel, e.User, e.Before));
                            if (msg != default(Message))
                                await msg.Edit(GetText(e.Server, e.Channel, e.User, e.After)).ConfigureAwait (false);
                        }
                    }

                }
                catch { }
            };
        }

        private string GetText(Server server, Channel channel, User user, Message message) =>
            $"**{server.Name} | {channel.Name}** `{user.Name}`: " + message.RawText;

        public static readonly ConcurrentDictionary<int, HashSet<Channel>> Subscribers = new ConcurrentDictionary<int, HashSet<Channel>>();

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "scsc")
                .Description("Startet eine Instanz eines Cross Server Channels. Du bekommst einen Token" +
                             "den andere Benutzer benutzen müssen, um auf die selbe Instanz zu kommen.")
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Do(async e => 
                {
                    var token = new Random().Next();
                    var set = new HashSet<Channel>();
                    if (Subscribers.TryAdd(token, set))
                    {
                        set.Add(e.Channel);
                        await e.User.SendMessage("Das ist dein CSC Token:" + token.ToString()).ConfigureAwait (false);
                    }
                });

            cgb.CreateCommand(Module.Prefix + "jcsc")
                .Description("Joint derzeitigen Channel einer Instanz des Cross Server Channel durch Benutzung des Tokens.")
                .Parameter("token")
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    int token;
                    if (!int.TryParse(e.GetArg("token"), out token))
                        return;
                    HashSet<Channel> set;
                    if (!Subscribers.TryGetValue(token, out set))
                        return;
                    set.Add(e.Channel);
                    await e.Channel.SendMessage(":ok:").ConfigureAwait (false);
                });

            cgb.CreateCommand(Module.Prefix + "lcsc")
                .Description("Verlässt Cross server Channel Instance von diesem Channel")
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    foreach (var subscriber in Subscribers)
                    {
                        subscriber.Value.Remove(e.Channel);
                    }
                    await e.Channel.SendMessage(":ok:").ConfigureAwait (false);
                });
        }
    }
}
