using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands;
using MidnightBot;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.Modules.Level.Classes;
using MidnightBot.Modules.Level.Commands;
using MidnightBot.DataModels;

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
                        var uid = (long)e.User.Id;

                        LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

                        if (ldm != null)
                        {
                            e.Channel.SendMessage($"{ e.User.Mention }: LEVEL { ldm.Level } | XP { ldm.CurrentXP }/{ ldm.XPForNextLevel } | TOTAL XP { ldm.TotalXP }");

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
                            var uid = (long)usr.Id;

                            LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);
                            if (ldm != null)
                            {
                                e.Channel.SendMessage($"{ e.User.Mention }: **{usr.Name}**\'s Rang > LEVEL { ldm.Level } | XP { ldm.CurrentXP }/{ ldm.XPForNextLevel } | TOTAL XP { ldm.TotalXP }");

                            }
                            else
                            {
                                e.Channel.SendMessage($"{ e.User.Mention }, dich kenne ich nicht.");
                            }
                        }
                    }
                });
        }

    }
}
