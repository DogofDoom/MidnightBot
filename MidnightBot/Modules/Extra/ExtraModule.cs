using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.Extensions;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;

namespace MidnightBot.Modules.Extra
{
    class ExtraModule : DiscordModule
    {
        private readonly Random rng;
        public ExtraModule () : base ()
        {
            // commands.Add(new OsuCommands());
            rng = new Random ();
        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Searches;

        public override void Install ( ModuleManager manager )
        {
            var client = MidnightBot.Client;

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
               
               cgb.CreateCommand (Prefix + "rip")
                   .Description ("RIP")
                   .Parameter ("text",ParameterType.Required)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       var targetStr = "";

                       var usr = e.Channel.FindUsers (e.GetArg ("text")).FirstOrDefault ();
                       if (usr == null)
                       {
                           targetStr = e.GetArg ("text")?.Trim ();
                           if (string.IsNullOrWhiteSpace (targetStr))
                               return;
                       }
                       else
                       {
                           targetStr = usr.Name;
                       }


                       targetStr = targetStr.Replace (" ","%20");
                       targetStr = targetStr.Replace ("@","");
                       await e.Channel.SendMessage ($"http://ripme.xyz/{targetStr}").ConfigureAwait (false);
                   });

               cgb.CreateCommand (Prefix + "randomschinken")
                 .Alias (Prefix + "rs")
                 .Description ("Zeigt ein zufälliges Schinkenbild.")
                     .Do (async e =>
                     {
                         SCHINKEN:
                         try
                         {
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=ham&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={MidnightBot.Creds.GoogleAPIKey}";
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
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=city&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={MidnightBot.Creds.GoogleAPIKey}";
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
                             var reqString = $"https://www.googleapis.com/customsearch/v1?q=image&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next (1,150) }&fields=items%2Flink&key={MidnightBot.Creds.GoogleAPIKey}";
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
           });
        }       
    }
}