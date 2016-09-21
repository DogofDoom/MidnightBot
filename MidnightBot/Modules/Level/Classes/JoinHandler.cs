using System;
using Discord;
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
                ldm.Level = 0;
                ldm.TotalXP = 0;
                ldm.CurrentXP = 0;
                ldm.DateAdded = DateTime.Now;

                DbHandler.Instance.Save(ldm);
            }
        }
    }
}
