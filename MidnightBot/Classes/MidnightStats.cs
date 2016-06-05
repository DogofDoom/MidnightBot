using Discord;
using Discord.Commands;
using MidnightBot.Extensions;
using MidnightBot.Modules.Administration.Commands;
using MidnightBot.Modules.Music;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace MidnightBot
{
    public class MidnightStats
    {
        public static MidnightStats Instance { get; } = new MidnightStats ();

        public string BotVersion => $"{Assembly.GetExecutingAssembly ().GetName ().Name} v{Assembly.GetExecutingAssembly ().GetName ().Version}";

        private int commandsRan = 0;
        private string statsCache = "";
        private readonly Stopwatch statsStopwatch = new Stopwatch ();

        public int ServerCount { get; private set; } = 0;
        public int TextChannelsCount { get; private set; } = 0;
        public int VoiceChannelsCount { get; private set; } = 0;

        private readonly Timer commandLogTimer = new Timer () { Interval = 10000 };
        private readonly Timer carbonStatusTimer = new Timer () { Interval = 3600000 };

        private static ulong messageCounter = 0;
        public static ulong MessageCounter => messageCounter;

        static MidnightStats () { }

        private MidnightStats ()
        {
            var commandService = MidnightBot.Client.GetService<CommandService> ();

            statsStopwatch.Start ();
            commandService.CommandExecuted += StatsCollector_RanCommand;

            Task.Run (StartCollecting);

            commandLogTimer.Start ();

            ServerCount = MidnightBot.Client.Servers.Count ();
            var channels = MidnightBot.Client.Servers.SelectMany (s => s.AllChannels);
            var channelsArray = channels as Channel[] ?? channels.ToArray ();
            TextChannelsCount = channelsArray.Count (c => c.Type == ChannelType.Text);
            VoiceChannelsCount = channelsArray.Count () - TextChannelsCount;

            MidnightBot.Client.MessageReceived += ( s,e ) => messageCounter++;

            MidnightBot.Client.JoinedServer += ( s,e ) =>
            {
                try
                {
                    ServerCount++;
                    TextChannelsCount += e.Server.TextChannels.Count ();
                    VoiceChannelsCount += e.Server.VoiceChannels.Count ();
                    //await SendUpdateToCarbon ().ConfigureAwait (false);
                }
                catch { }
            };
            MidnightBot.Client.LeftServer += ( s,e ) =>
            {
                try
                {
                    ServerCount--;
                    TextChannelsCount -= e.Server.TextChannels.Count ();
                    VoiceChannelsCount -= e.Server.VoiceChannels.Count ();
                    //await SendUpdateToCarbon ().ConfigureAwait (false);
                }
                catch { }
            };
            MidnightBot.Client.ChannelCreated += ( s,e ) =>
            {
                try
                {
                    if (e.Channel.IsPrivate)
                        return;
                    if (e.Channel.Type == ChannelType.Text)
                        TextChannelsCount++;
                    else if (e.Channel.Type == ChannelType.Voice)
                        VoiceChannelsCount++;
                }
                catch { }
            };
            MidnightBot.Client.ChannelDestroyed += ( s,e ) =>
            {
                try
                {
                    if (e.Channel.IsPrivate)
                        return;
                    if (e.Channel.Type == ChannelType.Text)
                        VoiceChannelsCount++;
                    else if (e.Channel.Type == ChannelType.Voice)
                        VoiceChannelsCount--;
                }
                catch { }
            };
            carbonStatusTimer.Elapsed += async ( s,e ) => await SendUpdateToCarbon ().ConfigureAwait (false);
            carbonStatusTimer.Start ();
        }

        HttpClient carbonClient = new HttpClient ();
        private async Task SendUpdateToCarbon ()
        {
            if (string.IsNullOrWhiteSpace (MidnightBot.Creds.CarbonKey))
                return;
            try
            {
                using (var content = new FormUrlEncodedContent (new Dictionary<string,string>
                {
                                { "servercount", MidnightBot.Client.Servers.Count().ToString() },
                                { "key", MidnightBot.Creds.CarbonKey }
                            }))
                {
                    content.Headers.Clear ();
                    content.Headers.Add ("Content-Type","application/x-www-form-urlencoded");

                    var res = await carbonClient.PostAsync ("https://www.carbonitex.net/discord/data/botdata.php",content).ConfigureAwait (false);
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Failed sending status update to carbon.");
                Console.WriteLine (ex);
            }
        }

        public TimeSpan GetUptime () =>
            DateTime.Now - Process.GetCurrentProcess ().StartTime;

        public string GetUptimeString ()
        {
            var time = GetUptime ();
            return time.Days + " Tage, " + time.Hours + " Stunden und " + time.Minutes + " Minuten.";
        }

        public Task LoadStats () =>
            Task.Run (() =>
             {
                 var songs = MusicModule.MusicPlayers.Count (mp => mp.Value.CurrentSong != null);
                 var sb = new System.Text.StringBuilder ();
                 sb.AppendLine ("`Autor: Midnight Myth` `Library: Discord.Net`");
                 sb.AppendLine ("`Basierend auf NadekoBot von Kwoth`");
                 sb.AppendLine ($"`Bot Version: {BotVersion}`");
                 sb.AppendLine ($"`Bot Id: {MidnightBot.Client.CurrentUser.Id}`");
                 sb.Append ("`Owners' Ids:` ");
                 sb.AppendLine ("`" + String.Join (", ",MidnightBot.Creds.OwnerIds) + "`");
                 sb.AppendLine ($"`Uptime: {GetUptimeString ()}`");
                 sb.Append ($"`Server: {ServerCount}");
                 sb.Append ($" | TextChannel: {TextChannelsCount}");
                 sb.AppendLine ($" | VoiceChannel: {VoiceChannelsCount}`");
                 sb.AppendLine ($"`Commands Ran this session: {commandsRan}`");
                 sb.AppendLine ($"`Message queue size: {MidnightBot.Client.MessageQueue.Count}`");
                 sb.Append ($"`Gegrüßt: {ServerGreetCommand.Greeted} mal.`");
                 sb.AppendLine ($" `| Spielt {songs} Lieder, ".SnPl (songs) +
                               $"{MusicModule.MusicPlayers.Sum (kvp => kvp.Value.Playlist.Count)} queued.`");
                 sb.AppendLine ($"`Nachrichten: {messageCounter} ({messageCounter / (double)GetUptime ().TotalSeconds:F2}/sec)`  `Heap: {Heap (false)}`");
                 statsCache = sb.ToString ();
             });

        public string Heap ( bool pass = true ) => Math.Round ((double)GC.GetTotalMemory (pass) / 1.MiB (),2).ToString ();

        public async Task<string> GetStats ()
        {
            if (statsStopwatch.Elapsed.Seconds < 4 &&
                !string.IsNullOrWhiteSpace (statsCache))
                return statsCache;
            await LoadStats ().ConfigureAwait (false);
            statsStopwatch.Restart ();
            return statsCache;
        }

        private async Task StartCollecting ()
        {
            while (true)
            {
                await Task.Delay (new TimeSpan (0,30,0)).ConfigureAwait (false);
                try
                {
                    var onlineUsers = await Task.Run (() => MidnightBot.Client.Servers.Sum (x => x.Users.Count ())).ConfigureAwait (false);
                    var realOnlineUsers = await Task.Run (() => MidnightBot.Client.Servers
                                                                         .Sum (x => x.Users.Count (u => u.Status == UserStatus.Online)))
                                                                         .ConfigureAwait (false);
                    var connectedServers = MidnightBot.Client.Servers.Count ();

                    Classes.DbHandler.Instance.InsertData (new DataModels.Stats
                    {
                        OnlineUsers = onlineUsers,
                        RealOnlineUsers = realOnlineUsers,
                        Uptime = GetUptime (),
                        ConnectedServers = connectedServers,
                        DateAdded = DateTime.Now
                    });
                }
                catch
                {
                    Console.WriteLine ("DB Exception in stats collecting.");
                    break;
                }
            }
        }

        private async void StatsCollector_RanCommand ( object sender,CommandEventArgs e )
        {
            Console.WriteLine ($">> Cmd: {e.Command.Text}\nMsg: {e.Message.Text}\nUsr: {e.User.Name} [{e.User.Id}]\nSrvr: {e.Server?.Name ?? "PRIVATE"} [{e.Server?.Id}]\n-----");
            await Task.Run (() =>
             {
                 try
                 {
                     commandsRan++;
                     Classes.DbHandler.Instance.InsertData (new DataModels.Command
                     {
                         ServerId = (long)(e.Server?.Id ?? 0),
                         ServerName = e.Server?.Name ?? "--Direct Message--",
                         ChannelId = (long)e.Channel.Id,
                         ChannelName = e.Channel.IsPrivate ? "--Direct Message" : e.Channel.Name,
                         UserId = (long)e.User.Id,
                         UserName = e.User.Name,
                         CommandName = e.Command.Text,
                         DateAdded = DateTime.Now
                     });
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine ("Probably unimportant error in ran command DB write.");
                     Console.WriteLine (ex);
                 }
             }).ConfigureAwait (false);
        }
    }
}