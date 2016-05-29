using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using NadekoBot.Modules.Searches.Commands;
using NadekoBot.Modules.Searches.Commands.IMDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace NadekoBot.Modules.Searches
{
    internal class SearchesModule : DiscordModule
    {
        private readonly Random rng;
        public SearchesModule ()
        {
            commands.Add (new LoLCommands (this));
            commands.Add (new StreamNotifications (this));
            commands.Add (new ConverterCommand (this));
            commands.Add (new RedditCommand (this));
            commands.Add (new WowJokeCommand (this));
            rng = new Random ();
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Searches;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "we")
                .Description ($"Zeigt Wetter-Daten für eine genannte Stadt und ein Land. BEIDES IST BENÖTIGT. Wetter Api ist sehr zufällig, wenn du einen Fehler machst.\n**Benutzung**: {Prefix}we Moskau RF")
                .Parameter ("city",ParameterType.Required)
                .Parameter ("country",ParameterType.Required)
                .Do (async e =>
                {
                    var city = e.GetArg ("city").Replace (" ","");
                    var country = e.GetArg ("country").Replace (" ","");
                    var response = await SearchHelper.GetResponseStringAsync ($"http://api.lawlypopzz.xyz/nadekobot/weather/?city={city}&country={country}").ConfigureAwait (false);

                    var obj = JObject.Parse (response)["weather"];

                    await e.Channel.SendMessage (
                        $@"🌍 **Wetter in** 【{obj["target"]}】
                        📏 **Lat,Long:** ({obj["latitude"]}, {obj["longitude"]}) ☁ **Condition:** {obj["condition"]}
                        😓 **Luftfeuchtigkeit:** {obj["humidity"]}% 💨 **Wind Geschwindigkeit:** {obj["windspeedk"]}km/h / {obj["windspeedm"]}mph 
                        🔆 **Temperatur:** {obj["centigrade"]}°C / {obj["fahrenheit"]}°F 🔆 **Gefühlt:** {obj["feelscentigrade"]}°C / {obj["feelsfahrenheit"]}°F
                        🌄 **Sonnenaufgang:** {obj["sunrise"]} 🌇 **Sonnenuntergang:** {obj["sunset"]}").ConfigureAwait (false);
                });



                cgb.CreateCommand (Prefix + "yt")
     .Parameter ("query",ParameterType.Unparsed)
     .Description ("Durchsucht Youtube und zeigt das erste Ergebnis.")
     .Do (async e =>
      {
          if (!(await SearchHelper.ValidateQuery (e.Channel,e.GetArg ("query"))))
              return;

          var link = await SearchHelper.FindYoutubeUrlByKeywords (e.GetArg ("query")).ConfigureAwait (false);
          if (string.IsNullOrWhiteSpace (link))
          {
              await e.Channel.SendMessage ("Kein Ergebnis mit diesem Begriff gefunden.");
              return;
          }
          var shortUrl = await SearchHelper.ShortenUrl (link).ConfigureAwait (false);
          await e.Channel.SendMessage (shortUrl).ConfigureAwait (false);
      });

                cgb.CreateCommand (Prefix + "ani")
                    .Alias (Prefix + "anime",Prefix + "aq")
                    .Parameter ("query",ParameterType.Unparsed)
                    .Description ("Durchsucht anilist nach einem Anime und zeigt das erste Ergebnis.")
                    .Do (async e =>
                     {
                         if (!(await SearchHelper.ValidateQuery (e.Channel,e.GetArg ("query")).ConfigureAwait (false)))
                             return;
                         string result;
                         try
                         {
                             result = (await SearchHelper.GetAnimeData (e.GetArg ("query")).ConfigureAwait (false)).ToString ();
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Anime konnte nicht gefunden werden.").ConfigureAwait (false);
                             return;
                         }

                         await e.Channel.SendMessage (result.ToString ()).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "imdb")
                    .Parameter ("query",ParameterType.Unparsed)
                    .Description ("Durchsucht IMDB nach Filmen oder Serien und zeigt erstes Ergebnis.")
                    .Do (async e =>
                    {
                        if (!(await SearchHelper.ValidateQuery (e.Channel,e.GetArg ("query")).ConfigureAwait (false)))
                            return;
                        await e.Channel.SendIsTyping ().ConfigureAwait (false);
                        string result;
                        try
                        {
                            var movie = ImdbScraper.ImdbScrape (e.GetArg ("query"),true);
                            if (movie.Status)
                                result = movie.ToString ();
                            else
                                result = "Film nicht gefunden.";
                        }
                        catch
                        {
                            await e.Channel.SendMessage ("Film nicht gefunden.").ConfigureAwait (false);
                            return;
                        }
                        await e.Channel.SendMessage (result.ToString ());
                    });

                cgb.CreateCommand (Prefix + "mang")
                    .Alias (Prefix + "manga").Alias (Prefix + "mq")
                    .Parameter ("query",ParameterType.Unparsed)
                    .Description ("Durchsucht anilist nach einem Manga und zeigt das erste Ergebnis.")
                    .Do (async e =>
                     {
                         if (!(await SearchHelper.ValidateQuery (e.Channel,e.GetArg ("query")).ConfigureAwait (false)))
                             return;
                         string result;
                         try
                         {
                             result = (await SearchHelper.GetMangaData (e.GetArg ("query")).ConfigureAwait (false)).ToString ();
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Manga konnte nicht gefunden werden.").ConfigureAwait (false);
                             return;
                         }
                         await e.Channel.SendMessage (result).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "randomcat")
                    .Alias (Prefix + "meow")
                    .Description ("Zeigt ein zufälliges Katzenbild.")
                    .Do (async e =>
                     {
                         await e.Channel.SendMessage (JObject.Parse (
                         await SearchHelper.GetResponseStringAsync ("http://www.random.cat/meow").ConfigureAwait (false))["file"].ToString ())
                         .ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "i")
                   .Description ("Zeigt das erste Ergebnis für eine Suche. Benutze ~ir für unterschiedliche Ergebnisse.\n**Benutzung**: ~i cute kitten")
                   .Parameter ("query",ParameterType.Unparsed)
                       .Do (async e =>
                        {
                            IMG:
                            if (string.IsNullOrWhiteSpace (e.GetArg ("query")))
                                return;
                            try
                            {
                                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString (e.GetArg ("query"))}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&fields=items%2Flink&key={NadekoBot.GetRndGoogleAPIKey ()}";
                                var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString).ConfigureAwait (false));

                                await e.Channel.SendMessage (obj["items"][0]["link"].ToString ()).ConfigureAwait (false);
                            }
                            catch (HttpRequestException exception)
                            {
                                if (exception.Message.Contains ("403 (Forbidden)"))
                                {
                                    await e.Channel.SendMessage ("Limit erreicht!").ConfigureAwait (false);
                                }
                                else
                                {
                                    goto IMG;
                                }
                            }
                        });

                cgb.CreateCommand (Prefix + "ir")
                   .Description ("Zeigt ein zufälliges Bild bei einem angegeben Suchwort.\n**Benutzung**: ~ir cute kitten")
                   .Parameter ("query",ParameterType.Unparsed)
                       .Do (async e =>
                        {
                            RANDOMIMG:
                            await e.Channel.SendIsTyping ();
                            if (string.IsNullOrWhiteSpace (e.GetArg ("query")))
                                return;
                            try
                            {
                                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString (e.GetArg ("query"))}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={NadekoBot.GetRndGoogleAPIKey ()}";
                                var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString).ConfigureAwait (false));

                                await e.Channel.SendMessage (obj["items"][0]["link"].ToString ()).ConfigureAwait (false);
                            }
                            catch (HttpRequestException exception)
                            {
                                if (exception.Message.Contains ("403 (Forbidden)"))
                                {
                                    await e.Channel.SendMessage ("Limit erreicht!").ConfigureAwait (false);
                                }
                                else
                                {
                                    goto RANDOMIMG;
                                }
                            }

                        });

                cgb.CreateCommand (Prefix + "lmgtfy")
                    .Description ("Google etwas für einen Idioten.")
                    .Parameter ("ffs",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         if (e.GetArg ("ffs") == null || e.GetArg ("ffs").Length < 1)
                             return;
                         await e.Channel.SendMessage (await $"http://lmgtfy.com/?q={ Uri.EscapeUriString (e.GetArg ("ffs").ToString ()) }".ShortenUrl ())
                         .ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "hs")
                  .Description ("Sucht eine Heartstone-Karte und zeigt ihr Bild. Braucht eine Weile zum beenden.\n**Benutzung**:~hs Ysera")
                  .Parameter ("name",ParameterType.Unparsed)
                  .Do (async e =>
                   {
                       var arg = e.GetArg ("name");
                       if (string.IsNullOrWhiteSpace (arg))
                       {
                           await e.Channel.SendMessage ("💢 Bitte gib einen Kartennamen ein.").ConfigureAwait (false);
                           return;
                       }
                       await e.Channel.SendIsTyping ();
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",NadekoBot.Creds.MashapeKey } };
                       var res = await SearchHelper.GetResponseStringAsync ($"https://omgvamp-hearthstone-v1.p.mashape.com/cards/search/{Uri.EscapeUriString (arg)}",headers)
                       .ConfigureAwait (false);
                       try
                       {
                           var items = JArray.Parse (res);
                           var images = new List<Image> ();
                           if (items == null)
                               throw new KeyNotFoundException ("Es wurde keine Karte mit diesem Namen gefunden.");
                           var cnt = 0;
                           items.Shuffle ();
                           foreach (var item in items.TakeWhile (item => cnt++ < 4).Where (item => item.HasValues && item["img"] != null))
                           {
                               images.Add (
                                   Image.FromStream (await SearchHelper.GetResponseStreamAsync (item["img"].ToString ()).ConfigureAwait (false)));
                           }
                           if (items.Count > 4)
                           {
                               await e.Channel.SendMessage ("⚠ Mehr als 4 Bilder gefunden. Zeige 4 Zufällige.").ConfigureAwait (false);
                           }
                           await e.Channel.SendFile (arg + ".png",(await images.MergeAsync ()).ToStream (System.Drawing.Imaging.ImageFormat.Png))
                           .ConfigureAwait (false);
                       }
                       catch (Exception ex)
                       {
                           await e.Channel.SendMessage ($"💢 Error {ex.Message}").ConfigureAwait (false);
                       }
                   });

                cgb.CreateCommand (Prefix + "osustats")
                  .Description ("Zeigt Osu-Statistiken für einen Spieler.\n**Benutzung**:~osustats Name")
                  .Parameter ("usr",ParameterType.Unparsed)
                  .Do (async e =>
                   {
                       if (string.IsNullOrWhiteSpace (e.GetArg ("usr")))
                           return;

                       using (WebClient cl = new WebClient ())
                       {
                           try
                           {
                               cl.CachePolicy = new System.Net.Cache.RequestCachePolicy (System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                               cl.Headers.Add (HttpRequestHeader.UserAgent,"Mozilla/5.0 (Windows NT 6.2; Win64; x64)");
                               cl.DownloadDataAsync (new Uri ($"http://lemmmy.pw/osusig/sig.php?uname={ e.GetArg ("usr") }&flagshadow&xpbar&xpbarhex&pp=2"));
                               cl.DownloadDataCompleted += async ( s,cle ) =>
                               {
                                   try
                                   {
                                       await e.Channel.SendFile ($"{e.GetArg ("usr")}.png",new MemoryStream (cle.Result)).ConfigureAwait (false);
                                       await e.Channel.SendMessage ($"`Profil Link:`https://osu.ppy.sh/u/{Uri.EscapeDataString (e.GetArg ("usr"))}\n`Bild bereitgestellt von https://lemmmy.pw/osusig`").ConfigureAwait (false);
                                   }
                                   catch { }
                               };
                           }
                           catch
                           {
                               await e.Channel.SendMessage ("💢 Osu Signatur konnte nicht bereitgestellt werden :\\").ConfigureAwait (false);
                           }
                       }
                   });

                cgb.CreateCommand (Prefix + "ud")
                  .Description ("Durchsucht das Urban Dictionary nach einem Wort.\n**Benutzung**:~ud Pineapple")
                  .Parameter ("query",ParameterType.Unparsed)
                  .Do (async e =>
                   {
                       var arg = e.GetArg ("query");
                       if (string.IsNullOrWhiteSpace (arg))
                       {
                           await e.Channel.SendMessage ("💢 Bitte gib einen Suchbegriff ein.").ConfigureAwait (false);
                           return;
                       }
                       await e.Channel.SendIsTyping ().ConfigureAwait (false);
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",NadekoBot.Creds.MashapeKey } };
                       var res = await SearchHelper.GetResponseStringAsync ($"https://mashape-community-urban-dictionary.p.mashape.com/define?term={Uri.EscapeUriString (arg)}",headers).ConfigureAwait (false);
                       try
                       {
                           var items = JObject.Parse (res);
                           var sb = new System.Text.StringBuilder ();
                           sb.AppendLine ($"`Term:` {items["list"][0]["word"].ToString ()}");
                           sb.AppendLine ($"`Definition:` {items["list"][0]["definition"].ToString ()}");
                           sb.Append ($"`Link:` <{await items["list"][0]["permalink"].ToString ().ShortenUrl ().ConfigureAwait (false)}>");
                           await e.Channel.SendMessage (sb.ToString ());
                       }
                       catch
                       {
                           await e.Channel.SendMessage ("💢 Keine Definition für den Begriff gefunden.").ConfigureAwait (false);
                       }
                   });
                // thanks to Blaubeerwald
                cgb.CreateCommand (Prefix + "#")
                 .Description ("Durchsucht Tagdef.com nach einem Hashtag.\n**Benutzung**:~# ff")
                 .Parameter ("query",ParameterType.Unparsed)
                 .Do (async e =>
                   {
                       var arg = e.GetArg ("query");
                       if (string.IsNullOrWhiteSpace (arg))
                       {
                           await e.Channel.SendMessage ("💢 Bitte gib einen Suchbegriff ein.").ConfigureAwait (false);
                           return;
                       }
                       await e.Channel.SendIsTyping ();
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",NadekoBot.Creds.MashapeKey } };
                       var res = await SearchHelper.GetResponseStringAsync ($"https://tagdef.p.mashape.com/one.{Uri.EscapeUriString (arg)}.json",headers).ConfigureAwait (false);
                       try
                       {
                           var items = JObject.Parse (res);
                           var sb = new System.Text.StringBuilder ();
                           sb.AppendLine ($"`Hashtag:` {items["defs"]["def"]["hashtag"].ToString ()}");
                           sb.AppendLine ($"`Definition:` {items["defs"]["def"]["text"].ToString ()}");
                           sb.Append ($"`Link:` <{await items["defs"]["def"]["uri"].ToString ().ShortenUrl ().ConfigureAwait (false)}>");
                           await e.Channel.SendMessage (sb.ToString ());
                       }
                       catch
                       {
                           await e.Channel.SendMessage ("💢 Keine Definition gefunden.").ConfigureAwait (false);
                       }
                   });

                cgb.CreateCommand (Prefix + "quote")
                .Description ("Zeigt ein zufälliges Zitat.")
                .Do (async e =>
                 {
                     var quote = NadekoBot.Config.Quotes[rng.Next (0,NadekoBot.Config.Quotes.Count)].ToString ();
                     await e.Channel.SendMessage (quote).ConfigureAwait (false);
                 });

                cgb.CreateCommand (Prefix + "catfact")
                     .Description ("Zeigt einen zufälligen Katzenfakt von <http://catfacts-api.appspot.com/api/facts>")
                     .Do (async e =>
                     {
                         var response = await SearchHelper.GetResponseStringAsync ("http://catfacts-api.appspot.com/api/facts").ConfigureAwait (false);
                         if (response == null)
                             return;
                         await e.Channel.SendMessage ("`" + JObject.Parse (response)["facts"].ToString () + "`").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "yomama")
                    .Alias (Prefix + "ym")
                    .Description ("Zeigt einen zufälligen Witz von <http://api.yomomma.info/>")
                    .Do (async e =>
                    {
                        var response = await SearchHelper.GetResponseStringAsync ("http://api.yomomma.info/").ConfigureAwait (false);
                        await e.Channel.SendMessage ("`" + JObject.Parse (response)["joke"].ToString () + "` 😆").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "randjoke")
                    .Alias (Prefix + "rj")
                    .Description ("Zeigt einen zufälligen Witz von <http://tambal.azurewebsites.net/joke/random>")
                    .Do (async e =>
                    {
                        var response = await SearchHelper.GetResponseStringAsync ("http://tambal.azurewebsites.net/joke/random").ConfigureAwait (false);
                        await e.Channel.SendMessage ("`" + JObject.Parse (response)["joke"].ToString () + "` 😆").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "chucknorris")
                   .Alias (Prefix + "cn")
                   .Description ("Zeigt einen zufälligen Chuck Norris Witz von <http://tambal.azurewebsites.net/joke/random>")
                   .Do (async e =>
                   {
                       var response = await SearchHelper.GetResponseStringAsync ("http://api.icndb.com/jokes/random/").ConfigureAwait (false);
                       await e.Channel.SendMessage ("`" + JObject.Parse (response)["value"]["joke"].ToString () + "` 😆").ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "osumap")
                   .Alias (Prefix + "om")
                   .Description ("Zeigt Informationen über eine bestimmte Beatmap\n**Benutzung: ~osumap 252002:std")
                   .Parameter ("input",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       if (string.IsNullOrWhiteSpace (e.GetArg ("input")))
                       {
                           await e.Send ("Bitte gib eine Beatmap-ID ein.").ConfigureAwait (false);
                           return;
                       }

                       var beatmap = e.GetArg ("input");

                       String[] uxm = beatmap.Split (':');
                       var mode = uxm[1];
                       try
                       {
                           if (uxm[1].Equals ("std",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("0") || uxm[1].Equals ("standard",StringComparison.OrdinalIgnoreCase))
                           {
                               mode = "0";
                           }
                           else if (uxm[1].Equals ("taiko",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("1"))
                           {
                               mode = "1";
                           }
                           else if (uxm[1].Equals ("ctb",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("2") || uxm[1].Equals ("catch the beat",StringComparison.OrdinalIgnoreCase))/*|| choice == "ctb" || choice == "Ctb" || choice == "CTb" || choice == "CTB" || choice == "cTb" || choice == "ctB"*/
                           {
                               mode = "2";
                           }
                           else if (uxm[1].Equals ("mania",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("3"))
                           {
                               mode = "3";
                           }
                           else if (uxm[1] == "")
                           {
                               uxm[1] = "std";
                               mode = "0";
                           }
                           else
                           {
                               await e.Send ("Muss ein gültiger Modus sein.").ConfigureAwait (false);
                               return;
                           }
                       }

                       catch (Exception ex)
                       {
                           Console.WriteLine ($"Fehler beim Modus: " + ex);
                           return;
                       }

                       var api = new Osu.OsuApi (NadekoBot.Creds.OsuAPIKey);
                       var oBeatmap = api.GetMap (uxm[0],mode);
                       var formatString =
                                "```" + "Modus: " + uxm[1]
                              + "\nBeatmapname: " + oBeatmap.title
                              + "\nLänge: " + oBeatmap.total_length + " Sekunden"
                              + "\nKünstler: " + oBeatmap.artist
                              + "\nBeatmap-ID: " + oBeatmap.beatmap_id
                              + "\nRanking Datum: " + oBeatmap.approved_date
                              + "\nErsteller: " + oBeatmap.creator
                              + "\nBPM: " + oBeatmap.bpm
                              + "\nSchwierigkeit: " + oBeatmap.difficultyrating + " Sterne"
                              + "\nCS: " + oBeatmap.diff_size
                              + "\nOD: " + oBeatmap.diff_overall
                              + "\nAR: " + oBeatmap.diff_approach
                              + "\nHP: " + oBeatmap.diff_drain
                              + "\nMaximale Combo: " + oBeatmap.max_combo + "```";

                       await e.Send (formatString).ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "osu")
                   .Alias (Prefix + "oq")
                   .Description ("Zeigt Osu Benutzer Statistiken\n**Benutzung**: ~osu Cookiezi:standard")
                   .Parameter ("input",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       if (string.IsNullOrWhiteSpace (e.GetArg ("input")))
                       {
                           await e.Send ("Bitte gib einen Benutzernamen ein.").ConfigureAwait (false);
                           return;
                       }


                       var user = e.GetArg ("input");

                       String[] uxm = user.Split (':');
                       var mode = uxm[1];
                       try
                       {
                           if (uxm[1].Equals ("std",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("0") || uxm[1].Equals ("standard",StringComparison.OrdinalIgnoreCase))
                           {
                               mode = "0";
                           }
                           else if (uxm[1].Equals ("taiko",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("1"))
                           {
                               mode = "1";
                           }
                           else if (uxm[1].Equals ("ctb",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("2") || uxm[1].Equals ("catch the beat",StringComparison.OrdinalIgnoreCase))/*|| choice == "ctb" || choice == "Ctb" || choice == "CTb" || choice == "CTB" || choice == "cTb" || choice == "ctB"*/
                           {
                               mode = "2";
                           }
                           else if (uxm[1].Equals ("mania",StringComparison.OrdinalIgnoreCase) || uxm[1].Equals ("3"))
                           {
                               mode = "3";
                           }
                           else if (uxm[1] == "")
                           {
                               uxm[1] = "std";
                               mode = "0";
                           }
                           else
                           {
                               await e.Send ("Muss ein gültiger Modus sein.").ConfigureAwait (false);
                               return;
                           }
                       }

                       catch (Exception ex)
                       {
                           Console.WriteLine ($"Fehler beim Modus: " + ex);
                           return;
                       }

                       var api = new Osu.OsuApi (NadekoBot.Creds.OsuAPIKey);
                       var oUser = api.GetUser (uxm[0],mode);
                       var formatString =
                                "```" + "Modus: " + uxm[1]
                              + "\nBenutzername: " + uxm[0]
                              + "\nLand: " + oUser.country
                              + "\nPP: " + oUser.pp_raw
                              + "\nPP Rang: #" + String.Format ("{0:###,###}",oUser.pp_rank)
                              + "\nGerankete Punktzahl: " + String.Format ("{0:###,###}",oUser.ranked_score)
                              + "\nLevel: " + oUser.level
                              + "\nGenauigkeit: " + oUser.accuracy + "%"
                              + "\n\nSS Ränge: " + oUser.count_rank_ss
                              + "\nS Ränge: " + oUser.count_rank_s
                              + "\nA Ränge: " + oUser.count_rank_a + "```";

                       await e.Send (formatString).ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "mi")
                   .Alias (Prefix + "magicitem")
                   .Description ("Zeigt ein zufälliges Magic-Item von <https://1d4chan.org/wiki/List_of_/tg/%27s_magic_items>")
                   .Do (async e =>
                   {
                       var magicItems = JsonConvert.DeserializeObject<List<MagicItem>> (File.ReadAllText ("data/magicitems.json"));
                       var item = magicItems[rng.Next (0,magicItems.Count)].ToString ();

                       await e.Channel.SendMessage (item).ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "revav")
                   .Description ("Gibt ein Google Reverse Image Search für das Profilbild einer Person zurück.")
                   .Parameter ("user",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       var usrStr = e.GetArg ("user")?.Trim ();

                       if (string.IsNullOrWhiteSpace (usrStr))
                           return;

                       var usr = e.Server.FindUsers (usrStr).FirstOrDefault ();

                       if (usr == null || string.IsNullOrWhiteSpace (usr.AvatarUrl))
                           return;
                       await e.Channel.SendIsTyping ();
                       await e.Channel.SendMessage ($"https://images.google.com/searchbyimage?image_url={usr.AvatarUrl}").ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "revimg")
                   .Description ($"Gibt eine 'Google Reverse Image Search' für ein Bild von einem Link zurück.")
                   .Parameter ("image",ParameterType.Unparsed)
                   .Do (async e =>
                   {
                       var imgLink = e.GetArg ("image")?.Trim ();

                       if (string.IsNullOrWhiteSpace (imgLink))
                           return;
                       await e.Channel.SendMessage ($"https://images.google.com/searchbyimage?image_url={imgLink}").ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "safebooru")
                    .Description ("Zeigt ein zufälliges Hentai Bild von safebooru  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags.\n**Benutzung**: ~safebooru yuri +kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetSafebooruImageLink (tag).ConfigureAwait (false);
                        if (link == null)
                            await e.Channel.SendMessage ("`Keine Ergebnisse.`");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });

                cgb.CreateCommand(Prefix + "pony")
                    .Alias(Prefix + "broni")
                    .Description("Shows a random image from bronibooru with a given tag. Tag is optional but preferred. (multiple tags are appended with +)\n**Usage**: ~pony scootaloo")
                    .Parameter("tag", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var tag = e.GetArg("tag")?.Trim() ?? "";
                        var broni = await SearchHelper.GetBronibooruImageLink(tag).ConfigureAwait(false);
                        if (broni != null)
                            await e.Channel.SendMessage("Bronibooru: " + broni).ConfigureAwait(false);
                            await e.Channel.SendMessage("Search yielded no results.");
                    });

                cgb.CreateCommand (Prefix + "wiki")
                    .Description ("Gibt einen Wikipedia-Link zurück.")
                    .Parameter ("query",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var query = e.GetArg ("query");
                        var result = await SearchHelper.GetResponseStringAsync ("https://en.wikipedia.org//w/api.php?action=query&format=json&prop=info&redirects=1&formatversion=2&inprop=url&titles=" + Uri.EscapeDataString (query));
                        var data = JsonConvert.DeserializeObject<WikipediaApiModel> (result);
                        if (data.Query.Pages[0].Missing)
                            await e.Channel.SendMessage ("`Diese Seite konnte nicht gefunden werden.`");
                        else
                            await e.Channel.SendMessage (data.Query.Pages[0].FullUrl);
                    });

                cgb.CreateCommand (Prefix + "clr")
                    .Description ("Zeigt dir die zum Hex zugehörige Farbe.\n**Benutztung**: `~clr 00ff00`")
                    .Parameter ("color",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var arg1 = e.GetArg ("color")?.Trim ()?.Replace ("#","");
                        if (string.IsNullOrWhiteSpace (arg1))
                            return;
                        var img = new Bitmap (50,50);

                        var red = Convert.ToInt32 (arg1.Substring (0,2),16);
                        var green = Convert.ToInt32 (arg1.Substring (2,2),16);
                        var blue = Convert.ToInt32 (arg1.Substring (4,2),16);
                        var brush = new SolidBrush (Color.FromArgb (red,green,blue));

                        using (Graphics g = Graphics.FromImage (img))
                        {
                            g.FillRectangle (brush,0,0,50,50);
                            g.Flush ();
                        }

                        await e.Channel.SendFile ("arg1.png",img.ToStream ());
                    });
            });
        }
    }
}