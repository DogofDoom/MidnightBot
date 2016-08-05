using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using MidnightBot;
using MidnightBot.DataModels;
using MidnightBot.Classes;

namespace MidnightBot.Modules.Level.Classes
{
    class MessageHandler
    {
        public async void messageReceived(object sender, MessageEventArgs e) {
            if(MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
            {
                if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
                    return;

                var uid =(long)e.User.Id;
                
                LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                if(ldm != null)
                {
                    var xpToGet= e.Message.RawText.Length;
                    if (xpToGet > 25)
                        xpToGet = 25;
                    ldm.CurrentXP += xpToGet;
                    ldm.TotalXP += xpToGet;

                    if(ldm.CurrentXP >= ldm.XPForNextLevel)
                    {
                        if(ldm.CurrentXP > ldm.XPForNextLevel)
                        {
                            ldm.CurrentXP = (ldm.CurrentXP - ldm.XPForNextLevel);
                        }
                        else
                        {
                            ldm.CurrentXP = 0;
                        }

                        ldm.Level += 1;
                        ldm.XPForNextLevel = 5 * (ldm.Level ^ 2) + 50 * ldm.Level + 100;
                        await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du hast Level { ldm.Level } erreicht!");
                    }
                    DbHandler.Instance.Save(ldm);
                }
                else
                {
                    var xpToGet = e.Message.RawText.Length;
                    if (xpToGet > 25)
                        xpToGet = 25;
                    ldm = new LevelData();

                    ldm.UserId = uid;
                    ldm.UniqueTag = (int)e.User.Discriminator;
                    ldm.Level = 1;
                    ldm.TotalXP = xpToGet;
                    ldm.CurrentXP = xpToGet;
                    ldm.XPForNextLevel = 5 * (ldm.Level ^ 2) + 50 * ldm.Level + 100;
                    ldm.DateAdded = DateTime.Now;

                    DbHandler.Instance.Save(ldm);
                }
            }
        }

        //public async void messageDeleted(object sender, MessageEventArgs e)
        //{
        //    if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
        //        return;
        //    if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
        //    {
        //        var uid = (long)e.User.Id;
        //        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

        //        if(ldm != null)
        //        {
        //            string rawText = e.Message.RawText;

        //            if((ldm.CurrentXP - rawText.Length) <= 0)
        //            {
        //                ldm.CurrentXP = 0;
        //            }
        //            else
        //            {
        //                ldm.CurrentXP -= rawText.Length;
        //            }

        //            if((ldm.TotalXP - rawText.Length) <= 0)
        //            {
        //                ldm.TotalXP = 0;
        //            }
        //            else
        //            {
        //                ldm.TotalXP -= rawText.Length;
        //            }

        //            //Reset Level to 1
        //            ldm.Level = 1;

        //            //Calculate new level
        //            int copyOfTotalXP = ldm.TotalXP;
        //            int calculatedLevel = 0;

        //            while(copyOfTotalXP > 0)
        //            {
        //                int xpNeededForNextLevel = 5 * (calculatedLevel) + 50 * calculatedLevel + 100;

        //                if (copyOfTotalXP > xpNeededForNextLevel)
        //                {
        //                    calculatedLevel++;

        //                    copyOfTotalXP -= xpNeededForNextLevel;
        //                }
        //                else
        //                {
        //                    //Calculation error... Set Current XP to remaining total xp
        //                    ldm.CurrentXP = copyOfTotalXP;

        //                    break;
        //                }
        //            }

        //            ldm.Level = calculatedLevel;

        //            DbHandler.Instance.Save(ldm);

        //            await e.Channel.SendMessage($"Schade { e.User.Mention }, deine Nachricht wurde gelöscht. Daher wird dein Level runtergesetzt. Informationen bekommst du mit {MidnightBot.Config.CommandPrefixes.Level}rank");
        //        }
        //    }
        //}

        //public async void messageUpdated(object sender, MessageUpdatedEventArgs e)
        //{
        //    if (MidnightBot.Client.CurrentUser.Id == e.User.Id)
        //        return;
        //    if (MidnightBot.Config.ListenChannels.Contains(e.Channel.Id))
        //    {
        //        var uid = (long)e.User.Id;
        //        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

        //        if (ldm != null)
        //        {
        //            string rawText = e.Before.RawText;

        //            if ((ldm.CurrentXP - rawText.Length) <= 0)
        //            {
        //                ldm.CurrentXP = 0;
        //            }
        //            else
        //            {
        //                ldm.CurrentXP -= rawText.Length;
        //            }

        //            if ((ldm.TotalXP - rawText.Length) <= 0)
        //            {
        //                ldm.TotalXP = 0;
        //            }
        //            else
        //            {
        //                ldm.TotalXP -= rawText.Length;
        //            }

        //            //Reset Level to 1
        //            ldm.Level = 1;

        //            //Calculate new level
        //            int copyOfTotalXP = ldm.TotalXP;
        //            int calculatedLevel = 0;

        //            while (copyOfTotalXP > 0)
        //            {
        //                int xpNeededForNextLevel = 5 * (calculatedLevel ^ 2) + 50 * calculatedLevel + 100;

        //                if (copyOfTotalXP > xpNeededForNextLevel)
        //                {
        //                    calculatedLevel++;

        //                    copyOfTotalXP -= xpNeededForNextLevel;
        //                }
        //                else
        //                {
        //                    //Calculation error... Set Current XP to remaining total xp
        //                    ldm.CurrentXP = copyOfTotalXP;

        //                    break;
        //                }
        //            }

        //            ldm.Level = calculatedLevel;


        //            //Add New Levels
        //            ldm.CurrentXP += e.After.RawText.Length;
        //            ldm.TotalXP += e.After.RawText.Length;

        //            if (ldm.CurrentXP >= ldm.XPForNextLevel)
        //            {
        //                if (ldm.CurrentXP > ldm.XPForNextLevel)
        //                {
        //                    ldm.CurrentXP = (ldm.XPForNextLevel - ldm.CurrentXP);
        //                }
        //                else
        //                {
        //                    ldm.CurrentXP = 0;
        //                }

        //                ldm.Level += 1;
        //                ldm.XPForNextLevel = 5 * (ldm.Level ^ 2) + 50 * ldm.Level + 100;

        //                await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du hast Level { ldm.Level } erreicht!");
        //            }

        //            DbHandler.Instance.Save(ldm);
        //        }
        //    }
        //}
    }
}
