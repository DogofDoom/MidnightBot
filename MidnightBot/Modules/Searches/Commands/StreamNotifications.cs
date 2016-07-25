using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Classes.JSONModels;
using MidnightBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;


namespace MidnightBot.Modules.Searches.Commands
{
    internal class StreamNotifications : DiscordCommand
    {

        private readonly Timer checkTimer = new Timer
        {
            Interval = new TimeSpan (0,0,15).TotalMilliseconds,
        };

        private ConcurrentDictionary<string,Tuple<bool,string>> cachedStatuses = new ConcurrentDictionary<string,Tuple<bool,string>> ();

        public StreamNotifications ( DiscordModule module ) : base (module)
        {

            checkTimer.Elapsed += async ( s,e ) =>
            {
                cachedStatuses.Clear ();
                try
                {
                    var streams = SpecificConfigurations.Default.AllConfigs.SelectMany (c => c.ObservingStreams);
                    if (!streams.Any ())
                        return;

                    foreach (var stream in streams)
                    {
                        Tuple<bool,string> data;
                        try
                        {
                            data = await GetStreamStatus (stream).ConfigureAwait (false);
                        }
                        catch
                        {
                            continue;
                        }

                        if (data.Item1 != stream.LastStatus)
                        {
                            stream.LastStatus = data.Item1;
                            var server = MidnightBot.Client.GetServer (stream.ServerId);
                            var channel = server?.GetChannel (stream.ChannelId);
                            if (channel == null)
                                continue;
                            var msg = $"`{stream.Username}`'s Stream ist nun " +
                                      $"**{(data.Item1 ? "ONLINE" : "OFFLINE")}** mit " +
                                      $"**{data.Item2}** Zuschauern.";
                            if (stream.LastStatus)
                                if (stream.Type == StreamNotificationConfig.StreamType.Hitbox)
                                    msg += $"\n`Hier ist der Link:`【 http://www.hitbox.tv/{stream.Username}/ 】";
                                else if (stream.Type == StreamNotificationConfig.StreamType.Twitch)
                                    msg += $"\n`Hier ist der Link:`【 http://www.twitch.tv/{stream.Username}/ 】";
                                else if (stream.Type == StreamNotificationConfig.StreamType.Beam)
                                    msg += $"\n`Hier ist der Link:`【 http://www.beam.pro/{stream.Username}/ 】";
                                else if (stream.Type == StreamNotificationConfig.StreamType.YoutubeGaming)
                                    msg += $"\n`Hier ist der Link:`【 noch nicht implementiert - {stream.Username} 】";
                            await channel.SendMessage (msg).ConfigureAwait (false);
                        }
                    }
                }
                catch { }
                await ConfigHandler.SaveConfig().ConfigureAwait(false);
            };

            checkTimer.Start ();
        }

        private async Task<Tuple<bool,string>> GetStreamStatus ( StreamNotificationConfig stream,bool checkCache = true )
        {
            bool isLive;
            string response;
            JObject data;
            Tuple<bool,string> result;
            switch (stream.Type)
            {
                case StreamNotificationConfig.StreamType.Hitbox:
                    var hitboxUrl = $"https://api.hitbox.tv/media/status/{stream.Username}";
                    if (checkCache && cachedStatuses.TryGetValue (hitboxUrl,out result))
                        return result;
                    response = await SearchHelper.GetResponseStringAsync (hitboxUrl).ConfigureAwait (false);
                    data = JObject.Parse (response);
                    isLive = data["media_is_live"].ToString () == "1";
                    result = new Tuple<bool,string> (isLive,data["media_views"].ToString ());
                    cachedStatuses.TryAdd (hitboxUrl,result);
                    return result;
                case StreamNotificationConfig.StreamType.Twitch:
                    var twitchUrl = $"https://api.twitch.tv/kraken/streams/{Uri.EscapeUriString (stream.Username)}";
                    if (checkCache && cachedStatuses.TryGetValue (twitchUrl,out result))
                        return result;
                    response = await SearchHelper.GetResponseStringAsync (twitchUrl).ConfigureAwait (false);
                    data = JObject.Parse (response);
                    isLive = !string.IsNullOrWhiteSpace (data["stream"].ToString ());
                    result = new Tuple<bool,string> (isLive,isLive ? data["stream"]["viewers"].ToString () : "0");
                    cachedStatuses.TryAdd (twitchUrl,result);
                    return result;
                case StreamNotificationConfig.StreamType.Beam:
                    var beamUrl = $"https://beam.pro/api/v1/channels/{stream.Username}";
                    if (checkCache && cachedStatuses.TryGetValue (beamUrl,out result))
                        return result;
                    response = await SearchHelper.GetResponseStringAsync (beamUrl).ConfigureAwait (false);
                    data = JObject.Parse (response);
                    isLive = data["online"].ToObject<bool> () == true;
                    result = new Tuple<bool,string> (isLive,data["viewersCurrent"].ToString ());
                    cachedStatuses.TryAdd (beamUrl,result);
                    return result;
                default:
                    break;
            }
            return new Tuple<bool,string> (false,"0");
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "hitbox")
                .Alias (Module.Prefix + "hb")
                .Description ("Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen." +
                             " | ~hitbox SomeStreamer")
                .Parameter ("username",ParameterType.Unparsed)
                .AddCheck (SimpleCheckers.ManageServer ())
                .Do (TrackStream (StreamNotificationConfig.StreamType.Hitbox));

