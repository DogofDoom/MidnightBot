﻿using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

namespace MidnightBot.Modules.Utility.Commands
{
    class Remind : DiscordCommand
    {

        Regex regex = new Regex (@"^(?:(?<months>\d)mo)?(?:(?<weeks>\d)w)?(?:(?<days>\d{1,2})d)?(?:(?<hours>\d{1,2})h)?(?:(?<minutes>\d{1,2})m)?$",
                                RegexOptions.Compiled | RegexOptions.Multiline);

        List<Timer> reminders = new List<Timer> ();

        IDictionary<string,Func<Reminder,string>> replacements = new Dictionary<string,Func<Reminder,string>>
        {
            { "%message%" , (r) => r.Message },
            { "%user%", (r) => $"<@!{r.UserId}>" },
            { "%target%", (r) =>  r.IsPrivate ? "Direct Message" : $"<#{r.ChannelId}>"}
        };

        public Remind ( DiscordModule module ) : base (module)
        {
            var remList = DbHandler.Instance.GetAllRows<Reminder> ();

            reminders = remList.Select (StartNewReminder).ToList ();
        }

        private Timer StartNewReminder ( Reminder r )
        {
            var now = DateTime.Now;
            var twoMins = new TimeSpan (0,2,0);
            TimeSpan time = (r.When - now) < twoMins
                            ? twoMins       //if the time is less than 2 minutes,
                            : r.When - now; //it will send the message 2 minutes after start
                                            //To account for high bot startup times
            if (time.TotalMilliseconds > int.MaxValue)
                return null;
            var t = new Timer (time.TotalMilliseconds);
            t.Elapsed += async ( s,e ) =>
            {
                try
                {
                    Channel ch;
                    if (r.IsPrivate)
                    {
                        ch = MidnightBot.Client.PrivateChannels.FirstOrDefault (c => (long)c.Id == r.ChannelId);
                        if (ch == null)
                            ch = await MidnightBot.Client.CreatePrivateChannel ((ulong)r.ChannelId).ConfigureAwait (false);
                    }
                    else
                        ch = MidnightBot.Client.GetServer ((ulong)r.ServerId)?.GetChannel ((ulong)r.ChannelId);

                    if (ch == null)
                        return;

                    await ch.SendMessage (
                                            replacements.Aggregate (MidnightBot.Config.RemindMessageFormat,
                                            ( cur,replace ) => cur.Replace (replace.Key,replace.Value (r)))
                                                ).ConfigureAwait (false); //it works trust me          
                }
                catch (Exception ex)
                {
                    Console.WriteLine ($"Timer error! {ex}");
                }
                finally
                {
                    DbHandler.Instance.Delete<Reminder> (r.Id.Value);
                    t.Stop ();
                    t.Dispose ();
                }
            };
            t.Start ();
            return t;
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Module.Prefix + "remind")
                .Description ("Sendet nach einer bestimmten Zeit eine Nachricht in den Channel. " +
                              "Erstes Argument ist me/here/'channelname'. Zweites Argument ist die Zeit in absteigender Reihenfolge (mo>w>d>h>m) Beispiel: 1w5d3h10m. " +
                              "Drittes Argument ist eine (Multiwort)Nachricht. " +
                              $" | `{Prefix}remind me 1d5h Do something` oder `{Prefix}remind #general Start now!`")
                .Parameter ("meorchannel",ParameterType.Required)
                .Parameter ("time",ParameterType.Required)
                .Parameter ("message",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var meorchStr = e.GetArg ("meorchannel").ToUpperInvariant ();
                    Channel ch;
                    bool isPrivate = false;
                    if (meorchStr == "ME")
                    {
                        isPrivate = true;
                        ch = await e.User.CreatePMChannel ().ConfigureAwait (false);
                    }
                    else if (meorchStr == "HERE")
                    {
                        ch = e.Channel;
                    }
                    else
                    {
                        ch = e.Server.FindChannels (meorchStr).FirstOrDefault ();
                    }

                    if (ch == null)
                    {
                        await e.Channel.SendMessage ($"{e.User.Mention} Etwas ist schief gelaufen (Channel konnte nicht gefunden werden) ;(").ConfigureAwait (false);
                        return;
                    }

                    var timeStr = e.GetArg ("time");

                    var m = regex.Match (timeStr);

                    if (m.Length == 0)
                    {
                        await e.Channel.SendMessage ("Kein gültiges Zeitformat blablabla").ConfigureAwait (false);
                        return;
                    }

                    string output = "";
                    var namesAndValues = new Dictionary<string,int> ();

                    foreach (var groupName in regex.GetGroupNames ())
                    {
                        if (groupName == "0")
                            continue;
                        int value = 0;
                        int.TryParse (m.Groups[groupName].Value,out value);

                        if (string.IsNullOrEmpty (m.Groups[groupName].Value))
                        {
                            namesAndValues[groupName] = 0;
                            continue;
                        }
                        else if (value < 1 ||
                            (groupName == "months" && value > 1) ||
                            (groupName == "weeks" && value > 4) ||
                            (groupName == "days" && value >= 7) ||
                            (groupName == "hours" && value > 23) ||
                            (groupName == "minutes" && value > 59))
                        {
                            await e.Channel.SendMessage ($"Ungültiger {groupName} Wert.").ConfigureAwait (false);
                            return;
                        }
                        else
                            namesAndValues[groupName] = value;
                        output += m.Groups[groupName].Value + " " + groupName + " ";
                    }
                    var time = DateTime.Now + new TimeSpan (30 * namesAndValues["months"] +
                                                            7 * namesAndValues["weeks"] +
                                                            namesAndValues["days"],
                                                            namesAndValues["hours"],
                                                            namesAndValues["minutes"],
                                                            0);

                    var rem = new Reminder
                    {
                        ChannelId = (long)ch.Id,
                        IsPrivate = isPrivate,
                        When = time,
                        Message = e.GetArg ("message"),
                        UserId = (long)e.User.Id,
                        ServerId = (long)e.Server.Id
                    };
                    DbHandler.Instance.Connection.Insert(rem);

                    reminders.Add (StartNewReminder (rem));

                    await e.Channel.SendMessage ($"⏰ I will remind \"{ch.Name}\" to \"{e.GetArg ("message").ToString ()}\" in {output}. ({time:d.M.yyyy.} at {time:HH:mm})").ConfigureAwait (false);
                });
            cgb.CreateCommand (Module.Prefix + "remindmsg")
                .Description ("Setzt Nachricht, wenn die Erinnerung ausgelöst wird. " +
                    " Verfügbare Platzhalter sind %user% - Benutzer der den Command ausgeführt hat, %message% -" +
                    $" Nachricht spezifiziert in Erinnerung, %target% - Ziel Channel der Erinnerung. **Bot Owner Only!** | `{Prefix}remindmsg do something else`")
                .Parameter ("msg",ParameterType.Unparsed)
                .AddCheck (SimpleCheckers.OwnerOnly ())
                .Do (async e =>
                {
                    var arg = e.GetArg ("msg")?.Trim ();
                    if (string.IsNullOrWhiteSpace (arg))
                        return;

                    MidnightBot.Config.RemindMessageFormat = arg;
                    await e.Channel.SendMessage ("`New remind message set.`");
                });
        }
    }
}