using MidnightBot.DataModels;
using System;
using Discord;

namespace MidnightBot.Classes
{
    internal static class IncidentsHandler
    {
        public static async void Add ( ulong serverId,ulong channelId,string text )
        {
            var def = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine ($"VORFALL: {text}");
            Console.ForegroundColor = def;
            Channel OwnerPrivateChannel = await MidnightBot.Client.CreatePrivateChannel (MidnightBot.Creds.OwnerIds[0]);
            await OwnerPrivateChannel.SendMessage ($"VORFALL: {text}");
            var incident = new Incident
            {
                ChannelId = (long)channelId,
                ServerId = (long)serverId,
                Text = text,
                Read = false
            };

            DbHandler.Instance.Connection.Insert(incident, typeof(Incident));
        }
    }
}
