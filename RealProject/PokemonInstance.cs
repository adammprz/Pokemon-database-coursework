using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace RealProject
{
    public class PokemonInstance
    {
        public int id;
        public string name;

        public bool isShiny = false;

        public int level;
        public int currentExp = 0;
        public int maxExp;
        public int expGainIndex;

        public int currentHealth;

        public int hpBase, atkBase, defBase, spaBase, spdBase, speBase;
        public int hpIV, atkIV, defIV, spaIV, spdIV, speIV;

        public int hpStat, atkStat, defStat, spaStat, spdStat, speStat;

        public int atkModifier, defModifier, spaModifier, spdModifier, speModifier, accModifier, evaModifier;

        public Texture2D frontTexture, backTexture;

        public MoveInstance[] moveset;

        public int type1, type2;

        public StatusEffects statusCondition;
        public List<StatusEffects> volatileStatuses = new List<StatusEffects>();

        public PokemonInstance(int id, string name, int x, int y, bool isShiny, int level, int expGainIndex, int[] baseStats, int type1, int type2 = -1)
        {
            this.id = id;
            this.name = name;
            this.level = level - 1;
            this.expGainIndex = expGainIndex;

            if (isShiny)
                x += 65;

            frontTexture = Global.CropTexture(UIManager.gen1SpriteSheet, new Rectangle(x, y, 64, 64));
            backTexture = Global.CropTexture(UIManager.gen1SpriteSheet, new Rectangle(x, y + 65, 64, 64));
            hpBase = baseStats[0];
            atkBase = baseStats[1];
            defBase = baseStats[2];
            spaBase = baseStats[3];
            spdBase = baseStats[4];
            speBase = baseStats[5];

            this.type1 = type1;
            this.type2 = type2;

            statusCondition = new StatusEffects(PrimaryStatusConditions.None);
        }

        public void RandomisePokemon(PokemonInstance[] playerTeam)
        {
            hpIV = RandomIV("hp", playerTeam);
            atkIV = RandomIV("atk", playerTeam);
            defIV = RandomIV("def", playerTeam);
            spaIV = RandomIV("spa", playerTeam);
            spdIV = RandomIV("spd", playerTeam);
            speIV = RandomIV("spe", playerTeam);

            RecalculateLevelAndExp();

            List<MoveInstance> entireMoveset = DatabaseManager.GetPokemonPriorMoves(id, 5);
            moveset = entireMoveset.Skip(Math.Max(0, entireMoveset.Count - 4)).Take(4).ToArray();

            foreach (var v in moveset)
                Debug.WriteLine(v.name);

            currentHealth = hpStat;
        }

        int RandomIV(string ivCase, PokemonInstance[] playerTeam)
        {
            int iv = 0;
            int amount = 1;

            foreach (var v in playerTeam)
            {
                if (v != null)
                {
                    if (v.id == id)
                    {
                        switch (ivCase)
                        {
                            case "hp":
                                iv += v.hpIV;
                                amount++;
                                break;
                            case "atk":
                                iv += v.atkIV;
                                amount++;
                                break;
                            case "def":
                                iv += v.defIV;
                                amount++;
                                break;
                            case "spa":
                                iv += v.spaIV;
                                amount++;
                                break;
                            case "spd":
                                iv += v.spdIV;
                                amount++;
                                break;
                            case "spe":
                                iv += v.speIV;
                                amount++;
                                break;
                        }
                    }
                }
            }

            int min = (int)MathF.Round(iv / amount, 0);

            iv = Global.random.Next(min, 32);

            return iv;
        }

        public void CalculateAllStats()
        {
            hpStat = CalculateHP();
            atkStat = CalculateStat(atkBase, atkIV);
            defStat = CalculateStat(defBase, defIV);
            spaStat = CalculateStat(spaBase, spaIV);
            spdStat = CalculateStat(spdBase, spdIV);
            speStat = CalculateStat(speBase, speIV);
        }

        int CalculateHP() //Acount for evs please
        {
            int h = 0;

            h = (2 * hpBase + hpIV) * level;
            h /= 100;
            h += level + 10;

            return h;
        }

        int CalculateStat(int baseS, int baseIv) //Acount for nature and Evs please
        {
            int stat = 0;

            stat = (2 * baseS + baseIv) * level;
            stat /= 100;
            stat += 5;
            //stat *= nature;

            return stat;
        }

        void RecalculateLevelAndExp()
        {
            int floorExp = 0;
            int nextLevelExp = 0;

            if (expGainIndex == 0) // Medium Fast
            {
                floorExp = (int)(4 * MathF.Pow(level, 3) / 5f);
                nextLevelExp = (int)(4 * MathF.Pow(level + 1, 3) / 5f);
            }
            else if (expGainIndex == 1) // Erratic
            {

            }
            else if (expGainIndex == 2) // Fluctuating
            {

            }
            else if (expGainIndex == 3) // Medium Slow
            {
                floorExp = (int)(1.2f * MathF.Pow(level, 3) - 15 * MathF.Pow(level, 2) + 100 * level - 140);
                nextLevelExp = (int)(1.2f * MathF.Pow(level + 1, 3) - 15 * MathF.Pow(level + 1, 2) + 100 * (level + 1) - 140);
            }
            else if (expGainIndex == 4) // Fast
            {
                floorExp = (int)MathF.Pow(level, 3);
                nextLevelExp = (int)MathF.Pow(level + 1, 3);
            }
            else // Slow
            {
                floorExp = (int)(5 * MathF.Floor(MathF.Pow(level, 3)) / 4f);
                nextLevelExp = (int)(5 * MathF.Floor(MathF.Pow(level + 1, 3)) / 4f);
            }

            Debug.WriteLine($"Current level {level}, exp = {currentExp}, max = {maxExp}");

            if (currentExp >= maxExp)
            {
                level++;
                currentExp = 0;
                maxExp = nextLevelExp - floorExp;
                CalculateAllStats();
            }
        }

        public void GainExp(int expGain)
        {
            currentExp += expGain;

            RecalculateLevelAndExp();
        }

        public int CalculateExpGainFromKO(int opposingLevel)
        {
            float exp = 0;
            int baseYield = 0;

            string dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT BaseExpYield FROM Pokemon WHERE PokeID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader1 = command.ExecuteReader();

            if (reader1.Read())
            {
                baseYield = reader1.GetInt32(reader1.GetOrdinal("BaseExpYield"));
            }

            exp = 2 * level + 10;
            exp /= (level + opposingLevel + 10);
            exp = MathF.Pow(exp, 2.5f);
            exp *= (baseYield * level / 5f); //Account for exp share?
            exp += 1;

            return (int)exp;
        }

        public PokemonInstance DuplicatePokemon()
        {
            PokemonInstance dupedPoke = new PokemonInstance(id, name, 0, 0, isShiny, level, expGainIndex, [hpBase, atkBase, defBase, spaBase, spdBase, speBase], type1, type2);
            dupedPoke.moveset = moveset;
            dupedPoke.statusCondition = statusCondition;
            dupedPoke.volatileStatuses = volatileStatuses;

            dupedPoke.hpIV = hpIV;
            dupedPoke.atkIV = atkIV;
            dupedPoke.defIV = defIV;
            dupedPoke.spaIV = spaIV;
            dupedPoke.spdIV = spdIV;
            dupedPoke.speIV = speIV;

            dupedPoke.hpStat = hpStat;
            dupedPoke.atkStat = atkStat;
            dupedPoke.defStat = defStat;
            dupedPoke.spaStat = spaStat;
            dupedPoke.spdStat = spdStat;
            dupedPoke.speStat = speStat;

            dupedPoke.evaModifier = evaModifier;
            dupedPoke.accModifier = accModifier;
            dupedPoke.atkModifier = atkModifier;
            dupedPoke.defModifier = defModifier;
            dupedPoke.spaModifier = spaModifier;
            dupedPoke.spdModifier = spdModifier;
            dupedPoke.speModifier = speModifier;

            dupedPoke.currentHealth = currentHealth;

            return dupedPoke;
        }
    }

    public enum PrimaryStatusConditions
    {
        None,
        Burn, //
        Paralysis, //
        Poison, //
        Toxic, //
        Freeze, //
        Sleep //
    } 
    public enum VolatileStatusConditions
    {
        Empty,
        Bound,
        Curse,
        Nightmare,
        PerishSong,
        Autotomize,
        Grounded,
        AquaRing,
        TakingAim,
        Drowsy,
        DefenseCurl,
        Rolling,
        CantEscape,
        Embargo,
        Imprison,
        Confusion, //
        GettingPumped,
        Rampage,
        Recharge,
        Charging,
        SemiInvulnerable,
        Protection,
        Flinch,
        Substitute
    }


    public class StatusEffects
    {
        public int turnCount;

        public PrimaryStatusConditions primaryCondition;
        public VolatileStatusConditions volatileCondition;

        public StatusEffects(PrimaryStatusConditions primary, int turns = -1)
        {
            primaryCondition = primary;
            volatileCondition = VolatileStatusConditions.Empty;
            turnCount = turns;
        }

        public StatusEffects(int turns, VolatileStatusConditions volatileCondition)
        {
            primaryCondition = PrimaryStatusConditions.None;
            this.volatileCondition = volatileCondition;
            turnCount = turns;
        }
    }
}
