using MidnightBot.Classes;
using System;
using Mathos.Parser;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace MidnightBot.Modules.Searches.Commands
{
    class EvalCommand : DiscordCommand
    {
        public EvalCommand ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "evaluate")
                .Alias (Module.Prefix + "eval")
                .Description ("Berechnet eine mathematische Angabe")
                .Parameter ("expression",ParameterType.Unparsed)
                .Do (EvalFunc ());
        }
        
        private CustomParser parser = new CustomParser ();
        private Func<CommandEventArgs,Task> EvalFunc () => async e =>
        {
            string expression = e.GetArg ("expression")?.Trim ();
            if (string.IsNullOrWhiteSpace (expression))
            { 
                await e.Channel.SendMessage ("Berechnung muss angegeben werden");
                return;
            }
            string answer = evaluate (expression);
            if (answer == null)
            {
                await e.Channel.SendMessage ($"Aufgabe {expression} konnte nicht berechnet werden");
                return;
            }
            await e.Channel.SendMessage ($"Aufgabe:`{expression}`\n`Ergebnis: {answer}`");
        };

        private string evaluate ( string expression )
        {
            //check for factorial
            expression = Regex.Replace (expression,@"\d+!",x => x.Value + "0");
            try
            {
                string result = parser.Parse (expression).ToString ();
                return result;
            }
            catch (OverflowException)
            {
                return $"Overflow error on {expression}";
            }
            catch (FormatException)
            {
                return $"\"{expression}\" was not formatted correctly";
            }
        }
        
        class CustomParser : MathParser
        {
            public CustomParser () : base ()
            {
                OperatorList.Add ("!");
                OperatorList.Add ("_");
                OperatorAction.Add ("!",( x,y ) => factorial (x));
                OperatorAction.Add ("_",( x,y ) => 10.130M);
            }

            static decimal factorial ( decimal x )
            {
                decimal y = x - 1;
                while (y > 0)
                {
                    x = x * y--;
                }
                return x;
            }
        }
    }
}