﻿using Discord.Commands;
using MidnightBot.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MidnightBot.Modules.Searches.Commands
{
    class PokemonSearchCommands : DiscordCommand
    {
        private static Dictionary<string,SearchPokemon> pokemons;
        private static Dictionary<string,SearchPokemonAbility> pokemonAbilities;

        public PokemonSearchCommands ( DiscordModule module ) : base (module)
        {

            pokemons = JsonConvert.DeserializeObject<Dictionary<string,SearchPokemon>> (File.ReadAllText ("data/pokemon/pokemon_list.json"));
            pokemonAbilities = JsonConvert.DeserializeObject<Dictionary<string,SearchPokemonAbility>> (File.ReadAllText ("data/pokemon/pokemon_abilities.json"));
        }

        internal override void Init ( CommandGroupBuilder cgb )
        {
            cgb.CreateCommand (Prefix + "pokemon")
                .Alias (Prefix + "poke")
                .Description ("Sucht nach einem Pokemon.")
                .Parameter ("pokemon",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var pok = e.GetArg ("pokemon")?.Trim ().ToUpperInvariant ();
                    if (string.IsNullOrWhiteSpace (pok))
                        return;

                    foreach (var kvp in pokemons)
                    {
                        if (kvp.Key.ToUpperInvariant () == pok.ToUpperInvariant ())
                        {
                            await e.Channel.SendMessage ($"`Stats für \"{kvp.Key}\" Pokemon:`\n{kvp.Value}");
                            return;
                        }
                    }
                    await e.Channel.SendMessage ("`Kein Pokemon gefunden.`");
                });

            cgb.CreateCommand (Prefix + "pokemonability")
                .Alias (Prefix + "pokab")
                .Description ("Sucht nach einer Pokemon Fähigkeit.")
                .Parameter ("abil",ParameterType.Unparsed)
                .Do (async e =>
                {
                    var ab = e.GetArg ("abil")?.Trim ().ToUpperInvariant ();
                    if (string.IsNullOrWhiteSpace (ab))
                        return;
                    foreach (var kvp in pokemonAbilities)
                    {
                        if (kvp.Key.ToUpperInvariant () == ab)
                        {
                            await e.Channel.SendMessage ($"`Info für \"{kvp.Key}\" Fähigkeit:`\n{kvp.Value}");
                            return;
                        }
                    }
                    await e.Channel.SendMessage ("`Keine Fähigkeit gefunden.`");
                });
        }
    }

    public class SearchPokemon
    {
        public class GenderRatioClass
        {
            public float M { get; set; }
            public float F { get; set; }
        }
        public class BaseStatsClass
        {
            public int HP { get; set; }
            public int ATK { get; set; }
            public int DEF { get; set; }
            public int SPA { get; set; }
            public int SPD { get; set; }
            public int SPE { get; set; }

            public override string ToString () => $@"
    **HP:**  {HP,-4} **ATK:** {ATK,-4} **DEF:** {DEF,-4}
    **SPA:** {SPA,-4} **SPD:** {SPD,-4} **SPE:** {SPE,-4}";
        }
        public int Id { get; set; }
        public string Species { get; set; }
        public string[] Types { get; set; }
        public GenderRatioClass GenderRatio { get; set; }
        public BaseStatsClass BaseStats { get; set; }
        public Dictionary<string,string> Abilities { get; set; }
        public float HeightM { get; set; }
        public float WeightKg { get; set; }
        public string Color { get; set; }
        public string[] Evos { get; set; }
        public string[] EggGroups { get; set; }

        public override string ToString () => $@"`Name:` {Species}
`Typen:` {string.Join (", ",Types)}
`Stats:` {BaseStats}
`Höhe:` {HeightM,4}m `Gewicht:` {WeightKg}kg
`Fähigkeiten:` {string.Join (", ",Abilities.Values)}";

    }

    public class SearchPokemonAbility
    {
        public string Desc { get; set; }
        public string Name { get; set; }
        public float Rating { get; set; }

        public override string ToString () => $@"`Name:` : {Name}
`Bewertung:` {Rating}
`Beschreibung:` {Desc}";
    }
}