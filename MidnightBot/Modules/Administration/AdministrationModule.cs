using Discord;
using Discord.Commands;
using Discord.Modules;
using MidnightBot.DataModels;
using MidnightBot.Classes;
using MidnightBot.Extensions;
using MidnightBot.Modules.Administration.Commands;
using MidnightBot.Modules.Permissions.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightBot.Modules.Administration
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
            commands.Add (new CustomReactionsCommands (this));
            commands.Add (new AutoAssignRole (this));
            commands.Add (new SelfCommands (this));
            commands.Add (new IncidentsCommands (this));

            MidnightBot.Client.GetService<CommandService> ().CommandExecuted += DeleteCommandMessage;
        }

        private void DeleteCommandMessage ( object sender,CommandEventArgs e )
        {
            if (e.Server == null || e.Channel.IsPrivate)
                return;
            var conf = SpecificConfigurations.Default.Of (e.Server.Id);
            if (!conf.AutoDeleteMessagesOnCommand)
                return;
            try
            {
                e.Message.Delete();
            }
            catch { }
        }

        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Administration;

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
                        var rules = new StringBuilder ();
                        var count = 1;
                        foreach(string s in MidnightBot.Config.Regeln)
                        {
                            rules.AppendLine ($"{count}. {s}");
                            count++;
                        }
                        await e.Channel.SendMessage (rules.ToString ()).ConfigureAwait (false);
                    });

                cgb.CreateCommand(Prefix + "delmsgoncmd")
                    .Description($"Ändert das automatische Löschen von erfolgreichen Befehls Aufrufen um Chat Spam zu verhindern. Server Manager Only. | `{Prefix}delmsgoncmd`")
                    .AddCheck(SimpleCheckers.ManageServer())
                    .Do(async e =>
                    {
                        var conf = SpecificConfigurations.Default.Of(e.Server.Id);
                        conf.AutoDeleteMessagesOnCommand = !conf.AutoDeleteMessagesOnCommand;
                        await Classes.JSONModels.ConfigHandler.SaveConfig().ConfigureAwait(false);
                        if (conf.AutoDeleteMessagesOnCommand)
                            await e.Channel.SendMessage("❗`Ich lösche jetzt automatisch die Befehlsaufrufe.`");
                        else
                            await e.Channel.SendMessage("❗`Ich lösche jetzt nicht mehr automatisch die Befehlsaufrufe.`");

                    });

                cgb.CreateCommand (Prefix + "restart")
                    .Description ($"Startet den Bot neu. Könnte nicht funktionieren. | `{Prefix}restart`")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        var msg = await e.Channel.SendMessage ("`Neustart in 3 Sekunden...`");
                        await Task.Delay (1000);
                        await msg.Edit ("`Neustart in 2 Sekunden...`");
                        await Task.Delay (1000);
                        await msg.Edit ("`Neustart in 1 Sekunden...`");
                        await Task.Delay (1000);
                        await msg.Edit ("`Startet neu...`");
                        await Task.Delay (1000);
                        System.Diagnostics.Process.Start (System.Reflection.Assembly.GetExecutingAssembly ().Location);
                        Environment.Exit (0);
                    });

                cgb.CreateCommand (Prefix + "setrole").Alias (Prefix + "sr")
                    .Description ($"Setzt die Rolle für einen gegebenen Benutzer. | `{Prefix}sr @User Gast`")
                    .Parameter ("user_name",ParameterType.Required)
                    .Parameter ("role_name",ParameterType.Unparsed)
                    //.AddCheck (SimpleCheckers.CanManageRoles)
                    .Do (async e =>
                     {
                         var userName = e.GetArg ("user_name");
                         var roleName = e.GetArg ("role_name");

                         if (string.IsNullOrWhiteSpace (roleName))
                             return;

                         if (!e.User.ServerPermissions.ManageRoles && !MidnightBot.IsOwner (e.User.Id))
                         {
                             await e.Channel.SendMessage ("Du hast zu wenig Rechte.").ConfigureAwait (false);
                             return;
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

                cgb.CreateCommand (Prefix + "removerole").Alias (Prefix + "rr")
                    .Description ($"Entfernt eine Rolle von einem gegebenen User. | `{Prefix}rr @User Admin`")
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

                cgb.CreateCommand (Prefix + "renamerole")
                    .Alias (Prefix + "renr")
                    .Description ($"Benennt eine Rolle um. Rolle die umbenannt werden soll muss muss in Liste niedriger sein als die höchste Rolle des Bots. | `{Prefix}renr \"Erste Rolle\" ZweiteRolle`")
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
                            await e.Channel.SendMessage ("Kann diese Rolle nicht finden.").ConfigureAwait (false);
                            return;
                        }

                        try
                        {
                            if (roleToEdit.Position > e.Server.CurrentUser.Roles.Max (r => r.Position))
                            {
                                await e.Channel.SendMessage ("Ich kann Rollen die höher sind als meine nicht bearbeiten.").ConfigureAwait (false);
                                return;
                            }
                            await roleToEdit.Edit (r2);
                            await e.Channel.SendMessage ("Rolle umbenannt.").ConfigureAwait (false);
                        }
                        catch (Exception)
                        {
                            await e.Channel.SendMessage ("Fehler beim umbennennen der Rolle. Wahrscheinlich nicht genug Rechte.").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "removeallroles").Alias (Prefix + "rar")
                    .Description ($"Entfernt alle Rollen eines Benutzers. | `{Prefix}rar @User`")
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

                cgb.CreateCommand (Prefix + "createrole").Alias (Prefix + "cr")
                    .Description ($"Erstelle eine Rolle mit einem bestimmten Namen. | `{Prefix}r Awesome Role`")
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
                    .Description ($"Setzt die Farbe einer Rolle zur Hex, oder RGB Farb-Value die gegeben wird. | `{Prefix}color Admin 255 200 100 oder .color Admin ffba55`")
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

                cgb.CreateCommand (Prefix + "ban").Alias (Prefix + "b")
                    .Parameter ("user",ParameterType.Required)
                    .Parameter ("msg",ParameterType.Unparsed)
                    .Description ($"Bannt einen erwähnten Benutzer. | `{Prefix}b \"@some Guy\" Your behaviour is toxic.`")
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
                                     await e.Server.Ban (usr,7).ConfigureAwait (false);
                                     await e.Channel.SendMessage ("User gebannt: " + usr.Name + " Id: " + usr.Id).ConfigureAwait (false);
                                 }
                                 catch
                                 {
                                     await e.Channel.SendMessage ("Fehler, nicht genug Rechte.").ConfigureAwait (false);
                                 }
                             }
                         });

                cgb.CreateCommand (Prefix + "softban").Alias (Prefix + "sb")
                    .Parameter("user", ParameterType.Required)
                    .Parameter("msg", ParameterType.Unparsed)
                    .Description($"Bannt und entbannt einen Benutzer per ID, oder Name mit optionaler Nachricht. | `{Prefix}sb \"@some Guy\" Your behaviour is toxic.`")
                        .Do(async e =>
                        {
                            var msg = e.GetArg("msg");
                            var user = e.GetArg("user");
                            if (e.User.ServerPermissions.BanMembers)
                            {
                                var usr = e.Server.FindUsers(user).FirstOrDefault();
                                if (usr == null)
                                {
                                    await e.Channel.SendMessage("Benutzer nicht gefunden.").ConfigureAwait(false);
                                    return;
                                }
                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    await usr.SendMessage($"**Du wurdest SOFT-GEBANNT vom Server `{e.Server.Name}`.**\n" +
                                                          $"Begründung: {msg}").ConfigureAwait(false);
                                    await Task.Delay(2000).ConfigureAwait(false); // temp solution; give time for a message to be send, fu volt
                                }
                                try
                                {
                                    await e.Server.Ban(usr, 7).ConfigureAwait(false);
                                    await e.Server.Unban(usr).ConfigureAwait(false);

                                    await e.Channel.SendMessage("Benutzer Soft-Banned" + usr.Name + " Id: " + usr.Id).ConfigureAwait(false);
                                }
                                catch
                                {
                                    await e.Channel.SendMessage("Fehler. Ich habe keine Rechte dafür.").ConfigureAwait(false);
                                }
                            }
                        });

                cgb.CreateCommand (Prefix + "kick").Alias (Prefix + "k")
                    .Parameter ("user")
                    .Parameter ("msg",ParameterType.Unparsed)
                    .Description ($"Kickt einen erwähnten User. | `{Prefix}k \"@some Guy\" Your behaviour is toxic.`")
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
                                 await usr.SendMessage ($"**Du wurdest vom Server `{e.Server.Name}` gekickt.**\n" +
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
                    .Description ($"Mutet erwähnte Benutzer. | `{Prefix}mute \"@Someguy\"` oder `{Prefix}mute \"@Someguy\" \"@Someguy\"`")
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
                    .Description ($"Entmutet erwähnte Benutzer. | `{Prefix}unmute \"@Someguy\"` oder `{Prefix}unmute \"@Someguy\" \"@Someguy\"`")
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
                    .Description ($"Stellt erwähnte Benutzer Taub. | `{Prefix}deaf \"@Someguy\"` oder `{Prefix}deaf \"@Someguy\" \"@Someguy\"`")
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
                    .Alias (Prefix + "undef")
                    .Description ($"Erwähnte Benutzer sind nicht mehr taub. | `{Prefix}undef \"@Someguy\"` oder `{Prefix}undef \"@Someguy\" \"@Someguy\"`")
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

                cgb.CreateCommand (Prefix + "delvoichanl")
                    .Alias (Prefix + "dvch")
                    .Description ($"Löscht einen Voice-Channel mit einem gegebenen Namen. | `{Prefix}dvch VoiceChannelName`")
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

                cgb.CreateCommand (Prefix + "creatvoichanl")
                    .Alias (Prefix + "cvch")
                    .Description ($"Erstellt einen neuen Voice-Channel mit einem gegebenen Namen. | `{Prefix}cvch VoiceChannelName`")
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

                cgb.CreateCommand (Prefix + "deltxtchanl")
                    .Alias (Prefix + "dtch")
                    .Description ($"Löscht einen Text-Channel mit einem gegebenen Namen. | `{Prefix}dtch TextChannelName`")
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

                cgb.CreateCommand (Prefix + "creatxtchanl")
                    .Alias (Prefix + "ctch")
                    .Description ($"Erstellt einen Text-Channel mit einem gegebenen Namen. | `{Prefix}ctch TextChannelName`")
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

                cgb.CreateCommand (Prefix + "settopic")
                    .Alias (Prefix + "st")
                    .Description ($"Setzt eine Beschreibung für den derzeitigen Channel. | `{Prefix}st My new topic`")
                    .AddCheck (SimpleCheckers.ManageChannels ())
                    .Parameter ("topic",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var topic = e.GetArg ("topic")?.Trim () ?? "";
                        await e.Channel.Edit (topic: topic).ConfigureAwait (false);
                        await e.Channel.SendMessage (":ok: **Channel Topic gesetzt.**").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "setchanlname")
                    .Alias (Prefix + "schn")
                    .Description ($"Ändert den Namen des derzeitigen Channels. | `{Prefix}schn NewName`")
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

                cgb.CreateCommand (Prefix + "heap")
                    .Description ($"Zeigt benutzten Speicher - **Bot Owner Only!** | `{Prefix}heap`")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         var heap = await Task.Run (() => MidnightStats.Instance.Heap ()).ConfigureAwait (false);
                         await e.Channel.SendMessage ($"`Heap Size:` {heap}").ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "getinactive")
                    .Alias(Prefix + "gi")
                    .Description ("Zeigt anzahl inaktiver Benutzer - **Bot Owner Only!**")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Parameter ("days",ParameterType.Required)
                    .Parameter ("prune",ParameterType.Optional)
                    .Do (async e =>
                    {
                        if (string.IsNullOrWhiteSpace (e.GetArg ("prune"))|| e.GetArg ("prune") != "true")
                        {
                            var users = await e.Server.PruneUsers (Convert.ToInt32 (e.GetArg ("days")),true);
                            await e.Channel.SendMessage ($"Inaktive Benutzer:` {users}").ConfigureAwait (false);
                            return;
                        }
                        else
                        {
                            var users = await e.Server.PruneUsers (Convert.ToInt32 (e.GetArg ("days")),false);
                            await e.Channel.SendMessage ($"Inaktive Benutzer gelöscht:` {users}").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "prune")
                    .Alias (Prefix + "clr")
                    .Description ($"`{Prefix}prune` entfernt alle von MidnightBots Nachrichten, in den letzten 100 Nachrichten.`{Prefix}prune X` entfernt die letzten X Nachrichten von diesem Channel (bis zu 100)`{Prefix}prune @Someone` Entfernt alle Nachrichten einer Person. in den letzten 100 Nachrichten.`{Prefix}prune @Someone X` Entfernt die letzen X Nachrichten einer Person in diesem Channel." +
                        $"| `{Prefix}prune` oder `{Prefix}prune 5` oder `{Prefix}prune @Someone` oder `{Prefix}prune @Someone X`")
                    .Parameter ("user_or_num",ParameterType.Optional)
                    .Parameter ("num",ParameterType.Optional)
                    .Do (async e =>
                     {
                         if (e.Channel.IsPrivate)
                         {
                             var msgs = (await e.Channel.DownloadMessages (100).ConfigureAwait (false)).Where (m => m.User.Id == MidnightBot.Creds.BotId).ToArray();
                             if (!msgs.Any())
                                 return;
                             var toDelete = msgs as Message[] ?? msgs.ToArray();
                             await e.Channel.DeleteMessages(toDelete).ConfigureAwait(false);
                             return;
                         }
                         if (string.IsNullOrWhiteSpace (e.GetArg ("user_or_num"))) // if nothing is set, clear nadeko's messages, no permissions required
                         {
                             var msgs = (await e.Channel.DownloadMessages(100).ConfigureAwait(false)).Where(m => m.User?.Id == e.Server.CurrentUser.Id)?.ToArray();
                             if (msgs == null || !msgs.Any())
                                return;
                             var toDelete = msgs as Message[] ?? msgs.ToArray();
                             await e.Channel.DeleteMessages(toDelete).ConfigureAwait(false);
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
                             await e.Channel.DeleteMessages((await e.Channel.DownloadMessages(val).ConfigureAwait(false)).ToArray()).ConfigureAwait(false);
                             return;
                         }
                         //else if first argument is user
                         var usr = e.Server.FindUsers (e.GetArg ("user_or_num")).FirstOrDefault ();
                         if (usr == null)
                             return;
                         val = 100;
                         if (!int.TryParse (e.GetArg ("num"),out val))
                             val = 100;

                         var mesgs = (await e.Channel.DownloadMessages(100).ConfigureAwait(false)).Where(m => m.User?.Id == usr.Id).Take(val);
                         if (mesgs == null || !mesgs.Any())
                             return;
                         await e.Channel.DeleteMessages(mesgs as Message[] ?? mesgs.ToArray()).ConfigureAwait(false);
                     });

                cgb.CreateCommand (Prefix + "die")
                    .Description ($"Fährt den Bot herunter und benachrichtigt Benutzer über den Neustart. **Bot Owner Only!** | `{Prefix}die`")
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("`Fährt herunter.`").ConfigureAwait (false);
                        await Task.Delay (2000).ConfigureAwait (false);
                        Environment.Exit (0);
                    });

                cgb.CreateCommand (Prefix + "setname")
                    .Alias (Prefix + "newnm")
                    .Description ($"Gibt dem Bot einen neuen Namen. **Bot Owner Only!** | `{Prefix}newnm BotName`")
                    .Parameter ("new_name",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         if (e.GetArg("new_name") == null) return;

                         await client.CurrentUser.Edit ("",e.GetArg ("new_name")).ConfigureAwait (false);
                     });

                cgb.CreateCommand (Prefix + "newavatar")
                    .Alias (Prefix + "setavatar")
                    .Description ($"Setzt ein neues Profilbild für MidnightBot. **Bot Owner Only!** | `{Prefix}setavatar https://i.ytimg.com/vi/WDudkR1eTMM/maxresdefault.jpg`")
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
                         await client.CurrentUser.Edit ("",avatar: image.ToStream ()).ConfigureAwait (false);

                         // Send confirm.
                         await e.Channel.SendMessage ("Neuer Avatar gesetzt.").ConfigureAwait (false);

                         // Save the image to disk.
                         image.Save ("data/avatar.png",System.Drawing.Imaging.ImageFormat.Png);
                     });

                cgb.CreateCommand (Prefix + "setgame")
                  .Description ($"Setzt das Spiel des Bots. **Bot Owner Only!** | `{Prefix}setgame Playing with Midnight`")
                  .Parameter ("set_game",ParameterType.Unparsed)
                  .Do (e =>
                   {
                       if (!MidnightBot.IsOwner (e.User.Id) || e.GetArg ("set_game") == null)
                           return;

                       client.SetGame (e.GetArg ("set_game"));
                   });

                cgb.CreateCommand (Prefix + "send")
                    .Description ($"Sendet eine Nachricht an einen Benutzer auf einem anderen Server, über den Bot. **Bot Owner Only!** | `{Prefix}send serverid|u:user_id Send this to a user!` oder `{Prefix}send serverid|c:channel_id Send this to a channel!`")
                    .Parameter ("ids",ParameterType.Required)
                    .Parameter ("msg",ParameterType.Unparsed)
                    .AddCheck (SimpleCheckers.OwnerOnly ())
                    .Do (async e =>
                     {
                         var msg = e.GetArg ("msg")?.Trim ();

                         if (string.IsNullOrWhiteSpace(msg))
                            return;

                         var ids = e.GetArg("ids").Split('|');
                         if (ids.Length != 2)
                            return;
                        var sid = ulong.Parse(ids[0]);
                        var server = MidnightBot.Client.Servers.Where(s => s.Id == sid).FirstOrDefault();

                        if (server == null)
                            return;

                        if (ids[1].ToUpperInvariant().StartsWith("C:"))
                         {
                            var cid = ulong.Parse(ids[1].Substring(2));
                            var channel = server.TextChannels.Where(c => c.Id == cid).FirstOrDefault();
                            if (channel == null)
                            {
                                return;
                            }
                            await channel.SendMessage(msg);
                        }
                        else if (ids[1].ToUpperInvariant().StartsWith("U:"))
                        {
                            var uid = ulong.Parse(ids[1].Substring(2));
                            var user = server.Users.Where(u => u.Id == uid).FirstOrDefault();
                            if (user == null)
                            {
                                return;
                            }
                            await user.SendMessage(msg);
                         }
                         else
                             {
                            await e.Channel.SendMessage("`Ungültiges Format.`");
                        }
                     });

                cgb.CreateCommand (Prefix + "mentionrole")
                    .Alias (Prefix + "menro")
                    .Description ($"Erwähnt jeden User mit einer bestimmten Rolle oder bestimmten Rollen (Getrennt mit einem ',') auf diesem Server. 'Mention everyone' Berechtigung erforderlich. | `{Prefix}menro RoleName`")
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

                cgb.CreateCommand (Prefix + "unstuck")
                  .Description ($"Löscht die Nachrichten-Liste. **Bot Owner Only!** | `{Prefix}unstuck`")
                  .AddCheck (SimpleCheckers.OwnerOnly ())
                  .Do (e =>
                   {
                       MidnightBot.Client.MessageQueue.Clear ();
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

                cgb.CreateCommand (Prefix + "donadd")
                    .Description ($"Fügt einen Donator zur Datenbank hinzu. | `{Prefix}donadd Donate Amount`")
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
                                 DbHandler.Instance.Connection.Insert(new Donator
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

                cgb.CreateCommand (Prefix + "sendmsg")
                   .Description ($"Sendet eine Private Nachricht an einen User vom Bot aus.**Bot Owner Only** | `{Prefix}sendmsg @Username Nachricht`")
                   .Parameter ("user",ParameterType.Required)
                   .Parameter ("msg",ParameterType.Unparsed)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       var u = findUser(e.GetArg ("user"));
                       
                       if (u == null)
                       {
                           await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                           return;
                       }
                       else if (u.Id == MidnightBot.Client.CurrentUser.Id)
                       {
                           await e.Channel.SendMessage ("Ich kann mir selber keine Nachricht schicken.")
                           .ConfigureAwait (false);
                           return;
                       }

                       var msg = e.GetArg ("msg");
                       if (string.IsNullOrWhiteSpace (msg))
                           return;
                       
                       await u.SendMessage ($"{e.User.Name} schreibt: {msg}");
                   });

                cgb.CreateCommand (Prefix + "announce")
                   .Description ($"Sends a message to all servers' general channel bot is connected to.**Bot Owner Only!** | `{Prefix}announce Useless spam`")
                   .Parameter ("msg",ParameterType.Unparsed)
                   .AddCheck (SimpleCheckers.OwnerOnly ())
                   .Do (async e =>
                   {
                       foreach (var ch in MidnightBot.Client.Servers.Select (s => s.DefaultChannel))
                       {
                           await ch.SendMessage (e.GetArg ("msg"));
                       }

                       await e.User.SendMessage (":ok:");
                   });

                cgb.CreateCommand (Prefix + "servers")
                     .Description ("Zeigt alle Server an, auf denen der Bot ist.")
                     .AddCheck (SimpleCheckers.OwnerOnly ())
                     .Do (async e =>
                     {
                         var sb = new StringBuilder ();
                         foreach (Server s in MidnightBot.Client.Servers)
                         {
                             sb.AppendLine ($"ID: {s.Id} | Servername: {s.Name}");
                         }
                         await e.Channel.SendMessage (sb.ToString()).ConfigureAwait (false);
                     });

                cgb.CreateCommand(Prefix + "savechat")
                    .Description($"Speichert eine Anzahl an Nachrichten in eine Textdate und sendet sie zu dir. **Bot Owner Only** | `{Prefix}savechat 150`")
                    .Parameter("cnt", ParameterType.Required)
                    .AddCheck(SimpleCheckers.OwnerOnly())
                    .Do(async e =>
                    {
                        var cntstr = e.GetArg("cnt")?.Trim();
                        int cnt;
                        if (!int.TryParse(cntstr, out cnt))
                            return;
                        ulong? lastmsgId = null;
                        var sb = new StringBuilder();
                        var msgs = new List<Message>(cnt);
                        while (cnt > 0)
                        {
                            var dlcnt = cnt < 100 ? cnt : 100;

                            var dledMsgs = await e.Channel.DownloadMessages(dlcnt, lastmsgId);
                            if (!dledMsgs.Any())
                                break;
                            msgs.AddRange(dledMsgs);
                            lastmsgId = msgs[msgs.Count - 1].Id;
                            cnt -= 100;
                        }
                        await e.User.SendFile($"Chatlog-{e.Server.Name}/#{e.Channel.Name}-{DateTime.Now}.txt", JsonConvert.SerializeObject(new { Messages = msgs.Select(s => s.ToString()) }, Formatting.Indented).ToStream()).ConfigureAwait(false);
                    });
            });
        }

        public User findUser ( string v )
        {
            foreach (var ch in MidnightBot.Client.Servers.Select (s => s.DefaultChannel))
            {
                var u = ch.FindUsers (v).FirstOrDefault ();
                if (u != null)
                    return u;
            }
            return null;
        }
    }
}
