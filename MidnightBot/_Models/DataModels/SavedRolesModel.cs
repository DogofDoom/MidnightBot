namespace MidnightBot.DataModels
{
    internal class SavedRoles : IDataModel
    {
        [SQLite.Unique]
        public long UserId { get; set; }

        public string roles { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is SavedRoles && ((SavedRoles)obj).UserId == this.UserId);
        }
    }
}
