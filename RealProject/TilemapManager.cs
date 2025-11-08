using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using SharpDX.Direct2D1;

namespace RealProject
{
    static class TilemapManager
    {
        public static char[,] map = {
            { 'I', 'a', 'b', 'c', ' '},
            { 'd', 'E', 'i', 'j', ' '},
            { 'h', 'i', 'I', 'f', 'g'},
            { 'k', 'l', 'i', 'i', 'j'},
            { ' ', 'o', 'T', 'p', 'n'}};

        public static char[,] extraGroundMap = {
            { 'K', 'L', ' ', ' ', ' '},
            { ' ', 'H', ' ', ' ', ' '},
            { ' ', 'O', 'P', 'P', 'Q'},
            { ' ', ' ', ' ', ' ', ' '},
            { ' ', ' ', ' ', ' ', ' '}};

        static Dictionary<char, Vector2> tileDictionary = new Dictionary<char, Vector2>();

        static Texture2D mapPNG;
        static Texture2D tilemap;

        public static void Initialize(Texture2D t, Texture2D map)
        {
            mapPNG = t;
            tilemap = map;

            InitializeTiles();
            GetTileMapFromImage(mapPNG);
        }

        static void InitializeTiles()
        {
            //Dictionary<char, Vector2> tileDictionary = new Dictionary<char, Vector2>();

            tileDictionary.Add('a', new Vector2(32, 16));
            tileDictionary.Add('b', new Vector2(48, 16));
            tileDictionary.Add('c', new Vector2(64, 16));

            tileDictionary.Add('d', new Vector2(16, 32));
            tileDictionary.Add('e', new Vector2(32, 32));
            tileDictionary.Add('f', new Vector2(64, 32));
            tileDictionary.Add('g', new Vector2(80, 32));

            tileDictionary.Add('h', new Vector2(16, 48));
            tileDictionary.Add('i', new Vector2(48, 48)); // base grass
            tileDictionary.Add('j', new Vector2(80, 48));

            tileDictionary.Add('k', new Vector2(16, 64));
            tileDictionary.Add('l', new Vector2(32, 64));
            tileDictionary.Add('m', new Vector2(64, 64));
            tileDictionary.Add('n', new Vector2(80, 64));

            tileDictionary.Add('o', new Vector2(32, 80));
            tileDictionary.Add('p', new Vector2(48, 80));
            tileDictionary.Add('q', new Vector2(64, 80));



            tileDictionary.Add('A', new Vector2(32, 96));
            tileDictionary.Add('B', new Vector2(48, 96));
            tileDictionary.Add('C', new Vector2(64, 96));

            tileDictionary.Add('D', new Vector2(16, 112));
            tileDictionary.Add('E', new Vector2(32, 112));
            tileDictionary.Add('F', new Vector2(64, 112));
            tileDictionary.Add('G', new Vector2(80, 112));

            tileDictionary.Add('H', new Vector2(16, 128));
            tileDictionary.Add('I', new Vector2(112, 48)); // base sand
            tileDictionary.Add('J', new Vector2(80, 128));

            tileDictionary.Add('K', new Vector2(16, 144));
            tileDictionary.Add('L', new Vector2(32, 144));
            tileDictionary.Add('M', new Vector2(64, 144));
            tileDictionary.Add('N', new Vector2(80, 144));

            tileDictionary.Add('O', new Vector2(32, 160));
            tileDictionary.Add('P', new Vector2(48, 160));
            tileDictionary.Add('Q', new Vector2(64, 160));



            tileDictionary.Add('R', new Vector2(80, 160));
            tileDictionary.Add('S', new Vector2(96, 160));
            tileDictionary.Add('T', new Vector2(112, 160));

            Debug.WriteLine((int)'a');
        }

        static void GetTileMapFromImage(Texture2D mapImg)
        {
            int width = mapImg.Width;
            int height = mapImg.Height;

            map = new char[width, height];
            extraGroundMap = new char[width, height];

            Color[] pixelData = new Color[width * height];
            mapImg.GetData(pixelData);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = pixelData[y * width + x];

                    char steepChar = (char)pixel.R;
                    if (pixel.R == 255)
                        steepChar = ' ';
                    extraGroundMap[y, x] = steepChar;

                    char grassChar = (char)pixel.G;
                    if (pixel.G == 255)
                        grassChar = ' ';
                    map[y, x] = grassChar;

                    //char detailChar = (char)pixel.B;
                    //if (pixel.B == 255)
                    //    detailChar = ' ';
                    //extraGroundMap[y, x] = detailChar;
                }
            }
        }

        public static void OverrideBaseMap(Char c, int x, int y)
        {
            map[y, x] = c;
        }

        public static void OverrideExtrasMap(Char c, int x, int y)
        {
            extraGroundMap[y, x] = c;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Vector2 posOnScreen = Global.WorldToScreenPos(new Vector2(j, i));

                    if (!(posOnScreen.X + (16 * Global.pixelsPerUnit) < 0 || posOnScreen.X > Global.screenWidth
                        || posOnScreen.Y + (16 * Global.pixelsPerUnit) < 0 || posOnScreen.Y > Global.screenHeight))
                    {
                        if (map[i, j] != ' ')
                        {
                            Vector2 posInTileSet = tileDictionary[map[i, j]];

                            spriteBatch.Draw(tilemap,
                                new Rectangle((int)posOnScreen.X, (int)posOnScreen.Y, 16 * Global.pixelsPerUnit, 16 * Global.pixelsPerUnit), // position and scale (pos, pos, size, size)
                                new Rectangle((int)posInTileSet.X, (int)posInTileSet.Y, 16, 16), // section of tileset (pos, pos, size, size)
                                Color.White);
                        }

                        if (extraGroundMap[i, j] != ' ')
                        {
                            Vector2 posInTileSet = tileDictionary[extraGroundMap[i, j]];

                            spriteBatch.Draw(tilemap,
                                new Rectangle((int)posOnScreen.X, (int)posOnScreen.Y, 16 * Global.pixelsPerUnit, 16 * Global.pixelsPerUnit), // position and scale (pos, pos, size, size)
                                new Rectangle((int)posInTileSet.X, (int)posInTileSet.Y, 16, 16), // section of tileset (pos, pos, size, size)
                                Color.White);
                        }
                    }
                }
            }
        }
    }
}
