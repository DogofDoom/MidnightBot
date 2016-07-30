using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Generic;
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

            cgb.CreateCommand (Prefix + "addcustreact")
                .Alias (Prefix + "acr")
                .Description ($"Fügt eine \"Custom Reaction\" hinzu. **Bot Owner Only!** | `{Prefix}acr \"hello\" I love saying hello to %user%`")
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
                    await Classes.JSONModels.ConfigHandler.SaveConfig().ConfigureAwait(false);
                    await e.Channel.SendMessage ($"Hinzugefügt {name} : {message}").ConfigureAwait (false);

                });

            cgb.CreateCommand (Prefix + "listcustreact")
            .Alias (Prefix + "lcr")
            .Description($"Listet Custom Reactions auf(Seitenweise mit 30 Befehlen per Seite). Benutze 'all' anstatt einer Seitenzahl um alle Custom Reactions per Privater Nachricht zu erhalten. | `{Prefix}lcr 1`")
            .Parameter ("num",ParameterType.Required)
            .Do (async e =>
            {
                var numStr = e.GetArg("num");

                if (numStr.ToUpperInvariant() == "ALL")
                {
                    var fullstr = String.Join("\n", MidnightBot.Config.CustomReactions.Select(kvp => kvp.Key));
                    do
                    {
                        var str = string.Concat(fullstr.Take(1900));
                        fullstr = new string(fullstr.Skip(1900).ToArray());
                        await e.User.SendMessage("```xl\n" + str + "```");
                    } while (fullstr.Length != 0);
                    return;
                }
                int num;
                if (!int.TryParse(numStr, out num) || num <= 0) num = 1;
                var cmds = GetCustomsOnPage(num - 1);
                if (!cmds.Any())
                {
                    await e.Channel.SendMessage("`Da sind keine Custom Reactions.`");
                }
                else
                {
                    string result = SearchHelper.ShowInPrettyCode<string>(cmds, s => $"{s,-25}"); //People prefer starting with 1
                    await e.Channel.SendMessage($"`Zeige Seite {num}:`\n" + result).ConfigureAwait(false);
                }
            });

            cgb.CreateCommand(Prefix + "showcustreact")
                .Alias(Prefix + "scr")
                .Description($"Zeigt alle möglichen Reaktionen von einer einzigen Custom Reaction. | `{Prefix}scr %mention% bb`")
                .Parameter("name", ParameterType.Unparsed)
                .Do(async e =>
                {
                    var name = e.GetArg("name")?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        return;
                    if (!MidnightBot.Config.CustomReactions.ContainsKey(name))
                    {
                        await e.Channel.SendMessage("`Kann die Custom Reaction nicht finden.`").ConfigureAwait(false);
                        return;
                    }
                    var items = MidnightBot.Config.CustomReactions[name];
                    var message = new StringBuilder($"Antwort für {Format.Bold(name)}:\n");
                    var last = items.Last();

                    int i = 1;
                    foreach (var reaction in items)
                    {
                        message.AppendLine($"[{i++}] " + Format.Code(Format.Escape(reaction)));
                    }
                    await e.Channel.SendMessage(message.ToString());
                });

            cgb.CreateCommand(Prefix + "editcustreact")
                .Alias(Prefix + "ecr")
                .Description($"Bearbeitet eine Custom Reaction, Argumente sind der Custom Reaction Name, Index welcher geändert werden soll und eine (Multiwort) Nachricht.**Bot Owner Only** | `{Prefix}ecr \"%mention% disguise\" 2 Test 123`")
                .Parameter("name", ParameterType.Required)
                .Parameter("index", ParameterType.Required)
                .Parameter("message", ParameterType.Unparsed)
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Do(async e =>
                {
                    var name = e.GetArg("name")?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        return;
                    var indexstr = e.GetArg("index")?.Trim();
                    if (string.IsNullOrWhiteSpace(indexstr))
                        return;
                    var msg = e.GetArg("message")?.Trim();
                    if (string.IsNullOrWhiteSpace(msg))
                        return;



                    if (!MidnightBot.Config.CustomReactions.ContainsKey(name))
                    {
                        await e.Channel.SendMessage("`Konnte gegebenen Befehls-Namen nicht finden`").ConfigureAwait(false);
                        return;
                    }

                    int index;
                    if (!int.TryParse(indexstr, out index) || index < 1 || index > MidnightBot.Config.CustomReactions[name].Count)
                    {
                        await e.Channel.SendMessage("`Ungültiger Index.`").ConfigureAwait(false);
                        return;
                    }
                    index = index - 1;
                    MidnightBot.Config.CustomReactions[name][index] = msg;

                    await Classes.JSONModels.ConfigHandler.SaveConfig().ConfigureAwait(false);
                    await e.Channel.SendMessage($"Antwort #{index + 1} von `{name}` bearbeitet").ConfigureAwait(false);
            });

            cgb.CreateCommand (Prefix + "delcustreact")
            .Alias (Prefix + "dcr")
            .Description ($"Löscht eine \"Custome Reaction\" mit gegebenen Namen (und Index). | `{Prefix}dcr index`")
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
                    await Classes.JSONModels.ConfigHandler.SaveConfig().ConfigureAwait(false);
                    await e.Channel.SendMessage (message).ConfigureAwait (false);
                });
        }

        private readonly int ItemsPerPage = 30;

        private IEnumerable<string> GetCustomsOnPage ( int page )
        {
            var items = MidnightBot.Config.CustomReactions.Skip (page * ItemsPerPage).Take (ItemsPerPage);
            if (!items.Any ())
            {
                return Enumerable.Empty<string> ();
            }
            return items.Select (kvp => kvp.Key);
            /*
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
            */
        }
    }
}
// zeta is a god
//├
//─
//│
//└