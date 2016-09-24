using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.DataModels;

namespace MidnightBot.Modules.Level.Commands
{
    class XPCommand : DiscordCommand
    {
        public XPCommand(DiscordModule module) : base(module) { }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "addxp")
                .Description("Addet XP zu einem User")
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Parameter("xpToGet", ParameterType.Required)
                .Parameter("user", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var usr = e.Server.FindUsers(e.GetArg("user")).FirstOrDefault();
                    if (usr == null)
                    {
                        await e.Channel.SendMessage($"{ e.User.Mention }, diesen User kenne ich nicht.");
                    }
                    else
                    {
                        long uid = Convert.ToInt64(usr.Id);

                        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                        if (ldm != null)
                        {
                            int xpToGet = Convert.ToInt32(e.GetArg("xpToGet"));

                            ldm.TotalXP += xpToGet;

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
                            await e.Channel.SendMessage("XP geaddet.");
                            DbHandler.Instance.Save(ldm);
                        }
                        else
                        {
                            await e.Channel.SendMessage("User noch nicht vorhanden.");
                        }
                    }


                });

            cgb.CreateCommand(Module.Prefix + "removexp")
                .Description("Entfernt XP von einem User")
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Parameter("xpToLose", ParameterType.Required)
                .Parameter("user", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var usr = e.Server.FindUsers(e.GetArg("user")).FirstOrDefault();
                    if (usr == null)
                    {
                        await e.Channel.SendMessage($"{ e.User.Mention }, diesen User kenne ich nicht.");
                    }
                    else
                    {
                        long uid = Convert.ToInt64(usr.Id);

                        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                        if (ldm != null)
                        {
                            int xpToLose = Convert.ToInt32(e.GetArg("xpToLose"));
                            if ((ldm.TotalXP - xpToLose) <= 0)
                            {
                                ldm.TotalXP = 0;
                            }
                            else
                            {
                                ldm.TotalXP -= xpToLose;
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
                            await e.Channel.SendMessage("XP entfernt.");
                            DbHandler.Instance.Save(ldm);
                        }
                        else
                        {
                            await e.Channel.SendMessage("User noch nicht vorhanden.");
                        }
                    }
                });

            cgb.CreateCommand(Module.Prefix + "turnToXP")
                .Description($"Tauscht {MidnightBot.Config.CurrencyName} in XP um. Ratio 1/5")
                .Parameter("moneyToSpend", ParameterType.Required)
                .Do(async e =>
                {
                    var levelChanged = false;
                    long uid = Convert.ToInt64(e.User.Id);
                    int moneyToSpend = Convert.ToInt32(e.GetArg("moneyToSpend"));
                    var userMoney = DbHandler.Instance.GetStateByUserId((long)e.User.Id)?.Value ?? 0;

                    if (userMoney>=moneyToSpend)
                    {
                        if (moneyToSpend > 0)
                        {
                            LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                            if (ldm != null)
                            {
                                await FlowersHandler.RemoveFlowers(e.User, $"Traded for XP.({e.User.Name}/{e.User.Id})", (int)moneyToSpend, true).ConfigureAwait(false);

                                ldm.TotalXP += (moneyToSpend * 5);

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
                                DbHandler.Instance.Save(ldm);

                                if (levelChanged)
                                    await e.Channel.SendMessage($"{e.User.Mention} Dein neuer Level ist {calculatedLevel}");
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessage($"{e.User.Mention} Du musst mindestens 1 {MidnightBot.Config.CurrencySign} umtauschen.");
                        }

                        await e.Channel.SendMessage($"{e.User.Mention} hat erfolgreich {moneyToSpend} {MidnightBot.Config.CurrencySign} in {moneyToSpend * 5} XP umgewandelt."); 
                    }
                    else
                    {
                        await e.Channel.SendMessage($"{e.User.Mention} Du hast nicht genug {MidnightBot.Config.CurrencyName}. Du hast nur {userMoney} {MidnightBot.Config.CurrencySign}");
                    }
                });
            
        }

        int GetRank(LevelData ldm)
        {
            IList<LevelData> data = DbHandler.Instance.FindAll<LevelData>(p => true);
            data = data.OrderByDescending(p => p.TotalXP).ToList();

            return data.IndexOf(ldm) + 1;
        }

        public int getXPForNextLevel(int level)
        {
            double levelXP = 5 * (Math.Pow((double)level, 2)) + 50 * level + 100;

            return (int)levelXP;
        }
    }
}
