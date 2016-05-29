using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Games.Commands
{

    public static class SentencesProvider
    {
        internal static string GetRandomSentence ()
        {
            var data = DbHandler.Instance.GetAllRows<TypingArticle> ();
            try
            {
                return data.ToList ()[new Random ().Next (0,data.Count ())].Text;
            }
            catch
            {
                return "Noch keine Daten enthalten. Füge welche hinzu mit `typeadd`.";
            }
        }
    }

    public class TypingGame
    {
        public const float WORD_VALUE = 4.5f;
        private readonly Channel channel;
        public string CurrentSentence;
        public bool IsActive;
        private readonly Stopwatch sw;
        private readonly List<ulong> finishedUserIds;

        public TypingGame ( Channel channel )
        {
            this.channel = channel;
            IsActive = false;
            sw = new Stopwatch ();
            finishedUserIds = new List<ulong> ();
        }

        public Channel Channell { get; internal set; }

        internal async Task<bool> Stop ()
        {
            if (!IsActive)
                return false;
            MidnightBot.Client.MessageReceived -= AnswerReceived;
            finishedUserIds.Clear ();
            IsActive = false;
            sw.Stop ();
            sw.Reset ();
            await channel.Send ("Tipp-Wettbewerb gestoppt").ConfigureAwait (false);
            return true;
        }

        internal async Task Start ()
        {
            while (true)
            {
                if (IsActive)
                    return; // can't start running game
                IsActive = true;
                CurrentSentence = SentencesProvider.GetRandomSentence ();
                var i = (int)(CurrentSentence.Length / WORD_VALUE * 1.7f);
                await channel.SendMessage ($":clock2: Nächster Wettbewerb wird {i} Sekunden dauern. Schreibe den fetten Text so schnell wie du kannst.").ConfigureAwait (false);


                var msg = await channel.SendMessage ("Neuer Tipp-Wettbewerb startet in **3**...").ConfigureAwait (false);
                await Task.Delay (1000).ConfigureAwait (false);
                await msg.Edit ("Neuer Tipp-Wettbewerb startet in **2**...").ConfigureAwait (false);
                await Task.Delay (1000).ConfigureAwait (false);
                await msg.Edit ("Neuer Tipp-Wettbewerb startet in **1**...").ConfigureAwait (false);
                await Task.Delay (1000).ConfigureAwait (false);
                await msg.Edit ($":book:**{CurrentSentence.Replace (" "," \x200B")}**:book:").ConfigureAwait (false);
                sw.Start ();
                HandleAnswers ();

                while (i > 0)
                {
                    await Task.Delay (1000).ConfigureAwait (false);
                    i--;
                    if (!IsActive)
                        return;
                }

                await Stop ();
            }
        }

        private void HandleAnswers ()
        {
            MidnightBot.Client.MessageReceived += AnswerReceived;
        }

        private async void AnswerReceived ( object sender,MessageEventArgs e )
        {
            try
            {
                if (e.Channel == null || e.Channel.Id != channel.Id || e.User.Id == MidnightBot.Client.CurrentUser.Id)
                    return;

                var guess = e.Message.RawText;

                var distance = CurrentSentence.LevenshteinDistance (guess);
                var decision = Judge (distance,guess.Length);
                if (decision && !finishedUserIds.Contains (e.User.Id))
                {
                    finishedUserIds.Add (e.User.Id);
                    await channel.Send ($"{e.User.Mention} beendete mit einer Zeit von **{sw.Elapsed.Seconds}**  Sekunden und mit { distance } Fehlern, **{ CurrentSentence.Length / WORD_VALUE / sw.Elapsed.Seconds * 60 }** WPM!").ConfigureAwait (false);
                    if (finishedUserIds.Count % 2 == 0)
                    {
                        await e.Channel.SendMessage ($":exclamation: `Viele sind schon fertig. Hier ist der Text für die, die noch schreiben:`\n\n:book:**{CurrentSentence}**:book:").ConfigureAwait (false);
                    }
                }
            }
            catch { }
        }

        private bool Judge ( int errors,int textLength ) => errors <= textLength / 25;

    }

    internal class SpeedTyping : DiscordCommand
    {

        public static ConcurrentDictionary<ulong,TypingGame> RunningContests;

        public SpeedTyping ( DiscordModule module ) : base (module)
        {
            RunningContests = new ConcurrentDictionary<ulong,TypingGame> ();
        }

        public Func<CommandEventArgs,Task> DoFunc () =>
            async e =>
            {
                var game = RunningContests.GetOrAdd (e.User.Server.Id,id => new TypingGame (e.Channel));

                if (game.IsActive)
                {
                    await e.Channel.SendMessage (
                            $"Wettbewerb läuft bereits im Channel " +
                            $"{game.Channell.Mention}.")
                            .ConfigureAwait (false);
                }
                else
                {
                    await game.Start ().ConfigureAwait (false);
                }
            };

        private Func<CommandEventArgs,Task> QuitFunc () =>
            async e =>
            {
                TypingGame game;
                if (RunningContests.TryRemove (e.User.Server.Id,out game))
                {
                    await game.Stop ().ConfigureAwait (false);
                    return;
                }
                await e.Channel.SendMessage ("Es läuft derzeit kein Wettbewerb.").ConfigureAwait (false);
            };

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "typestart")
                .Description ("Startet einen Tipp-Wettbewerb.")
                .Do (DoFunc ());

            cgb.CreateCommand (Module.Prefix + "typestop")
                .Description ("Stoppt einen Tipp-Wettbewerb auf dem derzeitigen Channel.")
                .Do (QuitFunc ());

            cgb.CreateCommand (Module.Prefix + "typeadd")
                .Description ("Fügt einen neuen Text hinzu. Owner only.")
                .Parameter ("text",ParameterType.Unparsed)
                .Do (async e =>
                {
                    if (!MidnightBot.IsOwner (e.User.Id) || string.IsNullOrWhiteSpace (e.GetArg ("text")))
                        return;

                    DbHandler.Instance.InsertData (new TypingArticle
                    {
                        Text = e.GetArg ("text"),
                        DateAdded = DateTime.Now
                    });

                    await e.Channel.SendMessage ("Neuer Text hinzugefügt.").ConfigureAwait (false);
                });

            //todo add user submissions
        }
    }
}