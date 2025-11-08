using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using SharpDX.Direct2D1;

namespace RealProject
{
    static class Player
    {
        public static Vector2 playerPos;
        static Texture2D playerSheet;
        static Texture2D playerTexture;

        public static BoxCollider playerCollider;

        static float playerWalkSpeed = 1.3f;
        static float playerSprintSpeed = 3f;
        static bool isSprinting = false;

        static string facing;

        static Vector2 playerMove = Vector2.Zero;

        static AnimationManager animManager;

        public static PokemonInstance[] pokemonParty = new PokemonInstance[6];

        public static int currentPlayerEnvironmentIndex = 0;

        enum PlayerStates
        {
            Idle,
            Walk,
            Run
        }

        static PlayerStates playerState;

        public static void Initialize(Texture2D playerTexture)
        {
            playerSheet = playerTexture;

            playerPos = Vector2.One * 66.625f;

            playerCollider = new BoxCollider(new Vector2(-0.5f, 0.75f), false, new Vector2(0.5f, 0.5f));

            facing = "Down";

            animManager = new AnimationManager();

            List<Rectangle> bounds = new List<Rectangle>();
            Animation anim;

            //----------------------

            InitializePokemonParty();

            //----------------------

            bounds.Add(new Rectangle(25, 42, 16, 32));
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Idle_Down", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(25, 75, 16, 32));
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Idle_Up", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(25, 108, 16, 32));
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Idle_Left", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(25, 141, 16, 32));
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Idle_Right", anim);

            //----------------------

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(8, 42, 16, 32)); bounds.Add(new Rectangle(25, 42, 16, 32)); bounds.Add(new Rectangle(42, 42, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.3f);
            animManager.animations.Add("Walk_Down", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(8, 75, 16, 32)); bounds.Add(new Rectangle(25, 75, 16, 32)); bounds.Add(new Rectangle(42, 75, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.3f);
            animManager.animations.Add("Walk_Up", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(8, 108, 16, 32)); bounds.Add(new Rectangle(25, 108, 16, 32)); bounds.Add(new Rectangle(42, 108, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.3f);
            animManager.animations.Add("Walk_Left", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(8, 141, 16, 32)); bounds.Add(new Rectangle(25, 141, 16, 32)); bounds.Add(new Rectangle(42, 141, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.3f);
            animManager.animations.Add("Walk_Right", anim);

            //----------------------

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(68, 42, 16, 32)); bounds.Add(new Rectangle(85, 42, 16, 32)); bounds.Add(new Rectangle(102, 42, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Run_Down", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(68, 75, 16, 32)); bounds.Add(new Rectangle(85, 75, 16, 32)); bounds.Add(new Rectangle(102, 75, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Run_Up", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(68, 108, 16, 32)); bounds.Add(new Rectangle(85, 108, 16, 32)); bounds.Add(new Rectangle(102, 108, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Run_Left", anim);

            bounds = new List<Rectangle>();
            bounds.Add(new Rectangle(68, 141, 16, 32)); bounds.Add(new Rectangle(85, 141, 16, 32)); bounds.Add(new Rectangle(102, 141, 16, 32)); bounds.Add(bounds[1]);
            anim = new Animation(bounds, playerSheet, 0.1f);
            animManager.animations.Add("Run_Right", anim);

            animManager.Play("Walk_Down");
        }

        static void InitializePokemonParty()
        {
            PokemonInstance testTreeko = DatabaseManager.GetBasePokemonData(46, 7, true);
            testTreeko.RandomisePokemon([DatabaseManager.GetBasePokemonData(46, 1)]);
            testTreeko.name = "Tester";

            pokemonParty[0] = testTreeko;
        }

        public static void Update()
        {
            GetSprinting();

            GetWASD();

            if (GameStateManager.gameState == GameStateManager.GameState.Overworld)
            {

                ExtraDebug();

                MovePlayer();
            }

            SetPlayerAnimation();

            animManager.Update();
            playerTexture = animManager.GetFrameTexture();
        }

        static void SetPlayerAnimation()
        {
            string a = playerState.ToString();

            if(playerMove != Vector2.Zero)
            {
                if (playerMove.Y > 0)
                    facing = "Down";
                else if (playerMove.Y < 0)
                    facing = "Up";
                else if (playerMove.X > 0)
                    facing = "Right";
                else if (playerMove.X < 0)
                    facing = "Left";
            }

            animManager.Play($"{a}_{facing}");
        }

        static void GetWASD()
        {
            playerMove = Vector2.Zero;

            if (InputManager.GetKey(Keys.D))
                playerMove.X = 1;
            else if (InputManager.GetKey(Keys.A))
                playerMove.X = -1;

            if (InputManager.GetKey(Keys.W))
                playerMove.Y = -1;
            else if (InputManager.GetKey(Keys.S))
                playerMove.Y = 1;

            if (playerMove != Vector2.Zero)
                playerMove.Normalize();
            else
                playerState = PlayerStates.Idle;
        }

        static void GetSprinting()
        {
            if (InputManager.GetKey(Keys.LeftShift))
            {
                isSprinting = true;
                playerState = PlayerStates.Run;
            }
            else
            {
                isSprinting = false;
                playerState = PlayerStates.Walk;
            }
        }

        static void ExtraDebug()
        {
            if (InputManager.GetMouseButtonDown(1))
                playerPos = Global.ScreenToWorldPos(InputManager.GetMousePosition());

            if (InputManager.GetKeyDown(Keys.X))
            {
                if (InputManager.GetKey(Keys.C))
                    playerPos = new Vector2(MathF.Round(playerPos.X) + playerCollider.colliderOffset.X, playerPos.Y);
                else
                    playerPos = new Vector2(MathF.Round(playerPos.X), playerPos.Y);
            }
            if (InputManager.GetKeyDown(Keys.Y))
            {
                if (InputManager.GetKey(Keys.C))
                    playerPos = new Vector2(playerPos.X, MathF.Round(playerPos.Y) - playerCollider.colliderOffset.Y);
                else
                    playerPos = new Vector2(playerPos.X, MathF.Round(playerPos.Y));
            }
        }

        static void MovePlayer()
        {
            float speed = isSprinting ? playerSprintSpeed : playerWalkSpeed;

            playerPos.X += playerMove.X * speed * 0.25f * Global.pixelsPerUnit * 1920 / Global.screenWidth * Global.deltaTime;
            playerPos.Y += playerMove.Y * speed * 0.25f * Global.pixelsPerUnit * 1920 / Global.screenWidth * Global.deltaTime;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Vector2 playerPosToDraw = Global.WorldToScreenPos(new Vector2(playerPos.X - 0.5f , playerPos.Y - 0.5f));
            spriteBatch.Draw(playerTexture,
                new Rectangle((int)playerPosToDraw.X, (int)playerPosToDraw.Y, playerTexture.Width * Global.pixelsPerUnit, playerTexture.Height * Global.pixelsPerUnit),
                Color.White);

            DrawCollider();
        }

        static void DrawCollider()
        {
            Vector2 colliderPos = Global.WorldToScreenPos(new Vector2(playerPos.X - 0.35f, playerPos.Y + 0.75f));

            //_spriteBatch.Draw(tmManager., 
            //    new Rectangle((int)colliderPos.X, (int)colliderPos.Y, 8 * Global.pixelsPerUnit, 4 * Global.pixelsPerUnit),
            //    new Rectangle((int)tileDictionary['I'].X, (int)tileDictionary['I'].Y, 16, 16),
            //    new Color(255, 0, 0, 128));
        }
    }
}
