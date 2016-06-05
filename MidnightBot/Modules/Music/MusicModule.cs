using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Extensions;
using MidnightBot.Modules.Music.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Music
{
    internal class MusicModule : DiscordModule
    {

        public static ConcurrentDictionary<Server,MusicPlayer> MusicPlayers = new ConcurrentDictionary<Server,MusicPlayer> ();
        public static ConcurrentDictionary<ulong,float> DefaultMusicVolumes = new ConcurrentDictionary<ulong,float> ();

        public MusicModule ()
        {

            // ready for 1.0
            //MidnightBot.Client.UserUpdated += (s, e) =>
            //{
            //    try
            //    {
            //        if (e.Before.VoiceChannel != e.After.VoiceChannel &&
            //           e.Before.VoiceChannel.Members.Count() == 0)
            //        {
            //            MusicPlayer musicPlayer;
            //            if (!MusicPlayers.TryRemove(e.Server, out musicPlayer)) return;
            //            musicPlayer.Destroy();
            //        }
            //    }
            //    catch { }
            //};

        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Music;

        public override void Install ( ModuleManager manager )
        {
            var client = MidnightBot.Client;

            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "n")
                    .Alias (Prefix + "next")
                    .Alias (Prefix + "skip")
                    .Description ("Geht zum nächsten Song in der Liste. Du musst im gleichen Voice-Channel wie der Bot sein.\n**Benutzung**: `!n`")
                    .Do (e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (musicPlayer.PlaybackVoiceChannel == e.User.VoiceChannel)
                             musicPlayer.Next ();
                     });

                cgb.CreateCommand (Prefix + "s")
                    .Alias (Prefix + "stop")
                    .Description ("Stoppt die Musik komplett. Bleibt im Channel.Du musst im gleichen Voice-Channel wie der Bot sein.\n**Benutzung**: `!s`")
                    .Do (async e =>
                     {
                         await Task.Run (() =>
                         {
                             MusicPlayer musicPlayer;
                             if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                                 return;
                             if (e.User.VoiceChannel == musicPlayer.PlaybackVoiceChannel)
                                 musicPlayer.Stop ();
                         }).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "d")
                    .Alias (Prefix + "destroy")
                    .Description ("Stoppt die Musik komplett.")
                    .Do (async e =>
                     {
                         await Task.Run (() =>
                         {
                             MusicPlayer musicPlayer;
                             if (!MusicPlayers.TryRemove (e.Server,out musicPlayer))
                                 return;
                             musicPlayer.Destroy ();
                         }).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "p")
                    .Alias (Prefix + "pause")
                    .Description ("Pausiert, oder unpausiert ein Lied.")
                    .Do (async e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         musicPlayer.TogglePause ();
                         if (musicPlayer.Paused)
                             await e.Channel.SendMessage ("🎵`Musik pausiert.`").ConfigureAwait (false);
                         else
                             await e.Channel.SendMessage ("🎵`Musik wieder gestartet.`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "q")
                    .Alias (Prefix + "yq")
                    .Alias (Prefix + "songrequest")
                    .Description ("Listet einen Song mit Keyword oder Link. Bot joint dem eigenen Voice-Channel. **Du musst in einem Voice-Channel sein!**.\n**Benutzung**: `!q Dream Of Venice`")
                    .Parameter ("query",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         await QueueSong (e.Channel,e.User.VoiceChannel,e.GetArg ("query")).ConfigureAwait (false);
                         if (e.Server.CurrentUser.GetPermissions (e.Channel).ManageMessages)
                         {
                             await Task.Delay (10000).ConfigureAwait (false);
                             await e.Message.Delete ().ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "lq")
                    .Alias (Prefix + "ls").Alias (Prefix + "lp")
                    .Description ("Zeigt bis zu 15 Songs die zurzeit in der Liste sind.")
                    .Do (async e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                         {
                             await e.Channel.SendMessage ("🎵 Kein aktiver Musik-Player.").ConfigureAwait (false);
                             return;
                         }
                         var currentSong = musicPlayer.CurrentSong;
                         if (currentSong == null)
                             return;
                         var toSend = $"🎵`Derzeit läuft: ` {currentSong.PrettyName} " + $"{currentSong.PrettyCurrentTime ()}\n";
                         if (musicPlayer.RepeatSong)
                             toSend += "🔂";
                         else if (musicPlayer.RepeatPlaylist)
                             toSend += "🔁";
                         toSend += $" **{musicPlayer.Playlist.Count}** `Lieder, derzeit gelistet.` ";
                         if (musicPlayer.Playlist.Count >= MusicPlayer.MaximumPlaylistSize)
                             toSend += "**Song Liste ist voll!**\n";
                         else
                             toSend += "\n";
                         var number = 1;
                         await e.Channel.SendMessage (toSend + string.Join ("\n",musicPlayer.Playlist.Take (15).Select (v => $"`{number++}.` {v.PrettyName}"))).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "np")
                    .Alias (Prefix + "playing")
                    .Description ("Zeigt den derzeit spielenden Song.")
                    .Do (async e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         var currentSong = musicPlayer.CurrentSong;
                         if (currentSong != null)
                             return;
                         await e.Channel.SendMessage ($"🎵`Derzeit wird gespielt:` {currentSong.PrettyName} " +
                             $"{currentSong.PrettyCurrentTime ()}").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "vol")
                    .Description ("Setzt die Lautstärke auf 0-100%")
                    .Parameter ("val",ParameterType.Required)
                    .Do (async e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         var arg = e.GetArg ("val");
                         int volume;
                         if (!int.TryParse (arg,out volume))
                         {
                             await e.Channel.SendMessage ("Lautstärke ungültig.").ConfigureAwait (false);
                             return;
                         }
                         volume = musicPlayer.SetVolume (volume);
                         await e.Channel.SendMessage ($"🎵 `Lautstärke auf {volume}% gesetzt.`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "dv")
                    .Alias (Prefix + "defvol")
                    .Description ("Setzt die Standardlautstärke, wenn Musik startet. (0-100). Muss nach neustart neu eingestellt werden.\n**Benutzung**: !dv 80")
                    .Parameter ("val",ParameterType.Required)
                    .Do (async e =>
                     {
                         var arg = e.GetArg ("val");
                         float volume;
                         if (!float.TryParse (arg,out volume) || volume < 0 || volume > 100)
                         {
                             await e.Channel.SendMessage ("Lautstärke ungültig.").ConfigureAwait (false);
                             return;
                         }
                         DefaultMusicVolumes.AddOrUpdate (e.Server.Id,volume / 100,( key,newval ) => volume / 100);
                         await e.Channel.SendMessage ($"🎵 `Standardlautstärke auf {volume}% gesetzt.`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "min").Alias (Prefix + "mute")
                    .Description ("Setzt die Lautstärke auf 0%")
                    .Do (e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         musicPlayer.SetVolume (0);
                     });

                cgb.CreateCommand (Prefix + "max")
                    .Description ("Setzt de Lautstärke auf 100% (Echte Höchstgrenze ist bei 150%).")
                    .Do (e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         musicPlayer.SetVolume (100);
                     });

                cgb.CreateCommand (Prefix + "half")
                    .Description ("Setzt die Lautstärke auf 50%.")
                    .Do (e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         musicPlayer.SetVolume (50);
                     });

                cgb.CreateCommand (Prefix + "sh")
                    .Description ("Mischt die derzeitiege Abspielliste.")
                    .Do (async e =>
                     {
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         if (musicPlayer.Playlist.Count < 2)
                         {
                             await e.Channel.SendMessage ("💢 Nicht genug Lieder um zu mischen.").ConfigureAwait (false);
                             return;
                         }

                         musicPlayer.Shuffle ();
                         await e.Channel.SendMessage ("🎵 `Lieder gemischt.`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "setgame")
                    .Description ("Setzt das Spiel auf die Nummer der Lieder die gespielt werden. **Owner Only!**")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         await e.Channel.SendMessage ("❗This command is deprecated. " +
                                                     "Use:\n `.ropl`\n `.adpl %playing% songs, %queued% queued.` instead.\n " +
                                                     "It even persists through restarts.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "pl")
                .Alias (Prefix + "playlistrequest")
                    .Description ("Listet bis zu 50 Lieder aus einer Youtubeplaylist, oder aus einem Suchbegriff.")
                    .Parameter ("playlist",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         var arg = e.GetArg ("playlist");
                         if (string.IsNullOrWhiteSpace (arg))
                             return;
                         if (e.User.VoiceChannel?.Server != e.Server)
                         {
                             await e.Channel.SendMessage ("💢 Du musst auf einem Voice Channel dieses Servers sein.").ConfigureAwait (false);
                             return;
                         }
                         var plId = await SearchHelper.GetPlaylistIdByKeyword (arg).ConfigureAwait (false);
                         if (plId == null)
                         {
                             await e.Channel.SendMessage ("Keine Suchergebnisse für diesen Begriff.");
                             return;
                         }
                         var ids = await SearchHelper.GetVideoIDs (plId,500).ConfigureAwait (false);
                         if (ids == null || ids.Count == 0)
                         {
                             await e.Channel.SendMessage ($"🎵`Keine Lieder gefunden.`");
                             return;
                         }
                         var idArray = ids as string[] ?? ids.ToArray ();
                         var count = idArray.Length;
                         var msg =
                             await e.Channel.SendMessage ($"🎵 `Versuche **{count}** Lieder zu listen".SnPl (count) + "...`").ConfigureAwait (false);
                         foreach (var id in idArray)
                         {
                             try
                             {
                                 await QueueSong (e.Channel,e.User.VoiceChannel,id,true).ConfigureAwait (false);
                             }
                             catch { }
                         }
                         await msg.Edit ("🎵 `Playlist Listung erledigt.`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "lopl")
                    .Description ("Listet alle Lieder von einem Verzeichnis. **Owner Only!**")
                    .Parameter ("directory",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         var arg = e.GetArg ("directory");
                         if (string.IsNullOrWhiteSpace (arg))
                             return;
                         try
                         {
                             var fileEnum = new DirectoryInfo (arg).GetFiles ()
                                                .Where (x => !x.Attributes.HasFlag (FileAttributes.Hidden | FileAttributes.System));
                             foreach (var file in fileEnum)
                             {
                                 await QueueSong (e.Channel,e.User.VoiceChannel,file.FullName,true,MusicType.Local).ConfigureAwait (false);
                             }
                             await e.Channel.SendMessage ("🎵 `Verzeichnis-Listung komplett.`").ConfigureAwait (false);
                         }
                         catch { }
                     });

                cgb.CreateCommand (Prefix + "radio").Alias (Prefix + "ra")
                    .Description ("Listet einen direkten Radio Stream von einem Link.")
                    .Parameter ("radio_link",ParameterType.Required)
                    .Do (async e =>
                     {
                         if (e.User.VoiceChannel?.Server != e.Server)
                         {
                             await e.Channel.SendMessage ("💢 Du musst in einem Voice-Channel auf diesem Server sein.").ConfigureAwait (false);
                             return;
                         }
                         await QueueSong (e.Channel,e.User.VoiceChannel,e.GetArg ("radio_link"),musicType: MusicType.Radio).ConfigureAwait (false);
                         if (e.Server.CurrentUser.GetPermissions (e.Channel).ManageMessages)
                         {
                             await Task.Delay (10000).ConfigureAwait (false);
                             await e.Message.Delete ().ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "lo")
                    .Description ("Listet einen lokalen Song mit vollen Pfad. **Owner Only!**\n**Benutzung**: `!lo C:/music/mysong.mp3`")
                    .Parameter ("path",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         var arg = e.GetArg ("path");
                         if (string.IsNullOrWhiteSpace (arg))
                             return;
                         await QueueSong (e.Channel,e.User.VoiceChannel,e.GetArg ("path"),musicType: MusicType.Local).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "mv")
                    .Description ("Verschiebt den Bot in den eigenen Voice-Channel. (Funktioniert nur, wenn schon Musik läuft)")
                    .Do (e =>
                     {
                         MusicPlayer musicPlayer;
                         var voiceChannel = e.User.VoiceChannel;
                         if (voiceChannel == null || voiceChannel.Server != e.Server || !MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                             return;
                         musicPlayer.MoveToVoiceChannel (voiceChannel);
                     });

                cgb.CreateCommand (Prefix + "rm")
                    .Description ("Entfernt einen Song mit seiner Id, oder 'all' um die komplette Liste zu löschen.")
                    .Parameter ("num",ParameterType.Required)
                    .Do (async e =>
                     {
                         var arg = e.GetArg ("num");
                         MusicPlayer musicPlayer;
                         if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                         {
                             return;
                         }
                         if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                             return;
                         if (arg?.ToLower () == "all")
                         {
                             musicPlayer.ClearQueue ();
                             await e.Channel.SendMessage ($"🎵`Playlist gelöscht!`").ConfigureAwait (false);
                             return;
                         }
                         int num;
                         if (!int.TryParse (arg,out num))
                         {
                             return;
                         }
                         if (num <= 0 || num > musicPlayer.Playlist.Count)
                             return;

                         var song = (musicPlayer.Playlist as List<Song>)?[num - 1];
                         musicPlayer.RemoveSongAt (num - 1);
                         await e.Channel.SendMessage ($"🎵**Lied {song.PrettyName} auf Position `{num}` wurde entfernt.**").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "cleanup")
                    .Description ("Bereinigt hängende Voice-Verbindung. **Owner Only!**")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (e =>
                     {
                         foreach (var kvp in MusicPlayers)
                         {
                             var songs = kvp.Value.Playlist;
                             var currentSong = kvp.Value.CurrentSong;
                             if (songs.Count == 0 && currentSong == null)
                             {
                                 MusicPlayer throwaway;
                                 MusicPlayers.TryRemove (kvp.Key,out throwaway);
                                 throwaway.Destroy ();
                             }
                         }
                     });

                cgb.CreateCommand (Prefix + "rcs")
                    .Alias (Prefix + "repeatcurrentsong")
                    .Description ("Schaltet das Wiederholen des derzeitigen Liedes um.")
                    .Do (async e =>
                    {
                        MusicPlayer musicPlayer;
                        if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                            return;
                        if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                            return;
                        var currentSong = musicPlayer.CurrentSong;
                        if (currentSong == null)
                            return;
                        var currentValue = musicPlayer.ToggleRepeatSong ();
                        await e.Channel.SendMessage (currentValue ?
                        $"🎵🔂 `Wiederhole Lied:`{currentSong.PrettyName}" :
                        $"🎵🔂 `Derzeitige Liedwiederholung angehalten.`")
                        .ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "rpl")
                    .Alias (Prefix + "repeatplaylist")
                    .Description ("Schaltet das Wiederholen aller Songs in der Liste um. (Jedes beendete Lied wird an das Ende der Liste hinzugefügt).")
                    .Do (async e =>
                    {
                        MusicPlayer musicPlayer;
                        if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                            return;
                        if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                            return;
                        var currentValue = musicPlayer.ToggleRepeatPlaylist ();
                        await e.Channel.SendMessage ($"🎵🔁`Playlist Wiederholung {(currentValue ? "aktiviert" : "deaktiviert")}`")
                        .ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "save")
                    .Description ("Speichert eine Playlist unter einem bestimmten Namen. Name darf nicht länger als 20 Zeichen sein und darf keine Kommas beinhalten.\n**Benutzung**: `!save classical1`")
                    .Parameter ("name",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var name = e.GetArg ("name")?.Trim ();

                        if (string.IsNullOrWhiteSpace (name) ||
                                    name.Length > 20 ||
                                    name.Contains ("-"))
                            return;

                        MusicPlayer musicPlayer;
                        if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                            return;

                        //to avoid concurrency issues
                        var currentPlaylist = new List<Song> (musicPlayer.Playlist);
                        var curSong = musicPlayer.CurrentSong;
                        if (curSong != null)
                            currentPlaylist.Insert (0,curSong);

                        if (!currentPlaylist.Any ())
                            return;

                        var songInfos = currentPlaylist.Select (s => new DataModels.SongInfo

                        {
                            Provider = s.SongInfo.Provider,
                            ProviderType = (int)s.SongInfo.ProviderType,
                            Title = s.SongInfo.Title,
                            Uri = s.SongInfo.Uri,
                            Query = s.SongInfo.Query,
                        }).ToList ();

                        var playlist = new MusicPlaylist
                        {
                            CreatorId = (long)e.User.Id,
                            CreatorName = e.User.Name,
                            Name = name.ToLowerInvariant (),
                        };

                        DbHandler.Instance.SaveAll (songInfos);
                        DbHandler.Instance.Save (playlist);
                        DbHandler.Instance.InsertMany (songInfos.Select (s => new PlaylistSongInfo
                        {
                            PlaylistId = playlist.Id.Value,
                            SongInfoId = s.Id.Value
                        }));

                        await e.Channel.SendMessage ($"🎵 `Playlist gespeichert als {name}-{playlist.Id}`").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "load")
                    .Description ("Lädt eine Playlist mit bestimmten Namen.\n**Benutzung**: `!load classical1`")
                    .Parameter ("name",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var voiceCh = e.User.VoiceChannel;
                        var textCh = e.Channel;
                        if (voiceCh == null || voiceCh.Server != textCh.Server)
                        {
                            await textCh.SendMessage ("💢 Du musst in einem Voice Channel auf diesem Server sein.\n Falls du bereits in einem Voice Channel bist, versuche es mit neu joinen.").ConfigureAwait (false);
                            return;
                        }
                        var name = e.GetArg ("name")?.Trim ().ToLowerInvariant ();

                        if (string.IsNullOrWhiteSpace (name))
                            return;

                        var parts = name.Split ('-');
                        if (parts.Length != 2)
                            return;
                        var playlistName = parts[0];

                        int playlistNumber;
                        if (!int.TryParse (parts[1],out playlistNumber))
                            return;

                        var playlist = DbHandler.Instance.FindOne<MusicPlaylist> (
                        p => p.Id == playlistNumber);

                        if (playlist == null)
                        {
                            await e.Channel.SendMessage ("Kann keine Playlist mit diesem Namen finden.").ConfigureAwait (false);
                            return;
                        }

                        var psis = DbHandler.Instance.FindAll<PlaylistSongInfo> (psi =>
                        psi.PlaylistId == playlist.Id);

                        var songInfos = psis.Select (psi => DbHandler.Instance
                            .FindOne<DataModels.SongInfo> (si => si.Id == psi.SongInfoId));

                        await e.Channel.SendMessage ($"`Versuche {songInfos.Count ()} Lieder zu laden.`").ConfigureAwait (false);
                        foreach (var si in songInfos)
                        {
                            try
                            {
                                await QueueSong (textCh,voiceCh,si.Query,true,(MusicType)si.ProviderType).ConfigureAwait (false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine ($"Failed QueueSong in load playlist. {ex}");
                            }
                        }
                    });

                 cgb.CreateCommand(Prefix + "playlists")
                    .Alias(Prefix + "pls")
                    .Description("Listet alle Playlisten. Seitenweiße. 20 je Seote. Standard-Seite ist 0.\n**Benutzung**:`!pls 1`")
                    .Parameter("num", ParameterType.Optional)
                    .Do(e =>
                    {
                        int num = 0;
                        int.TryParse(e.GetArg("num"), out num);
                        if (num < 0)
                            return;
                        var result = DbHandler.Instance.GetPlaylistData(num);
                        if (result.Count == 0)
                            e.Channel.SendMessage($"`Keine gespeicherten Playlisten auf Seite {num} gefunden`");
                        else
                            e.Channel.SendMessage($"```js\n--- Liste der gespeicherten Playlisten ---\n\n" + string.Join("\n", result.Select(r => $"'{r.Name}-{r.Id}' von {r.Creator} ({r.SongCnt} Lieder)")) + $"\n\n        --- Seite {num} ---```");
                    });

                cgb.CreateCommand (Prefix + "goto")
                    .Description ("Skipped zu einer bestimmten Zeit in Sekunden im aktuellen Lied.")
                    .Parameter ("time")
                    .Do (async e =>
                    {
                        var skipToStr = e.GetArg ("time")?.Trim ();
                        MusicPlayer musicPlayer;
                        if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                            return;

                        if (e.User.VoiceChannel != musicPlayer.PlaybackVoiceChannel)
                            return;
                        int skipTo;
                        if (!int.TryParse (skipToStr,out skipTo) || skipTo < 0)
                            return;

                        var currentSong = musicPlayer.CurrentSong;

                        if (currentSong == null)
                            return;

                        //currentSong.PrintStatusMessage = false;
                        var gotoSong = currentSong.Clone ();
                        gotoSong.SkipTo = skipTo;
                        musicPlayer.AddSong (gotoSong,0);
                        musicPlayer.Next ();

                        var minutes = (skipTo / 60).ToString ();
                        var seconds = (skipTo % 60).ToString ();

                        if (minutes.Length == 1)
                            minutes = "0" + minutes;
                        if (seconds.Length == 1)
                            seconds = "0" + seconds;

                        await e.Channel.SendMessage ($"`Auf {minutes}:{seconds} geskipped`").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "getlink")
                   .Alias (Prefix + "gl")
                   .Description ("Zeigt einen Link zum derzeitigen Lied.")
                   .Do (async e =>
                   {
                       MusicPlayer musicPlayer;
                       if (!MusicPlayers.TryGetValue (e.Server,out musicPlayer))
                           return;
                       var curSong = musicPlayer.CurrentSong;
                       if (curSong == null)
                           return;
                       await e.Channel.SendMessage ($"🎶`Derzeitiges Lied:` <{curSong.SongInfo.Query}>");
                   });
            });
        }

        private async Task QueueSong ( Channel textCh,Channel voiceCh,string query,bool silent = false,MusicType musicType = MusicType.Normal )
        {
            if (voiceCh == null || voiceCh.Server != textCh.Server)
            {
                if (!silent)
                    await textCh.SendMessage ("💢 Du musst in einem Voice-Channel auf diesem Server sein.").ConfigureAwait (false);
                throw new ArgumentNullException (nameof (voiceCh));
            }
            if (string.IsNullOrWhiteSpace (query) || query.Length < 3)
                throw new ArgumentException ("💢 Falsche Angabe zur Songlistung.",nameof (query));

            var musicPlayer = MusicPlayers.GetOrAdd (textCh.Server,server =>
            {
                float? vol = null;
                float throwAway;
                if (DefaultMusicVolumes.TryGetValue (server.Id,out throwAway))
                    vol = throwAway;
                var mp = new MusicPlayer (voiceCh,vol);


                Message playingMessage = null;
                Message lastFinishedMessage = null;
                mp.OnCompleted += async ( s,song ) =>
                {
                    if (song.PrintStatusMessage)
                    {
                        try
                        {
                            if (lastFinishedMessage != null)
                                await lastFinishedMessage.Delete ().ConfigureAwait (false);
                            if (playingMessage != null)
                                await playingMessage.Delete ().ConfigureAwait (false);
                            lastFinishedMessage = await textCh.SendMessage ($"🎵`Finished`{song.PrettyName}").ConfigureAwait (false);
                        }
                        catch { }
                    }
                    
                };
                mp.OnStarted += async ( s,song ) =>
                {
                    if (song.PrintStatusMessage)
                    {
                        var sender = s as MusicPlayer;
                        if (sender == null)
                            return;

                        try
                        {

                            var msgTxt = $"🎵`Playing`{song.PrettyName} `Vol: {(int)(sender.Volume * 100)}%`";
                            playingMessage = await textCh.SendMessage (msgTxt).ConfigureAwait (false);
                        }
                        catch { }
                    }
                };
                return mp;
            });
            var resolvedSong = await Song.ResolveSong (query,musicType);
            resolvedSong.MusicPlayer = musicPlayer;

            musicPlayer.AddSong (resolvedSong);
            if (!silent)
            {
                var queuedMessage = await textCh.SendMessage ($"🎵`Queued`{resolvedSong.PrettyName} **at** `#{musicPlayer.Playlist.Count}`").ConfigureAwait (false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run (async () =>
                                {
                                    await Task.Delay (10000).ConfigureAwait (false);
                                    try
                                    {
                                        await queuedMessage.Delete ().ConfigureAwait (false);
                                    }
                                    catch { }
                                }).ConfigureAwait (false);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }
}