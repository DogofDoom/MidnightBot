using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Games.Commands.Trivia;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MidnightBot.Modules.Games.Commands
{
    internal class TriviaCommands : DiscordCommand
    {
        public static ConcurrentDictionary<ulong,TriviaGame> RunningTrivias = new ConcurrentDictionary<ulong,TriviaGame> ();

        public TriviaCommands ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "t")
                .Description ($"Startet ein Quiz. Du kannst nohint hinzufügen um Tipps zu verhindern." +
                               "Erster Spieler mit 10 Punkten gewinnt. 30 Sekunden je Frage." +
                              $"\n**Benutzung**:`{Module.Prefix}t nohint` oder `{Module.Prefix}t 5 nohint`")
                 .Parameter ("args",ParameterType.Multiple)
                 .AddCheck(SimpleCheckers.ManageMessages())
                 .Do (async e =>
                  {
                      TriviaGame trivia;
                      if (!RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                      {
                          var showHints = !e.Args.Contains ("nohint");
                          var number = e.Args.Select (s =>
                          {
                              int num;
                              return new Tuple<bool,int> (int.TryParse (s,out num),num);
                          }).Where (t => t.Item1).Select (t => t.Item2).FirstOrDefault ();
                          if (number < 0)
                              return;
                          var triviaGame = new TriviaGame (e,showHints,number == 0 ? 10 : number);
                          if (RunningTrivias.TryAdd (e.Server.Id,triviaGame))
                              await e.Channel.SendMessage ($"**Trivia Game gestartet! {triviaGame.WinRequirement} Punkte benötigt um zu gewinnen.**").ConfigureAwait (false);
                          else
                              await triviaGame.StopGame ().ConfigureAwait (false);
                      }
                      else
                          await e.Channel.SendMessage ("Auf diesem Server läuft bereits ein Quiz.\n" + trivia.CurrentQuestion).ConfigureAwait (false);
                  });

            cgb.CreateCommand (Module.Prefix + "tl")
                .Description ("Zeigt eine Rangliste des derzeitigen Quiz.")
                .Do (async e =>
                {
                    TriviaGame trivia;
                    if (RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                        await e.Channel.SendMessage (trivia.GetLeaderboard ()).ConfigureAwait (false);
                    else
                        await e.Channel.SendMessage ("Es läuft kein Quiz auf diesem Server.").ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "tq")
                .Description ("Beendet Quiz nach der derzeitgen Frage.")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Do (async e =>
                {
                    TriviaGame trivia;
                    if (RunningTrivias.TryGetValue (e.Server.Id,out trivia))
                    {
                        await trivia.StopGame ().ConfigureAwait (false);
                    }
                    else
                        await e.Channel.SendMessage ("Es läuft kein Quiz auf diesem Server.").ConfigureAwait (false);
                });
        }
    }
}