using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using MidnightBot.DataModels;

namespace MidnightBot.Modules.Administration.Commands
{
    class MuteCommand : DiscordCommand
    {
        private string prettyCurrentTime => $"【{DateTime.Now:HH:mm:ss}】";

        public MuteCommand(DiscordModule module) : base(module)
        {
            MidnightBot.OnReady += () => MidnightBot.Client.MessageReceived += (s, e) =>
            {
                User user = e.User;

                DateTime today = DateTime.Now;
                var uid = (long)e.User.Id;

                var rs = DbHandler.Instance.FindOne<Mute>(dm => dm.UserId == uid);
                if (rs != null)
                {
                    if (DateTime.Compare(rs.MutedUntil,today)<=0)
                    {
                        Role roleBanned = e.Server.FindRoles("Banned").FirstOrDefault();

                        if (roleBanned != null)
                        {
                            if (user.HasRole(roleBanned))
                            {
                                user.RemoveRoles(roleBanned);

                                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                                if (chId != null)
                                { 
                                Channel ch;
                                if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) != null)
                                    ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde entmuted.");
                                }
                                user.SendMessage($"Du wurdest entmuted.");
                                DbHandler.Instance.Delete<Mute>(Convert.ToInt32(rs.Id));
                            }
                            else
                            {
                                DbHandler.Instance.Delete<Mute>(Convert.ToInt32(rs.Id));
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No role named Banned found!");
                        }
                    }
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
                                await ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde von **{ e.User.Name }** gemuted..");

                                await user.SendMessage($"Du wurdest von { e.User.Mention } gemuted.");
                            }
                            else
                            {
                                await e.Channel.SendMessage("User ist bereits gemuted.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("No role named Mute found!");
                        }
                    }
                });

            cgb.CreateCommand(Module.Prefix + "tmute")
                .AddCheck(SimpleCheckers.ManageMessages())
                .Description($"Mute einen Benutzer für eine bestimmte Zeit!| {Prefix}tmute @User 7d5h40m20s")
                .Parameter("user", ParameterType.Required)
                .Parameter("time", ParameterType.Required)
                .Parameter("reason", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string argUser = e.GetArg("user");
                    string argTime = e.GetArg("time");
                    string argReason = e.GetArg("reason");
                    int days = 0, hours = 0, minutes = 0, seconds = 0;
                    string sdays = "0", shours = "0", sminutes = "0", sseconds = "0";


                    if (argTime.Contains('d'))
                    {
                        sdays = argTime.Split('d')[0];
                        argTime = argTime.Split('d')[1];
                    }

                    if (argTime.Contains('h'))
                    {
                        shours = argTime.Split('h')[0];
                        argTime = argTime.Split('h')[1];
                    }
                    if (argTime.Contains('m'))
                    {
                        sminutes = argTime.Split('m')[0];
                        argTime = argTime.Split('m')[1];
                    }
                    if (argTime.Contains('s'))
                    {
                        sseconds = argTime.Split('s')[0];
                        argTime = argTime.Split('s')[1];
                    }

                    days = Convert.ToInt32(sdays);
                    hours = Convert.ToInt32(shours);
                    minutes = Convert.ToInt32(sminutes);
                    seconds = Convert.ToInt32(sseconds);


                    if (argReason == "")
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

                                


                                TimeSpan timeToAdd = new TimeSpan(days, hours, minutes, seconds);
                                DateTime today = DateTime.Now;

                                DateTime MuteUntil = today.Add(timeToAdd);

                                var uid = (long)user.Id;

                                Mute rs = new Mute();

                                rs.UserId = Convert.ToInt64(user.Id);

                                rs.MutedUntil = MuteUntil;

                                DbHandler.Instance.Save(rs);

                                var chId = SpecificConfigurations.Default.Of(e.Server.Id).LogServerChannel;
                                if (chId != null)
                                {
                                    Channel ch;
                                    if ((ch = e.Server.TextChannels.Where(tc => tc.Id == chId).FirstOrDefault()) != null)
                                        await ch.SendMessage($"❗`{prettyCurrentTime}` **{user.Name}** wurde von **{ e.User.Name }** gemuted, für {days} Tage {hours} Stunden {minutes} Minuten {seconds} Sekunden.");
                                }
                                await user.SendMessage($"Du wurdest von { e.User.Mention } gemuted. Für {days} Tage {hours} Stunden {minutes} Minuten {seconds} Sekunden.");
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

                                await user.SendMessage($"Du wurdest entmuted.");
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