using System.Threading.Tasks;

namespace MidnightBot.Classes
{
    internal static class FlowersHandler
    {
        public static async Task AddFlowersAsync(Discord.User u, string reason, int amount, bool silent = false)
        {
            if (amount <= 0)
                return;
            await Task.Run(() =>
            {
                DbHandler.Instance.InsertData(new DataModels.CurrencyTransaction
                {
                    Reason = reason,
                    UserId = (long)u.Id,
                    Value = amount,
                });
            }).ConfigureAwait (false);

            if (silent)
                return;

            var flows = amount + " " + MidnightBot.Config.CurrencySign;

            await u.SendMessage("Glückwunsch!👑\nDu hast folgendes erhalten: " + flows).ConfigureAwait (false);
        }

        public static async Task<bool> RemoveFlowers(Discord.User u, string reason, int amount)
        {
            if (amount <= 0)
                return false;
            var uid = (long)u.Id;
            var state = DbHandler.Instance.FindOne<DataModels.CurrencyState>(cs => cs.UserId == uid);

            if (state.Value < amount)
                return false;

            DbHandler.Instance.InsertData(new DataModels.CurrencyTransaction
            {
                Reason = reason,
                UserId = (long)u.Id,
                Value = -amount,
            });
            await u.SendMessage($"👎`Bot Owner hat: {amount}{MidnightBot.Config.CurrencySign} von dir entfernt.`").ConfigureAwait(false);
            return true;
        }
    }
}