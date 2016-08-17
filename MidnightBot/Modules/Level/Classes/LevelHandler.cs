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
    class LevelHandler
    {

        public async void levelChanged(object sender, LevelChangedEventArgs e)
        {
            try
            {
                long uid = Convert.ToInt64(e.User.Id);

                int level = e.NewLevel;

                IEnumerable<KeyValuePair<int, string>> enumerable = MidnightBot.Config.LevelRanks.Where(i => i.Key == level);

                if (enumerable != null)
                {
                    if (enumerable.Count() > 0)
                    {
                        KeyValuePair<int, string> keyValue = enumerable.First();

                        string rankId = keyValue.Value;

                        Role role = e.Channel.Server.FindRoles(rankId).First();

                        if (role != null)
                        {
                            if (e.User.HasRole(role))
                                return;

                            try
                            {
                                await e.User.AddRoles(e.Channel.Server.FindRoles(rankId).FirstOrDefault()).ConfigureAwait(false);

                                await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du bist nun { e.Channel.Server.FindRoles(rankId).FirstOrDefault().Mention }").ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Bot has insufficient rights. { ex.Message }");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Role { rankId } does not exist.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
