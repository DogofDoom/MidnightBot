using Discord.Commands;
using MidnightBot.Extensions;
using MidnightBot.Modules;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Threading.Tasks;

namespace MidnightBot.Classes.Conversations.Commands
{
    internal class RequestsCommand : DiscordCommand
    {
        public string BotName { get; set; } = MidnightBot.BotName;
        public void SaveRequest ( CommandEventArgs e,string text )
        {
            DbHandler.Instance.InsertData (new DataModels.Request
            {
                RequestText = text,
                UserName = e.User.Name,
                UserId = (long)e.User.Id,
                ServerId = (long)e.Server.Id,
                ServerName = e.Server.Name,
                DateAdded = DateTime.Now
            });
        }
        // todo what if it's too long?
        public string GetRequests ()
        {
            var task = DbHandler.Instance.GetAllRows<DataModels.Request> ();

            var str = $"Alle derzeiten Anfragen für {BotName}:\n\n";
            foreach (var reqObj in task)
            {
                str += $"{reqObj.Id}. by **{reqObj.UserName}** from **{reqObj.ServerName}** at {reqObj.DateAdded.ToLocalTime ()}\n" +
                       $"**{reqObj.RequestText}**\n----------\n";
            }
            return str + $"\n__Gib [@{BotName} clr] ein, um alle meine Nachrichten zu löschen.__";
        }

        public bool DeleteRequest ( int requestNumber ) =>
            DbHandler.Instance.Delete<DataModels.Request> (requestNumber) != null;

        /// <summary>
        /// Delete a request with a number and returns that request object.
        /// </summary>
        /// <returns>RequestObject of the request. Null if none</returns>
        public DataModels.Request ResolveRequest ( int requestNumber ) =>
            DbHandler.Instance.Delete<DataModels.Request> (requestNumber);

        internal override void Init ( CommandGroupBuilder cgb )
        {

            cgb.CreateCommand ("req")
                .Alias ("request")
                .Description ($"Fordere ein Feature für {BotName}.\n**Benutzung**: @{BotName} req new_feature")
                .Parameter ("all",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var str = e.Args[0];

                    try
                    {
                        SaveRequest (e,str);
                    }
                    catch
                    {
                        await e.Channel.SendMessage ("Etwas ist falsch gelaufen.").ConfigureAwait (false);
                        return;
                    }
                    await e.Channel.SendMessage ("Danke für deine Forderung.").ConfigureAwait (false);
                });

            cgb.CreateCommand ("lr")
                .Description ("Alle Anfragen werden dem User per PN geschickt.")
                .Do (async e =>
                {
                    var str = await Task.Run (() => GetRequests ()).ConfigureAwait (false);
                    if (str.Trim ().Length > 110)
                        await e.User.Send (str).ConfigureAwait (false);
                    else
                        await e.User.Send ("Keine Anfragen derzeit.").ConfigureAwait (false);
                });

            cgb.CreateCommand ("dr")
                .Description ("Löscht eine Forderung. **Owner Only!**")
                .Parameter ("reqNumber",ParameterType.Required)
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                {
                    try
                    {
                        if (DeleteRequest (int.Parse (e.Args[0])))
                        {
                            await e.Channel.SendMessage (e.User.Mention + " Forderung gelöscht.").ConfigureAwait (false);
                        }
                        else
                        {
                            await e.Channel.SendMessage ("Keine Forderung mit dieser Nummer.").ConfigureAwait (false);
                        }
                    }
                    catch
                    {
                        await e.Channel.SendMessage ("Fehler beim Löschen der Forderung.").ConfigureAwait (false);
                    }
                });

            cgb.CreateCommand ("rr")
                .Description ("Erledigt eine Forderung. **Owner Only!**")
                .Parameter ("reqNumber",ParameterType.Required)
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                {
                    try
                    {
                        var sc = ResolveRequest (int.Parse (e.Args[0]));
                        if (sc != null)
                        {

                            await e.Channel.SendMessage (e.User.Mention + " Forderung erledigt, Benachrichtigung gesendet.").ConfigureAwait (false);
                            await MidnightBot.Client.GetServer ((ulong)sc.ServerId).GetUser ((ulong)sc.UserId).Send ("**Diese Forderung von dir wurde erledigt:**\n" + sc.RequestText).ConfigureAwait (false);
                        }
                        else
                        {
                            await e.Channel.SendMessage ("Keine Forderung mit dieser Nummer.").ConfigureAwait (false);
                        }
                    }
                    catch
                    {
                        await e.Channel.SendMessage ("Fehler beim Auflösen der Forderung.").ConfigureAwait (false);
                    }
                });
        }

        public RequestsCommand ( DiscordModule module ) : base (module) { }
    }
}