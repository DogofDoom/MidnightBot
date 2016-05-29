using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Administration.Commands
{
    class CustomReactionsCommands : DiscordCommand
    {
        public CustomReactionsCommands ( DiscordModule module ) : base (module)
        {

        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            var Prefix = Module.Prefix;

            cgb.CreateCommand (Prefix + "addcustomreaction")
                .Alias (Prefix + "acr")
                .Description ($"Fügt eine \"Custom Reaction\" hinzu. **Owner Only!**\n**Benutzung**: {Prefix}acr \"hello\" I love saying hello to %user%")
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Parameter ("name",ParameterType.Required)
                .Parameter ("message",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var name = e.GetArg ("name");
                    var message = e.GetArg ("message")?.Trim ();
                    if (string.IsNullOrWhiteSpace (message))
                    {
                        await e.Channel.SendMessage ($"Falsche Command-Benutzung. Gib -h {Prefix}acr ein für die richtige Formatierung.").ConfigureAwait (false);
                        return;
                    }
                    if (MidnightBot.Config.CustomReactions.ContainsKey (name))
                        MidnightBot.Config.CustomReactions[name].Add (message);
                    else
                        MidnightBot.Config.CustomReactions.Add (name,new System.Collections.Generic.List<string> () { message });
                    await Task.Run (() => Classes.JSONModels.ConfigHandler.SaveConfig ());
                    await e.Channel.SendMessage ($"Hinzugefügt {name} : {message}").ConfigureAwait (false);

                });

            cgb.CreateCommand (Prefix + "listcustomreactions")
            .Alias (Prefix + "lcr")
            .Description ("Listet alle derzeitigen \"Custom Reactions\" (Seitenweise mit 5 Commands je Seite).\n**Benutzung**:.lcr 1")
            .Parameter ("num",ParameterType.Required)
            .Do (async e =>
            {
                int num;
                if (!int.TryParse (e.GetArg ("num"),out num) || num <= 0)
                    return;
                string result = GetCustomsOnPage (num - 1); //People prefer starting with 1
                await e.Channel.SendMessage (result);
            });

            cgb.CreateCommand (Prefix + "deletecustomreaction")
            .Alias (Prefix + "dcr")
            .Description ("Löscht eine \"Custome Reaction\" mit gegebenen Namen (und Index)")
            .AddCheck (SimpleCheckers.OwnerOnly ())
            .Parameter ("name",ParameterType.Required)
            .Parameter ("index",ParameterType.Optional)
            .Do (async e =>
            
                {
                    var name = e.GetArg ("name")?.Trim ();
                    if (string.IsNullOrWhiteSpace (name))
                        return;
                    if (!MidnightBot.Config.CustomReactions.ContainsKey (name))
                    {
                        await e.Channel.SendMessage ("Gegebener Command-Name nicht gefunden.");
                        return;
                    }
                    string message = "";
                    int index;
                    if (int.TryParse (e.GetArg ("index")?.Trim () ?? "",out index))
                    {
                        index = index - 1;
                        if (index < 0 || index > MidnightBot.Config.CustomReactions[name].Count)
                        {
                            await e.Channel.SendMessage ("Gegebener Index nicht vorhanden.").ConfigureAwait (false);
                            return;

                        }
                        MidnightBot.Config.CustomReactions[name].RemoveAt (index);
                        if (!MidnightBot.Config.CustomReactions[name].Any ())
                        {
                            MidnightBot.Config.CustomReactions.Remove (name);
                        }
                        message = $"Antwort #{index + 1} von `{name}` gelöscht.";
                    }
                    else
                    {
                        MidnightBot.Config.CustomReactions.Remove (name);
                        message = $"Custom Reaction: `{name}` gelöscht";
                    }
                    await Task.Run (() => Classes.JSONModels.ConfigHandler.SaveConfig ());
                    await e.Channel.SendMessage (message);
                });
        }

        private readonly int ItemsPerPage = 5;

        private string GetCustomsOnPage ( int page )
        {
            var items = MidnightBot.Config.CustomReactions.Skip (page * ItemsPerPage).Take (ItemsPerPage);
            if (!items.Any ())
            {
                return $"Keine Reactions auf Seite {page + 1}.";
            }
            var message = new StringBuilder ($"--- Custom Reactions - Seite {page + 1} ---\n");
            foreach (var cr in items)
            {
                message.Append ($"{ Format.Code (cr.Key)}\n");
                int i = 1;
                var last = cr.Value.Last ();
                foreach (var reaction in cr.Value)
                {
                    if (last != reaction)
                        message.AppendLine ("  `├" + i++ + "─`" + Format.Bold (reaction));
                    else
                        message.AppendLine ("  `└" + i++ + "─`" + Format.Bold (reaction));
                }
            }
            return message.ToString () + "\n";
        }
    }
}
// zeta is a god
//├
//─
//│
//└