using MidnightBot.Classes.JSONModels;
using MidnightBot.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;
using System.Xml;

namespace MidnightBot.Classes
{
    public enum RequestHttpMethod
    {
        Get,
        Post
    }

    public static class SearchHelper
    {
        private static DateTime lastRefreshed = DateTime.MinValue;
        private static string token { get; set; } = "";

        public static async Task<Stream> GetResponseStreamAsync ( string url,
            IEnumerable<KeyValuePair<string,string>> headers = null,RequestHttpMethod method = RequestHttpMethod.Get )
        {
            if (string.IsNullOrWhiteSpace (url))
                throw new ArgumentNullException (nameof (url));
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Clear ();
            try
            {
                switch (method)
                {
                    case RequestHttpMethod.Get:
                        if (headers != null)
                        {
                            foreach (var header in headers)
                            {
                                cl.DefaultRequestHeaders.TryAddWithoutValidation (header.Key,header.Value);
                            }
                        }
                        return await cl.GetStreamAsync (url).ConfigureAwait (false);
                    case RequestHttpMethod.Post:
                        FormUrlEncodedContent formContent = null;
                        if (headers != null)
                        {
                            formContent = new FormUrlEncodedContent (headers);
                        }
                        var message = await cl.PostAsync (url,formContent).ConfigureAwait (false);
                        return await message.Content.ReadAsStreamAsync ().ConfigureAwait (false);
                    default:
                        throw new NotImplementedException ("That type of request is unsupported.");
                }
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
        }

        public static async Task<string> GetResponseStringAsync ( string url,
            IEnumerable<KeyValuePair<string,string>> headers = null,
            RequestHttpMethod method = RequestHttpMethod.Get )
        {

            using (var streamReader = new StreamReader (await GetResponseStreamAsync (url,headers,method).ConfigureAwait (false)))
            {
                return await streamReader.ReadToEndAsync ().ConfigureAwait (false);
            }
        }

        public static async Task<AnimeResult> GetAnimeData ( string query )
        {
            if (string.IsNullOrWhiteSpace (query))
                throw new ArgumentNullException (nameof (query));

            await RefreshAnilistToken ().ConfigureAwait (false);

            var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString (query);
            var smallContent = "";
            var cl = new RestSharp.RestClient ("http://anilist.co/api");
            var rq = new RestSharp.RestRequest ("/anime/search/" + Uri.EscapeUriString (query));
            rq.AddParameter ("access_token",token);
            smallContent = cl.Execute (rq).Content;
            var smallObj = JArray.Parse (smallContent)[0];

            rq = new RestSharp.RestRequest ("/anime/" + smallObj["id"]);
            rq.AddParameter ("access_token",token);
            var content = cl.Execute (rq).Content;

            return await Task.Run (() => JsonConvert.DeserializeObject<AnimeResult> (content)).ConfigureAwait (false);
        }

        public static async Task<MangaResult> GetMangaData ( string query )
        {
            if (string.IsNullOrWhiteSpace (query))
                throw new ArgumentNullException (nameof (query));

            await RefreshAnilistToken ().ConfigureAwait (false);

            var link = "http://anilist.co/api/anime/search/" + Uri.EscapeUriString (query);
            var smallContent = "";
            var cl = new RestSharp.RestClient ("http://anilist.co/api");
            var rq = new RestSharp.RestRequest ("/manga/search/" + Uri.EscapeUriString (query));
            rq.AddParameter ("access_token",token);
            smallContent = cl.Execute (rq).Content;
            var smallObj = JArray.Parse (smallContent)[0];

            rq = new RestSharp.RestRequest ("/manga/" + smallObj["id"]);
            rq.AddParameter ("access_token",token);
            var content = cl.Execute (rq).Content;

            return await Task.Run (() => JsonConvert.DeserializeObject<MangaResult> (content)).ConfigureAwait (false);
        }

        private static async Task RefreshAnilistToken ()
        {
            if (DateTime.Now - lastRefreshed > TimeSpan.FromMinutes (29))
                lastRefreshed = DateTime.Now;
            else
            {
                return;
            }
            var headers = new Dictionary<string,string> {
                {"grant_type", "client_credentials"},
                {"client_id", "kwoth-w0ki9"},
                {"client_secret", "Qd6j4FIAi1ZK6Pc7N7V4Z"},
            };
            var content = await GetResponseStringAsync (
                           "http://anilist.co/api/auth/access_token",
                           headers,
                           RequestHttpMethod.Post).ConfigureAwait (false);

            token = JObject.Parse (content)["access_token"].ToString ();
        }

        public static async Task<bool> ValidateQuery ( Discord.Channel ch,string query )
        {
            if (!string.IsNullOrEmpty (query.Trim ()))
                return true;
            await ch.Send ("Please specify search parameters.").ConfigureAwait (false);
            return false;
        }

        public static async Task<string> FindYoutubeUrlByKeywords ( string keywords )
        {
            if (string.IsNullOrWhiteSpace (keywords))
                throw new ArgumentNullException (nameof (keywords),"Query not specified.");
            if (keywords.Length > 150)
                throw new ArgumentException ("Query is too long.");

            //maybe it is already a youtube url, in which case we will just extract the id and prepend it with youtube.com?v=
            var match = new Regex ("(?:youtu\\.be\\/|v=)(?<id>[\\da-zA-Z\\-_]*)").Match (keywords);
            if (match.Length > 1)
            {
                return $"https://www.youtube.com/watch?v={match.Groups["id"].Value}";
            }

            if (string.IsNullOrWhiteSpace(MidnightBot.GetRndGoogleAPIKey()))
                throw new InvalidCredentialException("Google API Key is missing.");
            var response = await GetResponseStringAsync(
                                           $"https://www.googleapis.com/youtube/v3/search?" +
                                           $"part=snippet&maxResults=1" +
                                           $"&q={Uri.EscapeDataString (keywords)}" +
                                           $"&key={MidnightBot.GetRndGoogleAPIKey ()}").ConfigureAwait (false);
            JObject obj = JObject.Parse (response);

            var data = JsonConvert.DeserializeObject<YoutubeVideoSearch> (response);

            if (data.items.Length > 0)
            {
                var toReturn = "http://www.youtube.com/watch?v=" + data.items[0].id.videoId.ToString ();
                return toReturn;
            }
            else
                return null;
        }

        public static async Task<string> GetRelatedVideoId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            var match = new Regex("(?:youtu\\.be\\/|v=)(?<id>[\\da-zA-Z\\-_]*)").Match(id);
            if (match.Length > 1)
            {
                id = match.Groups["id"].Value;
            }
            var response = await GetResponseStringAsync(
                                    $"https://www.googleapis.com/youtube/v3/search?" +
                                    $"part=snippet&maxResults=1&type=video" +
                                    $"&relatedToVideoId={id}" +
                                    $"&key={MidnightBot.GetRndGoogleAPIKey()}").ConfigureAwait(false);
            JObject obj = JObject.Parse(response);

            var data = JsonConvert.DeserializeObject<YoutubeVideoSearch>(response);

            if (data.items.Length > 0)
            {
                var toReturn = "http://www.youtube.com/watch?v=" + data.items[0].id.videoId.ToString();
                return toReturn;
            }
            else
                return null;
        }

