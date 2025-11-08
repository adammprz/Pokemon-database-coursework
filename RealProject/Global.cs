using SharpDX.Direct3D9;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using static RealProject.CoroutineManager;

namespace RealProject
{
    public static class Global
    {
        public static Random random = new Random();

        public static int screenWidth = 1920/2;
        public static int screenHeight = 1080/2;
        public static Vector2 screenCentre;

        public static int pixelsPerUnit = 6*screenWidth/1920;

        public static Vector2 camPos;
        static float camSpeed = 12f;

        public static float battleSpeed = 0.25f;

        static float textSpeed = 0.08f;
        public static string currentTypeText = "";
        public static Vector2 currentTypePos;
        public static bool isTyping = false;
        static CoroutineManager.CoroutineInstance currentTypewriter;

        public static float deltaTime;

        public static GraphicsDevice graphicsDevice;

        public static SpriteFont pokeFont;
        public static SpriteFont smallFont;

        public static void Initialize(GraphicsDevice graphics, ContentManager content)
        {
            graphicsDevice = graphics;
            pokeFont = content.Load<SpriteFont>("Text");
            smallFont = content.Load<SpriteFont>("SmallFont");
            screenCentre = new Vector2(screenWidth/2, screenHeight/2);
        }

        public static Vector2 WorldToScreenPos(Vector2 worldPos)
        {
            Vector2 screenPos = new Vector2(
                (worldPos.X - camPos.X) * 16 * pixelsPerUnit + screenWidth / 2,
                (worldPos.Y - camPos.Y) * 16 * pixelsPerUnit + screenHeight / 2);

            return screenPos;
        }

        public static Vector2 ScreenToWorldPos(Vector2 screenPos)
        {
            Vector2 worldPos = new Vector2(
                (screenPos.X - screenWidth / 2) / (16 * pixelsPerUnit) + camPos.X,
                (screenPos.Y - screenHeight / 2) / (16 * pixelsPerUnit) + camPos.Y);

            return worldPos;
        }

        public static void Update(GameTime gameTime, Vector2 playerPos, Vector2 mousePos)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //if (GameStateManager.gameState != GameStateManager.GameState.Overworld) return;

            Vector2 targetCamPos = Vector2.Lerp(playerPos, Global.ScreenToWorldPos(mousePos), 0.2f);
            camPos = Vector2.Lerp(camPos, targetCamPos, deltaTime * camSpeed);
        }

        public static Texture2D CropTexture(Texture2D sourceTexture, Rectangle sourceRect)
        {
            Color[] data = new Color[sourceRect.Width * sourceRect.Height];

            sourceTexture.GetData(0, sourceRect, data, 0, data.Length);

            Texture2D croppedTexture = new Texture2D(Global.graphicsDevice, sourceRect.Width, sourceRect.Height);
            croppedTexture.SetData(data);

            return croppedTexture;
        }

        public static CoroutineInstance StartTypewriter(string text, Vector2 pos)
        {
            if (currentTypewriter != null)
            {
                CoroutineManager.Stop(currentTypewriter);
                currentTypewriter = null;
            }

            currentTypeText = "";
            var coroutine = DoTypewriter(text, pos);
            currentTypewriter = CoroutineManager.Start(coroutine);
            return currentTypewriter;
        }

        static IEnumerator<object> DoTypewriter(string text, Vector2 pos)
        {
            StringBuilder s = new StringBuilder();
            float speed = textSpeed;
            isTyping = true;
            currentTypePos = pos;

            for (int i = 0; i < text.Length; i++)
            {
                if (InputManager.GetKey(Keys.Space) || InputManager.GetMouseButton(0))
                    speed = textSpeed / 30;
                else
                    speed = textSpeed;

                s.Append(text[i]);
                currentTypeText = s.ToString();
                //Debug.WriteLine(currentTypeText);

                yield return speed;
            }

            isTyping = false;
        }

        public static void FillBlock(Color[] buffer, int startX, int startY, int width, int height, Color color)
        {
            int texWidth = Global.screenWidth;

            for (int y = 0; y < height; y++)
            {
                int row = startY + y;
                for (int x = 0; x < width; x++)
                {
                    int col = startX + x;

                    if (row >= 0 && row < Global.screenHeight && col >= 0 && col < texWidth)
                        buffer[row * texWidth + col] = color;
                }
            }
        }
    }
}
