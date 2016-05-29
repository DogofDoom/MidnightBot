using Discord;
using Discord.Modules;
using MidnightBot.Extensions;
using MidnightBot.Modules.Music.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Sound
{
    class SoundsModule : DiscordModule
    {
        public static ConcurrentDictionary<Server,MusicPlayer> MusicPlayers = new ConcurrentDictionary<Server,MusicPlayer> ();
        public static ConcurrentDictionary<ulong,float> DefaultMusicVolumes = new ConcurrentDictionary<ulong,float> ();

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Searches;

        public override void Install ( ModuleManager manager )
        {
            var client = MidnightBot.Client;
            string path = Directory.GetCurrentDirectory ();

            manager.CreateCommands ("",cgb =>
            {
                var rng = new Random ();
                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand ("!patrick")
                     .Description ("Hier ist Patrick!")
                     .Do (async e =>
                     {
                         await QueueSong (e.Channel,e.User.VoiceChannel,path+@"\data\sounds\patrick.wav",musicType: MusicType.Local).ConfigureAwait (false);
                     });

                cgb.CreateCommand ("!rage")
                     .Description ("Das ist jetzt nicht euer Ernst!")
                     .Do (async e =>
                     {
                         await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\rage.wav",musicType: MusicType.Local).ConfigureAwait (false);
                     });

                cgb.CreateCommand ("!airhorn")
                    .Description ("Airhorn!")
                    .Do (async e =>
                    {
                        var sound = rng.Next (1,15);
                        switch (sound)
                        {
                            case 1:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_clownfull.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 2:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_clownshort.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 3:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_clownspam.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 4:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_default.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 5:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_distant.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 6:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_echo.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 7:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_fourtap.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 8:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_highfartlong.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 9:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_highfartshort.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 10:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_midshort.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 11:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_reverb.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 12:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_spam.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 13:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_tripletap.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 14:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\airhorn_truck.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            default:
                                break;
                        }
                    });

                cgb.CreateCommand ("!johncena")
                    .Alias ("!cena")
                    .Description ("JOHN CENA!")
                    .Do (async e =>
                    {
                        var sound = rng.Next (1,7);
                        switch (sound)
                        {
                            case 1:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_airhorn.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 2:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_echo.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 3:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_full.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 4:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_jc.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 5:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_nameis.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 6:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\jc_spam.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            default:
                                break;
                        }
                    });

                cgb.CreateCommand ("!e")
                    .Alias ("!ethanbradberry")
                    .Alias ("!h3h3")
                    .Description ("Ethanbradberry!")
                    .Do (async e =>
                    {
                        var sound = rng.Next (1,12);
                        switch (sound)
                        {
                            case 1:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_areyou_classic.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 2:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_areyou_condensed.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 3:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_areyou_crazy.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 4:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_areyou_ethan.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 5:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_beat.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 6:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_classic.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 7:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_cuts.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 8:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_echo.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 9:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_high.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 10:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_slowandlow.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 11:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\ethan_sodiepop.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            default:
                                break;
                        }
                    });

                cgb.CreateCommand ("!anotha")
                    .Alias ("!anothaone")
                    .Description ("Anothaone")
                    .Do (async e =>
                    {
                        var sound = rng.Next (1,4);
                        switch (sound)
                        {
                            case 1:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\another_one.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 2:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\another_one_classic.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            case 3:
                                await QueueSong (e.Channel,e.User.VoiceChannel,path + @"\data\sounds\another_one_echo.wav",musicType: MusicType.Local).ConfigureAwait (false);
                                break;
                            default:
                                break;
                        }
                    });
            });
        }
        private async Task QueueSong ( Channel textCh,Channel voiceCh,string query,bool silent = false,MusicType musicType = MusicType.Normal )
        {
            if (voiceCh == null || voiceCh.Server != textCh.Server)
            {
                if (!silent)
                    return;
                throw new ArgumentNullException (nameof (voiceCh));
            }
            if (string.IsNullOrWhiteSpace (query) || query.Length < 3)
                return;

            var musicPlayer = MusicPlayers.GetOrAdd (textCh.Server,server =>
            {
                float? vol = null;
                float throwAway;
                if (DefaultMusicVolumes.TryGetValue (server.Id,out throwAway))
                    vol = throwAway;
                var mp = new MusicPlayer (voiceCh,vol);
                mp.OnCompleted += async ( s,song ) =>
                {
                    try
                    {
                        await Task.Run (() =>
                        {
                            if (!MusicPlayers.TryRemove (textCh.Server,out mp))
                                return;
                            mp.Destroy ();
                        });
                    }
                    catch { }
                };
                return mp;
            });
            var resolvedSong = await Song.ResolveSong (query,musicType);
            resolvedSong.MusicPlayer = musicPlayer;
            musicPlayer.AddSong (resolvedSong);
        }
    }
}