        public static async Task<string> GetPlaylistIdByKeyword ( string query )
        {
            if (string.IsNullOrWhiteSpace (MidnightBot.GetRndGoogleAPIKey ()))
                throw new ArgumentNullException (nameof (query));

            var match = new Regex ("(?:youtu\\.be\\/|list=)(?<id>[\\da-zA-Z\\-_]*)").Match (query);
            if (match.Length > 1)
            {
                return match.Groups["id"].Value.ToString ();
            }

            var link = "https://www.googleapis.com/youtube/v3/search?part=snippet" +
                        "&maxResults=1&type=playlist" +
                       $"&q={Uri.EscapeDataString (query)}" +
                       $"&key={MidnightBot.GetRndGoogleAPIKey ()}";

            var response = await GetResponseStringAsync (link).ConfigureAwait (false);
            var data = JsonConvert.DeserializeObject<YoutubePlaylistSearch> (response);
            JObject obj = JObject.Parse (response);

            return data.items.Length > 0 ? data.items[0].id.playlistId.ToString () : null;
        }

        public static async Task<IList<string>> GetVideoIDs ( string playlist,int number = 50 )
        {
            if (string.IsNullOrWhiteSpace (MidnightBot.GetRndGoogleAPIKey ()))
            {
                throw new ArgumentNullException (nameof (playlist));
            }
            if (number < 1)
                throw new ArgumentOutOfRangeException ();
            string nextPageToken = null;

            List<string> toReturn = new List<string> ();

            do
            {
                var toGet = number > 50 ? 50 : number;
                number -= toGet;
                var link =
                    $"https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails" +
                    $"&maxResults={toGet}" +
                    $"&playlistId={playlist}" +
                    $"&key={MidnightBot.GetRndGoogleAPIKey ()}";
                if (!string.IsNullOrWhiteSpace (nextPageToken))
                    link += $"&pageToken={nextPageToken}";
                var response = await GetResponseStringAsync (link).ConfigureAwait (false);
                var data = await Task.Run (() => JsonConvert.DeserializeObject<PlaylistItemsSearch> (response)).ConfigureAwait (false);
                nextPageToken = data.nextPageToken;
                toReturn.AddRange (data.items.Select (i => i.contentDetails.videoId));
            } while (number > 0 && !string.IsNullOrWhiteSpace (nextPageToken));

            return toReturn;
        }


