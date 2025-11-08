using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RealProject
{
    
    static class EnvironmentManager
    {
        public static Texture2D square;
        public static float timeOfDay; // 0 = midday, 1 = midnight
        static float dayLength = 64;
        static int timeDirection;
        static int alpha;

        public static List<OverworldPokemonInstance> pokemonInstances = new List<OverworldPokemonInstance>();

        public enum DayTime
        {
            Day,
            DuskDawn,
            Night
        }

        public static DayTime dayTime;

        public static void Initialize(ContentManager content)
        {
            square = content.Load<Texture2D>("Solid_white");
            timeDirection = 1;
            timeOfDay = 0.3f;
            alpha = 0;
            dayTime = DayTime.Day;

            pokemonInstances.Add(new OverworldPokemonInstance(new Vector2(55, 75), content.Load<Texture2D>("Solid_white"), 1));
            //e = new Environment(0);
        }

        public static void Update()
        {
            timeOfDay += Global.deltaTime * timeDirection / dayLength;

            //if(InputManager.GetMouseButton(1))
            //    timeOfDay += Global.deltaTime * timeDirection / dayLength;

            if (timeOfDay >= 1)
                timeDirection = -1;
            else if(timeOfDay <= 0)
                timeDirection = 1;

            timeOfDay = MathHelper.Clamp(timeOfDay, 0, 1);

            //Debug.WriteLine(timeOfDay);

            SetAlpha();

            if (InputManager.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Z))
                InstantiateRandomPokeInstance();
        }

        static void InstantiateRandomPokeInstance()
        {
            OverworldPokemonInstance poke = Environment.GetRandomPokemonFromPool(Player.currentPlayerEnvironmentIndex);

            if (poke != null)
                pokemonInstances.Add(poke);
            else
                Debug.WriteLine("Error when instantiating random pokemon - random integer outside of available chances.");
        }

        public static void ChangeInstancesEnable(bool enabled)
        {
            foreach(var v in pokemonInstances)
            {
                v.SetEnabled(enabled);
            }
        }

        static void SetAlpha()
        {
            if (timeOfDay < 0.05f)
            {
                alpha = 1;
                dayTime = DayTime.Night;
            }
            if (timeOfDay < 0.2f) // Midnight to dawn
            {
                alpha = (int)MathHelper.Lerp(188, 100, (timeOfDay - 0.05f) / 0.15f);
                dayTime = DayTime.Night;
            }
            else if (timeOfDay < 0.3f) // Dawn to day
            {
                alpha = (int)MathHelper.Lerp(100, 0, (timeOfDay - 0.2f) / 0.10f);
                dayTime = DayTime.DuskDawn;
            }
            else if (timeOfDay <= 0.7f) // Day
            {
                alpha = 0;
                dayTime = DayTime.Day;
            }
            else if (timeOfDay <= 0.8f) // Day to dusk
            {
                alpha = (int)MathHelper.Lerp(0, 100, (timeOfDay - 0.7f) / 0.10f);
                dayTime = DayTime.DuskDawn;
            }
            else if (timeOfDay < 0.95f) // Dusk to midnight
            {
                alpha = (int)MathHelper.Lerp(100, 188, (timeOfDay - 0.8f) / 0.15f);
                dayTime = DayTime.Night;
            }
            else if (timeOfDay <= 1)
            {
                alpha = 188;
                dayTime = DayTime.Night;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (OverworldPokemonInstance p in pokemonInstances)
            {
                p.Draw(spriteBatch);
            }

            spriteBatch.Draw(square,
                new Rectangle(0, 0, Global.screenWidth, Global.screenHeight),
                new Color(0, 0, 32, alpha));
        }
    }

    public static class Environment
    {
        static string textRelativePath = @"..\..\..\..\Environment.txt";

        public static OverworldPokemonInstance GetRandomPokemonFromPool(int environmentID)
        {
            StreamReader sr = new StreamReader(textRelativePath);

            for (int i = 1; i < environmentID; i++)
            {
                sr.ReadLine();
            }

            int pokeID = 0;
            int randNum = Global.random.Next(0, Int32.Parse(sr.ReadLine().Trim('%')));

            while (randNum >= 0)
            {
                string[] line = sr.ReadLine().Split("-");

                if (line[0][0] == '%')
                    return null;

                line[0] = line[0].Trim(' ');
                line[0] = line[0].Trim('#');
                line[1] = line[1].Trim('%');
                int chance = Int32.Parse(line[1]);
                
                if(randNum <= chance)
                {
                    pokeID = int.Parse(line[0]);
                    randNum = -1;
                }
                else
                {
                    randNum -= chance;
                }
            }

            sr.Close();

            return new OverworldPokemonInstance(new Vector2(60, 70), null, pokeID);
        }
    }
}
