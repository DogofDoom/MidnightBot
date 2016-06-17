using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Modules;
using MidnightBot.Modules.Permissions.Classes;
using Discord.Commands;
using MidnightBot.DataModels;
using Discord;
using MidnightBot.Classes;
using MidnightBot.Modules.Pokemon.Extensions;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace MidnightBot.Modules.Pokemon
{
    class PokemonModule : DiscordModule
    {
        public override string Prefix { get; } = MidnightBot.Config.CommandPrefixes.Pokemon;

        public ConcurrentDictionary<ulong,TrainerStats> UserStats = new ConcurrentDictionary<ulong,TrainerStats> ();

        public override void Install ( ModuleManager manager )
        {
            manager.CreateCommands ("",cgb =>
            {
                cgb.AddCheck (PermissionChecker.Instance);

                cgb.CreateCommand (Prefix + "active")
                .Description ("Zeigt das aktive Pokemon von jemandem oder einem selbst\n**Benutzung**:{Prefix}active oder {Prefix}active @Someone")
                .Parameter ("target",ParameterType.Optional)
                .Do (async e =>
                {
                    var target = e.Server.FindUsers (e.GetArg ("target")).DefaultIfEmpty (null).FirstOrDefault () ?? e.User;

                    await e.Channel.SendMessage ($"**{target.Mention}**:\n{ActivePokemon (target).PokemonString ()}");
                    });
  
                cgb.CreateCommand(Prefix + "pokehelp")
                .Description($"Zeigt die Basis Hilfe für Pokemon Kämpfe")
                .Alias(Prefix + "ph")
                .Do(async e =>
                {
                    User user = null;
                    try {
                        user = PermissionHelper.ValidateUser(e.Server, e.GetArg("user"));
                    }
                    catch (Exception)
                    {
                        user = e.User;
                    }
                    await e.User.SendMessage($">list um deine Pokemon anzuzeigen. >switch name um dein aktives Pokemon auszutauschen. >ml um seine Angriffe anzuzeigen. >attack move @person um anzugreifen.\n Du bekommst {MidnightBot.Config.CurrencySign} für das besiegen anderer Pokemon, welche als Tränke mit >heal oder als Pokebälle mit >catch benutzt werden können.");
                    await e.Message.Delete ().ConfigureAwait (false);
                });

                cgb.CreateCommand(Prefix + "movelist")
                .Alias(Prefix + "ml")
                .Description("Zeigt eine Liste der verfügbaren Angriffe.\n**Benutzung**:{Prefix}movelist, {Prefix}ml, {Prefix}ml charmander")
                .Parameter ("name",ParameterType.Optional)
                .Do(async e =>
                {
                    var list = PokemonList(e.User);
                    var pkm = list.Where(x => x.NickName == e.GetArg("name").Trim()).DefaultIfEmpty(null).FirstOrDefault() ?? ActivePokemon(e.User);
                    
                    await e.User.SendMessage($"{e.User.Mention}\n**{pkm.NickName}'s Angriffe:**\n{pkm.PokemonMoves()}");
                });

                cgb.CreateCommand (Prefix + "switch")
                .Description ($"Setzt dein aktives Pokemon per Nickname\n**Benutzung**:{Prefix}switch mudkip")
                .Parameter ("name",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var list = PokemonList (e.User);
                    var toSet = list.Where (x => x.NickName == e.GetArg ("name").Trim ()).DefaultIfEmpty (null).FirstOrDefault ();
                    if (toSet == null)
                    {
                        await e.Channel.SendMessage ($"Konnte Pokemon mit Name \"{e.GetArg ("name").Trim ()}\" nicht finden.");
                        return;
                    }

                    var trainerStats = UserStats.GetOrAdd (e.User.Id,new TrainerStats ());
                    if (trainerStats.LastAttacked.Contains(e.User.Id))
                    {
                        await e.Channel.SendMessage($"{e.User.Mention} kann sich nicht bewegen!");
                        return;
                    }
                    if (trainerStats.MovesMade > TrainerStats.MaxMoves)
                    {
                        await e.Channel.SendMessage ($"{e.User.Mention} hat zu oft angegriffen und kann sich nicht bewegen!");
                        return;
                    }

                    switch (SwitchPokemon (e.User,toSet))
                    {
                        case 0:
                            {
                                trainerStats.LastAttacked.Add (e.User.Id);
                                trainerStats.MovesMade++;
                                UserStats.AddOrUpdate(e.User.Id, trainerStats, (s, t) => trainerStats);
                                await e.Channel.SendMessage($"Aktives Pokemon von {e.User.Mention} auf {toSet.NickName} gesetzt");
                                break;
                            }
                        case 2:
                            {
                                await e.Channel.SendMessage($"Das ausgetauschte Pokemon muss HP haben!");
                                break;
                            }
                        case 1:
                            {
                                await e.Channel.SendMessage($"Kein aktives Pokemon!");
                                break;
                            }
                    }
                });

                cgb.CreateCommand(Prefix + "allmoves")
                .Description("Sendet dir eine private Nachticht mit allen Attacken deiner Pokemon.\n**Benutzung**:{Prefix}allmoves, {Prefix}am")
                .Alias(Prefix + "am")
                .Do(async e =>
                {
                var list = PokemonList(e.User);
                string str = $"{e.User.Mention}'s Pokemon sind:\n";
                    foreach (var pkm in list)
                    {
                        str += $"`{pkm.NickName}:` Level: {pkm.Level}\n{pkm.PokemonMoves ()}\n";
                    }
                    var msg = await e.Channel.SendMessage($"{e.User.Mention} ich habe dir eine Liste aller deiner Angriffe geschickt.");
                    await e.User.SendMessage(str);
                });

                cgb.CreateCommand (Prefix + "list")
                .Description ("Gibt eine Liste deiner Pokemon (6) zurück (aktives Pokemon ist unterstrichen)")
                .Do (async e =>
                {
                    var list = PokemonList (e.User);
                    string str = $"{e.User.Mention}'s Pokemon sind:\n";
                    foreach (var pkm in list)
                    {
                        if (pkm.HP <= 0)
                        {
                            str += $"~~**{pkm.NickName}** : *{pkm.GetSpecies().name}*  HP: {pkm.HP}/{pkm.MaxHP}~~💀\nBildlink: {pkm.GetSpecies().imageLink}\n";
                        }
                        else if (pkm.IsActive)
                        {
                            str += $"__**{pkm.NickName}** : *{pkm.GetSpecies().name}*  HP: {pkm.HP}/{pkm.MaxHP}__\nBildlink: {pkm.GetSpecies ().imageLink}\n";
                        }
                        else
                        {
                            str += $"**{pkm.NickName}** : *{pkm.GetSpecies ().name}*  HP: {pkm.HP}/{pkm.MaxHP}\nBildlink: {pkm.GetSpecies ().imageLink}\n";
                        }
                    }
                    await e.User.SendMessage (str);
                });

                cgb.CreateCommand(Prefix + "elite4")
                .Description($"Zeigt die 5 stärksten Pokemon.\n**Benutzung**:{Prefix}elite4")
                .Alias(Prefix + "e4")
                .Do(async e =>
                {
                    var db = DbHandler.Instance.GetAllRows<PokemonSprite>();
                    var elites = db.OrderByDescending(i => i.XP).Take(4);
                    string str = "**Top 4 Pokemon:**";
                    foreach (PokemonSprite elite in elites)
                    {
                        str += $"\n`Trainer:` <@{elite.OwnerId}>";
                        str += $"\n```Pokemon Name: {elite.NickName}";
                        str += $"\nPokemon Level: {elite.Level}```";
                    }
                    await e.Channel.SendMessage(str);
                });

                cgb.CreateCommand (Prefix + "heal")
                .Description ($"Heilt dein angegebenes Pokemon (per Nicknamen) oder das aktive Pokemon der gegebenen Person.\n**Benutzung**:{Prefix}heal bulbasaur, {Prefix}heal @user, {Prefix}heal all")
                .Parameter ("args",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var args = e.GetArg ("args");

                    var target = e.Server.FindUsers (args).DefaultIfEmpty (null).FirstOrDefault () ?? e.User;

                    if (target == e.User)
                    {
                        if (args == "all")
                        {
                            var pkms = PokemonList(target).Where(x => x.HP < x.MaxHP);
                            string str = $"{target.Mention} geheilt:";
                            int count = pkms.Count();
                            if (count <= 0)
                            {
                                await e.Channel.SendMessage($"{e.User.Mention}, All of your pokemon are at full health!");
                                return;
                            }
                            if (FlowersHandler.RemoveFlowers(target, "Healed pokemon", count))
                            {
                                foreach (var pkm in pkms)
                                {
                                    var hp = pkm.HP;
                                    pkm.HP = pkm.MaxHP;
                                    str += $"\n{pkm.NickName} um {pkm.HP - hp} HP";
                                    //await e.Channel.SendMessage($"{target.Mention} successfully healed {pkm.NickName} for {pkm.HP - hp} HP for a {Uni.Config.CurrencySign}");
                                    DbHandler.Instance.Save(pkm);
                                    //Heal your own userstats as well?
                                }
                            }
                            else
                            {
                                await e.Channel.SendMessage($"Konnte {MidnightBot.Config.CurrencySign} nicht bezahlen.**");
                                return;
                            }
                            str += $"\n*Benutzte {count.ToString()} {MidnightBot.Config.CurrencySign}*";
                            await e.Channel.SendMessage(str);
                            return;
                        }

                        var toHeal = PokemonList (target).Where (x => x.NickName == args.Trim ()).DefaultIfEmpty (null).FirstOrDefault ();
                        if (toHeal == null)
                        {
                            await e.Channel.SendMessage ($"Konnte Pokemon mit Namen \"{e.GetArg ("args").Trim ()}\" nicht finden");
                            return;
                        }

                        if (FlowersHandler.RemoveFlowers (target,"Healed pokemon",1))
                        {
                            var hp = toHeal.HP;
                            toHeal.HP = toHeal.MaxHP;
                            await e.Channel.SendMessage ($"{target.Mention} hat {toHeal.NickName} erfolgreich um {toHeal.HP - hp} HP für einen {MidnightBot.Config.CurrencySign} geheilt.");
                            DbHandler.Instance.Save (toHeal);
                            //Heal your own userstats as well?
                        }
                        else
                        {
                            await e.Channel.SendMessage ($"Konnte {MidnightBot.Config.CurrencySign} nicht zahlen.");
                        }
                        return;
                    }
                    var toHealn = ActivePokemon (target);
                    if (toHealn == null)
                    {
                        await e.Channel.SendMessage ($"Konnte Pokemon von {target.Name} nicht finden");
                        return;
                    }
                    if (FlowersHandler.RemoveFlowers (target,"Healed pokemon",1))
                    {
                        var hp = toHealn.HP;
                        toHealn.HP = toHealn.MaxHP;
                        await e.Channel.SendMessage ($"{target.Mention} hat {toHealn.NickName} erfolgreich um {toHealn.HP - hp} HP geheilt, für einen {MidnightBot.Config.CurrencySign}");
                        DbHandler.Instance.Save (toHealn);
                    }
                    else
                    {
                        await e.Channel.SendMessage ($"Konnte {MidnightBot.Config.CurrencySign} nicht bezahlen.");
                    }
                    return;
                });

                cgb.CreateCommand (Prefix + "rename")
                .Alias (Prefix + "rn")
                .Description ($"Benennt dein aktives Pokemon um.\n**Benutzung**: {Prefix}rename dickbutt, {Prefix}rn Mittens")
                .Parameter ("name",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var newName = e.GetArg ("name");
                    if (string.IsNullOrWhiteSpace (newName))
                    {
                        await e.Channel.SendMessage ("Name benötigt");
                        return;
                    }
                    var activePkm = ActivePokemon (e.User);
                    activePkm.NickName = newName;
                    DbHandler.Instance.Save (activePkm);
                    await e.Channel.SendMessage ($"Aktives Pokemon zu {newName} umbenannt");
                });


                cgb.CreateCommand (Prefix + "reset")
                .Description ($"Setzt deine Pokemon zurück. KANN NICHT RÜCKGÄNGIG GEMACHT WERDEN\n**Benutzung**:{Prefix}reset true")
                .Parameter ("true",ParameterType.Unparsed)
                .Do (async e =>
                {
                    bool willReset;
                    if (bool.TryParse (e.GetArg ("true"),out willReset))
                    {
                        if (willReset)
                        {
                            var db = DbHandler.Instance.GetAllRows<PokemonSprite>();
                            var row = db.Where(x => x.OwnerId == (long)e.User.Id);
                            //var toDelete = DbHandler.Instance.FindAll<PokemonSprite>(s => s.OwnerId == (long)e.User.Id);
                            foreach (var todel in row)
                            {
                                DbHandler.Instance.Delete<PokemonSprite>(todel.Id.Value);
                            }
                            await e.Channel.SendMessage ("Deine Pokemon wurden gelöscht.\n\nGrauenhaft.\n\n\nIch habe keine Worte dafür.");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage ($"Benutze `{Prefix}reset true` um deine Pokemon wirklich zu töten 🔫");
                    }
                });

                Random rand = new Random ();
                cgb.CreateCommand(Prefix + "catch")
                .Description($"Versucht das derzeitige wilde Pokemon zu fangen. Du musst das Pokemon angeben, welches du ersetzen willst. Kostet einen {MidnightBot.Config.CurrencyName}\n**Benutzung**:{Prefix}catch MyMudkip")
                .Parameter("replace", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string str = "";
                    var defenderPokemon = ActivePokemon(e.Server.GetUser(MidnightBot.Creds.BotId));
                    if (!FlowersHandler.RemoveFlowers(e.User,"Catching pokemon", 1)){
                        await e.Channel.SendMessage($"{e.User.Mention}, es benötigt einen {MidnightBot.Config.CurrencySign} {MidnightBot.Config.CurrencyName} um einen Pokeball zu kaufen!");
                        return;
                    }
                    var list = PokemonList(e.User);
                    var pkm = list.Where(x => x.NickName == e.GetArg("replace").Trim()).DefaultIfEmpty(null).FirstOrDefault();
                    if (pkm == null)
                    {
                        await e.Channel.SendMessage($"Konnte Pokemon mit Namen \"{e.GetArg("replace").Trim()}\" nicht finden.");
                        return;
                    }

                    int rate = 1;
                    int chance = ((3 * defenderPokemon.MaxHP - 2 * defenderPokemon.HP) * rate) / 3 * defenderPokemon.MaxHP;
                    //str += chance.ToString() + "\n";
                    int randm = rand.Next(1, 255);
                    //str += randm.ToString();
                    if (randm < chance)
                    {
                        pkm.NickName = defenderPokemon.NickName;
                        pkm.HP = defenderPokemon.HP;
                        pkm.SpeciesId = defenderPokemon.SpeciesId;
                        pkm.Speed = defenderPokemon.Speed;
                        pkm.Level = defenderPokemon.Level;
                        pkm.Attack = defenderPokemon.Attack;
                        pkm.Defense = defenderPokemon.Defense;
                        pkm.SpecialAttack = defenderPokemon.SpecialAttack;
                        pkm.SpecialDefense = defenderPokemon.SpecialDefense;
                        pkm.XP = defenderPokemon.XP;
                        pkm.MaxHP = defenderPokemon.MaxHP;
                        DbHandler.Instance.Save(pkm);
                        var db = DbHandler.Instance.GetAllRows<PokemonSprite>();
                        var row = db.Where(x => x.OwnerId == (long)MidnightBot.Creds.BotId);
                        //var toDelete = DbHandler.Instance.FindAll<PokemonSprite>(s => s.OwnerId == (long)e.User.Id);
                        foreach (var todel in row)
                        {
                            DbHandler.Instance.Delete<PokemonSprite>(todel.Id.Value);
                        }
                        str += $"{e.User.Mention} hat {defenderPokemon.NickName} gefangen!";
                    }
                    else
                        str += $"{defenderPokemon.NickName} escaped!";
                    await e.Channel.SendMessage(str);
                });

                cgb.CreateCommand (Prefix + "attack")
                .Alias (Prefix)
                .Description ($"Greift gegebenes Ziel mit gegebener Attacke an.\n**Benutzung**: {Prefix}attack hyperbeam @user, {Prefix}attack @user flame-charge, {Prefix} sunny-day @user")
                .Parameter ("args",ParameterType.Unparsed)
                .Do (async e =>
                {
                    Regex regex = new Regex (@"<@!?(?<id>\d+)>");
                    var args = e.GetArg ("args");
                    if (!regex.IsMatch (args))
                    {
                        await e.Channel.SendMessage ("Bitte gib Ziel ein");
                        return;
                    }
                    Match m = regex.Match (args);
                    var id = ulong.Parse (m.Groups["id"].Value);
                    if (id == MidnightBot.Creds.BotId)
                    {

                    }
                    var target = e.Server.GetUser (id);
                    if (target == null)
                    {
                        await e.Channel.SendMessage ("Bitte gib Ziel ein");
                        return;
                    }
                    if (target == e.User)
                    {
                        await e.Channel.SendMessage("Du kannst dich nicht selbst angreifen!");
                        return;
                    }
                    var moveString = args.Replace (m.Value,string.Empty).Replace ("\"","").Trim ();
                    string attstr = await Attack (e.User,target,moveString);
                    await e.Channel.SendMessage (attstr);

                    if (target.Id == MidnightBot.Creds.BotId && !attstr.Contains ("Cannot use"))
                    {
                        string moves = ActivePokemon (target).PokemonMoves (true);
                        string[] lines = moves.Split ('\n');
                        int linenum = rand.Next (0,3);
                        string move = $"{lines[linenum]}";
                        move = move.Trim ('\n',' ');
                        string output = $"{e.User.Mention}\n";
                        output += await Attack (target,e.User,move);
                        await e.Channel.SendMessage (output);
                    }
                });
            });
        }

        /// <summary>
        /// Sets the active pokemon of the given user to the given Sprite
        /// </summary>
        /// <param name="u"></param>
        /// <param name="newActive"></param>
        /// <returns></returns>
        int SwitchPokemon ( User u,PokemonSprite newActive )
        {
            var toUnset = PokemonList (u).Where (x => x.IsActive).FirstOrDefault ();
            if (toUnset == null)
            {
                return 1;
            }
            if (newActive.HP <= 0)
            {
                return 2;
            }
            toUnset.IsActive = false;
            newActive.IsActive = true;
            DbHandler.Instance.Save (toUnset);
            DbHandler.Instance.Save (newActive);

            return 0;
        }

        PokemonSprite ActivePokemon ( User u )
        {
            var list = PokemonList (u);
            return list.Where (x => x.IsActive).FirstOrDefault ();
        }

        List<PokemonSprite> PokemonList ( User u,int level = 5 )
        {
            var db = DbHandler.Instance.GetAllRows<PokemonSprite> ();
            var row = db.Where (x => x.OwnerId == (long)u.Id);
            if (row.Count () >= 6 || (u.Id == MidnightBot.Creds.BotId && row.Count () >= 1))
            {
                return row.ToList ();
            }
            else
            {
                var list = new List<PokemonSprite> ();
                while ((u.Id != MidnightBot.Creds.BotId && list.Count < 6) || (u.Id == MidnightBot.Creds.BotId && list.Count < 1))
                {
                    var pkm = GeneratePokemon (u,level);
                    if (!list.Where (x => x.IsActive).Any ())
                    {
                        pkm.IsActive = true;
                    }
                    list.Add (pkm);
                    DbHandler.Instance.Save (pkm);
                }
                //Set an active pokemon
                return list;
            }
        }

        Random rng = new Random ();
        private PokemonSprite GeneratePokemon ( User u,int level )
        {
            var list = PokemonMain.Instance.pokemonClasses.Where (x => x.evolveLevel != -1).ToList ();
            var speciesIndex = rng.Next (0,list.Count () - 1);
            
            var species = list[speciesIndex];

            PokemonSprite sprite = new PokemonSprite
            {
                SpeciesId = species.number,
                HP = species.baseStats["hp"],
                Level = 1,
                NickName = species.name,
                OwnerId = (long)u.Id,
                XP = 0,
                Attack = species.baseStats["attack"],
                Defense = species.baseStats["defense"],
                SpecialAttack = species.baseStats["special-attack"],
                SpecialDefense = species.baseStats["special-defense"],
                Speed = species.baseStats["speed"],
                MaxHP = species.baseStats["hp"]
            };

            while (sprite.Level < level - 1)
            {
                sprite.LevelUp ();
            }
            sprite.XP = sprite.XPRequired ();
            sprite.LevelUp ();
            return sprite;
        }

        private async System.Threading.Tasks.Task<string> Attack ( User attacker,User target,string moveString )
        {
            PokemonSprite attackerPokemon = ActivePokemon (attacker);
            string returnstring = "";
            var species = attackerPokemon.GetSpecies ();
            if (!species.moves.Keys.Contains(moveString))
            {
                returnstring = $"Kann \"{moveString}\" nicht benutzen, benutze `{Prefix}ml` um eine Liste der Attacken zu sehen";
                return returnstring;
            }
            var attackerStats = UserStats.GetOrAdd (attacker.Id,new TrainerStats ());
            var defenderStats = UserStats.GetOrAdd (target.Id,new TrainerStats ());
            if (attackerStats.MovesMade > TrainerStats.MaxMoves || attackerStats.LastAttacked.Contains(target.Id))
            {
                returnstring = $"{attacker.Mention} kann sich nicht bewegen!";
                return returnstring;
            }
            if (attackerPokemon.HP == 0)
            {
                returnstring = $"{attackerPokemon.NickName} wurde besiegt und kann nicht angreifen!";
                return returnstring;
            }
            var defenderPokemon = ActivePokemon (target);
            if (defenderPokemon.HP <= 0)
            {
                returnstring = $"{defenderPokemon.NickName} wurde bereits besiegt!";
                return returnstring;
            }
            double p = 0.90 * attackerPokemon.Speed / defenderPokemon.Speed;
            //await e.Channel.SendMessage($"p: {p.ToString()} 1-p: {(1- p).ToString()}");
            if (rng.NextDouble() < (1 - p))
            {
                if (target.Id == MidnightBot.Creds.BotId)
                {
                    attackerStats.LastAttacked.Remove(target.Id);
                    attackerStats.MovesMade = 0;
                    UserStats.AddOrUpdate(attacker.Id, x => attackerStats, (s, t) => attackerStats);
                    UserStats.AddOrUpdate(target.Id, x => defenderStats, (s, t) => defenderStats);
                    DbHandler.Instance.Save(defenderPokemon);
                    DbHandler.Instance.Save(attackerPokemon);
                }
                else
                {
                    defenderStats.LastAttacked.Remove(target.Id);
                    attackerStats.LastAttacked.Add(target.Id);
                    attackerStats.MovesMade++;
                    defenderStats.LastAttacked = new List<ulong>();
                    defenderStats.MovesMade = 0;
                    UserStats.AddOrUpdate(attacker.Id, x => attackerStats, (s, t) => attackerStats);
                    UserStats.AddOrUpdate(target.Id, x => defenderStats, (s, t) => defenderStats);
                    DbHandler.Instance.Save(defenderPokemon);
                    DbHandler.Instance.Save(attackerPokemon);
                }
                returnstring = $"{attackerPokemon.NickName}'s Attacke fehlgeschlagen!";
                return returnstring;
            }

            KeyValuePair<string,string> move = new KeyValuePair<string,string> (moveString,species.moves[moveString]);

            PokemonAttack attack = new PokemonAttack (attackerPokemon,defenderPokemon,move);
            returnstring += attack.AttackString();
            defenderPokemon.HP -= attack.Damage;

            var HP = (defenderPokemon.HP < 0) ? 0 : defenderPokemon.HP;
            returnstring += $"{defenderPokemon.NickName} hat {HP} HP über!\n";
            

            attackerStats.LastAttacked.Add(target.Id);
            attackerStats.MovesMade++;
            defenderStats.LastAttacked = new List<ulong>();
            defenderStats.MovesMade = 0;
            UserStats.AddOrUpdate(attacker.Id, x => attackerStats, (s, t) => attackerStats);
            UserStats.AddOrUpdate(target.Id, x => defenderStats, (s, t) => defenderStats);
            DbHandler.Instance.Save(defenderPokemon);
            DbHandler.Instance.Save(attackerPokemon);
            if (defenderPokemon.HP <= 0)
            {
                defenderPokemon.HP = 0;
                
                if (target.Id != MidnightBot.Creds.BotId)
                    returnstring += $"{defenderPokemon.NickName} wurde besiegt!\n{attackerPokemon.NickName}'s Besitzer {attacker.Mention} erhält 1 {MidnightBot.Config.CurrencySign}\n";
                var lvl = attackerPokemon.Level;

                    var extraXP = attackerPokemon.Reward (defenderPokemon);

                    returnstring += $"{attackerPokemon.NickName} erhält {extraXP} von diesem Kampf\n";

                if (attackerPokemon.Level > lvl) //levled up
                {
                    returnstring += $"**{attackerPokemon.NickName}** ist aufgelevelt!\n**{attackerPokemon.NickName}** ist nun Level **{attackerPokemon.Level}**";
                    //Check evostatus
                }
                if (target.Id != MidnightBot.Creds.BotId)
                {
                    var list = PokemonList (target).Where (s => (s.HP > 0 && s != defenderPokemon));
                    if (list.Any())
                    {
                        var toSet = list.FirstOrDefault ();
                        switch (SwitchPokemon ( target,toSet))
                        {
                            case 0:
                                {
                                    returnstring += $"\n{target.Mention}'s aktives Pokemon auf **{toSet.NickName}** gesetzt";
                                    defenderPokemon.IsActive = false;
                                    break;
                                }
                            case 1:
                            case 2:
                                {
                                    returnstring += $"\n **Error:** konnte Pokemon nicht austauschen";
                                    break;
                                }
                        }
                    }
                    else
                    {
                        returnstring += $"\n{target.Mention} hat keine Pokemon über!";
                        returnstring += $"\n{attacker.Mention} erhält 3 {MidnightBot.Config.CurrencySign} für das Besiegen von {target.Mention}";
                        await FlowersHandler.AddFlowersAsync ( attacker, $"Defeated {target.Mention} in pokemon!", 3);

                        //do something?
                    }
                }
                //await e.Channel.SendMessage(str);
                if (target.Id != MidnightBot.Creds.BotId)
                    await FlowersHandler.AddFlowersAsync ( attacker, "Victorious in pokemon", 1, true);
            }

                //Update stats, you shall
                defenderStats.LastAttacked.Remove(target.Id);
                attackerStats.LastAttacked.Add(target.Id);
                attackerStats.MovesMade++;
                defenderStats.LastAttacked = new List<ulong>();
                defenderStats.MovesMade = 0;
                UserStats.AddOrUpdate(attacker.Id, x => attackerStats, (s, t) => attackerStats);
                UserStats.AddOrUpdate(target.Id, x => defenderStats, (s, t) => defenderStats);
                DbHandler.Instance.Save(defenderPokemon);
                DbHandler.Instance.Save(attackerPokemon);
            if (defenderPokemon.HP == 0 && target.Id == MidnightBot.Creds.BotId)
            {
                var db = DbHandler.Instance.GetAllRows<PokemonSprite> ();
                var row = db.Where (x => x.OwnerId == (long)MidnightBot.Creds.BotId);
                //var toDelete = DbHandler.Instance.FindAll<PokemonSprite>(s => s.OwnerId == (long)e.User.Id);
                foreach (var todel in row)
                {
                    DbHandler.Instance.Delete<PokemonSprite>(todel.Id.Value);
                }
                PokemonList ( target,attackerPokemon.Level);
            }
            return returnstring;
        }
    }
}