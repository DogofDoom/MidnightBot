using System;
using System.Collections.Generic;

namespace MidnightBot._Models.JSONModels
{
    class APIConfig
    {
        /// <summary>
        /// Names of the command
        /// </summary>
        public List<string> Names { get; set; }

        /// <summary>
        /// API endpoint
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Whether or not the command should accept empty strings
        /// </summary>
        public bool AllowEmpty { get; set; } = false;

        /// <summary>
        /// Any additions after main query to URL. %1random15% will be replaced with random number in range 
        /// </summary>
        public string URLSuffix { get; set; } = "";

        /// <summary>
        /// Any headers to add to the request
        /// </summary>
        public Dictionary<string,string> Headers { get; set; } = new Dictionary<string,string> ();

        /// <summary>
        /// How to handle the response, current possibilities:
        /// {"JSON", 0,0{outputformat with {nameofdescendant}}
        /// {"XDOC", [nameofdescendant]}
        /// {"REGEX", regex}
        /// </summary>
        public Tuple<string,string> ResponseHandling { get; set; }

        /// <summary>
        /// Replacements to execute on query- like " " -> "_"
        /// </summary>
        public Dictionary<string,string> QueryReplacements { get; set; } = new Dictionary<string,string> ();

        /// <summary>
        /// Regexes to be executed on response
        /// </summary>
        public Dictionary<string,string> RegexOnResponse { get; set; } = new Dictionary<string,string> ();
    }
}