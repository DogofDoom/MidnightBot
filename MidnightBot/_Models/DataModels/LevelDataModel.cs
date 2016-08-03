namespace MidnightBot.DataModels
{
    class LevelData : IDataModel
    {
        public long UserId { get; set; }
        public int UniqueTag { get; set; }

        public int Level { get; set; }
        public int TotalXP { get; set; }
        public int CurrentXP { get; set; }
        public int XPForNextLevel { get; set; }
    }
}
