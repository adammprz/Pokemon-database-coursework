using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using SharpDX.Direct2D1;

namespace RealProject
{
    static class ColliderManager
    {
        public static char[,] collisionMap;
        public static Dictionary<char, Collider> colliderDictionary = new Dictionary<char, Collider>();

        static Texture2D mapPNG;
        static SpriteFont font;

        public static void Initialize(Texture2D texture, SpriteFont fnt)
        {
            mapPNG = texture;
            font = fnt;

            InitializeColliders();
            GetTileMapFromImage(mapPNG);
        }
        static void InitializeColliders()
        {
            colliderDictionary.Add('x', new BoxCollider(Vector2.Zero, false, Vector2.One));
            colliderDictionary.Add('b', new BoxCollider(new Vector2(0, 0.2f), false, new Vector2(1, 0.5f)));
            colliderDictionary.Add('T', new BoxCollider(new Vector2(0, -0.2f), false, new Vector2(1, 0.5f)));
            colliderDictionary.Add('l', new BoxCollider(new Vector2(-0.2f, 0), false, new Vector2(0.5f, 1)));
            colliderDictionary.Add('R', new BoxCollider(new Vector2(0.2f, 0), false, new Vector2(0.5f, 1)));
            //colliderDictionary.Add('L', new RightTriangleCollider(Vector2.Zero, false, Vector2.One, 1));
        }

        static void GetTileMapFromImage(Texture2D mapImg)
        {
            int width = mapImg.Width;
            int height = mapImg.Height;

            collisionMap = new char[width, height];

            Color[] pixelData = new Color[width * height];
            mapImg.GetData(pixelData);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixelData[y * width + x];

                    char collisionChar = (char)pixel.B;
                    if (pixel.B == 255)
                        collisionChar = ' ';
                    collisionMap[y, x] = collisionChar;
                }
            }
        }

        public static void OverrideColliderMap(Char c, int x, int y)
        {
            collisionMap[y, x] = c;
        }

        public static void Update()
        {
            if (GameStateManager.gameState != GameStateManager.GameState.Overworld) return;

            CheckPlayerCollision();
        }

        static void CheckPlayerCollision()
        {
            char[,] colMap = collisionMap;
            int x = (int)MathF.Round(Player.playerPos.X);
            x = (int)Math.Clamp(x, 1, colMap.GetLength(0) - 2);
            int y = (int)MathF.Round(Player.playerPos.Y);
            y = (int)Math.Clamp(y, 1, colMap.GetLength(1) - 2);

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (colMap[y + j, x + i] != ' ')
                    {
                        Collider collider = colliderDictionary[colMap[y + j, x + i]];
                        Vector2 tileCenter = new Vector2(x + i, y + j);

                        bool intersecting = collider.CheckPlayerCollision(tileCenter, Player.playerPos, Player.playerCollider.colliderSize, Player.playerCollider.colliderOffset);

                        if (intersecting)
                        {
                            Debug.WriteLine(Player.playerPos);

                            if (collider.isTrigger)
                            {
                                if (!collider.colliding && intersecting)
                                    collider.OnTriggerEnter();
                            } 
                            else
                                Player.playerPos = collider.ResolveCollision(tileCenter, Player.playerPos, Player.playerCollider.colliderSize, Player.playerCollider.colliderOffset);
                        }

                        collider.colliding = intersecting;
                    }
                }
            }

            foreach(OverworldPokemonInstance p in EnvironmentManager.pokemonInstances)
            {
                bool intersecting = p.collider.CheckPlayerCollision(p.position, Player.playerPos, Player.playerCollider.colliderSize, Player.playerCollider.colliderOffset);

                if (intersecting)
                {
                    if (p.collider.isTrigger)
                    {
                        if (!p.collider.colliding && intersecting)
                            p.collider.OnTriggerEnter();
                    } else
                        Player.playerPos = p.collider.ResolveCollision(p.position, Player.playerPos, Player.playerCollider.colliderSize, Player.playerCollider.colliderOffset);
                }

                p.collider.colliding = intersecting;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            int x = (int)MathF.Round(Player.playerPos.X);
            x = (int)Math.Clamp(x, 1, collisionMap.GetLength(0) - 2);
            int y = (int)MathF.Round(Player.playerPos.Y);
            y = (int)Math.Clamp(y, 1, collisionMap.GetLength(1) - 2);
            spriteBatch.DrawString(font, $"{x}, {y}, {Player.playerPos + Player.playerCollider.colliderOffset}", Vector2.Zero, Color.Black, 0f, Vector2.Zero, 4, SpriteEffects.None, 0);
        }
    }
}
