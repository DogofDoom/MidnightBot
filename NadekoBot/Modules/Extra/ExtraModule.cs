﻿using Discord;
using Discord.Commands;
using Discord.Modules;
using NadekoBot.Extensions;
using NadekoBot.Classes;
using NadekoBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;

namespace NadekoBot.Modules.Extra
{
    class ExtraModule : DiscordModule
    {
        private readonly Random rng;
        public ExtraModule () : base ()
        {
            // commands.Add(new OsuCommands());
            rng = new Random ();
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Searches;

        public override void Install ( ModuleManager manager )
        {
            var client = NadekoBot.Client;

            manager.CreateCommands ("",cgb =>
           {

               cgb.AddCheck (PermissionChecker.Instance);

               commands.ForEach (cmd => cmd.Init (cgb));

               cgb.CreateCommand ("#Schinken")
               .Description ("Schinken")
               .Do (async e =>
               {
                   await e.Channel.SendMessage ("Schinken hat mehrere Bedeutungen. Das Wort bezeichnete ursprünglich das „Bein“ im Sinne von „Knochen“. Gemeinsam mit „Schenkel“ stammt es von der indoeuropäischen Wurzel [s]keng für „schief, krumm“ ab (vermutlich wegen der Krümmung des Oberschenkelknochens). Der Name der Krankheit Skoliose (des „Buckels“) ist gleichen Ursprungs.")
                   .ConfigureAwait (false);
               });

               cgb.CreateCommand ("#Wambo")
               .Description ("Wambo")
               .Do (async e =>
               {
                   await e.Channel.SendMessage ("Ich wambo, du wambo, er sie es wambo, wambo, wamboen, gewambot, werden gewambot haben, Wamborama, Wambologie!!! Die Lehre des Wambo!")
                   .ConfigureAwait (false);
               });

               cgb.CreateCommand ("AND HIS NAME IS")
               .Description ("John Cena")
               .Do (async e =>
               {
                   await e.Channel.SendMessage ("https://www.youtube.com/watch?v=4k1xY7v8dDQ").ConfigureAwait (false);
               });

               cgb.CreateCommand (".kekse")
                   .Description ("Verteilt Kekse an eine bestimmte Person")
                   .Parameter ("user",ParameterType.Required)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       try
                       {
                           var targetStr = e.GetArg ("user")?.Trim ();
                           if (string.IsNullOrWhiteSpace (targetStr))
                               return;
                           var user = e.Server.FindUsers (targetStr).FirstOrDefault ();
                           if (user == null)
                           {
                               await e.Channel.SendMessage ("Benutzer nicht vorhanden.").ConfigureAwait (false);
                               return;
                           }
                           else
                           {
                               await e.Channel.SendMessage ($"{user.Mention} bekommt Kekse hingeschoben!").ConfigureAwait (false);
                           }
                       }
                       catch
                       { }
                   });

               cgb.CreateCommand (Prefix + "randomschinken")
                 .Alias (Prefix + "rs")
                 .Description ("Zeigt ein zufälliges Schinkenbild.")
                     .Do (async e =>
                     {
                         SCHINKEN:
                         try
                         {
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=ham&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={NadekoBot.Creds.GoogleAPIKey}";
                             var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString));
                             
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
                                 goto SCHINKEN;
                             }
                         }
                     });

               cgb.CreateCommand (Prefix + "randomlocation")
                 .Alias (Prefix + "rl")
                 .Description ("Zeigt eine zufällige Stadt.")
                     .Do (async e =>
                     {
                         LOCATION:
                         try
                         {
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=city&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={NadekoBot.Creds.GoogleAPIKey}";
                             var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString));
                             
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
                                 goto LOCATION;
                             }
                         }
                     });

               cgb.CreateCommand (Prefix + "randomimage")
                 .Alias (Prefix + "ri")
                 .Description ("Zeigt ein zufälliges Bild.")
                     .Do (async e =>
                     {
                         RANDIMG:
                         try
                         {
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=image&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={NadekoBot.Creds.GoogleAPIKey}";
                             var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString));
                             
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
                                 goto RANDIMG;
                             }
                         }
                     });

               cgb.CreateCommand (Prefix + "random9gag")
                 .Alias (Prefix + "r9")
                 .Description ("Zeigt ein zufälliges Bild von 9Gag.")
                     .Do (async e =>
                     {
                         RANDNINE:
                         try
                         {
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=9gag&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={NadekoBot.Creds.GoogleAPIKey}";
                             var obj = JObject.Parse (await SearchHelper.GetResponseStringAsync (reqString));
                             
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
                                 goto RANDNINE;
                             }
                         }
                     });
           });
        }       
    }
}