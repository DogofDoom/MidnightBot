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
            cgb
                .CreateCommand(Module.Prefix + "mute")
                .AddCheck(new SimpleCheckers.ManageRoles())
                .Description("Mute einen Benutzer!")
                .Parameter("user", ParameterType.Required)
                .Parameter("reason", ParameterType.Optional)
                .Do(e =>
                {
                    string argUser = e.GetArg("user");
                    string argReason = e.GetArg("reason");

                    if (argReason == null)
                        argReason = "Regelverstoß";

                    User user = e.Server.FindUsers(argUser).FirstOrDefault();

                    if(user != null)
                    {
                        Role role = e.Server.FindRoles("Mute").FirstOrDefault();

                        if(role != null)
                        {
                            user.AddRoles(role);

                            user.PrivateChannel.SendMessage($"Du wurdest von { e.User.Mention } gemuted, aufgrund von { argReason }.");
                        } else
                        {
                            throw new InvalidOperationException("No role named Mute found!");
                        }
                    }
                });
        }

    }
}
