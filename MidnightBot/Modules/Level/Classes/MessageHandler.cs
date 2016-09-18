using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot;
using MidnightBot.DataModels;
using MidnightBot.Classes;
using System.Collections.Concurrent;

namespace MidnightBot.Modules.Level.Classes
{
    class MessageHandler
    {
        LevelModule module { get; set; }

        public MessageHandler(LevelModule module)
        {
            this.module = module;

        }

        public async void messageReceived(object sender, MessageEventArgs e)
        {
            if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
            {
                if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
                    return;

                if (this.isCommand(e.Message.RawText))
                    return;

                if (e.Message.RawText.Length <= 10)
                    return;

                long uid = Convert.ToInt64(e.User.Id);

                LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                if (ldm != null)
                {
                    int xpToGet = (e.Message.RawText.Length > 25 ? 25 : e.Message.RawText.Length);
                    long currentTick = DateTime.Now.Ticks;
                    long seconds = (currentTick - ldm.timestamp.Ticks) / TimeSpan.TicksPerSecond;

                    if (seconds < 60)
                        return;

                    ldm.CurrentXP += xpToGet;
                    ldm.TotalXP += xpToGet;
                    ldm.timestamp = DateTime.Now;

                    if (ldm.CurrentXP >= getXPForNextLevel(ldm.Level))
                    {
                        ldm.CurrentXP = (ldm.CurrentXP - getXPForNextLevel(ldm.Level));

                        ldm.Level += 1;

                        module.OnLevelChanged(this, new LevelChangedEventArgs(e.Channel, e.User, ldm.Level));

                        await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du hast Level { ldm.Level } erreicht!");
                    }

                    DbHandler.Instance.Save(ldm);
                }
                else
                {
                    int xpToGet = (e.Message.RawText.Length > 25 ? 25 : e.Message.RawText.Length);

                    ldm = new LevelData();
                    ldm.UserId = uid;
                    ldm.Level = 0;
                    ldm.TotalXP = xpToGet;
                    ldm.CurrentXP = xpToGet;
                    ldm.DateAdded = DateTime.Now;
                    ldm.timestamp = DateTime.Now;

                    DbHandler.Instance.Save(ldm);
                }
            }
        }

        public async void messageDeleted(object sender, MessageEventArgs e)
        {
            if (e == null || e.Message == null || e.User == null || e.Channel == null || e.Server == null)
                return;

            if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
                return;
            if (this.isCommand(e.Message.RawText))
                return;
            if (e.Message.RawText.Length <= 10)
                return;
            if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
            {
                var levelChanged = false;

                var uid = Convert.ToInt64(e.User.Id);
                LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                if (ldm != null)
                {
                    int xpToGet = (e.Message.RawText.Length > 25 ? 25 : e.Message.RawText.Length);

                    if ((ldm.TotalXP - xpToGet) <= 0)
                    {
                        ldm.TotalXP = 0;
                    }
                    else
                    {
                        ldm.TotalXP -= xpToGet;
                    }

                    //Calculate new level
                    int copyOfTotalXP = ldm.TotalXP;
                    int calculatedLevel = 0;

                    while (copyOfTotalXP > 0)
                    {
                        int xpNeededForNextLevel = getXPForNextLevel(calculatedLevel);

                        if (copyOfTotalXP > xpNeededForNextLevel)
                        {
                            calculatedLevel++;

                            copyOfTotalXP -= xpNeededForNextLevel;
                        }
                        else
                        {
                            ldm.CurrentXP = copyOfTotalXP;

                            break;
                        }
                    }
                    if (ldm.Level > calculatedLevel)
                        levelChanged = true;

                    ldm.Level = calculatedLevel;

                    DbHandler.Instance.Save(ldm);

                    if (levelChanged)
                        await e.Channel.SendMessage($"Schade { e.User.Mention }, deine Nachricht wurde gelöscht. Daher wird dein Level runtergesetzt. Informationen bekommst du mit {MidnightBot.Config.CommandPrefixes.Level}rank");
                }
            }
        }