            cgb.CreateCommand (Module.Prefix + "twitch")
                .Alias (Module.Prefix + "tw")
                .Description ("Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen." +
                             " | ~twitch SomeStreamer")
                .AddCheck (SimpleCheckers.ManageServer ())
                .Parameter ("username",ParameterType.Unparsed)
                .Do (TrackStream (StreamNotificationConfig.StreamType.Twitch));

            cgb.CreateCommand (Module.Prefix + "beam")
                .Alias (Module.Prefix + "bm")
                .Description ("Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen." +
                                " | ~beam SomeStreamer")
                .AddCheck (SimpleCheckers.ManageServer ())
                .Parameter ("username",ParameterType.Unparsed)
                .Do (TrackStream (StreamNotificationConfig.StreamType.Beam));

            cgb.CreateCommand(Module.Prefix + "checkhitbox")
                .Alias(Module.Prefix + "chhb")
                .Description("Checkt ob ein bestimmter User auf gerade auf Hitbox streamt." +
                             " | ~chhb SomeStreamer")
                .Parameter("username", ParameterType.Unparsed)
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    var stream = e.GetArg("username")?.Trim();
                    if (string.IsNullOrWhiteSpace(stream))
                        return;
                    try
                    {
                        var streamStatus = (await GetStreamStatus(new StreamNotificationConfig
                        {
                            Username = stream,
                            Type = StreamNotificationConfig.StreamType.Hitbox
                        }));
                        if (streamStatus.Item1)
                        {
                            await e.Channel.SendMessage($"`Streamer {streamStatus.Item2} ist online.`");
                        }
                    }
                    catch
                    {
                        await e.Channel.SendMessage("Kein Channel gefunden.");
                    }
                });

            cgb.CreateCommand(Module.Prefix + "checktwitch")
                .Alias(Module.Prefix + "chtw")
                .Description("Checkt ob ein bestimmter User auf gerade auf Twitch streamt." +
                             " | ~chtw SomeStreamer")
                .AddCheck(SimpleCheckers.ManageServer())
                .Parameter("username", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var stream = e.GetArg("username")?.Trim();
                    if (string.IsNullOrWhiteSpace(stream))
                        return;
                    try
                    {
                        var streamStatus = (await GetStreamStatus(new StreamNotificationConfig
                        {
                            Username = stream,
                            Type = StreamNotificationConfig.StreamType.Twitch
                        }));
                        if (streamStatus.Item1)
                        {
                            await e.Channel.SendMessage($"`Streamer {streamStatus.Item2} ist online.`");
                        }
                    }
                    catch
                    {
                        await e.Channel.SendMessage("Kein Channel gefunden.");
                    }
                });

            cgb.CreateCommand(Module.Prefix + "checkbeam")
                .Alias(Module.Prefix + "chbm")
                .Description("Checkt ob ein bestimmter User auf gerade auf Beam streamt." +
                             " | ~chbm SomeStreamer")
                .AddCheck(SimpleCheckers.ManageServer())
                .Parameter("username", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var stream = e.GetArg("username")?.Trim();
                    if (string.IsNullOrWhiteSpace(stream))
                        return;
                    try
                    {
                        var streamStatus = (await GetStreamStatus(new StreamNotificationConfig
                        {
                            Username = stream,
                            Type = StreamNotificationConfig.StreamType.Beam
                        }));
                        if (streamStatus.Item1)
                        {
                            await e.Channel.SendMessage($"`Streamer {streamStatus.Item2} ist online.`");
                        }
                    }
                    catch
                    {
                        await e.Channel.SendMessage("Kein Channel gefunden.");
                    }
                });

            cgb.CreateCommand (Module.Prefix + "removestream")
                .Alias (Module.Prefix + "rms")
                .Description ("Entfernt Benachrichtigung eines bestimmten Streamers auf diesem Channel." +
                             " | ~rms SomeGuy")
                .AddCheck (SimpleCheckers.ManageServer ())
                .Parameter ("username",ParameterType.Unparsed)
                .Do (async e =>
                 {
                     var username = e.GetArg ("username")?.ToLower ().Trim ();
                     if (string.IsNullOrWhiteSpace (username))
                         return;

                     var config = SpecificConfigurations.Default.Of (e.Server.Id);

                     var toRemove = config.ObservingStreams
                        .FirstOrDefault (snc => snc.ChannelId == e.Channel.Id &&
                                         snc.Username.ToLower ().Trim () == username);
                     if (toRemove == null)
                     {
                         await e.Channel.SendMessage (":anger: Stream nicht vorhanden.").ConfigureAwait (false);
                         return;
                     }

                     config.ObservingStreams.Remove (toRemove);
                     await ConfigHandler.SaveConfig().ConfigureAwait(false);
                     await e.Channel.SendMessage ($":ok: `{toRemove.Username}`'s Stream von Benachrichtigungen entfernt.").ConfigureAwait (false);
                 });

            cgb.CreateCommand (Module.Prefix + "liststreams")
                .Alias (Module.Prefix + "ls")
                .Description ("Listet alle Streams die du auf diesem Server folgst." +
                             " | ~ls")
                .Do (async e =>
                 {
                     var config = SpecificConfigurations.Default.Of (e.Server.Id);

                     var streams = config.ObservingStreams.Where (snc =>
                                            snc.ServerId == e.Server.Id);

                     var streamsArray = streams as StreamNotificationConfig[] ?? streams.ToArray ();

                     if (streamsArray.Length == 0)
                     {
                         await e.Channel.SendMessage ("Du folgst keinem Stream auf diesem Server.").ConfigureAwait (false);
                         return;
                     }

                     var text = string.Join ("\n",streamsArray.Select (snc =>
                     {
                         try
                         {
                             return $"`{snc.Username}`'s Stream im Channel **{e.Server.GetChannel (e.Channel.Id).Name}**. 【`{snc.Type.ToString ()}`】";
                         }
                         catch { }
                         return "";
                     }));

                     await e.Channel.SendMessage ($"Du folgst **{streamsArray.Length}** Streams auf diesem Server.\n\n" + text).ConfigureAwait (false);
                 });
        }

        private Func<CommandEventArgs,Task> TrackStream ( StreamNotificationConfig.StreamType type ) =>
            async e =>
            {
                var username = e.GetArg ("username")?.ToLowerInvariant ();
                if (string.IsNullOrWhiteSpace (username))
                    return;

                var config = SpecificConfigurations.Default.Of (e.Server.Id);

                var stream = new StreamNotificationConfig
                {
                    ServerId = e.Server.Id,
                    ChannelId = e.Channel.Id,
                    Username = username,
                    Type = type,
                };
                var exists = config.ObservingStreams.Contains (stream);
                if (exists)
                {
                    await e.Channel.SendMessage (":anger: Ich benachrichtige diesen Channel schon über diesen Stream.").ConfigureAwait (false);
                }
                Tuple<bool,string> data;
                try
                {
                    data = await GetStreamStatus (stream).ConfigureAwait (false);
                }
                catch
                {
                    await e.Channel.SendMessage (":anger: Stream existiert wahrscheinlich nicht.").ConfigureAwait (false);
                    return;
                }
                var msg = $"Stream ist derzeit **{(data.Item1 ? "ONLINE" : "OFFLINE")}** mit **{data.Item2}** Zuschauern";
                if (data.Item1)
                    if (type == StreamNotificationConfig.StreamType.Hitbox)
                        msg += $"\n`Hier ist der Link:`【 http://www.hitbox.tv/{stream.Username}/ 】";
                    else if (type == StreamNotificationConfig.StreamType.Twitch)
                        msg += $"\n`Hier ist der Link:`【 http://www.twitch.tv/{stream.Username}/ 】";
                    else if (type == StreamNotificationConfig.StreamType.Beam)
                        msg += $"\n`Here is the Link:`【 https://beam.pro/{stream.Username}/ 】";
                    else if (type == StreamNotificationConfig.StreamType.YoutubeGaming)
                        msg += $"\n`Hier ist der Link:`【 Derzeit nicht implementiert - {stream.Username} 】";
                stream.LastStatus = data.Item1;
                if (!exists)
                    msg = $":ok: Ich werde diesen Channel benachrichtigen wenn sich der Status ändert.\n{msg}";
                await e.Channel.SendMessage (msg).ConfigureAwait (false);
                config.ObservingStreams.Add (stream);
            };
    }
}