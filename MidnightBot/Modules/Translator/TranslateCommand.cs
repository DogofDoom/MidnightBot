using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Translator.Helpers;
using System;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Translator
{
    class TranslateCommand : DiscordCommand
    {
        public TranslateCommand ( DiscordModule module ) : base (module) { }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "translate")
                .Alias (Module.Prefix + "trans")
                .Description ($"Übersetzt Text von>zu. Von der gegebenen Sprache in die Zielsprache. | `{Module.Prefix}trans en>fr Hello`")
                .Parameter ("langs",ParameterType.Required)
                .Parameter ("text",ParameterType.Unparsed)
                .Do ( TranslateFunc());
        }
        private GoogleTranslator t = new GoogleTranslator ();
        private Func<CommandEventArgs,Task> TranslateFunc () => async e =>
        {
            try
            {
                await e.Channel.SendIsTyping ().ConfigureAwait (false);
                string from = e.GetArg ("langs").ToLowerInvariant ().Split ('>')[0];
                string to = e.GetArg ("langs").ToLowerInvariant ().Split ('>')[1];
                var text = e.GetArg("text")?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                string translation = await t.Translate (text,from,to).ConfigureAwait (false);
                await e.Channel.SendMessage ($"{e.User.Mention} hier ist die Übersetzung:\nUrsprung({from}): {text}\nÜbersetzung({to}):{translation}").ConfigureAwait (false);
            }
            catch (Exception ex)
            {
                Console.WriteLine (ex);
                await e.Channel.SendMessage ("Falsches Eingabeformat, oder etwas ist schief gelaufen...").ConfigureAwait (false);
            }

        };
    }
}