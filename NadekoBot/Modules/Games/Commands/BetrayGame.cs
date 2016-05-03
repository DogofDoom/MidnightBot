using Discord.Commands;
using NadekoBot.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Games.Commands
{
    class BetrayGame : DiscordCommand
    {
        public BetrayGame ( DiscordModule module ) : base (module) { }

        private enum Answers
        {
            Cooperate,
            Betray
        }
        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "betray")
                .Description ("BETRUGS-SPIEL. Betrüge MidnightBot in der nächsten Runde." +
                             "Wenn MidnightBot mit dir zusammenarbeitet - Du bekommst Extra-Punkte, MidnightBot verliert eine Menge." +
                             "Wenn MidnigtBot betrügt - Beide verlieren ein paar Punkte.")
                .Do (async e =>
                {
                    await ReceiveAnswer (e,Answers.Betray).ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "cooperate")
                .Description ("BETRUGS-SPIEL. Arbeite mit MidnightBot zusammen in der nächsten Runde." +
                             "When MiddnightBot mit dir zusammenarbeitet - Ihr beide bekommt Bonus-Punkte." +
                             "Wenn MidnightBot betrügt - Du verlierst eine Menge, MidnightBot bekommt Bonus-Punkte.")
                .Do (async e =>
                {

                    await ReceiveAnswer (e,Answers.Cooperate).ConfigureAwait (false);
                });
        }

        private int userPoints = 0;

        private int UserPoints
        {
            get { return userPoints; }
            set {
                if (value < 0)
                    userPoints = 0;
                userPoints = value;
            }
        }
        private int nadekoPoints = 0;
        private int NadekoPoints
        {
            get { return nadekoPoints; }
            set {
                if (value < 0)
                    nadekoPoints = 0;
                nadekoPoints = value;
            }
        }

        private int round = 0;
        private Answers NextAnswer = Answers.Cooperate;
        private async Task ReceiveAnswer ( CommandEventArgs e,Answers userAnswer )
        {
            var response = userAnswer == Answers.Betray
                ? ":no_entry: `Du hast MidnightBot betrogen` - Du Monster."
                : ":ok: `Du hast mit MidnightBot zusammengearbeitet.` ";
            var currentAnswer = NextAnswer;
            var nadekoResponse = currentAnswer == Answers.Betray
                ? ":no_entry: `Aww MidnightBot hat dich betrogen` - Er ist so niedlich"
                : ":ok: `MidnightBot hat mit dir zusammengearbeitet.`";
            NextAnswer = userAnswer;
            if (userAnswer == Answers.Betray && currentAnswer == Answers.Betray)
            {
                NadekoPoints--;
                UserPoints--;
            }
            else if (userAnswer == Answers.Cooperate && currentAnswer == Answers.Cooperate)
            {
                NadekoPoints += 2;
                UserPoints += 2;
            }
            else if (userAnswer == Answers.Betray && currentAnswer == Answers.Cooperate)
            {
                NadekoPoints -= 3;
                UserPoints += 3;
            }
            else if (userAnswer == Answers.Cooperate && currentAnswer == Answers.Betray)
            {
                NadekoPoints += 3;
                UserPoints -= 3;
            }

            await e.Channel.SendMessage ($"**Runde {++round}**\n" +
                                        $"{response}\n" +
                                        $"{nadekoResponse}\n" +
                                        $"--------------------------------\n" +
                                        $"MidnightBot hat {NadekoPoints} Punkte." +
                                        $"Du hast {UserPoints} Punkte." +
                                        $"--------------------------------\n")
                                        .ConfigureAwait (false);
            if (round < 10)
                return;
            if (nadekoPoints == userPoints)
                await e.Channel.SendMessage ("Unentschieden").ConfigureAwait (false);
            else if (nadekoPoints > userPoints)
                await e.Channel.SendMessage ("MidnightBot hat gewonnen.").ConfigureAwait (false);
            else
                await e.Channel.SendMessage ("Du hast gewonnen.").ConfigureAwait (false);
            nadekoPoints = 0;
            userPoints = 0;
            round = 0;
        }
    }

    public class BetraySetting
    {
        private string Story = $"{0} haben eine Bank überfallen und wurden von der Polizei gefasst.." +
                               $"Die Ermittler geben dir eine Wahl:\n" +
                               $"Du kannst entweder mit deinem Freund kooperieren(>COOPERATE) und " +
                               $"nicht erzählen, wessen Idee es war, oder du kannst deine Freunde betrügen. (>BETRAY)" +
                               $"Je nach Antwort deines Freundes variiert die Strafe.";

        public int DoubleCoop = 1;
        public int DoubleBetray = -1;
        public int BetrayCoop_Betrayer = 3;
        public int BetrayCoop_Cooperater = -3;

        public string GetStory ( IEnumerable<string> names ) => String.Format (Story,string.Join (", ",names));
    }
}