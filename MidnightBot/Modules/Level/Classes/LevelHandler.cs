using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

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
                        KeyValuePair<int, string> keyValue = enumerable.FirstOrDefault();

                        string rankId = keyValue.Value;

                        Role role = e.Channel.Server.FindRoles(rankId).FirstOrDefault();

                        if (role != null)
                        {
                            if (e.User.HasRole(role))
                                return;

                            try
                            {
                                await e.User.AddRoles(role).ConfigureAwait(false);

                                await e.Channel.SendMessage($"Herzlichen Glückwunsch { e.User.Mention }, du bist nun { role.Name }").ConfigureAwait(false);
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
