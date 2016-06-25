using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using System;
using System.Linq;
using System.Text;

namespace MidnightBot.Modules.Administration.Commands
{
    class InfoCommands : DiscordCommand
    {
        public InfoCommands ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "serverinfo")
                .Alias (Module.Prefix + "sinfo")
                .Description ($"Zeigt Infos über den Server, auf dem der Bot läuft. Falls kein Server ausgewählt, derzeitiger wird ausgewählt.\n**Benutzung**:{Module.Prefix}sinfo Some Server")
                .Parameter ("server",ParameterType.Optional)
                .Do (async e =>
                {
                    var servText = e.GetArg ("server")?.Trim ();
                    var server = string.IsNullOrWhiteSpace (servText)
                             ? e.Server
                             : MidnightBot.Client.FindServers (servText).FirstOrDefault ();
                    if (server == null)
                        return;
                    var createdAt = new DateTime (2015,1,1,0,0,0,0,DateTimeKind.Utc).AddMilliseconds (server.Id >> 22);
                    var sb = new StringBuilder ();
                    sb.AppendLine ($"`Name:` **#{server.Name}**");
                    sb.AppendLine ($"`Besitzer:` **{server.Owner}**");
                    sb.AppendLine ($"`Id:` **{server.Id}**");
                    sb.AppendLine ($"`Icon Url:` **{await server.IconUrl.ShortenUrl ().ConfigureAwait (false)}**");
                    sb.AppendLine ($"`TextChannel:` **{server.TextChannels.Count ()}** `VoiceChannel:` **{server.VoiceChannels.Count ()}**");
                    sb.AppendLine ($"`Benutzer:` **{server.UserCount}** `Online:` **{server.Users.Count (u => u.Status == UserStatus.Online)}** (könnte ungenau sein)");
                    sb.AppendLine ($"`Rollen:` **{server.Roles.Count ()}**");
                    sb.AppendLine ($"`Erstellt am:` **{createdAt}**");
                    if (server.CustomEmojis.Count () > 0)
                        sb.AppendLine ($"`Custom Emojis:` **{string.Join (", ",server.CustomEmojis)}**");
                    if (server.Features.Count () > 0)
                        sb.AppendLine ($"`Features:` **{string.Join (", ",server.Features)}**");
                    if (!string.IsNullOrWhiteSpace (server.SplashId))
                        sb.AppendLine ($"`Region:` **{server.Region.Name}**");
                    await e.Channel.SendMessage (sb.ToString ()).ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "channelinfo")
                .Alias (Module.Prefix + "cinfo")
                .Description ($"Zeigt Infos über einen Channel. Wenn kein Channel ausgewählt, derzeitiger wird angegeben.\n**Benutzung**:{Module.Prefix}cinfo #some-channel")
                .Parameter ("channel",ParameterType.Optional)
                .Do (async e =>
                {
                    var chText = e.GetArg ("channel")?.Trim ();
                    var ch = string.IsNullOrWhiteSpace (chText)
                             ? e.Channel
                             : e.Server.FindChannels (chText,Discord.ChannelType.Text).FirstOrDefault ();
                    if (ch == null)
                        return;
                    var createdAt = new DateTime (2015,1,1,0,0,0,0,DateTimeKind.Utc).AddMilliseconds (ch.Id >> 22);
                    var sb = new StringBuilder ();
                    sb.AppendLine ($"`Name:` **#{ch.Name}**");
                    sb.AppendLine ($"`Id:` **{ch.Id}**");
                    sb.AppendLine ($"`Erstellt am :` **{createdAt}**");
                    sb.AppendLine ($"`Topic:` **{ch.Topic}**");
                    sb.AppendLine ($"`Benutzer:` **{ch.Users.Count ()}**");
                    await e.Channel.SendMessage (sb.ToString ()).ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "userinfo")
                .Alias (Module.Prefix + "uinfo")
                .Description ($"Zeigt eine Info über den User. Wenn kein User angegeben, User der den Befehl eingibt.\n**Benutzung**:{Module.Prefix}uinfo @SomeUser")
                .Parameter ("user",ParameterType.Optional)
                .Do (async e =>
                {
                    TimeSpan twohours = new TimeSpan (2,0,0);
                    var userText = e.GetArg ("user")?.Trim ();
                    var user = string.IsNullOrWhiteSpace (userText)
                             ? e.User
                             : e.Server.FindUsers (userText).FirstOrDefault ();
                    if (user == null)
                        return;
                    var sb = new StringBuilder ();
                    sb.AppendLine ($"`Name#Discrim:` **#{user.Name}#{user.Discriminator}**");
                    if (!string.IsNullOrWhiteSpace (user.Nickname))
                        sb.AppendLine ($"`Nickname:` **{user.Nickname}**");
                    sb.AppendLine ($"`Id:` **{user.Id}**");
                    sb.AppendLine ($"`Status:` **{user.Status}**");
                    sb.AppendLine ($"`Derzeitiges Spiel:` **{(user.CurrentGame?.Name == null ? "-" : user.CurrentGame.Value.Name)}**");
                    if (user.LastOnlineAt != null)
                        sb.AppendLine ($"`Zuletzt online:` **{user.LastOnlineAt+twohours:HH:mm:ss}**");
                    sb.AppendLine ($"`Gejoint am:` **{user.JoinedAt}**");
                    sb.AppendLine ($"`Rollen:` **({user.Roles.Count ()}) - {string.Join (", ",user.Roles.Select (r => r.Name))}**");
                    sb.AppendLine ($"`AvatarUrl:` **{await user.AvatarUrl.ShortenUrl ().ConfigureAwait (false)}**");
                    await e.Channel.SendMessage (sb.ToString ()).ConfigureAwait (false);
                });
        }
    }
}