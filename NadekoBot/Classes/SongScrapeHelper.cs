
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;

namespace NadekoBot.Classes
{
    public static class SongScrapeHelper
    {
        public static ParsedSongInfo ParseQuery ( String queryString )
        {
            //maybe it is already a youtube url, in which case we will just extract the id and prepend it with http://v.redonapp.com/yt/
            var match = new Regex ("(?:youtu\\.be\\/|v=)(?<id>[\\da-zA-Z\\-_]*)").Match (queryString);
            if (match.Length > 1)
            {
                return ParseVideoPage (match.Groups["id"].Value);
            }

            //or not.. in which case, search RedonApp!
            // TODO

            return null;
        }

        public static ParsedSongInfo ParseVideoPage ( String id )
        {
            IWebDriver ffDriver = new FirefoxDriver ();
            var driverService = PhantomJSDriverService.CreateDefaultService ();
            driverService.HideCommandPromptWindow = true;

            var options = new PhantomJSOptions ();
            options.AddAdditionalCapability ("phantomjs.page.settings.userAgent","Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.94 Safari/537.36");

            var phantom = new PhantomJSDriver (driverService,options);

            phantom.Navigate ().GoToUrl ($"http://v.redonapp.com/yt/{id}");

            String Title = phantom.FindElement (By.Id ("result_title")).Text;
            String Uri = phantom.FindElement (By.Id ($"o_{id}")).GetAttribute ("rel");

            ParsedSongInfo info = new ParsedSongInfo (Uri,Title);
            return info;

        }
    }

    public class ParsedSongInfo
    {
        public String uri;
        public String title;

        public ParsedSongInfo ( String uri,String title )
        {
            this.uri = uri;
            this.title = title;
        }
    }
}