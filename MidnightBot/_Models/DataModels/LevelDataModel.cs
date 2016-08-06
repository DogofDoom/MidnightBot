using System;

namespace MidnightBot.DataModels
{
    class LevelData : IDataModel
    {
        [SQLite.Unique]
        public long UserId { get; set; }

        public int Level { get; set; }
        public int TotalXP { get; set; }
        public int CurrentXP { get; set; }
        public int XPForNextLevel { get; set; }
        public DateTime timestamp { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is LevelData && ((LevelData)obj).UserId == this.UserId);
        }
    }
}