        public async void messageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            Console.WriteLine("test1");
            if (e == null || e.Before == null || e.After == null || e.User == null || e.Channel == null || e.Server == null)
                return;
            Console.WriteLine("test2");
            if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
                return;
            Console.WriteLine("test3");
            if (e.After.RawText.Length <= 10 && e.Before.RawText.Length <= 10)
                return;
            Console.WriteLine("test4");
            if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
            {
                Console.WriteLine("test5");
                var uid = Convert.ToInt64(e.User.Id);
                Console.WriteLine("test6");
                LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);
                Console.WriteLine("test7");

                if (ldm != null)
                {
                    Console.WriteLine("test8");
                    int xpToRemove = (e.Before.RawText.Length > 25 ? 25 : e.Before.RawText.Length);
                    Console.WriteLine("test9");
                    if ((ldm.TotalXP - xpToRemove) <= 0)
                    {
                        Console.WriteLine("test10");
                        ldm.TotalXP = 0;
                        Console.WriteLine("test11");
                    }
                    else
                    {
                        Console.WriteLine("test12");
                        ldm.TotalXP -= xpToRemove;
                        Console.WriteLine("test13");
                    }
                    Console.WriteLine("test14");
                    //Calculate new level
                    int copyOfTotalXP = ldm.TotalXP;
                    Console.WriteLine("test15");
                    int calculatedLevel = 0;
                    Console.WriteLine("test16");

                    while (copyOfTotalXP > 0)
                    {
                        Console.WriteLine("test17");
                        int xpNeededForNextLevel = getXPForNextLevel(calculatedLevel);
                        Console.WriteLine("test18");

                        if (copyOfTotalXP > xpNeededForNextLevel)
                        {
                            Console.WriteLine("test19");
                            calculatedLevel++;
                            Console.WriteLine("test20");
                            copyOfTotalXP -= xpNeededForNextLevel;
                            Console.WriteLine("test21");
                        }
                        else
                        {
                            Console.WriteLine("test22");
                            ldm.CurrentXP = copyOfTotalXP;
                            Console.WriteLine("test23");
                            copyOfTotalXP = 0;
                            Console.WriteLine("test24");
                        }
                        Console.WriteLine("test25");
                    }
                    Console.WriteLine("test26");

                    ldm.Level = calculatedLevel;
                    Console.WriteLine("test27");
                    //Add New Levels
                    int xpToGet = (e.After.RawText.Length > 25 ? 25 : e.After.RawText.Length);
                    Console.WriteLine("test28");

                    ldm.CurrentXP += xpToGet;
                    Console.WriteLine("test29");
                    ldm.TotalXP += xpToGet;
                    Console.WriteLine("test30");

                    if (ldm.CurrentXP >= getXPForNextLevel(ldm.Level))
                    {
                        Console.WriteLine("test31");
                        if (ldm.CurrentXP > getXPForNextLevel(ldm.Level))
                        {
                            Console.WriteLine("test32");
                            ldm.CurrentXP = (ldm.CurrentXP - getXPForNextLevel(ldm.Level));
                            Console.WriteLine("test33");
                        }
                        else
                        {
                            Console.WriteLine("test34");
                            ldm.CurrentXP = 0;
                            Console.WriteLine("test35");
                        }
                        Console.WriteLine("test36");
                        ldm.Level += 1;
                        Console.WriteLine("test37");

                        await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du hast Level { ldm.Level } erreicht!");
                        Console.WriteLine("test38");
                    }
                    Console.WriteLine("test39");
                    DbHandler.Instance.Save(ldm);
                    Console.WriteLine("test40");
                }
                Console.WriteLine("test41");
            }
            Console.WriteLine("test42");
        }

        public bool isCommand(string text)
        {
            var enumerable = MidnightBot.Client.GetService<CommandService>().AllCommands;

            foreach(Discord.Commands.Command cmd in enumerable)
            {
                if(text.StartsWith(cmd.Text))
                {
                    return true;
                } else
                {
                    foreach(string alias in cmd.Aliases)
                    {
                        if (text.StartsWith(alias))
                            return true;
                    }
                }
            }

            return false;
        }

        public int getXPForNextLevel (int level)
        {
            double levelXP = 5 * (Math.Pow((double)level, 2)) + 50 * level + 100;

            return (int)levelXP;
        }
    }
}
