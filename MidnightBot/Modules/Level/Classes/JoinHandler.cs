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
    class JoinHandler
    {

        public void serverJoined(object sender, UserEventArgs e)
        {
            var uid = (long)e.User.Id;
            LevelData ldm = DbHandler.Instance.FindOne<LevelData>(p => p.UserId == uid);

            if(ldm == null)
            {
                ldm = new LevelData();

                ldm.UserId = Convert.ToInt64(e.User.Id);
                ldm.UniqueTag = (int)e.User.Discriminator;
                ldm.Level = 1;
                ldm.TotalXP = 0;
                ldm.CurrentXP = 0;
                ldm.XPForNextLevel = 5 * (ldm.Level ^ 2) + 50 * ldm.Level + 100;
                ldm.DateAdded = DateTime.Now;

                DbHandler.Instance.Save(ldm);
            }
        }
    }
}
