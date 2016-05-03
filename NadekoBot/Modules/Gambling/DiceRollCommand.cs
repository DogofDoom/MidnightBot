using Discord.Commands;
using NadekoBot.Classes;
using NadekoBot.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Gambling
{
    internal class DiceRollCommand : DiscordCommand
    {

        public DiceRollCommand ( DiscordModule module ) : base (module) { }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "roll")
                .Description ("Würfelt von 0-100. Wenn du eine Zahl [x] angibst werden bis zu 30 normale Würfel geworfen." +
                              " Wenn du 2 Zahlen mit einem d trennst (xdy) werden x Würfel von 0 bis y geworfen.\n**Benutzung**: $roll oder $roll 7 oder $roll 3d5")
                .Parameter ("num",ParameterType.Optional)
                .Do (RollFunc ());
            cgb.CreateCommand (Module.Prefix + "nroll")
                .Description ("Würfelt in einer gegebenen Zahlenreichweite.\n**Benutzung**: `$nroll 5` (rolls 0-5) or `$nroll 5-15`")
                .Parameter ("range",ParameterType.Required)
                .Do (NRollFunc ());
        }

        private Image GetDice ( int num ) => num != 10
                                          ? Properties.Resources.ResourceManager.GetObject ("_" + num) as Image
                                          : new[]
                                            {
                                              (Properties.Resources.ResourceManager.GetObject("_" + 1) as Image),
                                              (Properties.Resources.ResourceManager.GetObject("_" + 0) as Image),
                                            }.Merge ();

        Regex dndRegex = new Regex (@"(?<n1>\d+)d(?<n2>\d+)",RegexOptions.Compiled);
        private Func<CommandEventArgs,Task> RollFunc ()
        {
            var r = new Random ();
            return async e =>
            {
                var arg = e.Args[0]?.Trim ();
                if (string.IsNullOrWhiteSpace (arg))
                {
                    var gen = r.Next (0,101);

                    var num1 = gen / 10;
                    var num2 = gen % 10;

                    var imageStream = new Image[2] { GetDice (num1),GetDice (num2) }.Merge ().ToStream (ImageFormat.Png);

                    await e.Channel.SendFile ("dice.png",imageStream).ConfigureAwait (false);
                    return;
                }
                Match m;
                if ((m = dndRegex.Match (arg)).Length != 0)
                {
                    int n1;
                    int n2;
                    if (int.TryParse (m.Groups["n1"].ToString (),out n1) &&
                    int.TryParse (m.Groups["n2"].ToString (),out n2) &&
                    n1 <= 50 && n2 <= 100000 && n1 > 0 && n2 > 0)
                    {
                        var arr = new int[n1];
                        for (int i = 0; i < n1; i++)
                        {
                            arr[i] = r.Next (1,n2 + 1);
                        }
                        var elemCnt = 0;
                        await e.Channel.SendMessage ($"`Geworfene Würfel:  {n1} {(n1 == 1 ? "Würfel" : "Würfel")} 1-{n2}.`\n`Ergebnis:` " + string.Join (", ",arr.OrderBy (x => x).Select (x => elemCnt++ % 2 == 0 ? $"**{x}**" : x.ToString ())))
                        .ConfigureAwait (false);
                    }
                    return;
                }
                try
                {
                    var num = int.Parse (e.Args[0]);
                    if (num < 1)
                        num = 1;
                    if (num > 30)
                    {
                        await e.Channel.SendMessage ("Du kannst nur bis zu 30 Würfel gleichzeitig werfen.").ConfigureAwait (false);
                        num = 30;
                    }
                    var dices = new List<Image> (num);
                    var values = new List<int> (num);
                    for (var i = 0; i < num; i++)
                    {
                        var randomNumber = r.Next (1,7);
                        var toInsert = dices.Count;
                        if (randomNumber == 6 || dices.Count == 0)
                            toInsert = 0;
                        else if (randomNumber != 1)
                            for (var j = 0; j < dices.Count; j++)
                            {
                                if (values[j] < randomNumber)
                                {
                                toInsert = j;
                                    break;
                                }
                            }
                        dices.Insert (toInsert,GetDice (randomNumber));
                        values.Insert (toInsert,randomNumber);
                    }
                    var bitmap = dices.Merge ();
                    await e.Channel.SendMessage (values.Count + " Würfel geworfen. Insgesamt: **" + values.Sum () + "** Durschnitt: **" + (values.Sum () / (1.0f * values.Count)).ToString ("N2") + "**").ConfigureAwait (false);
                    await e.Channel.SendFile ("dice.png",bitmap.ToStream (ImageFormat.Png)).ConfigureAwait (false);
                }
                catch
                {
                    await e.Channel.SendMessage ("Bitte gib eine Anzahl von Wüfeln zum werfen ein.").ConfigureAwait (false);
                }
            };
        }

        private Func<CommandEventArgs,Task> NRollFunc () =>
            async e =>
            {
                try
                {
                    int rolled;
                    if (e.GetArg ("range").Contains ("-"))
                    {
                        var arr = e.GetArg ("range").Split ('-')
                                                 .Take (2)
                                                 .Select (int.Parse)
                                                 .ToArray ();
                        if (arr[0] > arr[1])
                            throw new ArgumentException ("Erste Zahl muss größer als die Zweite sein.");
                        rolled = new Random ().Next (arr[0],arr[1] + 1);
                    }
                    else
                    {
                        rolled = new Random ().Next (0,int.Parse (e.GetArg ("range")) + 1);
                    }

                    await e.Channel.SendMessage ($"{e.User.Mention} würfelte **{rolled}**.").ConfigureAwait (false);
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage ($":anger: {ex.Message}").ConfigureAwait (false);
                }
            };
    }
}