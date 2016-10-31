using Discord;
using Discord.Commands;
using MidnightBot.Classes;
using MidnightBot.DataModels;
using MidnightBot.Modules.Permissions.Classes;
using System;
using System.Linq;
using System.Text;

namespace MidnightBot.Modules.Administration.Commands
{
    class RoleSave : DiscordCommand
    {
        public RoleSave(DiscordModule module) : base(module)
        {
            MidnightBot.OnReady += () => MidnightBot.Client.UserJoined += (s, e) =>
            {
                try
                {
                    var uid = (long)e.User.Id;
                    SavedRoles rs = DbHandler.Instance.FindOne<SavedRoles>(p => p.UserId == uid);

                    if (rs == null)
                    {
                        rs = new SavedRoles();

                        rs.UserId = Convert.ToInt64(e.User.Id);

                        var configs = SpecificConfigurations.Default.Of(e.Server.Id);

                        var roles = e.Server.Roles.Where(r => r.Id == configs.AutoAssignedRole).FirstOrDefault();

                        if (roles != null)
                            rs.roles = $"{roles}";
                        
                        DbHandler.Instance.Save(rs);
                    }
                    else
                    {
                        var configs = SpecificConfigurations.Default.Of(e.Server.Id);

                        var autoRole = e.Server.Roles.Where(r => r.Id == configs.AutoAssignedRole).FirstOrDefault();

                        if (autoRole != null)
                            e.User.RemoveRoles(autoRole);

                        string[] roles = rs.roles.Split(',');
                        foreach (var rl in roles)
                        {
                            Role role = e.Server.FindRoles(rl).FirstOrDefault();
                            if (role == null)
                                return;
                            e.User.AddRoles(role);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"aar exception. {ex}");
                }
            };

            MidnightBot.OnReady += () => MidnightBot.Client.UserUpdated += (s, e) =>
            {
                try
                {
                    if (!e.Before.Roles.SequenceEqual(e.After.Roles))
                    {
                        var uid = (long)e.After.Id;
                        SavedRoles rs = DbHandler.Instance.FindOne<SavedRoles>(p => p.UserId == uid);
                        if (rs != null)
                        {
                            DbHandler.Instance.Delete<SavedRoles>(Convert.ToInt32(rs.Id));
                            rs = new SavedRoles();

                            rs.UserId = Convert.ToInt64(e.After.Id);

                            DbHandler.Instance.Save(rs);
                        }
                        else
                        {
                            rs = new SavedRoles();

                            rs.UserId = Convert.ToInt64(e.After.Id);

                            DbHandler.Instance.Save(rs);
                        }
                        var sb = new StringBuilder();
                        var roles = e.After.Roles;
                        foreach (Role rls in roles)
                        {
                            if (rs.roles == null)
                            {
                                rs.roles = $"{rls}";

                                DbHandler.Instance.Save(rs);
                            }
                            else
                            {
                                string[] rols = rs.roles.Split(',');
                                foreach (string role in rols)
                                {
                                    sb.Append(role + ",");
                                }
                                sb.Append(rls);
                                rs.roles = Convert.ToString(sb);
                                sb.Clear();
                                DbHandler.Instance.Save(rs);
                            }
                        }

                    }
                    else
                        return;
                }
                catch { }
            };
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "saveAllRoles")
                .Alias(Module.Prefix + "sar")
                .Description($"Speichert alle Rollen der derzeit anwesenden Benutzer")
                .AddCheck(SimpleCheckers.OwnerOnly())
                .Do(async e =>
                {
                    try
                    {
                        if (!e.Server.CurrentUser.ServerPermissions.ManageRoles)
                        {
                            await e.Channel.SendMessage("Ich habe keine Berechtigungen um Rollen zu bearbeiten.").ConfigureAwait(false);
                            return;
                        }
                        var user = e.Server.Users;
                        foreach (User u in user)
                        {
                            var sb = new StringBuilder();
                            var roles = u.Roles;
                            var uid = (long)u.Id;
                            SavedRoles rs = DbHandler.Instance.FindOne<SavedRoles>(p => p.UserId == uid);
                            if (rs != null)
                            {
                                DbHandler.Instance.Delete<SavedRoles>(Convert.ToInt32(rs.Id));
                                rs = new SavedRoles();

                                rs.UserId = Convert.ToInt64(u.Id);

                                DbHandler.Instance.Save(rs);
                            }
                            else
                            {
                                rs = new SavedRoles();

                                rs.UserId = Convert.ToInt64(u.Id);

                                DbHandler.Instance.Save(rs);
                            }
                            foreach (Role rls in roles)
                            {
                                    if (rs.roles == null)
                                    {
                                        rs.roles = $"{rls}";

                                        DbHandler.Instance.Save(rs);
                                    }
                                    else
                                    {
                                        string[] rols = rs.roles.Split(',');
                                        foreach (string s in rols)
                                        {
                                            sb.Append(s + ",");
                                        }
                                        sb.Append(rls);
                                        rs.roles = Convert.ToString(sb);
                                        sb.Clear();
                                        DbHandler.Instance.Save(rs);
                                    }
                            }

                        }
                    }
                    catch
                    {
                        Console.WriteLine("Failed saving.");
                    }

                });
        }
    }
}