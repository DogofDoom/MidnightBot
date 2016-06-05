using Discord.Commands;
using Mathos.Parser;
using MidnightBot.Classes;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Searches.Commands
{
    class CalcCommand : DiscordCommand
    {
        public CalcCommand ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "calculate")
                .Alias (Module.Prefix + "calc")
                .Description ("Berechnet eine mathematische Angabe")
                .Parameter ("expression",ParameterType.Unparsed)
                .Do (EvalFunc ());

            cgb.CreateCommand(Module.Prefix + "calclist")
                .Description("List operations of parser")
                .Do(async e=> {
                    await e.Channel.SendMessage (string.Join ("\n",parser.OperatorList) + "\n" + string.Join ("\n",parser.LocalFunctions.Keys));
                });
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
            await e.Channel.SendMessage ($"⌨ Aufgabe:`{expression}`\n⚙`Ergebnis: {answer}`");
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
                return $"\"{expression}\" war nicht richtig formatiert.";
            }
        }
        
        class CustomParser : MathParser
        {
            public CustomParser () : base ()
            {
                OperatorList.Add ("!");
                OperatorList.Add ("_");
                OperatorAction.Add ("!",( x,y ) => Factorial (x));
                OperatorAction.Add ("_",( x,y ) => 10.130M);
                LocalFunctions["log"] = (input) =>
                {
                    if (input.Length == 1)
                    {
                        return (decimal)Math.Log((double)input[0]);
                    }
                    else if (input.Length == 2)
                    {
                        return (decimal)Math.Log((double)input[1], (double)input[0]);
                    }
                    else
                    {
                        return 0; // false
                    }
                };
                LocalFunctions.Add("ln", x =>(decimal) Math.Log((double) x[0]));
                LocalFunctions.Add("logn", x => (decimal)Math.Log((double)x[0]));
            }

            static decimal Factorial ( decimal x )
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