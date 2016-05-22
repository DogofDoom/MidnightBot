using Discord;
using Discord.Commands;
using Discord.Modules;
using NadekoBot.DataModels;
using NadekoBot.Classes;
using NadekoBot.Extensions;
using NadekoBot.Modules.Administration.Commands;
using NadekoBot.Modules.Permissions.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Administration
{ 
    internal class AdministrationModule : DiscordModule
    {
        public AdministrationModule()
        {
            commands.Add(new ServerGreetCommand(this));
            commands.Add(new LogCommand(this));
            commands.Add(new MessageRepeater(this));
            commands.Add(new PlayingRotate(this));
            commands.Add(new RatelimitCommand(this));
            commands.Add(new VoicePlusTextCommand(this));
            commands.Add(new CrossServerTextChannel(this));
            commands.Add(new SelfAssignedRolesCommand (this));
            commands.Add (new Remind (this));
            commands.Add (new InfoCommands (this));
            commands.Add (new CustomReactionsCommands (this));
            commands.Add (new AutoAssignRole (this));
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Administration;

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {

                cgb.AddCheck (PermissionChecker.Instance);

                var client = manager.Client;

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "rules")
                    .Description ("Regeln")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ($"1. Keine Fremdwerbung jeglicher art! (Ob für Konfi,Server oder Forum)\n2.Obwohl es hoffentlich verständlich ist, keinerlei Rassistische, Sexistische Äußerungen!\n3.Kein Spam!Weder TTS, Command noch normal!\n4.Bei Benutzung des Voicechats hat der richtige Channel gewählt zu werden, sodass in den Spiel - Channeln gespielt wird und im Plauderchannel geredet wird!\n5.Bitte versucht wenigstens einigermaßen auf eure Rechtschreibung zu achten!\n6.Beleidigungen boshafter Art sind zu unterlassen!\nVergehen werden ja nach Stärke mit Bann oder Kick bestraft!");
                    });

                cgb.CreateCommand (Prefix + "restart")
                    .Description ("Startet den Bot neu. Könnte nicht funktionieren.")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("`Neustart in 2 Sekunden...`");
                        await Task.Delay (2000);
                        System.Diagnostics.Process.Start (System.Reflection.Assembly.GetExecutingAssembly ().Location);
                        Environment.Exit (0);
                    });

                cgb.CreateCommand (Prefix + "sr").Alias (Prefix + "setrole")
                    .Description ("Setzt die Rolle für einen gegebenen Benutzer.\n**Benutzung**: .sr @User Gast")
                    .Parameter ("user_name",ParameterType.Required)
                    .Parameter ("role_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.CanManageRoles)
                    .Do (async e =>
                     {
                         var userName = e.GetArg ("user_name");
                         var roleName = e.GetArg ("role_name");

                         if (string.IsNullOrWhiteSpace (roleName))
                             return;

                         if (!e.User.ServerPermissions.ManageRoles)
                         {
                             await e.Channel.SendMessage ("Du hast zu wenig Rechte.").ConfigureAwait (false);
                         }

                         var usr = e.Server.FindUsers (userName).FirstOrDefault ();
                         if (usr == null)
                         {
                             await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                             return;
                         }

                         var role = e.Server.FindRoles (roleName).FirstOrDefault ();
                         if (role == null)
                         {
                             await e.Channel.SendMessage ("Keine gültige Rolle.").ConfigureAwait (false);
                             return;
                         }

                         try
                         {
                             await usr.AddRoles (role).ConfigureAwait (false);
                             await e.Channel.SendMessage ($"Rolle **{role.Name}** erfolgreich zu Benutzer **{usr.Name}** hinzugefügt").ConfigureAwait (false);
                         }
                         catch (Exception ex)
                         {
                             await e.Channel.SendMessage ("Hinzufügen der Rolle gescheitert. Bot hat zu wenig Rechte.\n").ConfigureAwait (false);
                             Console.WriteLine (ex.ToString ());
                         }
                     });

                cgb.CreateCommand (Prefix + "rr").Alias (Prefix + "removerole")
                    .Description ("Entfernt eine Rolle von einem gegebenen User.\n**Benutzung**: .rr @User Admin")
                    .Parameter ("user_name",ParameterType.Required)
                    .Parameter ("role_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.CanManageRoles)
                    .Do (async e =>
                     {
                         var userName = e.GetArg ("user_name");
                         var roleName = e.GetArg ("role_name");

                         if (string.IsNullOrWhiteSpace (roleName))
                             return;

                         var usr = e.Server.FindUsers (userName).FirstOrDefault ();
                         if (usr == null)
                         {
                             await e.Channel.SendMessage ("Ungültiger Benutzername").ConfigureAwait (false);
                             return;
                         }

                         var role = e.Server.FindRoles (roleName).FirstOrDefault ();
                         if (role == null)
                         {
                             await e.Channel.SendMessage ("Ungültige Rolle").ConfigureAwait (false);
                             return;
                         }

                         try
                         {
                             await usr.RemoveRoles (role).ConfigureAwait (false);
                             await e.Channel.SendMessage ($"Rolle **{role.Name}** erfolgreich von User **{usr.Name}** entfernt.").ConfigureAwait (false);
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Entfernen von Rolle fehlgeschlagen. Wahrscheinlicher Grund: Nicht genug Berechtigung.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "renr")
                    .Alias (Prefix + "renamerole")
                    .Description ($"Benennt eine Rolle um. Rolle die umbenannt werden soll muss muss in Liste niedriger sein als die höchste Rolle des Bots.\n**Benutzung**: `{Prefix}renr \"Erste Rolle\" ZweiteRolle`")
                    .Parameter ("r1",ParameterType.Required)
                    .Parameter ("r2",ParameterType.Required)
                    .AddCheck (new SimpleCheckers.ManageRoles ())
                    .Do (async e =>
                    {
                        var r1 = e.GetArg ("r1").Trim ();
                        var r2 = e.GetArg ("r2").Trim ();

                        var roleToEdit = e.Server.FindRoles (r1).FirstOrDefault ();
                        if (roleToEdit == null)
                        {
                            await e.Channel.SendMessage ("Kann diese Rolle nicht finden.");
                            return;
                        }

                        try
                        {
                            if (roleToEdit.Position > e.Server.CurrentUser.Roles.Max (r => r.Position))
                            {
                                await e.Channel.SendMessage ("Ich kann Rollen die höher sind als meine nicht bearbeiten.");
                                return;
                            }
                            await roleToEdit.Edit (r2);
                            await e.Channel.SendMessage ("Rolle umbenannt.");
                        }
                        catch (Exception)
                        {
                            await e.Channel.SendMessage ("Fehler beim umbennennen der Rolle. Wahrscheinlich nicht genug Rechte.");
                        }
                    });

                cgb.CreateCommand (Prefix + "rar").Alias (Prefix + "removeallroles")
                    .Description ("Entfernt alle Rollen eines Benutzers.\n**Benutzung**: .rar @User")
                    .Parameter ("user_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.CanManageRoles)
                    .Do (async e =>
                    {
                        var userName = e.GetArg ("user_name");

                        var usr = e.Server.FindUsers (userName).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("Kein gültiger Benutzer.").ConfigureAwait (false);
                            return;
                        }

                        try
                        {
                            await usr.RemoveRoles (usr.Roles.ToArray ()).ConfigureAwait (false);
                            await e.Channel.SendMessage ($"Erfolgreich **alle** Rollen von Benutzer **{usr.Name}** entferrnt.").ConfigureAwait (false);
                        }
                        catch
                        {
                            await e.Channel.SendMessage ("Rollen konnten nicht entfernt werden. Nicht ausreichende Berechtigungen.").ConfigureAwait (false);
                        }
                    });

            cgb.CreateCommand (Prefix + "r").Alias (Prefix + "role").Alias (Prefix + "cr")
                    .Description ("Erstelle eine Rolle mit einem bestimmten Namen.**Benutzung**: `.r Awesome Role`")
                    .Parameter ("role_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.CanManageRoles)
                    .Do (async e =>
                     {
                         if (string.IsNullOrWhiteSpace (e.GetArg ("role_name")))
                             return;
                         try
                         {
                             var r = await e.Server.CreateRole (e.GetArg ("role_name")).ConfigureAwait (false);
                             await e.Channel.SendMessage ($"Rolle **{r.Name}** erfolgreich erstellt.").ConfigureAwait (false);
                         }
                         catch (Exception)
                         {
                             await e.Channel.SendMessage (":warning: Unspecified error.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "rolecolor").Alias (Prefix + "rc")
                    .Parameter ("role_name",ParameterType.Required)
                    .Parameter ("r",ParameterType.Optional)
                    .Parameter ("g",ParameterType.Optional)
                    .Parameter ("b",ParameterType.Optional)
                    .Description ("Setzt die Farbe einer Rolle zur Hex, oder RGB Farb-Value die gegeben wird.\n**Benutzung**: `.color Admin 255 200 100 oderr .color Admin ffba55`")
                    .Do (async e =>
                     {
                         if (!e.User.ServerPermissions.ManageRoles)
                         {
                             await e.Channel.SendMessage ("Du hast nicht genug Berechtigungen!").ConfigureAwait (false);
                             return;
                         }

                         var args = e.Args.Where (s => s != string.Empty);

                         if (args.Count () != 2 && args.Count () != 4)
                         {
                             await e.Channel.SendMessage ("Eingegebene Parameter sind falsch.").ConfigureAwait (false);
                             return;
                         }

                         var role = e.Server.FindRoles (e.Args[0]).FirstOrDefault ();

                         if (role == null)
                         {
                             await e.Channel.SendMessage ("Diese Rolle existiert nicht.").ConfigureAwait (false);
                             return;
                         }
                         try
                         {
                             var rgb = args.Count () == 4;
                             var arg1 = e.Args[1].Replace ("#","");

                             var red = Convert.ToByte (rgb ? int.Parse (arg1) : Convert.ToInt32 (arg1.Substring (0,2),16));
                             var green = Convert.ToByte (rgb ? int.Parse (e.Args[2]) : Convert.ToInt32 (arg1.Substring (2,2),16));
                             var blue = Convert.ToByte (rgb ? int.Parse (e.Args[3]) : Convert.ToInt32 (arg1.Substring (4,2),16));

                             await role.Edit (color: new Color (red,green,blue)).ConfigureAwait (false);
                             await e.Channel.SendMessage ($"Die Farbe der Rolle {role.Name} wurde geändert.").ConfigureAwait (false);
                         }
                         catch (Exception)
                         {
                             await e.Channel.SendMessage ("Fehler, nicht genug Rechte.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "roles")
                  .Description ("Listet alle Rollen auf diesem Server, oder die eines Benutzers wenn spezifiziert.")
                  .Parameter ("user",ParameterType.Unparsed)
                  .Do (async e =>
                   {
                       if (!string.IsNullOrWhiteSpace (e.GetArg ("user")))
                       {
                           var usr = e.Server.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                           if (usr == null)
                               return;

                           await e.Channel.SendMessage ($"`Liste der Rollen von **{usr.Name}**:` \n• " + string.Join ("\n• ",usr.Roles)).ConfigureAwait (false);
                           return;
                       }
                       await e.Channel.SendMessage ("`Liste der Rollen:` \n• " + string.Join ("\n• ",e.Server.Roles)).ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "b").Alias (Prefix + "ban")
                    .Parameter ("user",ParameterType.Required)
                    .Parameter ("msg",ParameterType.Optional)
                    .Description ("Bannt einen erwähnten Benutzer.\n**Benutzung**: .b \"@some Guy\" Your behaviour is toxic.")
                        .Do (async e =>
                         {
                             var msg = e.GetArg ("msg");
                             var user = e.GetArg ("user");
                             if (e.User.ServerPermissions.BanMembers)
                             {
                                 var usr = e.Server.FindUsers (user).FirstOrDefault ();
                                 if (usr == null)
                                 {
                                     await e.Channel.SendMessage ("Benutzer nicht gefunden.").ConfigureAwait (false);
                                     return;
                                 }
                                 if (!string.IsNullOrWhiteSpace (msg))
                                 {
                                     await usr.SendMessage ($"**Du wurdest vom Server `{e.Server.Name}` gebannt.**\n" +
                                                           $"Grund: {msg}").ConfigureAwait (false);
                                     await Task.Delay (2000).ConfigureAwait (false); // temp solution; give time for a message to be send, fu volt
                                }
                                 try
                                 {
                                     await e.Server.Ban (usr).ConfigureAwait (false);
                                     await e.Channel.SendMessage ("User gebannt: " + usr.Name + " Id: " + usr.Id).ConfigureAwait (false);
                                 }
                                 catch
                                 {
                                     await e.Channel.SendMessage ("Fehler, nicht genug Rechte.").ConfigureAwait (false);
                                 }
                             }
                         });

                cgb.CreateCommand (Prefix + "k").Alias (Prefix + "kick")
                    .Parameter ("user")
                    .Parameter ("msg",ParameterType.Unparsed)
                    .Description ("Kickt einen erwähnten User.")
                    .Do (async e =>
                     {
                         var msg = e.GetArg ("msg");
                         var user = e.GetArg ("user");
                         if (e.User.ServerPermissions.KickMembers)
                         {
                             var usr = e.Server.FindUsers (user).FirstOrDefault ();
                             if (usr == null)
                             {
                                 await e.Channel.SendMessage ("Benutzer nicht gefunden.").ConfigureAwait (false);
                                 return;
                             }
                             if (!string.IsNullOrWhiteSpace (msg))
                             {
                                 await usr.SendMessage ($"**Du wurdest vom Server `{e.Server.Name}` gekickkt.**\n" +
                                                       $"Grund: {msg}").ConfigureAwait (false);
                                 await Task.Delay (2000); // temp solution; give time for a message to be send, fu volt
                            }
                             try
                             {
                                 await usr.Kick ().ConfigureAwait (false);
                                 await e.Channel.SendMessage ("User gekickt: " + usr.Name + " Id: " + usr.Id).ConfigureAwait (false);
                             }
                             catch
                             {
                                 await e.Channel.SendMessage ("Fehler, nicht genug Rechte.").ConfigureAwait (false);
                             }
                         }
                     });
                cgb.CreateCommand (Prefix + "mute")
                    .Description ("Mutet erwähnte Benutzer.")
                    .Parameter ("throwaway",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         if (!e.User.ServerPermissions.MuteMembers)
                         {
                             await e.Channel.SendMessage ("Du hast nicht genug Berechtigungen dies zu tun.").ConfigureAwait (false);
                             return;
                         }
                         if (!e.Message.MentionedUsers.Any ())
                             return;
                         try
                         {
                             foreach (var u in e.Message.MentionedUsers)
                             {
                                 await u.Edit (isMuted: true).ConfigureAwait (false);
                             }
                             await e.Channel.SendMessage ("Mute erfolgreich").ConfigureAwait (false);
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Ich habe keine Berechtigungen dafür.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "unmute")
                    .Description ("Entmutet erwähnte Benutzer.")
                    .Parameter ("throwaway",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         if (!e.User.ServerPermissions.MuteMembers)
                         {
                             await e.Channel.SendMessage ("Du hast nicht genug Berechtigungen dies zu tun.").ConfigureAwait (false);
                             return;
                         }
                         if (!e.Message.MentionedUsers.Any ())
                             return;
                         try
                         {
                             foreach (var u in e.Message.MentionedUsers)
                             {
                                 await u.Edit (isMuted: false).ConfigureAwait (false);
                             }
                             await e.Channel.SendMessage ("Entmute erfolgreich.").ConfigureAwait (false);
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Ich habe keine Berechtigungen dafür.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "deafen")
                    .Alias (Prefix + "deaf")
                    .Description ("Stellt erwähnte Benutzer Taub.")
                    .Parameter ("throwaway",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         if (!e.User.ServerPermissions.DeafenMembers)
                         {
                             await e.Channel.SendMessage ("Du hast nicht genug Berechtigungen dies zu tun.").ConfigureAwait (false);
                             return;
                         }
                         if (!e.Message.MentionedUsers.Any ())
                             return;
                         try
                         {
                             foreach (var u in e.Message.MentionedUsers)
                             {
                                 await u.Edit (isDeafened: true).ConfigureAwait (false);
                             }
                             await e.Channel.SendMessage ("Taubstellung erfolgreich.").ConfigureAwait (false);
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Ich habe keine Berechtigungen dafür.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "undeafen")
                    .Alias (Prefix + "undeaf")
                    .Description ("Erwähnte Benutzer sind nicht mehr taub.")
                    .Parameter ("throwaway",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         if (!e.User.ServerPermissions.DeafenMembers)
                         {
                             await e.Channel.SendMessage ("Du hast nicht genug Berechtigungen dies zu tun.").ConfigureAwait (false);
                             return;
                         }
                         if (!e.Message.MentionedUsers.Any ())
                             return;
                         try
                         {
                             foreach (var u in e.Message.MentionedUsers)
                             {
                                 await u.Edit (isDeafened: false).ConfigureAwait (false);
                             }
                             await e.Channel.SendMessage ("Erfolgreich").ConfigureAwait (false);
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Ich habe keine Berechtigungen dafür.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "rvch")
                    .Description ("Entfernt einen Voice-Channel mit einem gegebenen Namen..")
                    .Parameter ("channel_name",ParameterType.Required)
                    .Do (async e =>
                     {
                         try
                         {
                             if (e.User.ServerPermissions.ManageChannels)
                             {
                                 var ch = e.Server.FindChannels (e.GetArg ("channel_name"),ChannelType.Voice).FirstOrDefault ();
                                 if (ch == null)
                                     return;
                                 await ch.Delete ().ConfigureAwait (false);
                                 await e.Channel.SendMessage ($"-VoiceChannel **{e.GetArg ("channel_name")}** entfernt.").ConfigureAwait (false);
                             }
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Nicht genug Rechte.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "vch").Alias (Prefix + "cvch")
                    .Description ("Erstellt einen neuen Voice-Channel mit einem gegebenen Namen.")
                    .Parameter ("channel_name",ParameterType.Required)
                    .Do (async e =>
                     {
                         try
                         {
                             if (e.User.ServerPermissions.ManageChannels)
                             {
                                 await e.Server.CreateChannel (e.GetArg ("channel_name"),ChannelType.Voice).ConfigureAwait (false);
                                 await e.Channel.SendMessage ($"Voice Channel **{e.GetArg ("channel_name")}** erstellt.").ConfigureAwait (false);
                             }
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Nicht genug Rechte.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "rch").Alias (Prefix + "rtch")
                    .Description ("Entfernt einen Text-Channel mit einem gegebenen Namen.")
                    .Parameter ("channel_name",ParameterType.Required)
                    .Do (async e =>
                     {
                         try
                         {
                             if (e.User.ServerPermissions.ManageChannels)
                             {
                                 var channel = e.Server.FindChannels (e.GetArg ("channel_name"),ChannelType.Text).FirstOrDefault ();
                                 if (channel == null)
                                     return;
                                 await channel.Delete ().ConfigureAwait (false);
                                 await e.Channel.SendMessage ($"Text-Channel **{e.GetArg ("channel_name")}** entfernt.").ConfigureAwait (false);
                             }
                         }
                         catch
                         {
                             await e.Channel.SendMessage ("Nicht genug Rechte.").ConfigureAwait (false);
                         }
                     });

                cgb.CreateCommand (Prefix + "ch").Alias (Prefix + "tch")
                    .Description ("Erstellt einen Text-Channel mit einem gegebenen Namen.")
                    .Parameter ("channel_name",ParameterType.Required)
                    .Do (async e =>
                    {
                        try
                        {
                            if (e.User.ServerPermissions.ManageChannels)
                            {
                                await e.Server.CreateChannel (e.GetArg ("channel_name"),ChannelType.Text).ConfigureAwait (false);
                                await e.Channel.SendMessage ($"Text-Channel **{e.GetArg ("channel_name")}** hinzugefügt.").ConfigureAwait (false);
                            }
                        }
                        catch
                        {
                            await e.Channel.SendMessage ("Insufficient permissions.").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "st").Alias (Prefix + "settopic")
                    .Alias (Prefix + "topic")
                    .Description ("Setzt eine Beschreibung für den derzeitigen Channel.\n**Benutzung**: `{Prefix}st My new topic`")
                    .AddCheck (SimpleCheckers.ManageChannels ())
                    .Parameter ("topic",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var topic = e.GetArg ("topic")?.Trim () ?? "";
                        await e.Channel.Edit (topic: topic).ConfigureAwait (false);
                        await e.Channel.SendMessage (":ok: **Channel Topic gesetzt.**").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "schn").Alias (Prefix + "setchannelname")
                    .Alias (Prefix + "topic")
                    .Description ("Ändert den Namen des derzeitigen Channels.")
                    .AddCheck (SimpleCheckers.ManageChannels ())
                    .Parameter ("name",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var name = e.GetArg ("name");
                        if (string.IsNullOrWhiteSpace (name))
                            return;
                        await e.Channel.Edit (name: name).ConfigureAwait (false);
                        await e.Channel.SendMessage (":ok: **Neuer Channel Name gesetzt.**").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "uid").Alias (Prefix + "userid")
                    .Description ("Zeigt die ID eines Benutzers.")
                    .Parameter ("user",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         var usr = e.User;
                         if (!string.IsNullOrWhiteSpace (e.GetArg ("user")))
                             usr = e.Channel.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                         if (usr == null)
                             return;
                         await e.Channel.SendMessage ($"Id des Users { usr.Name } ist { usr.Id }").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "cid").Alias (Prefix + "channelid")
                    .Description ("Zeigt ID des derzeitigen Channels")
                    .Do (async e => await e.Channel.SendMessage ("Die ID des derzeitigen Channels ist " + e.Channel.Id).ConfigureAwait (false));

                cgb.CreateCommand (Prefix + "sid").Alias (Prefix + "serverid")
                    .Description ("Zeigt ID des derzeitigen Servers.")
                    .Do (async e => await e.Channel.SendMessage ("Die ID des derzeitigen Servers ist " + e.Server.Id).ConfigureAwait (false));

                cgb.CreateCommand (Prefix + "stats")
                    .Description ("Zeigt ein paar Statisitken über MidnightBot.")
                    .Do (async e =>
                     {
                         await e.Channel.SendMessage (await NadekoStats.Instance.GetStats ());
                     });

                cgb.CreateCommand (Prefix + "dysyd")
                    .Description ("Zeigt ein paar Statisitken über MidnightBot.")
                    .Do (async e =>
                     {
                         await e.Channel.SendMessage ((await NadekoStats.Instance.GetStats ()).Matrix ().TrimTo (1990)).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "heap")
                  .Description ("Zeigt benutzten Speicher - **Owner Only!**")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .Do (async e =>
                   {
                       var heap = await Task.Run (() => NadekoStats.Instance.Heap ()).ConfigureAwait (false);
                       await e.Channel.SendMessage ($"`Heap Size:` {heap}").ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "prune")
                    .Alias (Prefix + "clr")
                    .Description (@"`.prune` alle von MidnightBots Nachrichten, in den letzten 100 Nachrichten.`.prune X` entfernt die letzten X Nachrichten von diesem Channel (bis zu 100)`.prune @Someone` Entfernt alle Nachrichten einer Person. in den letzten 100 Nachrichten.`.prune @Someone X` Entfernt die letzen X Nachrichten einer Person in diesem Channel.\n**Benutzung**: `.prune` oder `.prune 5` oder `.prune @Someone` oder `.prune @Someone X`")
                    .Parameter ("user_or_num",ParameterType.Optional)
                    .Parameter ("num",ParameterType.Optional)
                    .Do (async e =>
                     {
                         if (string.IsNullOrWhiteSpace (e.GetArg ("user_or_num"))) // if nothing is set, clear nadeko's messages, no permissions required
                         {
                         await Task.Run (async () =>
                         {
                             var msgs = (await e.Channel.DownloadMessages (100).ConfigureAwait (false)).Where (m => m.User.Id == e.Server.CurrentUser.Id);
                             foreach (var m in msgs)
                             {
                                 try
                                 {
                                     await m.Delete ().ConfigureAwait (false);
                                 }
                                 catch { }
                                 await Task.Delay (100).ConfigureAwait (false);
                             }

                         }).ConfigureAwait (false);
                             return;
                         }
                         if (!e.User.GetPermissions (e.Channel).ManageMessages)
                             return;
                         else if (!e.Server.CurrentUser.GetPermissions (e.Channel).ManageMessages)
                         {
                             await e.Channel.SendMessage ("💢Ich habe keine Berechtigungen um Nachrichten zu löschen.");
                             return;
                         }
                         int val;
                         if (int.TryParse (e.GetArg ("user_or_num"),out val)) // if num is set in the first argument, 
                                                                              //delete that number of messages.
                         {
                             if (val <= 0)
                                 return;
                             val++;
                             foreach (var msg in await e.Channel.DownloadMessages (val).ConfigureAwait (false))
                             {
                                 await msg.Delete ().ConfigureAwait (false);
                                 await Task.Delay (100).ConfigureAwait (false);
                             }
                             return;
                         }
                         //else if first argument is user
                         var usr = e.Server.FindUsers (e.GetArg ("user_or_num")).FirstOrDefault ();
                         if (usr == null)
                             return;
                         val = 100;
                         if (!int.TryParse (e.GetArg ("num"),out val))
                             val = 100;

                         await Task.Run (async () =>
                         {
                             var msgs = (await e.Channel.DownloadMessages (100).ConfigureAwait (false)).Where (m => m.User.Id == usr.Id).Take (val);
                             foreach (var m in msgs)
                             {
                                 try
                                 {
                                     await m.Delete ().ConfigureAwait (false);
                                 }
                                 catch { }
                                 await Task.Delay (100).ConfigureAwait (false);
                             }

                         }).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "die")
                    .Alias (Prefix + "graceful")
                    .Description ("Fährt den Bot herunter und benachrichtigt Benutzer über den Neustart. **Owner Only!**")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        foreach (var ch in NadekoBot.Client.Servers.Select (s => s.DefaultChannel))
                        {
                            await ch.SendMessage ("`Fährt herunter.`");
                        }
                        //await e.Channel.SendMessage ("`Fährt herunter.`").ConfigureAwait (false);
                        await Task.Delay (2000).ConfigureAwait (false);
                        Environment.Exit (0);
                    });

                //cgb.CreateCommand(Prefix + "newnick")
                //    .Alias(Prefix + "setnick")
                //    .Description("Gibt dem Bot einen neuen Nicknamen. Du benötigst 'manage server permissions'.")
                //    .Parameter("new_nick", ParameterType.Unparsed)
                //    .AddCheck(SimpleCheckers.ManageServer())
                //    .Do(async e =>
                //    {
                //        if (e.GetArg("new_nick") == null) return;

                //        await client.CurrentUser.Edit(NadekoBot.Creds.Password, e.GetArg("new_nick")).ConfigureAwait(false);
                //    });

                cgb.CreateCommand (Prefix + "newname")
                    .Alias (Prefix + "setname")
                    .Description ("Gibt dem Bot einen neuen Namen. **Owner Only!**")
                    .Parameter ("new_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         if (e.GetArg("new_name") == null) return;

                         await client.CurrentUser.Edit (NadekoBot.Creds.Password,e.GetArg ("new_name")).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "newavatar")
                    .Alias (Prefix + "setavatar")
                    .Description ("Setzt ein neues Profilbild für MidnightBot. **Owner Only!**")
                    .Parameter ("img",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         if (string.IsNullOrWhiteSpace (e.GetArg ("img")))
                             return;
                         // Gather user provided URL.
                         var avatarAddress = e.GetArg ("img");
                         var imageStream = await SearchHelper.GetResponseStreamAsync (avatarAddress).ConfigureAwait (false);
                         var image = System.Drawing.Image.FromStream (imageStream);
                         // Save the image to disk.
                         image.Save ("data/avatar.png",System.Drawing.Imaging.ImageFormat.Png);
                         await client.CurrentUser.Edit (NadekoBot.Creds.Password,avatar: image.ToStream ()).ConfigureAwait (false);
                         // Send confirm.
                         await e.Channel.SendMessage ("Neuer Avatar gesetzt.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "setgame")
                  .Description ("Setzt das Spiel des Bots. **Owner Only!**")
                  .Parameter ("set_game",ParameterType.Unparsed)
                  .Do (e =>
                   {
                       if (!NadekoBot.IsOwner (e.User.Id) || e.GetArg ("set_game") == null)
                           return;

                       client.SetGame (e.GetArg ("set_game"));
                   });

                cgb.CreateCommand (Prefix + "checkmyperms")
                    .Description ("Kontrolliere deine Berechtigungen auf diesem Server.")
                    .Do (async e =>
                     {
                         var output = "```\n";
                         foreach (var p in e.User.ServerPermissions.GetType ().GetProperties ().Where (p => !p.GetGetMethod ().GetParameters ().Any ()))
                         {
                             output += p.Name + ": " + p.GetValue (e.User.ServerPermissions,null).ToString () + "\n";
                         }
                         output += "```";
                         await e.User.SendMessage (output).ConfigureAwait (false);
                     });

                Server commsServer = null;
                User commsUser = null;
                Channel commsChannel = null;

                cgb.CreateCommand (Prefix + "commsuser")
                    .Description ("Setzt einen Benutzer für die Throug-Bot Kommunikation. Funktioniert nur, wenn Server gesetzt ist. Resettet commschannel. **Owner Only!**")
                    .Parameter ("name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         commsUser = commsServer?.FindUsers (e.GetArg ("name")).FirstOrDefault ();
                         if (commsUser != null)
                         {
                             commsChannel = null;
                             await e.Channel.SendMessage ("Benutzer für Kommunikation gesetzt.").ConfigureAwait (false);
                         }
                         else
                             await e.Channel.SendMessage ("Kein Server, oder Benutzer spezifiziert.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "commsserver")
                    .Description ("Setzt einen Server für Through-Bot Kommunikation. **Owner Only!**")
                    .Parameter ("server",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         commsServer = client.FindServers (e.GetArg ("server")).FirstOrDefault ();
                         if (commsServer != null)
                             await e.Channel.SendMessage ("Server for Kommunikation gesetzt.").ConfigureAwait (false);
                         else
                             await e.Channel.SendMessage ("Kein solcher Server.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "commschannel")
                    .Description ("Setzt einen Channel für Through-Bot Kommunikation. Funktioniert nur, wenn Server gesetzt ist. Resettet commsuser. **Owner Only!**")
                    .Parameter ("ch",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         commsChannel = commsServer?.FindChannels (e.GetArg ("ch"),ChannelType.Text).FirstOrDefault ();
                         if (commsChannel != null)
                         {
                             commsUser = null;
                             await e.Channel.SendMessage ("Server für Kommunikation gesetzt.").ConfigureAwait (false);
                         }
                         else
                             await e.Channel.SendMessage ("Kein Server spezifiziert, oder Channel ungültig.").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "send")
                    .Description ("Sende eine Nachricht an einen User auf einem anderen Server über den Bot..**Owner Only!****\n **Benutzung**: .send Message text multi word!")
                    .Parameter ("msg",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         if (commsUser != null)
                             await commsUser.SendMessage (e.GetArg ("msg")).ConfigureAwait (false);
                         else if (commsChannel != null)
                             await commsChannel.SendMessage (e.GetArg ("msg")).ConfigureAwait (false);
                         else
                             await e.Channel.SendMessage ("Fehlgeschlagen. Stell sicher, dass du den Server und [Channel oder User] angegeben hast.]").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "menrole")
                    .Alias (Prefix + "mentionrole")
                    .Description ("Erwähnt jeden User mit einer bestimmten Rolle oder bestimmten Rollen (Getrennt mit einem ',') auf diesem Server. 'Mention everyone' Berechtigung erforderlich.")
                    .Parameter ("roles",ParameterType.Unparsed)
                    .Do (async e =>
                     {
                         await Task.Run (async () =>
                         {
                             if (!e.User.ServerPermissions.MentionEveryone)
                                 return;
                             var arg = e.GetArg ("roles").Split (',').Select (r => r.Trim ());
                             string send = $"--{e.User.Mention} hat eine Erwähnung der folgenden Rollen aufgerufen--";
                             foreach (var roleStr in arg.Where (str => !string.IsNullOrWhiteSpace (str)))
                             {
                                 var role = e.Server.FindRoles (roleStr).FirstOrDefault ();
                                 if (role == null)
                                     continue;
                                 send += $"\n`{role.Name}`\n";
                                 send += string.Join (", ",role.Members.Select (r => r.Mention));
                             }

                             while (send.Length > 2000)
                             {
                                 var curstr = send.Substring (0,2000);
                                 await
                                     e.Channel.Send (curstr.Substring (0,
                                         curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1)).ConfigureAwait (false);
                                 send = curstr.Substring (curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1) +
                                        send.Substring (2000);
                             }
                             await e.Channel.Send (send).ConfigureAwait (false);
                         }).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "inrole")
                    .Description ("Listet alle Benutzer von einer angegebenen Rolle, oder Rollen (getrennt mit einem ',') auf diesem Server.")
                    .Parameter ("roles",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.User.ServerPermissions.MentionEveryone)
                                return;
                            var arg = e.GetArg ("roles").Split (',').Select (r => r.Trim ());
                            string send = $"`Hier ist eine Liste alle Benutzer mit einer bestimmten Rolle:`";
                            foreach (var roleStr in arg.Where (str => !string.IsNullOrWhiteSpace (str)))
                            {
                                var role = e.Server.FindRoles (roleStr).FirstOrDefault ();
                                if (role == null)
                                    continue;
                                send += $"\n`{role.Name}`\n";
                                send += string.Join (", ",role.Members.Select (r => "**" + r.Name + "**#" + r.Discriminator));
                            }

                            while (send.Length > 2000)
                            {
                                var curstr = send.Substring (0,2000);
                                await
                                    e.Channel.Send (curstr.Substring (0,
                                        curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1)).ConfigureAwait (false);
                                send = curstr.Substring (curstr.LastIndexOf (", ",StringComparison.Ordinal) + 1) +
                                       send.Substring (2000);
                            }
                            await e.Channel.Send (send).ConfigureAwait (false);
                        }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "parsetosql")
                  .Description ("Lädt exportierte Parsedata von /data/parsedata/ in Sqlite Datenbank.")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .Do (async e =>
                   {
                       await Task.Run (() =>
                       {
                           SaveParseToDb<Announcement> ("data/parsedata/Announcements.json");
                           SaveParseToDb<DataModels.Command> ("data/parsedata/CommandsRan.json");
                           SaveParseToDb<Request> ("data/parsedata/Requests.json");
                           SaveParseToDb<Stats> ("data/parsedata/Stats.json");
                           SaveParseToDb<TypingArticle> ("data/parsedata/TypingArticles.json");
                       }).ConfigureAwait (false);
                   });

                cgb.CreateCommand (Prefix + "unstuck")
                  .Description ("Löscht die Nachrichten-Liste. **Owner Only!**")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .Do (e =>
                   {
                       NadekoBot.Client.MessageQueue.Clear ();
                   });

                cgb.CreateCommand (Prefix + "donators")
                    .Description ("Liste von Leuten die dieses Projekt unterstützen.")
                    .Do (async e =>
                     {
                         await Task.Run (async () =>
                         {
                             var rows = DbHandler.Instance.GetAllRows<Donator> ();
                             var donatorsOrdered = rows.OrderByDescending (d => d.Amount);
                             string str = $"**Danke an die Leute die unten gelistet sind, für das unterstützen dieses Projektes!**\n";

                             await e.Channel.SendMessage (str + string.Join ("⭐",donatorsOrdered.Select (d => d.UserName))).ConfigureAwait (false);
                         }).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "clear")
                    .Parameter ("num",ParameterType.Required)
                    .Parameter ("user",ParameterType.Required)
                    .Description ("Entfernt eine Anzahl Nachrichten eines bestimmten Users aus dem Chat. **Owner Only!**\n **Benutzung**: .clear 5 @User")
                    .Do (async e =>
                    {
                        await Task.Run (async () =>
                        {
                            if (!e.User.ServerPermissions.ManageMessages)
                                return;
                            int val;
                            if (string.IsNullOrWhiteSpace (e.GetArg ("num")) || !int.TryParse (e.GetArg ("num"),out val) || val < 0)
                                return;
                            var tester = 0;
                            var u = e.Channel.FindUsers (e.GetArg ("user")).FirstOrDefault ();
                            if (u == null)
                            {
                                await e.Send ("Ungültiger Benutzer.").ConfigureAwait (false);
                                return;
                            }
                            var msgs = (await e.Channel.DownloadMessages (100)).Where (m => m.User.Id == u.Id);
                            foreach (var m in msgs)
                            {
                                if (tester <= val)
                                {
                                    await m.Delete ().ConfigureAwait (false);
                                    tester++;
                                }
                            }
                        });
                    });
                //THIS IS INTENTED TO BE USED ONLY BY THE ORIGINAL BOT OWNER
                cgb.CreateCommand (Prefix + "adddon")
                    .Alias (Prefix + "donadd")
                    .Description ("Fügt einen Donator zur Datenbank hinzu.")
                    .Parameter ("donator")
                    .Parameter ("amount")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await Task.Run (() =>
                         {
                             var donator = e.Server.FindUsers (e.GetArg ("donator")).FirstOrDefault ();
                             var amount = int.Parse (e.GetArg ("amount"));
                             if (donator == null)
                                 return;
                             try
                             {
                                 DbHandler.Instance.InsertData (new Donator
                                 {
                                     Amount = amount,
                                     UserName = donator.Name,
                                     UserId = (long)donator.Id
                                 });
                                 e.Channel.SendMessage ("Donator erfolgreich hinzugefügt. 👑").ConfigureAwait (false);
                             }
                             catch { }
                         }).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "videocall")
                  .Description ("Erstellt privaten appear.in Video Anruf Link für dich und eine erwähnte Person und sendet sie per privater Nachricht.")
                  .Parameter ("arg",ParameterType.Unparsed)
                  .Do (async e =>
                   {
                       try
                       {
                           var allUsrs = e.Message.MentionedUsers.Union (new User[] { e.User });
                           var allUsrsArray = allUsrs as User[] ?? allUsrs.ToArray ();
                           var str = allUsrsArray.Aggregate ("http://appear.in/",( current,usr ) => current + Uri.EscapeUriString (usr.Name[0].ToString ()));
                           str += new Random ().Next ();
                           foreach (var usr in allUsrsArray)
                           {
                               await usr.SendMessage (str).ConfigureAwait (false);
                           }
                       }
                       catch (Exception ex)
                       {
                           Console.WriteLine (ex);
                       }
                   });

                cgb.CreateCommand (Prefix + "announce")
                   .Description ($"Sends a message to all servers' general channel bot is connected to.**Owner Only!**\n**Benutzung**: {Prefix}announce Useless spam")
                   .Parameter ("msg",ParameterType.Unparsed)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       foreach (var ch in NadekoBot.Client.Servers.Select (s => s.DefaultChannel))
                       {
                           await ch.SendMessage (e.GetArg ("msg"));
                       }

                       await e.User.SendMessage (":ok:");
                   });
                cgb.CreateCommand (Prefix + "whoplays")
                    .Description ("Zeigt eine Liste von Benutzern die ein gewähltes Spiel spielen.")
                    .Parameter ("game",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var game = e.GetArg ("game")?.Trim ().ToUpperInvariant ();
                        if (string.IsNullOrWhiteSpace (game))
                            return;
                        var en = e.Server.Users
                            .Where (u => u.CurrentGame?.ToUpperInvariant () == game)
                                .Select (u => $"{u.Name}");

                        var arr = en as string[] ?? en.ToArray ();

                        if (arr.Length == 0)
                            await e.Channel.SendMessage ("Niemand. (nicht 100% sicher)");
                        else
                            await e.Channel.SendMessage ("• " + string.Join ("\n• ",arr));

                    });
            });
        }

        public void SaveParseToDb<T>(string where) where T : IDataModel
        {
            try {
                var data = File.ReadAllText(where);
                var arr = JObject.Parse(data)["results"] as JArray;
                if (arr == null)
                    return;
                var objects = arr.Select(x => x.ToObject<T>());
                DbHandler.Instance.InsertMany(objects);
            }
            catch { }
        }
    }
}
