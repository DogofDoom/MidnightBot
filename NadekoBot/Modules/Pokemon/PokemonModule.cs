using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.DataModels;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NadekoBot.Modules.Pokemon
{
    class PokemonModule : DiscordModule
    {
        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Pokemon;

        private ConcurrentDictionary<ulong,PokeStats> Stats = new ConcurrentDictionary<ulong,PokeStats> ();

        public PokemonModule ()
        {
        }

        private int GetDamage ( PokemonType usertype,PokemonType targetType )
        {
            var crit = new Random ();
            var fail = new Random ();
            var rng = new Random ();
            int damage = rng.Next (40,60);
            foreach (PokemonMultiplier Multiplier in usertype.Multipliers)
            {
                if (Multiplier.Type == targetType.Name)
                {
                    var multiplier = Multiplier.Multiplication;
                    damage = (int)(damage * multiplier);
                }
            }

            return damage;
        }

        private PokemonType GetPokeType ( ulong id )
        {

            var db = DbHandler.Instance.GetAllRows<UserPokeTypes> ();
            Dictionary<long,string> setTypes = db.ToDictionary (x => x.UserId,y => y.type);
            if (setTypes.ContainsKey ((long)id))
            {
                return stringToPokemonType (setTypes[(long)id]);
            }
            int count = NadekoBot.Config.PokemonTypes.Count;
            
            int remainder = Math.Abs ((int)(id % (ulong)count));
            return NadekoBot.Config.PokemonTypes[remainder];
        }

        private PokemonType stringToPokemonType ( string v )
        {
            var str = v.ToUpperInvariant ();
            var list = NadekoBot.Config.PokemonTypes;
            foreach (PokemonType p in list)
            {
                if (str == p.Name)
                {
                    return p;
                }
            }
            return null;
        }

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                commands.ForEach (cmd => cmd.Init (cgb));

                cgb.CreateCommand (Prefix + "attack")
                    .Description ("Greift jemanden mit der angegebenen Attacke an.")
                    .Parameter ("move",ParameterType.Required)
                    .Parameter ("target",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var crit = new Random ();
                        var fail = new Random ();
                        var response = $"";
                        var move = e.GetArg ("move");
                        var targetStr = e.GetArg ("target")?.Trim ();
                        if (string.IsNullOrWhiteSpace (targetStr))
                            return;
                        var target = e.Server.FindUsers (targetStr).FirstOrDefault ();
                        if (target == null)
                        {
                            await e.Channel.SendMessage ("Benutzer nicht vorhanden.").ConfigureAwait (false);
                            return;
                        }
                        else if (target == e.User)
                        {
                            await e.Channel.SendMessage ("Du kannst dich nicht selbst angreifen.").ConfigureAwait (false);
                            return;
                        }
                        
                        // Checking stats first, then move
                        //Set up the userstats
                        PokeStats userStats;
                        userStats = Stats.GetOrAdd (e.User.Id,new PokeStats ());

                        //Check if able to move
                        //User not able if HP < 0, has made more than 4 attacks
                        if (userStats.Hp < 0)
                        {
                            await e.Channel.SendMessage ($"{e.User.Mention} ist besiegt und konnte nicht angreifen! Gib >heal {e.User.Mention} um zu heilen. (Gratis)").ConfigureAwait (false);
                            return;
                        }
                        if (userStats.MovesMade >= 5)
                        {
                            await e.Channel.SendMessage ($"{e.User.Mention} hat zu viele Angriffe hintereinander ausgeführt und konnte nicht nocheinmal angreifen!").ConfigureAwait (false);
                            return;
                        }
                        if (userStats.LastAttacked.Contains (target.Id))
                        {
                            await e.Channel.SendMessage ($"{e.User.Mention} kann ohne Gegenangriff nicht nocheinmal angreifen!").ConfigureAwait (false);
                            return;
                        }
                        //get target stats
                        PokeStats targetStats;
                        targetStats = Stats.GetOrAdd (target.Id,new PokeStats ());

                        //If target's HP is below 0, no use attacking
                        if (targetStats.Hp <= 0)
                        {
                            await e.Channel.SendMessage ($"{target.Mention} ist bereits besiegt!").ConfigureAwait (false);
                            return;
                        }

                        //Check whether move can be used
                        PokemonType userType = GetPokeType (e.User.Id);

                        if (userType.Name == "ADMIN" && !(NadekoBot.IsOwner (e.User.Id)))
                        {
                            await e.Channel.SendMessage ("Fehlerhaften Typ. Such dir einen Typen mit >settype aus.").ConfigureAwait (false);
                            return;
                        }

                        var enabledMoves = userType.Moves;
                        if (!enabledMoves.Contains (move.ToLowerInvariant ()))
                        {
                            await e.Channel.SendMessage ($"{e.User.Mention} war nicht in der Lage **{move}** zu benutzen, gib {Prefix}listmoves ein, um die Attacken zu sehen die du benutzen kannst.").ConfigureAwait (false);
                            return;
                        }

                        //get target type
                        PokemonType targetType = GetPokeType (target.Id);
                        if (targetType.Name == "ADMIN" && !(NadekoBot.IsOwner (target.Id)) && !(target.Id == (NadekoBot.Client.CurrentUser.Id))&& !(target.Id == 159985870458322944))
                        {
                            targetType = stringToPokemonType ("FEUER");
                        }
                        //generate damage
                        int damage = GetDamage (userType,targetType);
                        int damgecheck = damage;

                        if (userType.Name != "ADMIN")
                        {
                            if (fail.Next (0,101) <= 2)
                            {
                                damage *= 0;
                                response += "Daneben...";
                                goto AFTERATTACK;
                            }
                            else if (crit.Next (0,101) <= 6)
                            {
                                damage *= 2;
                                response += "Kritischer Treffer!";
                            }
                        }
                        
                        //apply damage to target
                        targetStats.Hp -= damage;

                        response += $"\n{e.User.Mention} benutze **{move}**{userType.Icon} bei {target.Mention}{targetType.Icon} und fügte **{damage}** Schaden zu.";

                        //Damage type
                        if (damgecheck < 40)
                        {
                            response += "\nDas ist nicht sehr effektiv...";
                        }
                        else if (damgecheck > 60)
                        {
                            response += "\nEs ist sehr effektiv!";
                        }
                        else
                        {
                            response += "\nGetroffen.";
                        }

                        //check fainted

                        if (targetStats.Hp <= 0)
                        {
                            response += $"\n**{target.Name}** wurde besiegt!";
                        }
                        else
                        {
                            response += $"\n**{target.Name}** hat noch {targetStats.Hp} HP übrig.";
                        }

                        AFTERATTACK:
                        //update other stats
                        userStats.LastAttacked.Add (target.Id);
                        userStats.MovesMade++;
                        targetStats.MovesMade = 0;
                        if (targetStats.LastAttacked.Contains (e.User.Id))
                        {
                            targetStats.LastAttacked.Remove (e.User.Id);
                        }

                        //update dictionary
                        //This can stay the same right?
                        Stats[e.User.Id] = userStats;
                        Stats[target.Id] = targetStats;

                        await e.Channel.SendMessage (response).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "ml")
                    .Alias (Prefix + "movelist")
                    .Alias (Prefix + "listmoves")
                    .Description ("Listet alle Attacken auf, die du benutzen kannst.")
                    .Do (async e =>
                    {
                        var userType = GetPokeType (e.User.Id);
                        if (userType.Name == "ADMIN" && !(NadekoBot.IsOwner (e.User.Id)))
                        {
                            await e.Channel.SendMessage ("Fehlerhaften Typ. Such dir einen Typen mit >settype aus.").ConfigureAwait (false);
                            return;
                        }
                        var movesList = userType.Moves;
                        var str = $"**Attacken für `{userType.Name}` Typ.**";
                        foreach (string m in movesList)
                        {
                            str += $"\n{userType.Icon}{m}";
                        }
                        await e.User.SendMessage (str).ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "heal")
                    .Description ($"Heilt jemanden. Belebt jene, die besiegt wurden. Kostet einen {NadekoBot.Config.CurrencyName}. \n**Benutzung**:{Prefix}heal @someone")
                    .Parameter ("target",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var targetStr = e.GetArg ("target")?.Trim ();
                        if (string.IsNullOrWhiteSpace (targetStr))
                            return;
                        var usr = e.Server.FindUsers (targetStr).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("Benutzer nicht vorhanden.").ConfigureAwait (false);
                            return;
                        }
                        if (Stats.ContainsKey (usr.Id))
                        {

                            var targetStats = Stats[usr.Id];
                            int HP = targetStats.Hp;
                            if (targetStats.Hp == targetStats.MaxHp)
                            {
                                await e.Channel.SendMessage ($"{usr.Name} hat bereits volle HP!").ConfigureAwait (false);
                                return;
                            }
                            //Payment~
                            var amount = 1;
                            if (targetStats.Hp <= 0)
                            {
                                amount = 0;
                            }
                            var pts = Classes.DbHandler.Instance.GetStateByUserId ((long)e.User.Id)?.Value ?? 0;
                            if (pts < amount)
                            {
                                await e.Channel.SendMessage ($"{e.User.Mention} Du hast nicht genug {NadekoBot.Config.CurrencyName}! \nDu brauchst noch {amount - pts} {NadekoBot.Config.CurrencyName} um dies zu tun!").ConfigureAwait (false);
                                return;
                            }
                            var target = (usr.Id == e.User.Id) ? "yourself" : usr.Name;
                            FlowersHandler.RemoveFlowers (e.User,$"Poke-Heal {target}",amount);
                            //healing
                            targetStats.Hp = targetStats.MaxHp;
                            if (HP < 0)
                            {
                                //Could heal only for half HP?
                                Stats[usr.Id].Hp = (targetStats.MaxHp);
                                await e.Channel.SendMessage ($"{e.User.Name} belebte {usr.Name} wieder, mit {amount} {NadekoBot.Config.CurrencySign}").ConfigureAwait (false);
                                return;
                            }
                            var vowelFirst = new[] { 'a','e','i','o','u' }.Contains (NadekoBot.Config.CurrencyName[0]);
                            await e.Channel.SendMessage ($"{e.User.Name} heilte {usr.Name} um {targetStats.MaxHp - HP} HP mit {(vowelFirst ? "einem" : "einem")} {NadekoBot.Config.CurrencySign}").ConfigureAwait (false);
                            return;
                        }
                        else
                        {
                            await e.Channel.SendMessage ($"{usr.Name} hat bereits volle HP!").ConfigureAwait (false);
                        }
                    });

                cgb.CreateCommand (Prefix + "type")
                    .Description ($"Gibt den Typ des angegebenen Benutzers aus.\n**Benutzung**: {Prefix}type @someone")
                    .Parameter ("target",ParameterType.Unparsed)
                    .Do (async e =>
                    {
                        var usrStr = e.GetArg ("target")?.Trim ();
                        if (string.IsNullOrWhiteSpace (usrStr))
                            return;
                        var usr = e.Server.FindUsers (usrStr).FirstOrDefault ();
                        if (usr == null)
                        {
                            await e.Channel.SendMessage ("Benutzer nicht vorhanden.").ConfigureAwait (false);
                            return;
                        }
                        var pType = GetPokeType (usr.Id);
                        if (pType.Name == "ADMIN" && !(NadekoBot.IsOwner (usr.Id)) && !(usr.Id == (NadekoBot.Client.CurrentUser.Id)) && !(usr.Id == 159985870458322944))
                        {
                            pType.Name = "FEUER";
                            await e.Channel.SendMessage ($"Typ von {usr.Name} ist **{pType.Name.ToLowerInvariant ()}**{pType.Icon}").ConfigureAwait (false);
                            return;
                        }
                        else
                        await e.Channel.SendMessage ($"Typ von {usr.Name} ist **{pType.Name.ToLowerInvariant ()}**{pType.Icon}").ConfigureAwait (false);

                    });

                cgb.CreateCommand (Prefix + "typelist")
                    .Alias (Prefix + "listtypes")
                    .Description ("Liste der möglichen Typen")
                    .Do (async e =>
                    {
                        await e.Channel.SendMessage ("Verfügbare Typen:\nNORMAL, FEUER, WASSER, ELEKTRO, PFLANZE, EIS, KAMPF, GIFT, BODEN, FLUG, PSYCHO, KAEFER, GESTEIN, GEIST, DRACHE, UNLICHT, STAHL, FEE").ConfigureAwait (false);
                    });

                cgb.CreateCommand (Prefix + "settype")
                        .Description ($"Setzt deinen Typen. Kostet einen {NadekoBot.Config.CurrencyName}.\n**Benutzer**: {Prefix}settype fire")
                        .Parameter ("targetType",ParameterType.Unparsed)
                        .Do (async e =>
                        {
                            if (Stats.ContainsKey (e.User.Id) && !(NadekoBot.IsOwner (e.User.Id)))
                            {
                                var targetStats = Stats[e.User.Id];
                                if (targetStats.Hp != targetStats.MaxHp)
                                {
                                    await e.Channel.SendMessage ($"Ändern des Types nur mit vollen {targetStats.MaxHp}HP möglich!").ConfigureAwait (false);
                                    return;
                                }
                            }
                            var targetTypeStr = e.GetArg ("targetType")?.ToUpperInvariant ();
                            if (string.IsNullOrWhiteSpace (targetTypeStr))
                                return;
                            var targetType = stringToPokemonType (targetTypeStr);
                            if (targetType == null || targetTypeStr == "ADMIN" && !(NadekoBot.IsOwner (e.User.Id)))
                            {
                                await e.Channel.SendMessage ("Fehlerhaften Typ angegeben. Typ muss einer der folgenden sein:\nNORMAL, FEUER, WASSER, ELEKTRO, PFLANZE, EIS, KAMPF, GIFT, BODEN, FLUG, PSYCHO, KAEFER, GESTEIN, GEIST, DRACHE, UNLICHT, STAHL, FEE").ConfigureAwait (false);
                                return;
                            }
                            if (targetType == GetPokeType (e.User.Id))
                            {
                                await e.Channel.SendMessage ($"Dein Typ ist bereits {targetType.Name.ToLowerInvariant ()}{targetType.Icon}").ConfigureAwait (false);
                                return;
                            }

                        //Payment~
                        var amount = 0;
                            var pts = DbHandler.Instance.GetStateByUserId ((long)e.User.Id)?.Value ?? 0;
                            if (pts < amount)
                            {
                                await e.Channel.SendMessage ($"{e.User.Mention} du hast nicht genug {NadekoBot.Config.CurrencyName}! \nDu brauchst noch {amount - pts} {NadekoBot.Config.CurrencyName} um dies zu tun!").ConfigureAwait (false);
                                return;
                            }
                            FlowersHandler.RemoveFlowers (e.User,$"set usertype to {targetTypeStr}",amount);
                            //Actually changing the type here
                            var preTypes = DbHandler.Instance.GetAllRows<UserPokeTypes> ();
                            Dictionary<long,int> Dict = preTypes.ToDictionary (x => x.UserId,y => y.Id.Value);
                            if (Dict.ContainsKey ((long)e.User.Id))
                            {
                            //delete previous type
                            DbHandler.Instance.Delete<UserPokeTypes> (Dict[(long)e.User.Id]);
                            }

                            DbHandler.Instance.InsertData (new UserPokeTypes
                            {
                                UserId = (long)e.User.Id,
                                type = targetType.Name
                            });

                        //Now for the response

                        await e.Channel.SendMessage ($"Typ von {e.User.Mention} auf {targetTypeStr}{targetType.Icon} gesetzt für {amount} {NadekoBot.Config.CurrencySign}.").ConfigureAwait (false);
                        });

                cgb.CreateCommand (Prefix + "adminsettype")
                        .Description ($"Setzt den Typen eines Benutzers. **Owner only**.\n**Benutzer**: {Prefix}adminsettype feuer @Benutzer")
                        .Parameter ("targetType",ParameterType.Required)
                        .Parameter ("targetUser,",ParameterType.Unparsed)
                        .AddCheck (SimpleCheckers.OwnerOnly())
                        .Do (async e =>
                        {
                            var targetTypeStr = e.GetArg ("targetType")?.ToUpperInvariant ().Trim();
                            if (string.IsNullOrWhiteSpace (targetTypeStr))
                                return;
                            var targetType = stringToPokemonType (targetTypeStr);
                            var u = e.Message.MentionedUsers.FirstOrDefault ();
                            if (u == null)
                            {
                                await e.Channel.SendMessage ("Ungültiger Benutzer.").ConfigureAwait (false);
                                return;
                            }
                            
                            if (targetType == GetPokeType (u.Id))
                            {
                                await e.Channel.SendMessage ($"Der Typ von {u.Mention} ist bereits {targetType.Name.ToLowerInvariant ()}{targetType.Icon}").ConfigureAwait (false);
                                return;
                            }

                           
                            //Actually changing the type here
                            var preTypes = DbHandler.Instance.GetAllRows<UserPokeTypes> ();
                            Dictionary<long,int> Dict = preTypes.ToDictionary (x => x.UserId,y => y.Id.Value);
                            if (Dict.ContainsKey ((long)u.Id))
                            {
                                //delete previous type
                                DbHandler.Instance.Delete<UserPokeTypes> (Dict[(long)u.Id]);
                            }

                            DbHandler.Instance.InsertData (new UserPokeTypes
                            {
                                UserId = (long)u.Id,
                                type = targetType.Name
                            });

                            //Now for the response

                            await e.Channel.SendMessage ($"Typ von {u.Mention} auf {targetTypeStr}{targetType.Icon} gesetzt.").ConfigureAwait (false);
                        });
            });
        }
    }
}