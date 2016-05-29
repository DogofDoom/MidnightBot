using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;

namespace NadekoBot.Modules.NSFW
{
    internal class NSFWModule : DiscordModule
    {

        private readonly Random rng = new Random ();

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.NSFW;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                cgb.CreateCommand (Prefix + "hentai")
                    .Description ("Zeigt ein zufälliges NSFW Hentai Bild von gelbooru und danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags)\n**Benutzung**: ~hentai yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var gel = await SearchHelper.GetGelbooruImageLink ("rating%3Aexplicit+" + tag).ConfigureAwait (false);
                        if (gel != null)
                            await e.Channel.SendMessage (":heart: Gelbooru: " + gel)
                                           .ConfigureAwait (false);
                        var dan = await SearchHelper.GetDanbooruImageLink ("rating%3Aexplicit+" + tag).ConfigureAwait (false);
                        if (dan != null)
                            await e.Channel.SendMessage (":heart: Danbooru: " + dan)
                                           .ConfigureAwait (false);
                        var atf = await SearchHelper.GetAtfbooruImageLink("rating%3Aexplicit+" + tag).ConfigureAwait(false);
                        if (atf != null)
                            await e.Channel.SendMessage(":heart: ATFbooru: " + atf)
                                           .ConfigureAwait(false);
                        if (dan == null && gel == null && atf == null)
                            await e.Channel.SendMessage ("`Keine Ergebnisse.`");
                    });

                cgb.CreateCommand(Prefix + "atfbooru")
                    .Description("Shows a random hentai image from atfbooru with a given tag. Tag is optional but preffered. (multiple tags are appended with +)\n**Usage**: ~danbooru yuri+kissing")
                    .Parameter("tag", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        var tag = e.GetArg("tag")?.Trim() ?? "";
                        var link = await SearchHelper.GetAtfbooruImageLink(tag).ConfigureAwait(false);
                        if (string.IsNullOrWhiteSpace(link))
                            await e.Channel.SendMessage("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage(link).ConfigureAwait(false);
                    });

                cgb.CreateCommand (Prefix + "danbooru")
                    .Description ("Zeigt ein zufälliges Hentai Bild von danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags)\n**Benutzung**: ~danbooru yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetDanbooruImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "gelbooru")
                    .Description ("Zeigt ein zufälliges Hentai Bild von gelbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags)\n**Benutzung**: ~gelbooru yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetGelbooruImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "rule34")
                    .Description ("Zeigt ein zufälliges Hentai Bild von rule34.xx  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags.\n**Benutzung**: ~rule34 yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        var link = await SearchHelper.GetRule34ImageLink (tag).ConfigureAwait (false);
                        if (string.IsNullOrWhiteSpace (link))
                            await e.Channel.SendMessage ("Suche ergab keine Ergebnisse ;(");
                        else
                            await e.Channel.SendMessage (link).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "e621")
                    .Description ("Zeigt ein zufälliges Hentai Bild von e621.net mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze Leerzeichen für mehrere Tags.\n**Benutzung**: ~e621 yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        await e.Channel.SendMessage (await SearchHelper.GetE621ImageLink (tag).ConfigureAwait (false)).ConfigureAwait (false);
                    });
                cgb.CreateCommand (Prefix + "derpi")
                    .Description ("Zeigt ein zufälliges Hentai Bild von derpiboo.ru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags.\n**Benutzung**: ~derpi yuri+kissing")
                    .Parameter ("tag",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var tag = e.GetArg ("tag")?.Trim () ?? "";
                        await e.Channel.SendIsTyping ().ConfigureAwait (false);
                        await e.Channel.SendMessage (await SearchHelper.GetDerpibooruImageLink (tag).ConfigureAwait (false)).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "boobs")
                    .Description ("Erwachsenen Inhalt.")
                    .Do (async e =>
                    {
                        try
                        {
                            var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync ($"http://api.oboobs.ru/boobs/{rng.Next (0,9380)}").ConfigureAwait (false))[0];
                            await e.Channel.SendMessage ($"http://media.oboobs.ru/{ obj["preview"].ToString () }").ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢 {ex.Message}").ConfigureAwait (false);
                        }
                    });
                cgb.CreateCommand (Prefix + "butts")
                    .Alias (Prefix + "ass",Prefix + "butt")
                    .Description ("Erwachsenen Inhalt.")
                    .Do (async e =>
                    {
                        try
                        {
                            var obj = JArray.Parse (await SearchHelper.GetResponseStringAsync ($"http://api.obutts.ru/butts/{rng.Next (0,3373)}").ConfigureAwait (false))[0];
                            await e.Channel.SendMessage ($"http://media.obutts.ru/{ obj["preview"].ToString () }").ConfigureAwait (false);
                        }
                        catch (Exception ex)
                        {
                            await e.Channel.SendMessage ($"💢 {ex.Message}").ConfigureAwait (false);
                        }
                    });
            });
        }
    }
}