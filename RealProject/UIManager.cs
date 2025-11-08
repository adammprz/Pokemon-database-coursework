using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
//using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace RealProject
{
    static class UIManager
    {
        static Dictionary<string, UIRect> overworldRects = new Dictionary<string, UIRect>();
        static Dictionary<string, UIRect> bagRects = new Dictionary<string, UIRect>();
        public static Dictionary<string, UIRect> battleRects = new Dictionary<string, UIRect>();

        static Color[] transitionAnimationData;
        static Texture2D screenBuffer;

        public static Texture2D debugSquare;
        public static bool doDebug = false;

        public static Texture2D gen1SpriteSheet;

        public static void Initialize(ContentManager content, Texture2D buffer)
        {
            transitionAnimationData = new Color[Global.screenWidth * Global.screenHeight];
            transitionAnimationData[0] = Color.HotPink;
            screenBuffer = buffer;
            debugSquare = content.Load<Texture2D>("Solid_white");
            gen1SpriteSheet = content.Load<Texture2D>("PokemonSpriteSheet_1");

            overworldRects.Add("Button_Pokemon",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(18, 212, 76, 24)), -31 * Global.pixelsPerUnit, 13 * Global.pixelsPerUnit, null, new Vector2(76, 24) * Global.pixelsPerUnit)); // Pokemon
            overworldRects.Add("Button_Bag",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(18, 244, 76, 24)), -31 * Global.pixelsPerUnit, 40 * Global.pixelsPerUnit, SwitchBagMenu, new Vector2(76, 24) * Global.pixelsPerUnit, overworldRects["Button_Pokemon"])); // Bag
            overworldRects.Add("Button_Pokedex",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(18, 276, 76, 24)), -31 * Global.pixelsPerUnit, 67 * Global.pixelsPerUnit, null, new Vector2(76, 24) * Global.pixelsPerUnit, overworldRects["Button_Pokemon"])); // Pokedex
            overworldRects.Add("Button_Save",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(18, 308, 76, 24)), -31 * Global.pixelsPerUnit, 94 * Global.pixelsPerUnit, null, new Vector2(76, 24) * Global.pixelsPerUnit, overworldRects["Button_Pokemon"])); // Save




            bagRects.Add("Image_Bag",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(112, 192, 320, 180)), Global.screenWidth / 2, Global.screenHeight / 2)); // Bag
            bagRects.Add("Button_Close",
                new UIButton(null, 308 * Global.pixelsPerUnit, 10 * Global.pixelsPerUnit, SwitchBagMenu, new Vector2(2, 2) * Global.pixelsPerUnit, bagRects["Image_Bag"])); // Bag Back

            bagRects["Image_Bag"].SetEnabled(false);






            battleRects.Add("Image_Background",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleBackgrounds"), new Rectangle(6, 6, 320, 180)), Global.screenWidth / 2, Global.screenHeight / 2)); // Background

            battleRects.Add("Image_PlayerPokemon",
                new UIRect(Global.CropTexture(gen1SpriteSheet, new Rectangle(596, 1101, 64, 64)), 95 * Global.pixelsPerUnit, 100 * Global.pixelsPerUnit, battleRects["Image_Background"]));

            battleRects.Add("Image_EnemyPokemon",
                new UIRect(Global.CropTexture(gen1SpriteSheet, new Rectangle(661, 46, 64, 64)), 230 * Global.pixelsPerUnit, 46 * Global.pixelsPerUnit, battleRects["Image_Background"]));

            battleRects.Add("Image_BottomEmptyBar",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(297, 278, 320, 48)), Global.screenWidth / 2, Global.screenHeight * 0.875f - 9, battleRects["Image_Background"])); // BattleMenu
            battleRects.Add("Image_BattleMenu",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(297, 56, 320, 48)), Global.screenWidth / 2, Global.screenHeight * 0.875f-9, battleRects["Image_Background"])); // BattleMenu
            battleRects.Add("Image_AttackMenu",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(297, 4, 320, 48)), Global.screenWidth / 2, Global.screenHeight * 0.875f - 9, battleRects["Image_Background"])); // BattleMenu

            battleRects.Add("Button_Fight",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 222 * Global.pixelsPerUnit, 147.5f * Global.pixelsPerUnit, PokeBattleManager.ShowAttacks, new Vector2(45, 12) * Global.pixelsPerUnit, battleRects["Image_BattleMenu"])); // Fight
            battleRects.Add("Button_Bag", 
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 283 * Global.pixelsPerUnit, 147.5f * Global.pixelsPerUnit, null, new Vector2(45, 12) * Global.pixelsPerUnit, battleRects["Image_BattleMenu"])); // Bag
            battleRects.Add("Button_Pokemon", 
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 222 * Global.pixelsPerUnit, 163.5f * Global.pixelsPerUnit, null, new Vector2(45, 12) * Global.pixelsPerUnit, battleRects["Image_BattleMenu"])); // Pokemon
            battleRects.Add("Button_Run", 
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 283 * Global.pixelsPerUnit, 163.5f * Global.pixelsPerUnit, PokeBattleManager.OnRunChosen, new Vector2(45, 12) * Global.pixelsPerUnit, battleRects["Image_BattleMenu"])); // Run
            battleRects.Add("Image_Selected", 
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(200, 16, 47, 14)), 1329, 900, battleRects["Image_BattleMenu"])); // Select Image

            battleRects.Add("Button_Attack1",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 51 * Global.pixelsPerUnit, 147 * Global.pixelsPerUnit, () => CoroutineManager.Start(PokeBattleManager.OnAttackChosen(0)), new Vector2(76, 14) * Global.pixelsPerUnit, battleRects["Image_AttackMenu"]));
            battleRects.Add("Button_Attack2",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 155 * Global.pixelsPerUnit, 147 * Global.pixelsPerUnit, () => CoroutineManager.Start(PokeBattleManager.OnAttackChosen(1)), new Vector2(76, 14) * Global.pixelsPerUnit, battleRects["Image_AttackMenu"]));
            battleRects.Add("Button_Attack3",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 51 * Global.pixelsPerUnit, 164 * Global.pixelsPerUnit, () => CoroutineManager.Start(PokeBattleManager.OnAttackChosen(2)), new Vector2(76, 14) * Global.pixelsPerUnit, battleRects["Image_AttackMenu"]));
            battleRects.Add("Button_Attack4",
                new UIButton(Global.CropTexture(content.Load<Texture2D>("menu1"), new Rectangle(0, 0, 47, 14)), 155 * Global.pixelsPerUnit, 164 * Global.pixelsPerUnit, () => CoroutineManager.Start(PokeBattleManager.OnAttackChosen(3)), new Vector2(76, 14) * Global.pixelsPerUnit, battleRects["Image_AttackMenu"]));

            battleRects.Add("Text_Attack1",
                new UITextBox(20 * Global.pixelsPerUnit, 139 * Global.pixelsPerUnit, "Attack_1", new Color(72, 72, 72), 0, true, new Color(208, 208, 200), battleRects["Button_Attack1"]));
            battleRects.Add("Text_Attack2",
                new UITextBox(124 * Global.pixelsPerUnit, 139 * Global.pixelsPerUnit, "Attack_2", new Color(72, 72, 72), 0, true, new Color(208, 208, 200), battleRects["Button_Attack2"]));
            battleRects.Add("Text_Attack3",
                new UITextBox(20 * Global.pixelsPerUnit, 154 * Global.pixelsPerUnit, "Attack_3", new Color(72, 72, 72), 0, true, new Color(208, 208, 200), battleRects["Button_Attack3"]));
            battleRects.Add("Text_Attack4",
                new UITextBox(124 * Global.pixelsPerUnit, 154 * Global.pixelsPerUnit, "Attack_4", new Color(72, 72, 72), 0, true, new Color(208, 208, 200), battleRects["Button_Attack4"]));

            battleRects.Add("Image_AttackSelect",
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(269, 4, 6, 10)), 3000, 900, battleRects["Image_AttackMenu"])); // Select Image

            battleRects.Add("Text_AttackType",
                new UITextBox(271 * Global.pixelsPerUnit, 157 * Global.pixelsPerUnit, "Blank", new Color(72, 72, 72), 0, true, new Color(208, 208, 200), battleRects["Image_AttackMenu"]));
            battleRects.Add("Text_PP",
                new UITextBox(312 * Global.pixelsPerUnit, 141 * Global.pixelsPerUnit, "0/99", new Color(32, 32, 32), 2, true, new Color(216, 216, 216), battleRects["Image_AttackMenu"]));

            battleRects.Add("Image_PlayerPokeDisplay", 
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(3, 42, 129, 39)), 253 * Global.pixelsPerUnit, 111.5f * Global.pixelsPerUnit, battleRects["Image_Background"])); // Your pokestats
            battleRects.Add("Image_EnemyPokeDisplay", 
                new UIRect(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(3, 3, 125, 29)), 75 * Global.pixelsPerUnit, 39.5f * Global.pixelsPerUnit, battleRects["Image_Background"])); // Enemy pokestats

            battleRects.Add("Text_PlayerName",
                new UITextBox(202.5f * Global.pixelsPerUnit, 93f * Global.pixelsPerUnit, "NAME", new Color(64, 64, 64), 0, true, new Color(216, 208, 176), battleRects["Image_PlayerPokeDisplay"]));
            battleRects.Add("Text_PlayerHealth", 
                new UITextBox(309.5f * Global.pixelsPerUnit, 112 * Global.pixelsPerUnit, "0/0", new Color(72, 72, 72), 2, true, new Color(216, 208, 176), battleRects["Image_PlayerPokeDisplay"]));
            battleRects.Add("Slider_PlayerHealth", 
                new UISlider(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(167, 9, 3, 3)), 277 * Global.pixelsPerUnit, 110.5f * Global.pixelsPerUnit, 
                0, 22, 22, new Vector2(65*Global.pixelsPerUnit, 3*Global.pixelsPerUnit), Color.LawnGreen, false, true, Color.Red, battleRects["Image_PlayerPokeDisplay"])); // Your healthslider
            battleRects.Add("Slider_PlayerExp",
                new UISlider(debugSquare, 265 * Global.pixelsPerUnit, 128 * Global.pixelsPerUnit,
                0, 88, 0, new Vector2(89 * Global.pixelsPerUnit, 2 * Global.pixelsPerUnit), new Color(44, 180, 228), false, false, Color.OrangeRed, battleRects["Image_PlayerPokeDisplay"])); // Your healthslider
            battleRects.Add("Text_PlayerLevel",
                new UITextBox(309.5f * Global.pixelsPerUnit, 93 * Global.pixelsPerUnit, "Lv100", new Color(72, 72, 72), 2, true, new Color(216, 208, 176), battleRects["Image_PlayerPokeDisplay"]));

            battleRects.Add("Text_EnemyName",
                new UITextBox(17.5f * Global.pixelsPerUnit, 26f * Global.pixelsPerUnit, "NAME", new Color(64, 64, 64), 0, true, new Color(216, 208, 176), battleRects["Image_EnemyPokeDisplay"]));
            battleRects.Add("Slider_EnemyHealth",
                new UISlider(Global.CropTexture(content.Load<Texture2D>("battleMenu"), new Rectangle(167, 9, 3, 3)), 92 * Global.pixelsPerUnit, 43.5f * Global.pixelsPerUnit,
                0, 22, 22, new Vector2(65 * Global.pixelsPerUnit, 3 * Global.pixelsPerUnit), Color.LawnGreen, false, true, Color.Red, battleRects["Image_EnemyPokeDisplay"])); // Your healthslider
            battleRects.Add("Text_EnemyLevel",
                new UITextBox(124.5f * Global.pixelsPerUnit, 26 * Global.pixelsPerUnit, "Lv100", new Color(72, 72, 72), 2, true, new Color(216, 208, 176), battleRects["Image_EnemyPokeDisplay"]));

            ((UISlider)battleRects["Slider_PlayerHealth"]).SetAction(() => PokeBattleManager.ChangeHPText("Text_PlayerHealth", "Slider_PlayerHealth"));

            battleRects["Image_Background"].SetEnabled(false);
        }

        public static void Update()
        {
            DoButtonMoveAnimation((UIButton)overworldRects["Button_Pokemon"]);
            DoButtonMoveAnimation((UIButton)overworldRects["Button_Bag"]);
            DoButtonMoveAnimation((UIButton)overworldRects["Button_Pokedex"]);
            DoButtonMoveAnimation((UIButton)overworldRects["Button_Save"]);

            UpdateAllUI();

            if (InputManager.GetKeyDown(Keys.Q))
                CoroutineManager.Start(DoTransitionAnimation());

            if (battleRects["Image_BattleMenu"].enabled)
            {
                UIButton a = (UIButton)battleRects["Button_Fight"];
                UIButton b = (UIButton)battleRects["Button_Bag"];
                UIButton c = (UIButton)battleRects["Button_Pokemon"];
                UIButton d = (UIButton)battleRects["Button_Run"];

                if (a.CheckHover())
                    battleRects["Image_Selected"].SetPosition(a.pos + Vector2.UnitY * 1 * Global.pixelsPerUnit);
                else if (b.CheckHover())
                    battleRects["Image_Selected"].SetPosition(b.pos + Vector2.UnitY * 1 * Global.pixelsPerUnit);
                else if (c.CheckHover())
                    battleRects["Image_Selected"].SetPosition(c.pos + Vector2.UnitY * 1 * Global.pixelsPerUnit);
                else if (d.CheckHover())
                    battleRects["Image_Selected"].SetPosition(d.pos + Vector2.UnitY * 1 * Global.pixelsPerUnit);
            }
            if (battleRects["Image_AttackMenu"].enabled)
            {
                UIButton a = (UIButton)battleRects["Button_Attack1"];
                UIButton b = (UIButton)battleRects["Button_Attack2"];
                UIButton c = (UIButton)battleRects["Button_Attack3"];
                UIButton d = (UIButton)battleRects["Button_Attack4"];

                PokemonInstance instance = PokeBattleManager.currentBattle.playerTeam[PokeBattleManager.currentBattle.playerTeamIndex];

                if (a.CheckHover())
                {
                    battleRects["Image_AttackSelect"].SetPosition(a.pos - Vector2.UnitX * 38 * Global.pixelsPerUnit);
                    ((UITextBox)battleRects["Text_AttackType"]).SetText(DatabaseManager.GetTypeName(instance.moveset[0].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetColor(DatabaseManager.GetTypeColor(instance.moveset[0].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetDropShadowColor(DatabaseManager.GetTypeColor(instance.moveset[0].typeID, true));

                    ((UITextBox)battleRects["Text_PP"]).SetText($"{instance.moveset[0].currentPP}/{instance.moveset[0].maxPP}");
                }
                else if (b.CheckHover())
                {
                    battleRects["Image_AttackSelect"].SetPosition(b.pos - Vector2.UnitX * 38 * Global.pixelsPerUnit);
                    ((UITextBox)battleRects["Text_AttackType"]).SetText(DatabaseManager.GetTypeName(instance.moveset[1].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetColor(DatabaseManager.GetTypeColor(instance.moveset[1].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetDropShadowColor(DatabaseManager.GetTypeColor(instance.moveset[1].typeID, true));

                    ((UITextBox)battleRects["Text_PP"]).SetText($"{instance.moveset[1].currentPP}/{instance.moveset[1].maxPP}");
                }
                else if (c.CheckHover())
                {
                    battleRects["Image_AttackSelect"].SetPosition(c.pos - Vector2.UnitX * 38 * Global.pixelsPerUnit);
                    ((UITextBox)battleRects["Text_AttackType"]).SetText(DatabaseManager.GetTypeName(instance.moveset[2].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetColor(DatabaseManager.GetTypeColor(instance.moveset[2].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetDropShadowColor(DatabaseManager.GetTypeColor(instance.moveset[2].typeID, true));

                    ((UITextBox)battleRects["Text_PP"]).SetText($"{instance.moveset[2].currentPP}/{instance.moveset[2].maxPP}");
                }
                else if (d.CheckHover())
                {
                    battleRects["Image_AttackSelect"].SetPosition(d.pos - Vector2.UnitX * 38 * Global.pixelsPerUnit);
                    ((UITextBox)battleRects["Text_AttackType"]).SetText(DatabaseManager.GetTypeName(instance.moveset[3].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetColor(DatabaseManager.GetTypeColor(instance.moveset[3].typeID));
                    ((UITextBox)battleRects["Text_AttackType"]).SetDropShadowColor(DatabaseManager.GetTypeColor(instance.moveset[3].typeID, true));

                    ((UITextBox)battleRects["Text_PP"]).SetText($"{instance.moveset[3].currentPP}/{instance.moveset[3].maxPP}");
                }
            }

            if (InputManager.GetMouseButton(2))
            {
                UISlider s = (UISlider)battleRects["Slider_PlayerHealth"];
                s.SetValue(s.rawValue - Global.deltaTime * 5);
            }
        }

        static void UpdateAllUI()
        {
            foreach (var rect in overworldRects.Values)
            {
                if (rect.enabled)
                    rect.Update();
            }

            foreach (var rect in bagRects.Values)
            {
                if (rect.enabled)
                    rect.Update();
            }

            foreach (var rect in battleRects.Values)
            {
                if (rect.enabled)
                    rect.Update();
            }
        }

        static void DoButtonMoveAnimation(UIButton b)
        {
            if (b.CheckHover())
            {
                b.SetPosition(Vector2.Lerp(b.pos, new Vector2(31 * Global.pixelsPerUnit, b.pos.Y), Global.deltaTime * 6));
            }
            else
            {
                b.SetPosition(Vector2.Lerp(b.pos, new Vector2(-31 * Global.pixelsPerUnit, b.pos.Y), Global.deltaTime * 6));
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            DrawAllUI(spriteBatch);

            spriteBatch.DrawString(Global.pokeFont, Global.currentTypeText, Global.currentTypePos + new Vector2(Global.pixelsPerUnit, 0), new Color(50, 50, 50), 0, Vector2.Zero, Global.pixelsPerUnit, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.pokeFont, Global.currentTypeText, Global.currentTypePos + new Vector2(0, Global.pixelsPerUnit), new Color(50, 50, 50), 0, Vector2.Zero, Global.pixelsPerUnit, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.pokeFont, Global.currentTypeText, Global.currentTypePos + new Vector2(Global.pixelsPerUnit, Global.pixelsPerUnit), new Color(50, 50, 50), 0, Vector2.Zero, Global.pixelsPerUnit, SpriteEffects.None, 0);
            spriteBatch.DrawString(Global.pokeFont, Global.currentTypeText, Global.currentTypePos, Color.White, 0, Vector2.Zero, Global.pixelsPerUnit, SpriteEffects.None, 0);

            if (transitionAnimationData[0] != Color.HotPink)
            {
                screenBuffer.SetData(transitionAnimationData);
                spriteBatch.Draw(screenBuffer, Vector2.Zero, Color.White);
            }
        }

        public static void DrawAllUI(SpriteBatch spriteBatch)
        {
            foreach (var rect in overworldRects.Values)
            {
                if (rect.enabled)
                    rect.Draw(spriteBatch);
            }

            foreach (var rect in bagRects.Values)
            {
                if (rect.enabled)
                    rect.Draw(spriteBatch);
            }

            foreach (var rect in battleRects.Values)
            {
                if (rect.enabled)
                    rect.Draw(spriteBatch);
            }
        }

        //public static void Ab()
        //{
        //    if (overworldRects[0] is UIButton)
        //    {
        //        UIButton b = (UIButton)overworldRects[0];
        //        b.RandomPos();
        //    }
        //}

        public static void SwitchBagMenu()
        {
            if (!(GameStateManager.gameState == GameStateManager.GameState.Overworld || GameStateManager.gameState == GameStateManager.GameState.Bag)) return;
            if (bagRects["Image_Bag"].enabled) // Closing
            {
                overworldRects["Button_Pokemon"].SetEnabled(true);
                bagRects["Image_Bag"].SetEnabled(false);
                GameStateManager.ChangeState(GameStateManager.GameState.Overworld);
                Debug.WriteLine("Closing bag.");
            }
            else // Opening
            {
                overworldRects["Button_Pokemon"].SetEnabled(false);
                bagRects["Image_Bag"].SetEnabled(true);
                GameStateManager.ChangeState(GameStateManager.GameState.Bag);
                Debug.WriteLine("Opening bag.");
            }
        }

        public static IEnumerator<object> DoTransitionAnimation(Action onCompleteVoid = null)
        {
            Array.Fill(transitionAnimationData, Color.Transparent);

            int step = Global.pixelsPerUnit;
            int rowCount = Global.screenHeight / step;
            int colCount = Global.screenWidth / step;

            int blocksPerRowPerFrame = 8;

            for (int colStart = 0; colStart < colCount; colStart += blocksPerRowPerFrame)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    for (int i = 0; i < blocksPerRowPerFrame; i++)
                    {
                        int col = colStart + i;
                        if (col >= colCount) continue;

                        int x;
                        if (row % 2 == 0)
                        {
                            x = col * step;
                        }
                        else
                        {
                            x = (colCount - 1 - col) * step;
                        }

                        int y = row * step;
                        Global.FillBlock(transitionAnimationData, x, y, step, step, Color.Black);
                    }
                }

                yield return 0.01f;
            }

            yield return 0.5f;

            transitionAnimationData[0] = Color.HotPink;

            onCompleteVoid?.Invoke();

            yield return 1 / 60f;
        }
    }
}
