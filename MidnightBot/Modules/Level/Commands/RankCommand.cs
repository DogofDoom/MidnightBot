﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Extensions;

namespace MidnightBot.Modules.Level.Commands
{
    class RankCommand : DiscordCommand
    {
        public RankCommand( DiscordModule module ) : base(module) { }
        
        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "rank")
                .Description("Zeigt deinen zurzeitigen Rang an.")
                .Parameter("user", ParameterType.Unparsed)
                .Do(e =>
                {
                    if (string.IsNullOrWhiteSpace(e.GetArg("user")))
                    {
                        long uid = Convert.ToInt64(e.User.Id);

                        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                        if (ldm != null)
                        {
                            int total = DbHandler.Instance.FindAll<LevelData>(p => true).Count;
                            int rank = GetRank(ldm);

                            e.Channel.SendMessage($"{ e.User.Mention }: **LEVEL { ldm.Level } | XP { ldm.CurrentXP }/{ getXPForNextLevel(ldm.Level) } | TOTAL XP { ldm.TotalXP } | RANK { rank }/{ total }**");
                        }
                        else
                        {
                            e.Channel.SendMessage($"{ e.User.Mention }, dich kenne ich nicht.");
                        }
                    }
                    else
                    {

                        var usr = e.Server.FindUsers(e.GetArg("user")).FirstOrDefault();
                        if (usr == null)
                        {
                            e.Channel.SendMessage($"{ e.User.Mention }, diesen User kenne ich nicht.");
                        }
                        else
                        {
                            long uid = Convert.ToInt64(usr.Id);

                            LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);



                            if (ldm != null)
                            {
                                int total = DbHandler.Instance.FindAll<LevelData>(p => true).Count;
                                int rank = GetRank(ldm);

                                e.Channel.SendMessage($"{ e.User.Mention }: **{usr.Name}\'s Rang > LEVEL { ldm.Level } | XP { ldm.CurrentXP }/{ getXPForNextLevel(ldm.Level) } | TOTAL XP { ldm.TotalXP } | RANK { rank }/{ total }**");
                            }
                            else
                            {
                                e.Channel.SendMessage($"{ e.User.Mention }, dich kenne ich nicht.");
                            }
                        }
                    }
                });

            cgb.CreateCommand(Module.Prefix + "ranks")
                .Alias(Module.Prefix + "levels")
                .Description("Schickt eine Rangliste per PN.")
                .Do(async e =>
                {
                    int stringCount = 0;
                    int formatCounter = 0;
                    IList<LevelData> data = DbHandler.Instance.FindAll<LevelData>(p => true);
                    data = data.OrderByDescending(p => p.TotalXP).ToList();
                    string[] rankStrings = new string[20];
                    var sb = new StringBuilder();
                    sb.AppendLine("__**Rangliste**__");
                    foreach(LevelData user in data)
                    {
                        if (formatCounter == 0)
                        {
                            sb.AppendLine($"```Liste {stringCount+1}");
                            sb.AppendLine($"{GetRank(user),3}. | {(e.Server.Users.Where(u => u.Id == (ulong)user.UserId).FirstOrDefault()?.Name.TrimTo(18, true) ?? user.UserId.ToString()),-20} | LEVEL { user.Level,2 } | XP { user.CurrentXP,6 }/{ getXPForNextLevel(user.Level),6 } | TOTAL XP { user.TotalXP,8 }");
                            formatCounter++;
                        }
                        else if (formatCounter == 20)
                        {
                            sb.AppendLine($"{GetRank(user),3}. | {(e.Server.Users.Where(u => u.Id == (ulong)user.UserId).FirstOrDefault()?.Name.TrimTo(18, true) ?? user.UserId.ToString()),-20} | LEVEL { user.Level,2 } | XP { user.CurrentXP,6 }/{ getXPForNextLevel(user.Level),6 } | TOTAL XP { user.TotalXP,8 }```");
                            formatCounter = 0;
                            rankStrings[stringCount] = sb.ToString();
                            sb.Clear();
                            stringCount++;
                        }
                        else
                        {
                            sb.AppendLine($"{GetRank(user),3}. | {(e.Server.Users.Where(u => u.Id == (ulong)user.UserId).FirstOrDefault()?.Name.TrimTo(18, true) ?? user.UserId.ToString()),-20} | LEVEL { user.Level,2 } | XP { user.CurrentXP,6 }/{ getXPForNextLevel(user.Level),6 } | TOTAL XP { user.TotalXP,8 }");
                            formatCounter++;
                        }
                    }
                    if (formatCounter != 0)
                    {
                        sb.AppendLine("```");
                        rankStrings[stringCount] = sb.ToString();
                    }

                    foreach (string s in rankStrings)
                    {
                        await e.User.SendMessage(s);

                        Thread.Sleep(250);
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
            double levelXP = 5 * (Math.Pow((double)level,2)) + 50 * level + 100;

            return (int)levelXP;
        }
    }
}
