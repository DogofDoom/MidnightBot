﻿using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Modules.Permissions.Classes;
using System.IO;
using System.Linq;

namespace MidnightBot.Modules.Administration.Commands
{
    internal class IncidentsCommands : DiscordCommand
    {
        public IncidentsCommands(DiscordModule module) : base(module) { }
        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "listincidents")
                .Alias(Prefix + "lin")
                .Description($"Listet alle UNGELESENEN Vorfälle und markiert sie als gelesen. | `{Prefix}lin`")
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    var sid = (long)e.Server.Id;
                    var incs = DbHandler.Instance.FindAll<Incident>(i => i.ServerId == sid && i.Read == false);
                    DbHandler.Instance.Connection.UpdateAll(incs.Select(i => { i.Read = true; return i; }));

                    await e.User.SendMessage(string.Join("\n----------------------", incs.Select(i => i.Text)));
                });

            cgb.CreateCommand(Module.Prefix + "listallincidents")
                .Alias(Prefix + "lain")
                .Description($"Sendet dir eine Datei mit allen Vorfällen und markiert sie als gelesen. | `{Prefix}lain`")
                .AddCheck(SimpleCheckers.ManageServer())
                .Do(async e =>
                {
                    var sid = (long)e.Server.Id;
                    var incs = DbHandler.Instance.FindAll<Incident>(i => i.ServerId == sid);
                    DbHandler.Instance.Connection.UpdateAll(incs.Select(i => { i.Read = true; return i; }));
                    var data = string.Join("\n----------------------\n", incs.Select(i => i.Text));
                    MemoryStream ms = new MemoryStream();
                    var sw = new StreamWriter(ms);
                    sw.WriteLine(data);
                    sw.Flush();
                    sw.BaseStream.Position = 0;
                    await e.User.SendFile("incidents.txt", sw.BaseStream);
                });
        }
    }
}