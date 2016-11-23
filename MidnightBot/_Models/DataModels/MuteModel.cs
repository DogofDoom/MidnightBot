using SQLite;
using System;

namespace MidnightBot.DataModels
{
    internal class Mute : IDataModel
    {
        [Unique]
        public long UserId { get; set; }
        public DateTime MutedUntil { get; set; }
        public long ServerId { get; set; }
    }
}