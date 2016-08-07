using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes.Help.Commands;
using MidnightBot.Classes.JSONModels;
using MidnightBot.Modules.Administration;
using MidnightBot.Modules.ClashOfClans;
using MidnightBot.Modules.Conversations;
using MidnightBot.Modules.CustomReactions;
using MidnightBot.Modules.Extra;
using MidnightBot.Modules.Gambling;
using MidnightBot.Modules.Games;
using MidnightBot.Modules.Games.Commands;
using MidnightBot.Modules.Help;
using MidnightBot.Modules.Level;
using MidnightBot.Modules.Meme;
using MidnightBot.Modules.Music;
using MidnightBot.Modules.NSFW;
using MidnightBot.Modules.Permissions;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Pokemon;
using MidnightBot.Modules.Searches;
using MidnightBot.Modules.Sound;
using MidnightBot.Modules.Translator;
using MidnightBot.Modules.Trello;
using MidnightBot.Modules.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightBot
{
    public class MidnightBot
    {
        public static DiscordClient Client { get; private set; }
        public static Credentials Creds { get; set; }
        public static Configuration Config { get; set; }
        public static LocalizedStrings Locale { get; set; } = new LocalizedStrings ();
        public static string BotMention { get; set; } = "";
        public static bool Ready { get; set; } = false;
        public static Action OnReady { get; set; } = delegate { };

        private static List<Channel> OwnerPrivateChannels { get; set; }

        public static String GetRndGoogleAPIKey ()
        {
            Random rnd = new Random ();
            int i = rnd.Next (0,Creds.GoogleAPIKey.Length);
            return Creds.GoogleAPIKey[i];
        }

    private static void Main ()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.Title = "Midnight Bot | Starting...";
            
            try
            {
                List<_Models.JSONModels.APIConfig> apis = new List<_Models.JSONModels.APIConfig>() {
                    {new _Models.JSONModels.APIConfig
                    {
                        Names = new List<string>() {"randomcat", "meow" },
                        QueryReplacements = new Dictionary<string, string>() { { @".*", "" } },
                        URL = "http://www.random.cat/meow",
                        ResponseHandling = Tuple.Create("JSON", "file"),
                        URLSuffix = "",
                        AllowEmpty = true

                    } },
                    {new _Models.JSONModels.APIConfig
                    {
                        Names = new List<string>() {"i", "image" },

                    } }
                };
                File.WriteAllText("data/apis.json", JsonConvert.SerializeObject(apis, Formatting.Indented));
                File.WriteAllText ("data/config_example.json",JsonConvert.SerializeObject (new Configuration(), Formatting.Indented));
                if (!File.Exists ("data/config.json"))
                    File.Copy ("data/config_example.json","data/config.json");
                File.WriteAllText ("credentials_example.json",JsonConvert.SerializeObject (new Credentials (),Formatting.Indented));
            }
            catch
            {
                Console.WriteLine ("Failed writing credentials_example.json or data/config_example.json");
            }

            try
            {
                Config = JsonConvert.DeserializeObject<Configuration> (File.ReadAllText ("data/config.json",Encoding.UTF8));
                Config.Quotes = JsonConvert.DeserializeObject<List<Quote>> (File.ReadAllText ("data/quotes.json"));
                Config.PokemonTypes = JsonConvert.DeserializeObject<List<PokemonType>> (File.ReadAllText ("data/PokemonTypes.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Einstellungen konnten nicht geladen werden.");
                Console.WriteLine (ex);
                Console.ReadKey ();
                return;
            }

            try
            {
                //load credentials from credentials.json
                Creds = JsonConvert.DeserializeObject<Credentials> (File.ReadAllText ("credentials.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine ($"Failed to load stuff from credentials.json, RTFM\n{ex.Message}");
                Console.ReadKey ();
                return;
            }

            //if password is not entered, prompt for password
            if (string.IsNullOrWhiteSpace (Creds.Token))
            {
                Console.WriteLine ("Kein Token gesetzt. Bitte gib deinen Bot-Token ein:\n");
                Creds.Token = Console.ReadLine ();
            }

            Console.WriteLine (string.IsNullOrWhiteSpace (GetRndGoogleAPIKey ())
                ? "Kein Google Api Key gefunden. Du kannst keine Musik benutzen und Links werden nicht gekürzt."
                : "Google API key vorhanden.");
            Console.WriteLine (string.IsNullOrWhiteSpace (Creds.TrelloAppKey)
                ? "Kein trello appkey gefunden. Du kannst keine trello Befehle benutzen."
                : "Trello app key vorhanden.");
            Console.WriteLine (Config.ForwardMessages != true
                ? "Keine Weiterleitung von Nachrichten."
                : "Nachrichten werden weitergeleitet.");
            Console.WriteLine (string.IsNullOrWhiteSpace (Creds.SoundCloudClientID)
                ? "Keine Soundcloud Client ID gefunden. Soundcloud Streaming ist deaktiviert."
                : "SoundCloud Streaming aktiviert.");
            Console.WriteLine (string.IsNullOrWhiteSpace (Creds.OsuAPIKey)
                ? "Kein osu! API Key gefunden. Song & Top Score Suche werden nicht funktionieren. Benutzer Suche ist verwendbar."
                : "osu! API Key gefunden.");

            BotMention = $"<@{Creds.BotId}>";

            //create new discord client and log
            Client = new DiscordClient (new DiscordConfigBuilder ()
            {
                MessageCacheSize = 10,
                ConnectionTimeout = 120000,
                LogLevel = LogSeverity.Warning,
                LogHandler = ( s,e ) =>
                    Console.WriteLine ($"Severity: {e.Severity}" +
                                      $"Message: {e.Message}" +
                                      $"ExceptionMessage: {e.Exception?.Message ?? "-"}"),
            });

            //create a command service
            var commandService = new CommandService (new CommandServiceConfigBuilder
            {
                AllowMentionPrefix = false,
                CustomPrefixHandler = m => 0,
                HelpMode = HelpMode.Disabled,
                ErrorHandler = async ( s,e ) =>
                {
                    if (e.ErrorType != CommandErrorType.BadPermissions)
                        return;
                    if (string.IsNullOrWhiteSpace (e.Exception?.Message))
                        return;
                    try
                    {
                        await e.Channel.SendMessage (e.Exception.Message).ConfigureAwait (false);
                    }
                    catch { }
                }
            });

            //reply to personal messages and forward if enabled.
            Client.MessageReceived += Client_MessageReceived;

            //add command service
            Client.AddService<CommandService> (commandService);

            //create module service
            var modules = Client.AddService<ModuleService> (new ModuleService ());

            //add audio service
            Client.AddService<AudioService> (new AudioService (new AudioServiceConfigBuilder ()
            {
                Channels = 2,
                EnableEncryption = false,
                Bitrate = 128,
            }));

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                MidnightBot.shutdownGracefully();
            };

            //install modules
            modules.Add (new HelpModule (),"Help",ModuleFilter.None);
            modules.Add (new AdministrationModule (),"Administration",ModuleFilter.None);
            modules.Add (new UtilityModule (),"Utility",ModuleFilter.None);
            modules.Add (new PermissionModule (),"Permissions",ModuleFilter.None);
            modules.Add (new Conversations (),"Conversations",ModuleFilter.None);
            modules.Add (new GamblingModule (),"Gambling",ModuleFilter.None);
            modules.Add (new GamesModule (),"Games",ModuleFilter.None);
            modules.Add(new LevelModule(), "Level", ModuleFilter.None);
            modules.Add (new MusicModule(), "Music", ModuleFilter.None);
            modules.Add (new SearchesModule (),"Searches",ModuleFilter.None);
            modules.Add (new ExtraModule (),"Extra",ModuleFilter.None);
            modules.Add (new PokemonModule (),"Pokegame",ModuleFilter.None);
            modules.Add (new TranslatorModule (),"Translator",ModuleFilter.None);
            modules.Add (new MemeModule (),"Memes",ModuleFilter.None);
            modules.Add (new NSFWModule (),"NSFW",ModuleFilter.None);
            modules.Add (new SoundsModule (),"Sounds",ModuleFilter.None);
            modules.Add (new ClashOfClansModule (),"ClashOfClans",ModuleFilter.None);
            modules.Add (new CustomReactionsModule (),"Customreactions",ModuleFilter.None);

            if (!string.IsNullOrWhiteSpace (Creds.TrelloAppKey))
                modules.Add (new TrelloModule (),"Trello",ModuleFilter.None);

            //run the bot
            Client.ExecuteAndWait (async () =>
            {
                Console.WriteLine("Bitte warte während der Bot startet...");

                try
                {
                    await Client.Connect (Creds.Token).ConfigureAwait (false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine ($"Token ist fehlerhaft. Setze keinen Token wenn du keinen offiziellen BOT Account bestitzt.");
                    Console.WriteLine (ex);
                    Console.ReadKey ();
                    return;
                }
                //Console.Write("Startvorgang dauert 10 Sekunden: ");
                //for(int i=10;i!=0;i--)
                //{
                //    Console.Write(i + " ");
                //    await Task.Delay(1000).ConfigureAwait(false);
                //}
                //Console.WriteLine();
                //await Task.Delay(10000).ConfigureAwait(false);
                
                Console.WriteLine ("-----------------");
                Console.WriteLine (await MidnightStats.Instance.GetStats ().ConfigureAwait (false));
                Console.WriteLine ("-----------------");

                OwnerPrivateChannels = new List<Channel>(Creds.OwnerIds.Length);
                foreach (var id in Creds.OwnerIds)
                {
                    try
                    {
                        OwnerPrivateChannels.Add(await Client.CreatePrivateChannel(id).ConfigureAwait(false));
                    }
                    catch
                    {
                        Console.WriteLine($"Konnte keinen privaten Channel mit dem Owner, welcher mit der ID {id} in der credentials.json gelistet ist");
                    }
                }

                //foreach (var ch in MidnightBot.Client.Servers.Select (s => s.DefaultChannel))
                //{
                //    await ch.SendMessage ("`Hallo. Ich bin wieder da!`");
                //}

                Client.ClientAPI.SendingRequest += ( s,e ) =>
                {
                    var request = e.Request as Discord.API.Client.Rest.SendMessageRequest;
                    if (request == null)
                        return;
                    // meew0 is magic
                    request.Content = request.Content?.Replace ("@everyone","@everyοne").Replace ("@here","@һere") ?? "_error_";
                    if (string.IsNullOrWhiteSpace (request.Content))
                        e.Cancel = true;
                };

                //await Task.Delay(90000);
                PermissionsHandler.Initialize ();

                Console.Title = "Midnight Bot | Connected";
                Console.WriteLine("Bot ist initialisiert.");

                MidnightBot.Ready = true;
                MidnightBot.OnReady();
            });
            Console.WriteLine ("Beende...");
            Console.ReadKey ();
        }

        public static void shutdownGracefully()
        {
            try
            {
                Client.SetStatus(UserStatus.Offline);

                Client.Disconnect();
                Console.Title = "Midnight Bot | Disconnected";
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static string BotName { get; set; }

        public static bool IsOwner ( ulong id ) => Creds.OwnerIds.Contains (id);

        public static async Task SendMessageToOwner ( string message )
        {
            if (Config.ForwardMessages && OwnerPrivateChannels.Any())
                if (Config.ForwardToAllOwners)
                    OwnerPrivateChannels.ForEach(async c =>
                    {
                        try { await c.SendMessage(message).ConfigureAwait(false); } catch { }
                    });
                else
                {
                    var c = OwnerPrivateChannels.FirstOrDefault();
                    if (c != null)
                        await c.SendMessage(message).ConfigureAwait(false);
                }
        }

        private static bool repliedRecently = false;
        private static async void Client_MessageReceived ( object sender,MessageEventArgs e )
        {
            try
            {
                if (e.Server != null || e.User.Id == Client.CurrentUser.Id)
                    return;
                if (PollCommand.ActivePolls.SelectMany (kvp => kvp.Key.Users.Select (u => u.Id)).Contains (e.User.Id))
                    return;
                if (ConfigHandler.IsBlackListed (e))
                    return;

                if (Config.ForwardMessages && !MidnightBot.Creds.OwnerIds.Contains(e.User.Id) && OwnerPrivateChannels.Any())
                    await SendMessageToOwner(e.User + ": ```\n" + e.Message.Text + "\n```").ConfigureAwait(false);

                if (repliedRecently)
                    return;

                repliedRecently = true;
                if (e.Message.RawText != MidnightBot.Config.CommandPrefixes.Help + "h")
                    await e.Channel.SendMessage (HelpCommand.DMHelpString).ConfigureAwait (false);
                await Task.Delay (2000).ConfigureAwait (false);
                repliedRecently = false;
            }
            catch { }
        }
    }
}