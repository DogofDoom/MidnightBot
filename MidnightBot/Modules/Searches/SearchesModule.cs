using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Classes;
using MidnightBot.Classes.JSONModels;
using MidnightBot.Extensions;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Searches.Commands;
using MidnightBot.Modules.Searches.Commands.IMDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MidnightBot.Modules.Searches
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
            commands.Add (new CalcCommand (this));
            commands.Add (new OsuCommands (this));
            commands.Add (new PokemonSearchCommands (this));
            commands.Add (new APICommands (this));
            commands.Add (new MemegenCommands (this));
            rng = new Random ();
        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Searches;

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
                                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString (e.GetArg ("query"))}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&fields=items%2Flink&key={MidnightBot.GetRndGoogleAPIKey ()}";
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
                            var apikey = MidnightBot.GetRndGoogleAPIKey ();
                            RANDOMIMG:
                            await e.Channel.SendIsTyping ();
                            if (string.IsNullOrWhiteSpace (e.GetArg ("query")))
                                return;
                            try
                            {
                                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(e.GetArg("query"))}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={rng.Next(1,50)}&fields=items%2Flink&key={apikey}";
                                var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString).ConfigureAwait (false));
                                var items = obj["items"] as JArray;
                                await e.Channel.SendMessage (items[0]["link"].ToString ()).ConfigureAwait (false);
                            }
                            catch (HttpRequestException exception)
                            {
                                if (exception.Message.Contains ("403 (Forbidden)"))
                                {
                                    await e.Channel.SendMessage ("Tägliches Limit erreicht!").ConfigureAwait (false);
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

                cgb.CreateCommand(Prefix + "google")
                    .Description("Gibt einen Google-Suchlink für einen Begriff zurück.")
                    .Parameter("terms", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var terms = e.GetArg("terms")?.Trim().Replace(' ', '+');
                        if (string.IsNullOrWhiteSpace(terms))
                            return;
                        await e.Channel.SendMessage($"https://google.com/search?q={ HttpUtility.UrlEncode(terms) }")
                                       .ConfigureAwait(false);
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
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",MidnightBot.Creds.MashapeKey } };
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
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",MidnightBot.Creds.MashapeKey } };
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
                       var headers = new Dictionary<string,string> { { "X-Mashape-Key",MidnightBot.Creds.MashapeKey } };
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
                     var quote = MidnightBot.Config.Quotes[rng.Next (0,MidnightBot.Config.Quotes.Count)].ToString ();
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
                
                cgb.CreateCommand (Prefix + "stardew")
                    .Description ($"Gibt einen Link zum Stardew Valley Wiki mit gegebenem Topic zurück.\n**Benutzung**: {Prefix}stardew Cow")
                    .Parameter ("topic",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var topic = e.GetArg ("topic")?.Trim ().ToLowerInvariant ();
                        if (string.IsNullOrWhiteSpace (topic))
                            return;
                        
                        var upperTopic = topic[0].ToString ().ToUpper () + topic.Substring (1);
                        topic.Replace (" ","_");

                        await e.Channel.SendMessage ($"Ich habe nach: {upperTopic} gesucht und folgendes gefunden: http://stardewvalleywiki.com/{topic}").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "magicitem")
                .Alias (Prefix + "mi")
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
                    .Description("Shows a random image from bronibooru with a given tag. Tag is optional but preferred. (multiple tags are appended with +)\n**Benutzung**: ~pony scootaloo")
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
                    .Description ("Zeigt dir die zum Hex zugehörige Farbe.\n**Benutzung**: `~clr 00ff00`")
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
                        var brush = new SolidBrush (System.Drawing.Color.FromArgb (red,green,blue));

                        using (Graphics g = Graphics.FromImage (img))
                        {
                            g.FillRectangle (brush,0,0,50,50);
                            g.Flush ();
                        }

                        await e.Channel.SendFile ("arg1.png",img.ToStream ());
                    });

                cgb.CreateCommand(Prefix + "videocall")
                  .Description("Erstellt einen privaten <http://www.appear.in> Video Anruf Link für dich und andere erwähnte Personen. Der Link wird allen erwähnten Personen per persönlicher Nachricht geschickt.")
                  .Parameter("arg", ParameterType.Unparsed)
                  .Do(async e =>
                  {
                      try
                      {
                          var allUsrs = e.Message.MentionedUsers.Union(new User[] { e.User });
                          var allUsrsArray = allUsrs as User[] ?? allUsrs.ToArray();
                          var str = allUsrsArray.Aggregate("http://appear.in/", (current, usr) => current + Uri.EscapeUriString(usr.Name[0].ToString()));
                          str += new Random().Next();
                          foreach (var usr in allUsrsArray)
                          {
                              await usr.SendMessage(str).ConfigureAwait(false);
                          }
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex);
                      }
                  });

                cgb.CreateCommand (Prefix + "av").Alias (Prefix + "avatar")
                    .Parameter ("mention",ParameterType.Required)
                    .Description ("Zeigt den Avatar einer erwähnten Person.\n **Benutzung**: ~av @X")
                    .Do (async e =>
                    {
                        var usr = e.Channel.FindUsers (e.GetArg ("mention")).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }
                        await e.Channel.SendMessage (await usr.AvatarUrl.ShortenUrl ()).ConfigureAwait (false);
                    });
            });
        }
    }
}