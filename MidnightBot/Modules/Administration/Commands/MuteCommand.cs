using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;

namespace MidnightBot.Modules.Administration.Commands
{
    class MuteCommand : DiscordCommand
    {
        private string prettyCurrentTime => $"【{DateTime.Now:HH:mm:ss}】";

        public MuteCommand(DiscordModule module) : base (module)
        {
            MidnightBot.OnReady += () => MidnightBot.Client.MessageReceived += (s, e) =>
            {
                User user = e.User;

                Role role = e.Server.FindRoles("Mute").FirstOrDefault();

                if(role != null && user.HasRole(role))
                {
                    e.Message.Delete();

                    user.PrivateChannel.SendMessage("Du bist gemuted. Du kannst nicht schreiben in den Channeln des Servers.");
                }
            };
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "mute")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Description("Mute einen Benutzer!")
                .Parameter("user", ParameterType.Required)
                .Parameter("reason", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string argUser = e.GetArg("user");
                    string argReason = e.GetArg("reason");

                    

                    if (argReason == null)
                        argReason = "Regelverstoß";

                    User user = e.Server.FindUsers(argUser).FirstOrDefault();

                    if (MidnightBot.IsOwner(user.Id))
                    {
                        await e.Channel.SendMessage("Du hast nicht das Recht meinen Besitzer zu muten.");
                        return;
                    }

                        if (user != null)
                    {
                        Role role = e.Server.FindRoles("Banned").FirstOrDefault();

                        if(role != null)
                        {
                            if (!user.HasRole(role))
                            {
                                await user.AddRoles(role);

                                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                                if (chId == null)
                                    return;
                                Channel ch;
                                if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) == null)
                                    return;
                                await ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde von **{ e.User.Name }** gemuted, aufgrund von **{ argReason }**.");

                               await user.PrivateChannel.SendMessage($"Du wurdest von { e.User.Mention } gemuted, aufgrund von { argReason }.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("User ist bereits gemuted.");
                            }
                        } else
                        {
                            throw new InvalidOperationException("No role named Mute found!");
                        }
                    }
                });

            cgb.CreateCommand(Module.Prefix + "tmute")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Description($"Mute einen Benutzer für eine bestimmte Zeit! (Funktioniert derzeit noch nicht.) | {Prefix}tmute @User 7d5h40m20s")
                .Parameter("user", ParameterType.Required)
                .Parameter("time", ParameterType.Required)
                .Parameter("reason", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string argUser = e.GetArg("user");
                    string argTime = e.GetArg("time");
                    string argReason = e.GetArg("reason");
                    int days, hours, minutes, seconds;
                    string sdays = "", shours = "", sminutes = "", sseconds = "";

                    for (int durchlauf = 0; durchlauf < 4; durchlauf++)
                    {
                        if(durchlauf == 0)
                        {
                            foreach(char c in argTime)
                            {
                                if(Char.IsDigit(c))
                                {
                                    sdays += c;
                                }
                                if (c == 'd')
                                    break;
                            }

                            sdays += '0';
                        }
                        else if (durchlauf == 1)
                        {
                            foreach (char c in argTime)
                            {
                                if (Char.IsDigit(c))
                                {
                                    shours += c;
                                }
                                if (c == 'h')
                                    break;
                            }

                            shours += '0';
                        }
                        else if (durchlauf == 2)
                        {
                            foreach (char c in argTime)
                            {
                                if (Char.IsDigit(c))
                                {
                                    sminutes += c;
                                }
                                if (c == 'm')
                                    break;
                            }

                            sminutes += '0';
                        }
                        else if (durchlauf == 3)
                        {
                            foreach (char c in argTime)
                            {
                                if (Char.IsDigit(c))
                                {
                                    sseconds += c;
                                }
                                if (c == 's')
                                    break;
                            }

                            sseconds += '0';
                        }
                    }

                    days = Convert.ToInt32(sdays);
                    hours = Convert.ToInt32(shours);
                    minutes = Convert.ToInt32(sminutes);
                    seconds = Convert.ToInt32(sseconds);


                    if (argReason == null)
                        argReason = "Regelverstoß";

                    User user = e.Server.FindUsers(argUser).FirstOrDefault();

                    if (MidnightBot.IsOwner(user.Id))
                    {
                        await e.Channel.SendMessage("Du hast nicht das Recht meinen Besitzer zu muten.");
                        return;
                    }

                    if (user != null)
                    {
                        Role role = e.Server.FindRoles("Banned").FirstOrDefault();

                        if (role != null)
                        {
                            if (!user.HasRole(role))
                            {
                                await user.AddRoles(role);

                                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                                if (chId == null)
                                    return;
                                Channel ch;
                                if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) == null)
                                    return;
                                await ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde von **{ e.User.Name }** gemuted, aufgrund von **{ argReason }** für {days} Tage {hours} Stunden {minutes} Minuten {seconds} Sekunden.");

                                await user.PrivateChannel.SendMessage($"Du wurdest von { e.User.Mention } gemuted, aufgrund von { argReason } für {days} Tage {hours} Stunden {minutes} Minuten {seconds} Sekunden.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("User ist bereits gemuted.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No role named Banned found!");
                        }
                    }
                });

            cgb.CreateCommand(Module.Prefix + "unmute")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Description("Entmute einen Benutzer!")
                .Parameter("user", ParameterType.Required)
                .Do(async e =>
                {
                    string argUser = e.GetArg("user");

                    User user = e.Server.FindUsers(argUser).FirstOrDefault();

                    if (user != null)
                    {
                        Role role = e.Server.FindRoles("Banned").FirstOrDefault();

                        if (role != null)
                        {
                            if (user.HasRole(role))
                            {
                                await user.RemoveRoles(role);

                                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                                if (chId == null)
                                    return;
                                Channel ch;
                                if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) == null)
                                    return;
                                await ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde von **{ e.User.Name }** entmuted.");

                                await user.PrivateChannel.SendMessage($"Du wurdest entmuted."); 
                            }
                            else
                            {
                                await e.Channel.SendMessage("User ist nicht gemuted.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No role named Banned found!");
                        }
                    }
                });
        }

    }
}
