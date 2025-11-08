using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using SharpDX.Direct2D1.Effects;
//using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace RealProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D image, image2;

        Texture2D screenBuffer;
        Color[] pixelData;

        OverworldPokemonInstance a;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = Global.screenWidth;
            _graphics.PreferredBackBufferHeight = Global.screenHeight;
            screenBuffer = new Texture2D(GraphicsDevice, Global.screenWidth, Global.screenHeight);

            Global.Initialize(GraphicsDevice, this.Content);

            DatabaseManager.Start();

            UIManager.Initialize(this.Content, screenBuffer);

            EnvironmentManager.Initialize(this.Content);

            PokeBattleManager.Initialize(this.Content);

            //Global.StartTypewriter("Morning starchild; the world says hello!", new Vector2(0, Global.screenHeight/2));
  
            TilemapManager.Initialize(this.Content.Load<Texture2D>("test9"), this.Content.Load<Texture2D>("tileset"));
            TilemapManager.OverrideExtrasMap('I', 68, 67);

            Player.Initialize(this.Content.Load<Texture2D>("PlayerSprites"));

            ColliderManager.Initialize(this.Content.Load<Texture2D>("test9"), this.Content.Load<SpriteFont>("File"));
            ColliderManager.OverrideColliderMap('x', 68, 67);

            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            pixelData = new Color[Global.screenWidth * Global.screenHeight];
        }

        protected override void Update(GameTime gameTime)
        {
            Global.Update(gameTime, Player.playerPos, InputManager.GetMousePosition());
            CoroutineManager.Update();
            InputManager.Update();
            UIManager.Update();
            EnvironmentManager.Update();
            PokeBattleManager.Update();
            ColliderManager.Update();

            Player.Update();

            if (InputManager.GetKeyDown(Keys.Escape))
                if(GameStateManager.gameState == GameStateManager.GameState.Menu)
                    GameStateManager.ChangeState(GameStateManager.GameState.Overworld);//Exit();
                else if(GameStateManager.gameState == GameStateManager.GameState.Overworld)
                    GameStateManager.ChangeState(GameStateManager.GameState.Menu);

            if (InputManager.GetKeyDown(Keys.NumPad1))
                Exit();


            if (InputManager.GetKeyDown(Keys.P))
            {
                PokemonInstance enemyTreekoTest = DatabaseManager.GetBasePokemonData(1, 12);
                enemyTreekoTest.RandomisePokemon(Player.pokemonParty);
                //enemyTreekoTest.currentHealth = 4;
                //enemyTreekoTest.statusCondition = new StatusEffects(PrimaryStatusConditions.Sleep, Global.random.Next(1, 4));
                enemyTreekoTest.statusCondition = new StatusEffects(PrimaryStatusConditions.Freeze);
                //enemyTreekoTest.volatileStatuses.Add(new StatusEffects(Global.random.Next(2, 6), VolatileStatusConditions.Confusion));
                //enemyTreekoTest.speStat = 5;

                PokeBattleManager.BeginBattle(
                    Player.pokemonParty,
                    [enemyTreekoTest],
                    0,
                    2);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(83, 105, 255));
            Array.Fill(pixelData, Color.White);

            screenBuffer.SetData(pixelData);

            _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.NonPremultiplied,
            SamplerState.PointClamp,
            null, null, null, null);

                TilemapManager.Draw(_spriteBatch);

                Player.Draw(_spriteBatch);

                ColliderManager.Draw(_spriteBatch);

                EnvironmentManager.Draw(_spriteBatch);

                UIManager.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
