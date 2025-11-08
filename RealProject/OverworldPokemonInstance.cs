using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RealProject
{
    public class OverworldPokemonInstance
    {
        public Vector2 position;

        public BoxCollider collider = new BoxCollider(new Vector2(-1f, -1), true, Vector2.One);

        int pokeID;

        Vector2 minMaxTime = new Vector2(3, 8) / 8;
        float timer;

        float tilesPerSecond = 0.5f * 15;

        bool enabled;

        Vector2 minMaxTiles = new Vector2(3, 8);
        int tiles;

        float maxDistanceFromSpawn = 4;
        Vector2 spawnPos;

        Color debugColor;

        public Texture2D sprite;

        PokemonInstance thisPokemon;

        public OverworldPokemonInstance(Vector2 startPos, Texture2D text, int id)
        {
            position = startPos;
            spawnPos = startPos;
            debugColor = new Color((float)Global.random.NextDouble(), (float)Global.random.NextDouble(), (float)Global.random.NextDouble());

            pokeID = id;

            sprite = DatabaseManager.GetOverworldPokeData(id);

            enabled = true;

            collider.TriggerEnter += StartBattle;

            CoroutineManager.Start(DoWanderLoop());
        }

        void StartBattle(Collider col)
        {
            if (enabled)
            {
                if (PokeBattleManager.currentBattle.battleOver == true)
                {
                    if (thisPokemon == null)
                    {
                        PokemonInstance poke = DatabaseManager.GetBasePokemonData(pokeID, 5);
                        poke.RandomisePokemon(Player.pokemonParty);
                        thisPokemon = poke;
                    }

                    PokeBattleManager.BeginBattle(Player.pokemonParty, [thisPokemon], 2, 0);
                }

                Debug.WriteLine("hashash");
            }
        }

        public void SetEnabled(bool enbl)
        {
            if(!enabled && enbl)
            {
                collider.TriggerEnter += StartBattle;
                CoroutineManager.Start(DoWanderLoop());
            } else if(enabled && !enbl)
            {
                collider.TriggerEnter -= StartBattle;
            }

            enabled = enbl;
        }

        IEnumerator<object> DoWanderLoop()
        {
            Vector2 previousDir = Vector2.Zero;

            while(enabled)
            {
                timer = (float)Global.random.NextDouble() * (minMaxTime.Y - minMaxTime.X) + minMaxTime.X;

                yield return timer;

                tiles = Global.random.Next((int)minMaxTiles.X, (int)minMaxTiles.Y + 1);

                for (int i = 0; i < tiles; i ++)
                {
                    if (enabled)
                    {
                        previousDir = GetRandomTileDir(previousDir);
                        Vector2 targetPos = position + previousDir;// + new Vector2(Global.random.Next(-250, 250) * 0.0001f, Global.random.Next(-250, 250) * 0.0001f);

                        yield return CoroutineManager.Start(WalkToPos(targetPos));

                        yield return ((float)Global.random.NextDouble() / 2f);
                    }
                }
            }

            yield break;
        }

        IEnumerator<object> WalkToPos(Vector2 target)
        {
            Vector2 dir = target - position;

            while ((position.X < target.X - 0.2f || position.X > target.X + 0.2f) || (position.Y < target.Y - 0.2f || position.Y > target.Y + 0.2f))
            {
                dir.Normalize();

                position += dir * tilesPerSecond / 60f;

                yield return 1 / 60f;
            }
        }

        Vector2 GetRandomTileDir(Vector2 previousDir)
        {
            int x = 0;
            int y = 0;

            bool tileFound = false;

            while (!tileFound)
            {
                x = 0;
                y = 0;
                //(x == 0 && y == 0) || (x == -previousDir.X && y == -previousDir.Y) || Vector2.Distance(spawnPos, position + new Vector2(x, y)) > maxDistanceFromSpawn
                x = Global.random.Next(-1, 2);

                if(x == 0)
                    y = Global.random.Next(-1, 2);

                if (x == 0 && y == 0)
                    tileFound = false;
                else if (x == -previousDir.X && y == previousDir.Y)
                    tileFound = false;
                else if (Vector2.Distance(spawnPos, position + new Vector2(x, y)) > maxDistanceFromSpawn)
                    tileFound = false;
                else if (ColliderManager.collisionMap[(int)position.Y + y - 1, (int)position.X + x] != ' ')
                {
                    //Debug.WriteLine($"ColliderDetected at {(int)position.Y + y} {(int)position.X + x}");
                    tileFound = false;
                }
                else
                    tileFound = true;
            }

            //Debug.WriteLine($"a: {position + new Vector2(x, y)}");

            return new Vector2(x, y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 posToDraw = Global.WorldToScreenPos(new Vector2(position.X - 1.5f, position.Y - 2f));
            Vector2 pos2 = Global.WorldToScreenPos(position - Vector2.One * 0.5f);
            Vector2 pos3 = Global.WorldToScreenPos(spawnPos - Vector2.One * 0.5f);
            Vector2 size = new Vector2(sprite.Width, sprite.Height) * Global.pixelsPerUnit;

            spriteBatch.Draw(sprite, new Rectangle((int)posToDraw.X, (int)posToDraw.Y, (int)size.X, (int)size.Y), Color.White);
            spriteBatch.Draw(UIManager.debugSquare, new Rectangle((int)pos2.X, (int)pos2.Y, (int)8, (int)8), debugColor);
            spriteBatch.Draw(UIManager.debugSquare, new Rectangle((int)pos3.X, (int)pos3.Y, (int)16, (int)16), debugColor);
        }
    }
}
