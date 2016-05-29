using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Games.Commands
{
    /// <summary>
    /// Flower picking/planting idea is given to me by its
    /// inceptor Violent Crumble from Game Developers League discord server
    /// (he has !cookie and !nom) Thanks a lot Violent!
    /// Check out GDL (its a growing gamedev community):
    /// https://discord.gg/0TYNJfCU4De7YIk8
    /// </summary>
    class PlantPick : DiscordCommand
    {
        public PlantPick ( DiscordModule module ) : base (module)
        {

        }

        //channelid/messageid pair
        ConcurrentDictionary<ulong,Message> plantedFlowerChannels = new ConcurrentDictionary<ulong,Message> ();
        public string BotName { get; set; } = MidnightBot.BotName;
        private object locker = new object ();

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "pick")
                .Description ($"Nimmt einen in diesem Channel hinterlegten {MidnightBot.Config.CurrencyName}.")
                .Do (async e =>
                {
                    Message msg;

                    await e.Message.Delete ().ConfigureAwait (false);
                    if (!plantedFlowerChannels.TryRemove (e.Channel.Id,out msg))
                        return;
 
                    await msg.Delete ().ConfigureAwait (false);
                    await FlowersHandler.AddFlowersAsync (e.User,"Took a dollar.",1,true).ConfigureAwait (false);
                    msg = await e.Channel.SendMessage ($"**{e.User.Name}** nahm einen {MidnightBot.Config.CurrencyName}!").ConfigureAwait (false);
                    await Task.Delay (10000).ConfigureAwait (false);
                    await msg.Delete ().ConfigureAwait (false);
                });

            cgb.CreateCommand (Module.Prefix + "plant")
                .Description ($"Gib einen {MidnightBot.Config.CurrencyName} aus, um in in diesen Channel zu legen. (Wenn der Bot neustartet, oder crashed, ist der {MidnightBot.Config.CurrencyName} verloren)")
                .Do (async e =>
                {
                    lock (locker)
                    {
                        if (plantedFlowerChannels.ContainsKey (e.Channel.Id))
                        {
                            e.Channel.SendMessage ($"Hier liegt bereits ein {MidnightBot.Config.CurrencyName} in diesem Channel.");
                            return;
                        }
                        var removed = FlowersHandler.RemoveFlowers (e.User,"Put a dollar down.",1);
                        if (!removed)
                        {
                            e.Channel.SendMessage ($"Du hast keinen {MidnightBot.Config.CurrencyName}s.").Wait ();
                            return;
                        }

                        var rng = new Random ();
                        var file = Directory.GetFiles ("data/currency_images").OrderBy (s => rng.Next ()).FirstOrDefault ();
                        Message msg;
                        if (file == null)
                            msg = e.Channel.SendMessage (MidnightBot.Config.CurrencySign).GetAwaiter ().GetResult ();
                        else
                            msg = e.Channel.SendFile (file).GetAwaiter ().GetResult ();
                        plantedFlowerChannels.TryAdd (e.Channel.Id,msg);
                    }

                    var vowelFirst = new[] { 'a','e','i','o','u' }.Contains (MidnightBot.Config.CurrencyName[0]);
                    var msg2 = await e.Channel.SendMessage ($"Oh wie nett! **{e.User.Name}** hinterlies {(vowelFirst ? "einen" : "einen")} {MidnightBot.Config.CurrencyName}. Nimm ihn per {Module.Prefix}pick");
                    await Task.Delay (20000).ConfigureAwait (false);
                    await msg2.Delete ().ConfigureAwait (false);
                });
        }
    }
}