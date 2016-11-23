using System;

namespace MidnightBot.DataModels
{
    internal class Warns : IDataModel
    {
        public long UserId { get; set; }
        public int timesWarned { get; set; }
        public long ServerId { get; set; }
    }
}