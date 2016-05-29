using Discord.Commands;
using MidnightBot.Classes;

namespace MidnightBot.Modules.Searches.Commands
{
    class RedditCommand : DiscordCommand
    {
        public RedditCommand ( DiscordModule module ) : base (module)
        {
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            //throw new NotImplementedException ();
        }
    }
}