using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Gambling
{
    internal class FlipCoinCommand : DiscordCommand
    {

        public FlipCoinCommand(DiscordModule module) : base (module)
    {
    }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "flip")
                .Description("Wirft eine/mehrere Münze(n) - Kopf oder Zahl, und zeigt ein Bild. | `$flip` or `$flip 3`")
                .Parameter("count", ParameterType.Optional)
                .Do(FlipCoinFunc());

            cgb.CreateCommand(Module.Prefix + "betflip")
                .Alias(Prefix + "bf")
                .Description($"Wette auf das Ergebnis: Kopf, oder Zahl. Beim richtigen Raten werden die gesetzten {MidnightBot.Config.CurrencyName} verdoppelt. | `{Prefix}bf 5 kopf` or `{Prefix}bf 3 z`")
                .Parameter("amount", ParameterType.Required)
                .Parameter("guess", ParameterType.Required)
                .Do(BetFlipCoinFunc());
        }



        private readonly Random rng = new Random();
        public Func<CommandEventArgs, Task> BetFlipCoinFunc() => async e =>
        {

            var amountstr = e.GetArg("amount").Trim();

            var guessStr = e.GetArg("guess").Trim().ToUpperInvariant();
            if (guessStr != "K" && guessStr != "Z" && guessStr != "KOPF" && guessStr != "ZAHL")
                return;

            int amount;
            if (!int.TryParse(amountstr, out amount) || amount< 1)
                return;

            var userFlowers = GamblingModule.GetUserFlowers(e.User.Id);

            if (userFlowers<amount)
            {
                await e.Channel.SendMessage($"{e.User.Mention} Du hast nicht genug {MidnightBot.Config.CurrencyName}s. Du hast nur {userFlowers}{MidnightBot.Config.CurrencySign}.").ConfigureAwait(false);
                return;
            }

            await FlowersHandler.RemoveFlowers(e.User, "Betflip Gamble", (int)amount, true).ConfigureAwait(false);
            //kopf = true
            //zahl = false
            if (guessStr == "KOPF" || guessStr == "K")
            {
                var guess = guessStr == "KOPF" || guessStr == "K";
                bool result = false;
                if (rng.Next(0, 4) == 1)
                {
                    await e.Channel.SendFile("heads.png", Properties.Resources.heads.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait(false);
                    result = true;
                }
                else
                {
                    await e.Channel.SendFile("tails.png", Properties.Resources.tails.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait(false);
                }

                string str;
                if (guess == result)
                {
                    str = $"{e.User.Mention}`Du hast richtig geraten!` Du hast {amount * 2}{MidnightBot.Config.CurrencySign} gewonnen.";
                    await FlowersHandler.AddFlowersAsync(e.User, "Betflip Gamble", amount * 2, true).ConfigureAwait(false);

                }
                else
                    str = $"{e.User.Mention}`Viel Glück beim nächsten Mal.`";

                await e.Channel.SendMessage(str).ConfigureAwait(false);
            }
            else
            {
                var guess = guessStr == "ZAHL" || guessStr == "Z";
                bool result = false;
                if (rng.Next(0, 4) == 1)
                {
                    await e.Channel.SendFile("tails.png", Properties.Resources.heads.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait(false);
                    result = true;
                }
                else
                {
                    await e.Channel.SendFile("heads.png", Properties.Resources.tails.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait(false);
                }

                string str;
                if (guess == result)
                {
                    str = $"{e.User.Mention}`Du hast richtig geraten!` Du hast {amount * 3}{MidnightBot.Config.CurrencySign} gewonnen.";
                    await FlowersHandler.AddFlowersAsync(e.User, "Betflip Gamble", amount * 3, true).ConfigureAwait(false);

                }
                else
                    str = $"{e.User.Mention}`Viel Glück beim nächsten Mal.`";

                await e.Channel.SendMessage(str).ConfigureAwait(false);
            }
        };

        public Func<CommandEventArgs, Task> FlipCoinFunc() => async e =>
        {

            if (e.GetArg("count") == "")
            {
                if (rng.Next(0, 2) == 1)
                    await e.Channel.SendFile("heads.png", Properties.Resources.heads.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait (false);
                else
                    await e.Channel.SendFile("tails.png", Properties.Resources.tails.ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait (false);
            }
            else {
                int result;
                if (int.TryParse(e.GetArg("count"), out result)) {
                    if (result > 10)
                        result = 10;
                    var imgs = new Image[result];
                    for (var i = 0; i < result; i++)
                    {
                        imgs[i] = rng.Next(0, 2) == 0 ?
                                    Properties.Resources.tails :
                                    Properties.Resources.heads;
                    }
                    await e.Channel.SendFile($"{result} coins.png", imgs.Merge().ToStream(System.Drawing.Imaging.ImageFormat.Png)).ConfigureAwait (false);
                    return;
                }
                await e.Channel.SendMessage("Ungültige Nummer.").ConfigureAwait (false);
            }
        };
    }
}
