﻿using System;
using Discord;
using Discord.Commands;
using MidnightBot.DataModels;
using MidnightBot.Classes;

namespace MidnightBot.Modules.Level.Classes
{
    class MessageHandler
    {
        module module { get; set; }

        public MessageHandler(module module)
        {
            this.module = module;
        }

        public async void messageReceived(object sender, MessageEventArgs e)
        {
            try
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
                        else
                        {
                            module.OnLevelChanged(this, new LevelChangedEventArgs(e.Channel, e.User, ldm.Level));
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
            catch
            {
            }
        }

        public async void messageDeleted(object sender, MessageEventArgs e)
        {
            try
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
                        if (e.Message.RawText == null)
                            return;
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
            catch
            {
            }
        }

        public async void messageUpdated(object sender, MessageUpdatedEventArgs e)
        {
            try
            {
                if (e == null || e.Before == null || e.After == null || e.User == null || e.Channel == null || e.Server == null)
                    return;
                if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
                    return;
                if (e.After.RawText.Length <= 10 && e.Before.RawText.Length <= 10)
                    return;
                if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
                {
                    var uid = Convert.ToInt64(e.User.Id);
                    LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                    if (ldm != null)
                    {
                        if (e.Before.RawText == null)
                            return;
                        int xpToRemove = (e.Before.RawText.Length > 25 ? 25 : e.Before.RawText.Length);
                        if ((ldm.TotalXP - xpToRemove) <= 0)
                        {
                            ldm.TotalXP = 0;
                        }
                        else
                        {
                            ldm.TotalXP -= xpToRemove;
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
                                copyOfTotalXP = 0;
                            }
                        }

                        ldm.Level = calculatedLevel;
                        //Add New Levels
                        int xpToGet = (e.After.RawText.Length > 25 ? 25 : e.After.RawText.Length);

                        ldm.CurrentXP += xpToGet;
                        ldm.TotalXP += xpToGet;

                        if (ldm.CurrentXP >= getXPForNextLevel(ldm.Level))
                        {
                            if (ldm.CurrentXP > getXPForNextLevel(ldm.Level))
                            {
                                ldm.CurrentXP = (ldm.CurrentXP - getXPForNextLevel(ldm.Level));
                            }
                            else
                            {
                                ldm.CurrentXP = 0;
                            }
                            ldm.Level += 1;

                            module.OnLevelChanged(this, new LevelChangedEventArgs(e.Channel, e.User, ldm.Level));

                            await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du hast Level { ldm.Level } erreicht!");
                        }
                        DbHandler.Instance.Save(ldm);
                    }
                }
            }
            catch
            {
            }
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