        public static async Task<string> GetDanbooruImageLink ( string tag )
        {
            var rng = new Random ();

            if (tag == "loli") //loli doesn't work for some reason atm
                tag = "flat_chest";

            var link = $"http://danbooru.donmai.us/posts?" +
                        $"tags=order:random+-guro+";//$"page={rng.Next(0, 15)}";
            if (!string.IsNullOrWhiteSpace (tag))
                link += $"{tag.Replace (" ","_")}";

            var webpage = await GetResponseStringAsync (link).ConfigureAwait (false);
            var matches = Regex.Matches (webpage,"data-file-url=\"(?<id>.*?)\"");

            if (matches.Count == 0)
                return null;
            string outlink = $"http://danbooru.donmai.us" +
                 $"{matches[rng.Next(0, matches.Count)].Groups["id"].Value}";
            //outlink = outlink.Replace("sample/sample-", "");
            return outlink;
        }

        public static async Task<string> GetDerpibooruImageLink ( string tags )
        {
            var max = 101;
            var rng = new Random ();
            GETIMAGE:
            var url = $"https://derpiboo.ru/search.json?q=explicit%2C-guro";
            if (!string.IsNullOrWhiteSpace (tags))
                url += ($"%2C{(tags.Replace ("+","%2C").Replace (" ","+"))}");
            url += ($"&page={rng.Next (0,max)}&key=h-jh3W2FA7xpssjyyt1y");
            var webpage = await GetResponseStringAsync (url).ConfigureAwait (false);
            var json = new WebClient ().DownloadString (url);
            //"large":"//derpicdn.net/img/2016/4/6/1125874/large.png"
            var matches = Regex.Matches (json,@"derpicdn\.net\/img\/[0-9]+\/[0-9]+\/[0-9]+\/[0-9]+\/large\.png");
            if (matches.Count == 0)
            {
                max -= 5;
                goto GETIMAGE;
            }
            var match = matches[rng.Next (0,matches.Count)];
            //https://derpicdn.net/img/2016/4/6/1125874/large.png
            return $"https://{match.Value}";
        }

        public static async Task<string> GetATFBooruImageLink ( string tag )
        {
            var rng = new Random ();

            var link = $"http://atfbooru.ninja/posts?" +
                        $"tags=order:random+-guro+";//"page={rng.Next(0, 15)}";
            if (!string.IsNullOrWhiteSpace ( tag))
                link += $"{tag.Replace (" ","_")}";

            var webpage = await GetResponseStringAsync (link).ConfigureAwait (false);
            var matches = Regex.Matches (webpage,"data-file-url=\"(?<id>.*?)\"");

            if (matches.Count == 0)
                return null;
            string outlink = $"http://atfbooru.ninja" +
                   $"{matches[rng.Next(0, matches.Count)].Groups["id"].Value}";
             //outlink = outlink.Replace("sample/sample-", "");
            return outlink;
        }

        public static async Task<string> GetR34ImageLink(string tag)
        {
            tag = tag.Replace(" ", "_");
            Random rng = new Random();
            var link = $"http://rule34.paheal.net/api/danbooru/find_posts?{(tag.Trim() == "" ? "" : $"tags={tag}")}";
            XmlDocument results = new XmlDocument();
            string result = await GetResponseStringAsync(link+"&limit=1").ConfigureAwait(false);
            results.LoadXml(result);
            XmlNode post = results.DocumentElement;
            double count = int.Parse(post.Attributes["count"].Value);
            int pages = (int)Math.Ceiling(count / 100);
            string page = rng.Next(1, pages).ToString();
            string link2 = link + $"&page={page}";
            string result2 = await GetResponseStringAsync(link2);
            results = new XmlDocument();
            results.LoadXml(result2);
            int rescount = rng.Next(0,results.FirstChild.ChildNodes.Count-1);
            XmlNode last = results.FirstChild.ChildNodes[rescount];
            return last.Attributes["file_url"].Value;
        }

