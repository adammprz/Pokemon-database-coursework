using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RealProject
{
    static class DatabaseManager
    {
        static string dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

        public static void Start()
        {
            //dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            ////string fullPath = Path.GetFullPath(dbRelativePath);
            ////Debug.WriteLine("Opening DB at: " + fullPath);

            ////return;

            //using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            //connection.Open();

            ////string query = "SELECT * FROM Pokemon WHERE PokeID = @pokedexNumber";
            //string query = "SELECT Multiplier FROM TypeEffectiveness WHERE AttackTypeID = @attackerNum AND DefenseTypeID = @defenderNum";

            //using var command = new SqliteCommand(query, connection);
            //command.Parameters.AddWithValue("@attackerNum", 4);
            //command.Parameters.AddWithValue("@defenderNum", 5);

            //using var reader = command.ExecuteReader();

            //if (reader.Read())
            //{
            //    //int number = reader.GetInt32(reader.GetOrdinal("PokeID"));
            //    //string name = reader.GetString(reader.GetOrdinal("PokeName"));
            //    //int hp = reader.GetInt32(reader.GetOrdinal("BaseHP"));
            //    //int attack = reader.GetInt32(reader.GetOrdinal("BaseAtk"));

            //    float attack = (float)reader.GetDouble(reader.GetOrdinal("Multiplier"));

            //    //return new Pokemon
            //    //{
            //    //    PokedexNumber = number,
            //    //    Name = name,
            //    //    HP = hp,
            //    //    Attack = attack,
            //    //    // etc...
            //    //};

            //    //Debug.WriteLine($"{name} has pokedex id {number} and base {hp} HP, {attack} attack;");
            //    Debug.WriteLine($"multiplier is {attack}");
            //}
            //else
            //{
            //    //return null; // Not found
            //    Debug.WriteLine($"multiplier is 1");
            //}
        }

        public static PokemonInstance GetBasePokemonData(int id, int level = 1, bool shiny = false)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT * FROM Pokemon WHERE PokeID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("PokeName"));

                int levelIndex = reader.GetInt32(reader.GetOrdinal("LevelSpeed"));

                int x = reader.GetInt32(reader.GetOrdinal("TextureX"));
                int y = reader.GetInt32(reader.GetOrdinal("TextureY"));

                int hp = reader.GetInt32(reader.GetOrdinal("BaseHP"));
                int atk = reader.GetInt32(reader.GetOrdinal("BaseDef"));
                int def = reader.GetInt32(reader.GetOrdinal("BaseAtk"));
                int spa = reader.GetInt32(reader.GetOrdinal("BaseSpa"));
                int spd = reader.GetInt32(reader.GetOrdinal("BaseSpd"));
                int spe = reader.GetInt32(reader.GetOrdinal("BaseSpe"));

                int type1 = reader.GetInt32(reader.GetOrdinal("Type1ID"));
                int type2 = -1;

                if(!reader.IsDBNull(reader.GetOrdinal("Type2ID")))
                    type2 = reader.GetInt32(reader.GetOrdinal("Type2ID"));

                return new PokemonInstance(id, name, x, y, shiny, level, levelIndex, [hp, atk, def, spa, spd, spe], type1, type2);
            }

            return null;
        }

        public static Texture2D GetOverworldPokeData(int id)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT TextureX, TextureY FROM Pokemon WHERE PokeID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                int x = reader.GetInt32(reader.GetOrdinal("TextureX"));
                int y = reader.GetInt32(reader.GetOrdinal("TextureY"));

                return Global.CropTexture(UIManager.gen1SpriteSheet, new Rectangle(x + 63, y - 34, 32, 32)); ;
            }

            return null;
        }

        public static string GetTypeName(int id)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT TypeName FROM Types WHERE TypeID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("TypeName"));

                return name;
            }

            return null;
        }
        public static Color GetTypeColor(int id, bool dropShadow = false)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT TypeColour, TypeDropShadowColour FROM Types WHERE TypeID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string colorHex;

                if(!dropShadow)
                    colorHex = reader.GetString(reader.GetOrdinal("TypeColour"));
                else
                    colorHex = reader.GetString(reader.GetOrdinal("TypeDropShadowColour"));

                uint hex = uint.Parse(colorHex, System.Globalization.NumberStyles.HexNumber);

                // Extract ARGB components
                byte a = (byte)((hex & 0xFF000000) >> 24);
                byte r = (byte)((hex & 0x00FF0000) >> 16);
                byte g = (byte)((hex & 0x0000FF00) >> 8);
                byte b = (byte)(hex & 0x000000FF);

                return new Color(r,g,b,a);
            }

            return Color.HotPink;
        }

        public static MoveInstance GetBaseMoveInstance(int id)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT * FROM Moves WHERE MoveID = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("MoveName"));
                int type = reader.GetInt32(reader.GetOrdinal("MoveType"));
                int damage = reader.GetInt32(reader.GetOrdinal("MoveDamage"));
                int accuracy = reader.GetInt32(reader.GetOrdinal("MoveAccuracy"));
                int pp = reader.GetInt32(reader.GetOrdinal("MovePP"));
                int property = reader.GetInt32(reader.GetOrdinal("MoveProperty"));
                int priority = reader.GetInt32(reader.GetOrdinal("MovePriority"));
                string effect = reader.GetString(reader.GetOrdinal("MoveAction"));
                int affectsUser = reader.GetInt32(reader.GetOrdinal("ActionAffectsPlayer"));

                return new MoveInstance(id, name, damage, accuracy, type, pp, property, priority, effect, affectsUser);
            }

            return null;
        }

        public static List<MoveInstance> GetPokemonPriorMoves(int id, int level)
        {
            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";
            List<MoveInstance> moves = new List<MoveInstance>();

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT MoveID, LearnLevel FROM Movesets WHERE PokeID = @id AND LearnMethod = 'LEVEL' AND LearnLevel <= @level ORDER BY LearnLevel ASC;";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@level", level);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int moveId = reader.GetInt32(0);
                int learnLevel = reader.GetInt32(1);

                var moveData = GetBaseMoveInstance(moveId);

                moves.Add(moveData);
            }

            return moves;
        }

        public static float GetTypeEffectiveness(int attackType, int defType1, int defType2 = -1)
        {
            float multiplier = 1;

            dbRelativePath = @"..\..\..\..\PokemonDatabase.db";

            using var connection = new SqliteConnection($"Data Source={dbRelativePath}");
            connection.Open();

            string query = "SELECT Multiplier FROM TypeEffectiveness WHERE AttackTypeID = @attackid AND DefenseTypeID = @defendid";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@attackid", attackType);
            command.Parameters.AddWithValue("@defendid", defType1);

            using var reader1 = command.ExecuteReader();

            if (reader1.Read())
            {
                float effect = reader1.GetFloat(reader1.GetOrdinal("Multiplier"));

                multiplier *= effect;
            }

            if (defType2 != -1)
            {
                query = "SELECT Multiplier FROM TypeEffectiveness WHERE AttackTypeID = @attackid AND DefenseTypeID = @defendid";

                using var command1 = new SqliteCommand(query, connection);
                command1.Parameters.AddWithValue("@attackid", attackType);
                command1.Parameters.AddWithValue("@defendid", defType2);

                using var reader2 = command1.ExecuteReader();

                if (reader2.Read())
                {
                    float effect = reader2.GetFloat(reader2.GetOrdinal("Multiplier"));

                    multiplier *= effect;
                }
            }

            return multiplier;
        }
    }
    public class MoveInstance
    {
        public int id;
        public string name;

        public int damage;
        public int accuracy;

        public int maxPP;
        public int currentPP;

        public int typeID;
        public int property;

        public string effectKey;
        public bool affectsUser;

        public int priority;

        public MoveInstance(int id, string name, int damage, int accuracy, int type, int pp, int property, int priority, string effect = "", int userAffected = 1)
        {
            this.id = id;
            this.name = name;
            this.damage = damage;
            this.accuracy = accuracy;
            this.typeID = type;
            this.maxPP = pp;
            this.property = property;
            this.priority = priority;
            currentPP = pp;

            effectKey = effect;

            affectsUser = userAffected == 1? true : false;
        }
    }
}
