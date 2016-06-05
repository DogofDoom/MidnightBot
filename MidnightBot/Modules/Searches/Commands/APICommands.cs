using MidnightBot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MidnightBot._Models.JSONModels;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Manatee.Json;
using Newtonsoft.Json.Linq;

namespace MidnightBot.Modules.Searches.Commands
{
    class APICommands : DiscordCommand
    {
        public APICommands ( DiscordModule module ) : base (module)
        {
        }

        List<APIConfig> apiconfigs;
        Random Range = new Random ();
        internal override void Init ( CommandGroupBuilder cgb )
        {
            apiconfigs = JsonConvert.DeserializeObject<List<APIConfig>> (File.ReadAllText ("data/apis.json"));

            foreach (var config in apiconfigs)
            {
                var cmd = cgb.CreateCommand (Module.Prefix + config.Names.FirstOrDefault ());
                var aliases = config.Names.Skip (1);
                if (aliases.Any ())
                    cmd.Alias (aliases.Select (x => Module.Prefix + x).ToArray ());
                cmd.Parameter ("query",ParameterType.Unparsed);
                cmd.Description ($"Queries {config.URL}.");
                cmd.Do (APIQuery (config));
            }
        }

        private Func<CommandEventArgs,Task> APIQuery ( APIConfig config ) => async e =>
        {
            //get the parameter
            var query = e.GetArg ("query")?.Trim ();
            if (string.IsNullOrWhiteSpace (query) && !config.AllowEmpty)
            {
                await e.Channel.SendMessage ("Query must be provided");
                return;
            }
            //replace any characters or forbidden things
            foreach (var replacer in config.QueryReplacements)
            {
                query = Regex.Replace (query,replacer.Key,x => replacer.Value);
            }
            var link = config.URL + Uri.EscapeDataString (query) + Regex.Replace (config.URLSuffix,@"%(\d+)random(\d+)%",x => Range.Next (int.Parse (x.Groups[1].Value),int.Parse (x.Groups[2].Value)) + "");
            var response = await SearchHelper.GetResponseStringAsync (link,config.Headers).ConfigureAwait (false);
            string message = "";
            //Get our message
            switch (config.ResponseHandling.Item1)
            {
                case "JSON":
                    JToken obj = JObject.Parse (response);
                    message = getJsonToken (config.ResponseHandling.Item2,obj).ToString ();
                    break;
                case "XDOC":
                    var doc = XDocument.Load (response);
                    message = doc.Descendants (config.ResponseHandling.Item2).FirstOrDefault ().Value;
                    break;
                case "REGEX":
                    var matches = Regex.Matches (response,config.ResponseHandling.Item2);
                    if (matches.Count == 0)
                    {
                        await e.Channel.SendMessage ("responsehandling failed").ConfigureAwait (false);
                        return;
                    }
                    else
                        message = matches[Range.Next (0,matches.Count)].Value;
                    break;
                default:
                    await e.Channel.SendMessage ("Response handling not specified!").ConfigureAwait (false);
                    return;
            }
            //Change anything that needs to be changed
            foreach (var reg in config.RegexOnResponse)
            {
                message = Regex.Replace (message,reg.Key,x => reg.Value);
            }
            if (string.IsNullOrWhiteSpace (message))
            {
                await e.Channel.SendMessage ("Something went wrong, no message left").ConfigureAwait (false);
                return;
            }
            await e.Channel.SendMessage (message).ConfigureAwait (false);
        };

        private JToken getJsonToken ( string item2,JToken obj )
        {

            var split = item2.Split (',');
            var JSON = obj;
            foreach (var s in split)
            {
                if (s == split.Last ())
                {

                }
                int index;
                if (int.TryParse (s,out index))
                {
                    if (JSON.Count () <= index)
                        return null;

                    JSON = JSON[index];
                }
                else
                {
                    //Maybe add a check sometime 
                    JSON = JSON[s];
                }
            }
            return JSON;
        }
    }
}