    public static async Task<string> GetGelbooruImageLink ( string tag )
        {
            var headers = new Dictionary<string,string> () {
                {"User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1"},
                {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
            };
            var url = $"http://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags=-guro+{tag.Replace (" ","_")}";
            var webpage = await GetResponseStringAsync (url,headers).ConfigureAwait (false);
            var matches = Regex.Matches (webpage,"file_url=\"(?<url>.*?)\"");
            if (matches.Count == 0)
                return null;
            var rng = new Random ();
            var match = matches[rng.Next (0,matches.Count)];
            return matches[rng.Next (0,matches.Count)].Groups["url"].Value;
        }

        public static async Task<string> GetSafebooruImageLink ( string tag )
        {
            var rng = new Random ();
            var url = $"http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=100&tags=-guro+{tag.Replace (" ","_")}";
            var webpage = await GetResponseStringAsync (url).ConfigureAwait (false);
            var matches = Regex.Matches (webpage,"file_url=\"(?<url>.*?)\"");
            if (matches.Count == 0)
                return null;
            var match = matches[rng.Next (0,matches.Count)];
            return matches[rng.Next (0,matches.Count)].Groups["url"].Value;
        }

        public static async Task<string> GetRule34ImageLink ( string tag )
        {
            var rng = new Random ();
            var url = $"http://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=100&tags=-guro+{tag.Replace (" ","_")}";
            var webpage = await GetResponseStringAsync (url).ConfigureAwait (false);
            var matches = Regex.Matches (webpage,"file_url=\"(?<url>.*?)\"");
            if (matches.Count == 0)
                return null;
            var match = matches[rng.Next (0,matches.Count)];
            return "http:" + matches[rng.Next (0,matches.Count)].Groups["url"].Value;
        }

        internal static async Task<string> GetE621ImageLink ( string tags )
        {
            try
            {
                var headers = new Dictionary<string,string> () {
                    {"User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1"},
                    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                };
                var data = await GetResponseStreamAsync (
                    "http://e621.net/post/index.xml?tags=" + Uri.EscapeUriString (tags) + "%20-guro%20order:random&limit=1",
                    headers);
                var doc = XDocument.Load (data);
                return doc.Descendants ("file_url").FirstOrDefault ().Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Error in e621 search: \n" + ex);
                return "Error, do you have too many tags?";
            }
        }

        public static async Task<string> GetBronibooruImageLink ( string tag )
        {
            var rng = new Random ();

            var link = $"http://www.bronibooru.com/posts/random?";
            if (!string.IsNullOrWhiteSpace (tag))
                link += $"tags={tag.Replace (" ","_")}";

            var webpage = await GetResponseStringAsync (link).ConfigureAwait (false);
            var match = Regex.Match (webpage,"data-large-file-url=\"(?<id>.*?)\"");

            if (!match.Success)
                return null;
            return $"http:{match.Groups["id"].Value}";
        }

        public static Task<string> GetRandomGagPost()
        {
            var ta = Task.Run(() =>
            {
                var client = new RestClient("http://9gag.com/random");
                var request = new RestRequest(Method.GET);

                IRestResponse response = client.Execute(request);
                return response.ResponseUri.AbsoluteUri;
            });
            return ta;
        }

        public static async Task<string> ShortenUrl ( string url )
        {
            if (string.IsNullOrWhiteSpace (MidnightBot.GetRndGoogleAPIKey ()))
                return url;
            try
            {
                var httpWebRequest =
                    (HttpWebRequest)WebRequest.Create ("https://www.googleapis.com/urlshortener/v1/url?key=" +
                                                       MidnightBot.GetRndGoogleAPIKey ());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter (await httpWebRequest.GetRequestStreamAsync ().ConfigureAwait (false)))
                {
                    var json = "{\"longUrl\":\"" + Uri.EscapeDataString(url) + "\"}";
                    streamWriter.Write (json);
                }

                var httpResponse = (await httpWebRequest.GetResponseAsync ().ConfigureAwait (false)) as HttpWebResponse;
                var responseStream = httpResponse.GetResponseStream ();
                using (var streamReader = new StreamReader (responseStream))
                {
                    var responseText = await streamReader.ReadToEndAsync ().ConfigureAwait (false);
                    return Regex.Match (responseText,@"""id"": ?""(?<id>.+)""").Groups["id"].Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Shortening of this url failed: " + url);
                Console.WriteLine (ex.ToString ());
                return url;
            }
        }

        public static string ShowInPrettyCode<T> ( IEnumerable<T> items,Func<T,string> howToPrint,int cols = 3 )
        {
            var i = 0;
            return "```xl\n" + string.Join ("\n",items.GroupBy(item => (i++) / cols)
                                      .Select(ig => string.Concat(ig.Select(el => howToPrint(el)))))
                                      + $"\n```";
        }
}
}