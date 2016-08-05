using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

namespace MidnightBot.Modules.Level.Classes
{
    class LevelChangedEventArgs : EventArgs
    {
        public User @User { get; set; }

        public Channel @Channel { get; set; }

        public int NewLevel { get; set; } 

        public LevelChangedEventArgs(Channel ch, User user, int newLevel)
        {
            this.User = user;
            this.Channel = ch;

            this.NewLevel = newLevel;
        }

    }
}
