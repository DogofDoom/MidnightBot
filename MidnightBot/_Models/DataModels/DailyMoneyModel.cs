using System;

namespace MidnightBot.DataModels
{
    internal class DailyMoney : IDataModel
    {
        public long UserId { get; set; }
        public DateTime LastTimeGotten { get; set; }
        public long ServerId { get; set; }
    }
}
