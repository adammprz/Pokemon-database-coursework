using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static RealProject.CoroutineManager;

namespace RealProject
{
    static class PokeBattleManager
    {
        static Dictionary<int, Vector2> battleBackgroundsDic;
        static Dictionary<string, Func<bool, IEnumerator<object>>> moveActions = new Dictionary<string, Func<bool, IEnumerator<object>>>();
        static Texture2D battleBackgroundsSprite;
        static int currentArea = 0;
        public static PokeBattleInstance currentBattle;

        static Action backAction;

        enum BattleStates
        {
            BattleStart,
            PlayerTurn,
            DoingTurn,
            Win,
            Loss,
            Ran
        }

        static BattleStates state;

        public static void Initialize(ContentManager content)
        {
            battleBackgroundsDic = new Dictionary<int, Vector2>();

            battleBackgroundsDic.Add(0, new Vector2(6, 6));
            battleBackgroundsDic.Add(1, new Vector2(329, 6));
            battleBackgroundsDic.Add(2, new Vector2(652, 6));

            battleBackgroundsDic.Add(3, new Vector2(6, 141));
            battleBackgroundsDic.Add(4, new Vector2(329, 141));
            battleBackgroundsDic.Add(5, new Vector2(652, 141));

            battleBackgroundsDic.Add(6, new Vector2(6, 276));
            battleBackgroundsDic.Add(7, new Vector2(329, 276));
            battleBackgroundsDic.Add(8, new Vector2(652, 276));

            battleBackgroundsDic.Add(9, new Vector2(6, 411));

            battleBackgroundsSprite = content.Load<Texture2D>("battleBackgrounds");

            currentBattle = new PokeBattleInstance(Player.pokemonParty, null, false, 0);
            currentBattle.battleOver = true;

            InitializeMoveActions();
        }

        static void InitializeMoveActions()
        {
            moveActions.Add($"Debuff_Atk1", (isPlayer) => ApplyStatChange(isPlayer, 0, -1));
            moveActions.Add($"Debuff_Atk2", (isPlayer) => ApplyStatChange(isPlayer, 0, -2));
            moveActions.Add($"Debuff_Atk3", (isPlayer) => ApplyStatChange(isPlayer, 0, -3));
            moveActions.Add($"Buff_Atk1", (isPlayer) => ApplyStatChange(isPlayer, 0, 1));
            moveActions.Add($"Buff_Atk2", (isPlayer) => ApplyStatChange(isPlayer, 0, 2));
            moveActions.Add($"Buff_Atk3", (isPlayer) => ApplyStatChange(isPlayer, 0, 3));

            moveActions.Add($"DrainEffect", (isPlayer) => AbsorbEffect(isPlayer));
        }

        public static void Update()
        {
            if (InputManager.GetKeyDown(Keys.Escape) || InputManager.GetMouseButtonDown(4))
            {
                if (state == BattleStates.PlayerTurn)
                {
                    backAction?.Invoke();
                    //ShowOptions();
                }
            }
        }

        public static void BeginBattle(PokemonInstance[] party, PokemonInstance[] enemyTeam, int enemyAi, int background)
        {
            currentBattle = new PokeBattleInstance(party, enemyTeam, true, enemyAi);
            currentArea = background;
            state = BattleStates.BattleStart;

            GameStateManager.ChangeState(GameStateManager.GameState.Battle);
            EnvironmentManager.ChangeInstancesEnable(false);
            CoroutineManager.Start(IntroSequence());
        }

        static IEnumerator<object> IntroSequence()
        {
            yield return CoroutineManager.Start(UIManager.DoTransitionAnimation(null));

            Vector2 pos = battleBackgroundsDic[currentArea];
            UIManager.battleRects["Image_Background"].SetSprite(Global.CropTexture(battleBackgroundsSprite, new Rectangle((int)pos.X, (int)pos.Y, 320, 180)));
            UIManager.battleRects["Image_Background"].SetEnabled(true);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);
            UIManager.battleRects["Image_PlayerPokemon"].SetEnabled(false);
            UIManager.battleRects["Image_EnemyPokemon"].SetEnabled(false);

            if(currentBattle.isWildBattle)
                yield return Global.StartTypewriter($"A  Wild  {currentBattle.enemyTeam[0].name.ToUpper()}  appears!", currentBattle.battleTextPos);
            else
                yield return Global.StartTypewriter("TRAINER  {Something}  would  like  to  battle!", currentBattle.battleTextPos);

            yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

            if (!currentBattle.isWildBattle)
            {
                yield return Global.StartTypewriter($"TRAINER  (SOMETHING)  sent  out  {currentBattle.enemyTeam[0].name}!", currentBattle.battleTextPos);
                yield return Global.battleSpeed;
            }

            Enemy_SwitchInPokemon(0);
            UIManager.battleRects["Image_EnemyPokemon"].SetEnabled(true);

            yield return Global.StartTypewriter($"Go!  {currentBattle.playerTeam[0].name.ToUpper()}!", currentBattle.battleTextPos);
            yield return Global.battleSpeed;

            Player_SwitchInPokemon(0);
            UIManager.battleRects["Image_PlayerPokemon"].SetEnabled(true);

            PlayerTurnStart();
        }

        //static void DisplayUI()
        //{
        //    Vector2 pos = battleBackgroundsDic[currentArea];
        //    Global.StartTypewriter($"What  will  {DatabaseManager.GetBasePokemonData(1, 1).name}  do?", new Vector2(Global.screenWidth * 0.035f, Global.screenHeight * 0.78f));

        //    Enemy_SwitchInPokemon(0);
        //    Player_SwitchInPokemon(0);
        //}

        static void PlayerTurnStart()
        {
            Global.currentTypeText = $"What  will  {currentBattle.playerTeam[0].name.ToUpper()}  do?";
            state = BattleStates.PlayerTurn;
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(false);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(true);
            Debug.WriteLine($"Current state has a value of: {EnemyBattleAI.EvaluateBattleState(currentBattle)}.");
        }

        public static void ChangeHPText(string hpName, string sliderName)
        {
            UITextBox healthText = (UITextBox)UIManager.battleRects[hpName]; 
            UISlider slider = (UISlider)UIManager.battleRects[sliderName];
            healthText.SetText($"{Math.Ceiling(slider.value)}/{slider.maxValue}");
        }
        public static void ShowOptions()
        {
            //UIManager.battleRects["Image_BagMenu"].SetEnabled(false);
            //UIManager.battleRects["Image_PokemonMenu"].SetEnabled(false);
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(true);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);
            Global.currentTypeText = $"What  will  {currentBattle.playerTeam[0].name.ToUpper()}  do?";
        }

        public static void OnRunChosen()
        {
            if(!currentBattle.isWildBattle)
                CoroutineManager.Start(RunSequence(false));
            else if (IsPlayerFaster())
                CoroutineManager.Start(RunSequence(true));
            else
            {
                currentBattle.runAttempts++;

                float odds = ((currentBattle.playerTeam[currentBattle.playerTeamIndex].speStat * 32f) / (currentBattle.enemyTeam[currentBattle.enemyTeamIndex].speStat / 4f)) + 30 * currentBattle.runAttempts;

                int chance = Global.random.Next(0, 256);

                if(chance < odds)
                    CoroutineManager.Start(RunSequence(true));
                else
                {
                    CoroutineManager.Start(RunSequence(false));
                }
            }
        }

        static IEnumerator<object> RunSequence(bool success)
        {
            state = BattleStates.Ran;
            
            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);

            if (!currentBattle.isWildBattle)
            {
                UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
                UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);

                yield return Global.StartTypewriter("Can't  escape!", currentBattle.battleTextPos);

                yield return Global.battleSpeed;

                Global.currentTypeText = "";
                ShowOptions();
                yield break;
            }

            if (success)
            {
                yield return Global.StartTypewriter("Got  away  safely!", currentBattle.battleTextPos);
                yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                currentBattle.battleOver = true;

                GameStateManager.ChangeState(GameStateManager.GameState.Overworld);

                CoroutineManager.Start(UIManager.DoTransitionAnimation(() =>
                {
                    UIManager.battleRects["Image_Background"].SetEnabled(false);
                    Global.currentTypeText = "";
                    EnvironmentManager.ChangeInstancesEnable(true);
                }));
            }
            else
            {
                yield return Global.StartTypewriter("Can't  escape!", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                CoroutineManager.Start(OnAttackChosen(-1));
            }

            //currentBattle = null;
        }

        public static bool IsPlayerFaster()
        {
            float enemyMultiplier = currentBattle.enemyTeam[currentBattle.enemyTeamIndex].statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis ? 0.5f : 1;
            float playerMultiplier = currentBattle.playerTeam[currentBattle.playerTeamIndex].statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis ? 0.5f : 1;

            enemyMultiplier *= GetStageMultiplier(currentBattle.enemyTeam[currentBattle.enemyTeamIndex].speModifier);
            playerMultiplier *= GetStageMultiplier(currentBattle.playerTeam[currentBattle.playerTeamIndex].speModifier);

            return currentBattle.enemyTeam[currentBattle.enemyTeamIndex].speStat*enemyMultiplier <= currentBattle.playerTeam[currentBattle.playerTeamIndex].speStat*playerMultiplier;
        }

        static int PriorityCheck(int playerPriority, int enemyPriority)
        {
            if (playerPriority == enemyPriority)
                return 0;
            else if (playerPriority > enemyPriority)
                return 1;
            else if (playerPriority < enemyPriority)
                return 2;

            return 0;
        }

        public static void ShowAttacks()
        {
            backAction = () => ShowOptions();

            if (currentBattle.playerTeam[currentBattle.playerTeamIndex].moveset.Length > 0)
            {
                bool hasMove = false;

                foreach (var v in currentBattle.playerTeam[currentBattle.playerTeamIndex].moveset)
                {
                    if (v.currentPP > 0)
                        hasMove = true;
                }

                if (hasMove)
                {
                    UIManager.battleRects["Image_AttackMenu"].SetEnabled(true);
                    Global.currentTypeText = "";
                }
                else
                {
                    UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);
                    CoroutineManager.Start(OnAttackChosen(5));
                }
            }
            else
            {
                UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);
                CoroutineManager.Start(OnAttackChosen(5));
            }

            UIManager.battleRects["Image_BattleMenu"].SetEnabled(false);
        }

        static MoveInstance EnemyChooseMove()
        {
            MoveInstance moveChosen = EnemyBattleAI.ChooseMove(currentBattle.aiLevel, currentBattle);

            if (moveChosen == null)
            {
                Debug.WriteLine("Couldn't find a suitable move.");
                moveChosen = DatabaseManager.GetBaseMoveInstance(2);
            }

            Debug.WriteLine($"cab" + moveChosen.name);

            return moveChosen;
        }

        public static int CalculateAttack(PokemonInstance attackingPoke, PokemonInstance defendingPoke, MoveInstance moveChosen, bool crit)
        {
            int damageOutput = 0;
            float multiplier = 1;

            if (moveChosen.property == 0) // Status
            {
                //implement
                return 0;
            }
            else if (moveChosen.property == 1) // Physical
            {
                multiplier = 1;

                if(attackingPoke.statusCondition != null)
                    multiplier *= attackingPoke.statusCondition.primaryCondition == PrimaryStatusConditions.Burn ? 0.5f : 1;

                multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, defendingPoke.type1, defendingPoke.type2);

                if (moveChosen.typeID == attackingPoke.type1 || moveChosen.typeID == attackingPoke.type2)
                    multiplier *= 1.5f;

                int atkAfterAccounting = attackingPoke.atkStat;
                int defAfterAccounting = defendingPoke.defStat;
                //Account for crits
                Debug.WriteLine($"ahdashduahdihiahdsaudhi {atkAfterAccounting} {defAfterAccounting} {multiplier}");

                if (crit)
                {
                    multiplier *= 1.5f;

                    //disregard attacker's negative stats and defender's positive stats

                    atkAfterAccounting = (int)(atkAfterAccounting * MathHelper.Max(2, 2 + attackingPoke.atkModifier) / 2);
                    defAfterAccounting = (int)(defAfterAccounting * 2 / MathHelper.Max(2, 2 - defendingPoke.defModifier));
                }
                else
                {
                    atkAfterAccounting = (int)(atkAfterAccounting * GetStageMultiplier(attackingPoke.atkModifier));
                    defAfterAccounting = (int)(defAfterAccounting * GetStageMultiplier(defendingPoke.defModifier));
                }

                //Account for random

                multiplier *= Global.random.Next(85, 101) / 100f;

                damageOutput = (int)Math.Round(((2 * (float)attackingPoke.level / 5f + 2) * moveChosen.damage * atkAfterAccounting / defAfterAccounting / 50 + 2) * multiplier, 0);

                Debug.WriteLine($"Should be doing: {damageOutput} physical damage to {defendingPoke.name}.");
            }
            else if (moveChosen.property == 2) // Special
            {
                multiplier = 1;
                
                multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, defendingPoke.type1, defendingPoke.type2);

                if (moveChosen.typeID == attackingPoke.type1 || moveChosen.typeID == attackingPoke.type2)
                    multiplier *= 1.5f;

                int atkAfterAccounting = attackingPoke.spaStat;
                int defAfterAccounting = defendingPoke.spdStat;
                //Account for crits

                if (crit)
                {
                    multiplier *= 1.5f;

                    //disregard attacker's negative stats and defender's positive stats

                    atkAfterAccounting = (int)(atkAfterAccounting * MathHelper.Max(2, 2 + attackingPoke.spaModifier) / 2);
                    defAfterAccounting = (int)(defAfterAccounting * 2 / MathHelper.Max(2, 2 - defendingPoke.spdModifier));
                }
                else
                {
                    atkAfterAccounting = (int)(atkAfterAccounting * GetStageMultiplier(attackingPoke.spaModifier));
                    defAfterAccounting = (int)(defAfterAccounting * GetStageMultiplier(defendingPoke.spdModifier));
                }

                //Account for random

                multiplier *= Global.random.Next(85, 101) / 100f;

                damageOutput = (int)Math.Round(((2 * (float)attackingPoke.level / 5f + 2) * moveChosen.damage * atkAfterAccounting / defAfterAccounting / 50 + 2) * multiplier, 0);

                Debug.WriteLine($"Should be doing: {damageOutput} special damage to {defendingPoke.name}.");
            }
            else //Idk some type of fallback or something
                return 1;

            return (int)MathF.Max(1, damageOutput);
        }
        static float GetStageMultiplier(int stage, bool baseStat = true)
        {
            float number = baseStat? 2 : 3;
            if (stage >= 0)
                return (number + stage) / number;
            else
                return number / (number - stage);
        }

        public static bool AccuracyCheck(int chanceToSucceed)
        {
            return (Global.random.Next(1, 101) <= chanceToSucceed);
        }

        public static IEnumerator<object> OnAttackChosen(int attackIndex)
        {
            PokemonInstance currentPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
            PokemonInstance enemyPoke = currentBattle.enemyTeam[currentBattle.enemyTeamIndex];

            backAction = null;

            currentBattle.playerCanAct = true;
            currentBattle.enemyCanAct = true;

            if (attackIndex == -1)
            {
                currentBattle.playerCanAct = false;
            }

            MoveInstance moveChosen = null;
            MoveInstance enemyMove = EnemyChooseMove();

            if (currentBattle.playerCanAct)
            {
                Debug.WriteLine("a");
                if (attackIndex > 3)
                    moveChosen = DatabaseManager.GetBaseMoveInstance(1);
                else
                    moveChosen = currentPoke.moveset[attackIndex];
            }

            bool playerMoveFirst;

            int priorityC;

            if (moveChosen != null)
                priorityC = PriorityCheck(moveChosen.priority, enemyMove.priority);
            else
                priorityC = 0;

            if (priorityC == 1)
                playerMoveFirst = true;
            else if (priorityC == 2)
                playerMoveFirst = false;
            else
                playerMoveFirst = IsPlayerFaster();

            if (playerMoveFirst) //Player faster or equal
            {
                yield return CoroutineManager.Start(PlayerTurn(moveChosen));

                if (currentBattle.battleOver) { yield break; }

                yield return CoroutineManager.Start(EnemyTurn(enemyMove));
            }
            else //Enemy faster
            {
                yield return CoroutineManager.Start(EnemyTurn(enemyMove));

                if(currentBattle.battleOver) { yield break; }

                yield return CoroutineManager.Start(PlayerTurn(moveChosen));
            }

            //Do status stuff
            CheckDamagingStatuses(enemyPoke, $"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}", false);
            yield return currentBattle.CheckDeath(enemyPoke, false);

            if (currentBattle.battleOver) { yield break; }

            CheckDamagingStatuses(currentPoke, currentPoke.name.ToUpper(), true);
            yield return currentBattle.CheckDeath(currentPoke, true);

            if (currentBattle.battleOver) { yield break; }

            PlayerTurnStart();
        }

        static IEnumerator<object> PlayerTurn(MoveInstance moveChosen)
        {
            PokemonInstance currentPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
            PokemonInstance enemyPoke = currentBattle.enemyTeam[currentBattle.enemyTeamIndex];

            float multiplier = 1;
            int damage = 1;

            yield return CoroutineManager.Start(CheckProhibitingStatuses(currentPoke, enemyPoke, currentPoke.name.ToUpper(), true));

            if (currentBattle.playerCanAct == true)
            {
                multiplier *= DatabaseManager.GetTypeEffectiveness(moveChosen.typeID, enemyPoke.type1, enemyPoke.type2);
                bool crit = CheckCritical();

                damage = CalculateAttack(currentPoke, enemyPoke, moveChosen, crit);
                currentBattle.damageJustCalculated = MathHelper.Clamp(damage, 0, currentPoke.currentHealth);

                float attackerAccuracyMultiplier = GetStageMultiplier(currentPoke.accModifier, false);
                float defenderEvasionMultiplier = GetStageMultiplier(enemyPoke.evaModifier, false);

                yield return Global.StartTypewriter($"{currentPoke.name.ToUpper()}  used  {moveChosen.name.ToUpper()}!", currentBattle.battleTextPos);

                yield return Global.battleSpeed;

                if (AccuracyCheck((int)(moveChosen.accuracy * attackerAccuracyMultiplier / defenderEvasionMultiplier)) || moveChosen.accuracy == -1)
                {
                    moveChosen.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(currentPoke, enemyPoke, moveChosen.name, damage, moveChosen.accuracy, multiplier, true));

                    if (crit)
                    {
                        yield return Global.StartTypewriter("It's  a  critical  hit!", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;
                    }

                    if (moveActions.ContainsKey(moveChosen.effectKey))
                    {
                        yield return CoroutineManager.Start(moveActions[moveChosen.effectKey](moveChosen.affectsUser));
                        yield return Global.battleSpeed;
                    }

                    yield return currentBattle.CheckDeath(enemyPoke, false);
                }
                else
                {
                    yield return Global.StartTypewriter("The  attack  missed!", currentBattle.battleTextPos);

                    yield return Global.battleSpeed;
                }
            }
        }

        static IEnumerator<object> EnemyTurn(MoveInstance enemyMove)
        {
            PokemonInstance currentPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
            PokemonInstance enemyPoke = currentBattle.enemyTeam[currentBattle.enemyTeamIndex];

            float multiplier = 1;
            int damage = 1;

            yield return CoroutineManager.Start(CheckProhibitingStatuses(enemyPoke, currentPoke, $"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}", false));

            if (currentBattle.enemyCanAct == true)
            {
                multiplier *= DatabaseManager.GetTypeEffectiveness(enemyMove.typeID, currentPoke.type1, currentPoke.type2);
                bool crit = CheckCritical();

                damage = CalculateAttack(enemyPoke, currentPoke, enemyMove, crit);
                currentBattle.damageJustCalculated = MathHelper.Clamp(damage, 0, enemyPoke.currentHealth);

                float attackerAccuracyMultiplier = GetStageMultiplier(enemyPoke.accModifier, false);
                float defenderEvasionMultiplier = GetStageMultiplier(currentPoke.evaModifier, false);

                yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {enemyPoke.name.ToUpper()}  used  {enemyMove.name.ToUpper()}!", currentBattle.battleTextPos);

                yield return Global.battleSpeed;

                if (AccuracyCheck((int)(enemyMove.accuracy * attackerAccuracyMultiplier / defenderEvasionMultiplier)) || enemyMove.accuracy == -1)
                {
                    enemyMove.currentPP--;

                    yield return CoroutineManager.Start(AttackSequence(enemyPoke, currentPoke, enemyMove.name, damage, enemyMove.accuracy, multiplier, false));

                    if (crit)
                    {
                        yield return Global.StartTypewriter("It's  a  critical  hit!", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;
                    }

                    if (moveActions.ContainsKey(enemyMove.effectKey))
                    {
                        yield return CoroutineManager.Start(moveActions[enemyMove.effectKey](!enemyMove.affectsUser));
                        yield return Global.battleSpeed;
                    }

                    yield return currentBattle.CheckDeath(currentPoke, true);
                }
                else
                {
                    yield return Global.StartTypewriter("The  attack  missed!", currentBattle.battleTextPos);

                    yield return Global.battleSpeed;
                }
            }
        }

        static bool CheckCritical()
        {
            int critStage = 0; // Check for high crit moves or item
            int critChance = 1;

            switch(critStage)
            {
                case 0:
                    critChance = 24;
                    break;
                case 1:
                    critChance = 8;
                    break;
                case 3:
                    critChance = 2;
                    break;
            }

            bool crit = Global.random.Next(1, critChance + 1) == 1;

            return crit;
        }

        public static IEnumerator<object> CheckProhibitingStatuses(PokemonInstance poke, PokemonInstance defendingPoke, string displayName, bool isPlayer)
        {
            UIManager.battleRects["Image_BottomEmptyBar"].SetEnabled(true);
            UIManager.battleRects["Image_AttackMenu"].SetEnabled(false);

            if (poke.statusCondition == null)
                yield break;

            if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Freeze) // Frozen
            {
                yield return Global.StartTypewriter($"{displayName}  is  frozen...", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                if(AccuracyCheck(20))
                {
                    yield return Global.StartTypewriter($"{displayName}  broke  the  ice!", currentBattle.battleTextPos);
                    poke.statusCondition = new StatusEffects(PrimaryStatusConditions.None);
                }
                else
                {
                    yield return Global.StartTypewriter($"{displayName}  is  frozen  solid!", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;
                }

                yield return Global.battleSpeed;
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Paralysis) // Paralyzed
            {
                yield return Global.StartTypewriter($"{displayName}  is  paralyzed...", currentBattle.battleTextPos);
                yield return Global.battleSpeed;

                if (!AccuracyCheck(75))
                {
                    yield return Global.StartTypewriter($"{displayName}  cannot  move  due  to  its  paralysis!", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;

                    yield return Global.battleSpeed;
                }
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Sleep) // Sleep
            {
                if (poke.statusCondition.turnCount > 0)
                {
                    yield return Global.StartTypewriter($"{displayName}  is  asleep...", currentBattle.battleTextPos);

                    if (isPlayer) currentBattle.playerCanAct = false;
                    else currentBattle.enemyCanAct = false;

                    poke.statusCondition.turnCount--;
                }
                else
                {
                    yield return Global.StartTypewriter($"{displayName}  woke  up!", currentBattle.battleTextPos);
                    poke.statusCondition = new StatusEffects(PrimaryStatusConditions.None);
                }

                yield return Global.battleSpeed;
            } else // Check volatile prohibiting statuses
            {
                if (GetVolatileStatus(poke, VolatileStatusConditions.Confusion) != null) // Confusion
                {
                    StatusEffects confuse = GetVolatileStatus(poke, VolatileStatusConditions.Confusion);
                    if (confuse.turnCount > 0)
                    {
                        yield return Global.StartTypewriter($"{displayName}  is  confused...", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;

                        if (!AccuracyCheck(67))
                        {
                            yield return Global.StartTypewriter($"It hurt itself in its confusion!", currentBattle.battleTextPos);
                            yield return Global.battleSpeed;

                            MoveInstance confuseMove = new MoveInstance(0, "", 40, -1, 19, 100, 1, 0);
                            yield return currentBattle.TakeDamage(!isPlayer, CalculateAttack(poke, defendingPoke, confuseMove, false));
                            yield return currentBattle.CheckDeath(poke, isPlayer);

                            if (isPlayer) currentBattle.playerCanAct = false;
                            else currentBattle.enemyCanAct = false;
                        }

                        confuse.turnCount--;
                    }
                    else
                    {
                        yield return Global.StartTypewriter($"{displayName}  snapped  out  of  its  confusion!", currentBattle.battleTextPos);
                        yield return Global.battleSpeed;
                        poke.volatileStatuses.Remove(confuse);
                    }
                }
            }
        }

        public static IEnumerator<object> CheckDamagingStatuses(PokemonInstance poke, string displayName, bool isPlayer)
        {
            if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Burn)
            {
                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  poison!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.hpStat / 8, 1, 1000));
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Poison)
            {
                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  burn!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.hpStat / 16, 1, 1000));
            }
            else if (poke.statusCondition.primaryCondition == PrimaryStatusConditions.Toxic)
            {
                poke.statusCondition.turnCount++;

                yield return Global.StartTypewriter($"{displayName}  is  hurt  by  its  poison!", currentBattle.battleTextPos);
                yield return currentBattle.TakeDamage(!isPlayer, MathHelper.Clamp(poke.statusCondition.turnCount * poke.hpStat / 16, 1, 1000));
            }
        }

        public static StatusEffects GetVolatileStatus(PokemonInstance poke, VolatileStatusConditions condition)
        {
            foreach(var v in poke.volatileStatuses)
            {
                if (v.volatileCondition == condition)
                    return v;
            }

            return null;
        }

        static IEnumerator<object> AttackSequence(PokemonInstance attackingPoke, PokemonInstance defendingPoke, string attackName, int damage, int accuracy, float effective, bool playerAttack)
        {
            state = BattleStates.DoingTurn;

            yield return Global.battleSpeed;

            yield return currentBattle.TakeDamage(playerAttack, damage);

            if (damage > 0)
            {
                if (effective == 0)
                    yield return Global.StartTypewriter($"It  doesn't  affect  {defendingPoke.name.ToUpper()}!", currentBattle.battleTextPos);
                else if (effective < 1)
                    yield return Global.StartTypewriter($"It's  not  very  effective...", currentBattle.battleTextPos);
                else if (effective > 1)
                    yield return Global.StartTypewriter($"It's  super  effective!", currentBattle.battleTextPos);
            }

            if (effective != 1) yield return Global.battleSpeed;

            state = BattleStates.PlayerTurn;
        }

        public static IEnumerator<object> ApplyStatChange(bool isPlayer, int stat, int amount)
        {
            string statText = "";
            string amountText = "";
            PokemonInstance affectedPoke;
            string displayName;

            int preChange = 10;
            int postChange = 11;

            if (isPlayer)
            {
                affectedPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
                displayName = affectedPoke.name.ToUpper();
            }
            else
            {
                affectedPoke = currentBattle.enemyTeam[currentBattle.enemyTeamIndex];
                displayName = $"{currentBattle.enemyPrefix}  {affectedPoke.name.ToUpper()}";
            }

            switch (stat)
            {
                case 0: // ATTACK
                    statText = "attack";
                    preChange = affectedPoke.atkModifier;
                    affectedPoke.atkModifier = MathHelper.Clamp(affectedPoke.atkModifier + amount, -6, 6);
                    postChange = affectedPoke.atkModifier;
                    break;
                case 1: // DEFENSE
                    statText = "defense";
                    preChange = affectedPoke.defModifier;
                    affectedPoke.defModifier = MathHelper.Clamp(affectedPoke.defModifier + amount, -6, 6);
                    postChange = affectedPoke.defModifier;
                    break;
                case 2: // SPECIAL ATTACK
                    statText = "special  attack";
                    preChange = affectedPoke.spaModifier;
                    affectedPoke.spaModifier = MathHelper.Clamp(affectedPoke.spaModifier + amount, -6, 6);
                    postChange = affectedPoke.spaModifier;
                    break;
                case 3: // SPECIAL DEFENSE
                    statText = "special  defense";
                    preChange = affectedPoke.spdModifier;
                    affectedPoke.spdModifier = MathHelper.Clamp(affectedPoke.spdModifier + amount, -6, 6);
                    postChange = affectedPoke.spdModifier;
                    break;
                case 4: // SPEED
                    statText = "speed";
                    preChange = affectedPoke.speModifier;
                    affectedPoke.speModifier = MathHelper.Clamp(affectedPoke.speModifier + amount, -6, 6);
                    postChange = affectedPoke.speModifier;
                    break;
                case 5: // ACCURACY
                    statText = "accuracy";
                    preChange = affectedPoke.accModifier;
                    affectedPoke.accModifier = MathHelper.Clamp(affectedPoke.accModifier + amount, -6, 6);
                    postChange = affectedPoke.accModifier;
                    break;
                case 6: // EVASION
                    statText = "evasion";
                    preChange = affectedPoke.evaModifier;
                    affectedPoke.evaModifier = MathHelper.Clamp(affectedPoke.evaModifier + amount, -6, 6);
                    postChange = affectedPoke.evaModifier;
                    break;
            }

            switch (amount)
            {
                case -3:
                    amountText = "severely";
                    break;
                case -2:
                    amountText = "harshly";
                    break;
                case 2:
                    amountText = "  sharply";
                    break;
                case 3:
                    amountText = "  drastically";
                    break;
            }

            if (!currentBattle.simulation)
            {

                if (preChange == postChange)
                {
                    if (amount > 0)
                        yield return Global.StartTypewriter($"{displayName}'s  {statText}  can't  go  any  higher!", currentBattle.battleTextPos);
                    else
                        yield return Global.StartTypewriter($"{displayName}'s  {statText}  can't  go  any  lower!", currentBattle.battleTextPos);
                }
                else if (amount > 0)
                    yield return Global.StartTypewriter($"{displayName}'s  {statText}  rose  {amountText}!", currentBattle.battleTextPos);
                else
                    yield return Global.StartTypewriter($"{displayName}'s  {statText}  {amountText}  fell!", currentBattle.battleTextPos);
            }
        }

        static IEnumerator<object> HealEffect(bool isPlayer, int amount)
        {
            PokemonInstance poke = isPlayer ? currentBattle.playerTeam[currentBattle.playerTeamIndex] : currentBattle.enemyTeam[currentBattle.enemyTeamIndex];

            if (poke.currentHealth < poke.hpStat)
            {
                amount = (int)MathHelper.Clamp(amount, 1, int.MaxValue);
                if (!currentBattle.simulation)
                {
                    string text = isPlayer ? poke.name : $"{currentBattle.enemyPrefix}  {poke.name}";
                    yield return Global.StartTypewriter($"{text}  healed  {amount}  HP!", currentBattle.battleTextPos);

                    yield return Global.battleSpeed;
                }

                yield return currentBattle.TakeDamage(!isPlayer, -amount);
            }
            else
            {
                yield return Global.StartTypewriter("It  had  no  effect!", currentBattle.battleTextPos);
            }
        }

        static IEnumerator<object> AbsorbEffect(bool isPlayer)
        {
            PokemonInstance attackingInstance = isPlayer ? currentBattle.playerTeam[currentBattle.playerTeamIndex] : currentBattle.enemyTeam[currentBattle.enemyTeamIndex];

            if (attackingInstance.currentHealth != attackingInstance.hpStat)
            {
                if (!currentBattle.simulation)
                {
                    yield return Global.battleSpeed;
                    yield return CoroutineManager.Start(HealEffect(isPlayer, currentBattle.damageJustCalculated / 2)); // fix calculation
                } else
                    CoroutineManager.Start(HealEffect(isPlayer, currentBattle.damageJustCalculated / 2));
            }
        }

        public static IEnumerator<object> FaintSequence(PokemonInstance faintedPoke, bool isPlayer)
        {
            int option = 0;
            if (isPlayer)
                option = 1;
            else if (currentBattle.isWildBattle)
                option = 2;
            else
                option = 3;

            yield return Global.battleSpeed;

            switch (option)
            {
                case 1: //Player pokemon faints
                    yield return Global.StartTypewriter($"{faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    break;
                case 2: // Wild pokemon faints
                    currentBattle.battleOver = true;

                    yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    //Do animation and cry
                    yield return Global.battleSpeed;

                    PokemonInstance playerPoke = currentBattle.playerTeam[currentBattle.playerTeamIndex];
                    int totalExpGain = faintedPoke.CalculateExpGainFromKO(playerPoke.level);

                    yield return Global.StartTypewriter($"{playerPoke.name.ToUpper()}  gained  {totalExpGain}  EXP.  Points!", currentBattle.battleTextPos);
                    yield return Global.battleSpeed;

                    int expGain = Math.Clamp(totalExpGain, 0, playerPoke.maxExp - playerPoke.currentExp);

                    Debug.WriteLine(totalExpGain);

                    currentBattle.isAnimating = true;
                    yield return CoroutineManager.Start(currentBattle.AnimateSlider((UISlider)UIManager.battleRects["Slider_PlayerExp"], playerPoke.currentExp + expGain));

                    totalExpGain -= expGain;
                    playerPoke.GainExp(expGain);

                    while (totalExpGain > 0)
                    {
                        //Play sound
                        Debug.WriteLine(playerPoke.level);

                        yield return Global.battleSpeed;

                        ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).SetValue(0);

                        ((UITextBox)UIManager.battleRects["Text_PlayerLevel"]).SetText($"Lv{playerPoke.level}");
                        yield return Global.StartTypewriter($"{playerPoke.name.ToUpper()}  grew  to  Lv.  {playerPoke.level}!", currentBattle.battleTextPos);

                        yield return Global.battleSpeed;

                        ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).maxValue = playerPoke.maxExp;

                        expGain = MathHelper.Clamp(totalExpGain, 0, playerPoke.maxExp - playerPoke.currentExp);

                        currentBattle.isAnimating = true;
                        yield return CoroutineManager.Start(currentBattle.AnimateSlider((UISlider)UIManager.battleRects["Slider_PlayerExp"], playerPoke.currentExp + expGain));

                        totalExpGain -= expGain;
                        playerPoke.GainExp(expGain);
                    }

                    yield return Global.battleSpeed;

                    yield return Global.StartTypewriter($"Player   defeated  wild  {faintedPoke.name.ToUpper()}!", currentBattle.battleTextPos);

                    yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                    //yield return Global.StartTypewriter($"(PLAYER NAME)   got  $(MONEY)  for  winning!", currentBattle.battleTextPos);

                    //yield return CoroutineManager.Start(InputManager.GetButtonDown_Continue());

                    GameStateManager.ChangeState(GameStateManager.GameState.Overworld);

                    CoroutineManager.Start(UIManager.DoTransitionAnimation(() =>
                    {
                        UIManager.battleRects["Image_Background"].SetEnabled(false);
                        Global.currentTypeText = "";
                    }));

                    EnvironmentManager.ChangeInstancesEnable(true);


                    break;
                case 3: // Trainer pokemon faints
                    yield return Global.StartTypewriter($"{currentBattle.enemyPrefix}  {faintedPoke.name.ToUpper()}  fainted!", currentBattle.battleTextPos);

                    break;
            }

            //currentBattle = null;
        }

        public static void Player_SwitchInPokemon(int index)
        {
            currentBattle.playerTeamIndex = index;
            currentBattle.playerTeam[index].volatileStatuses = new List<StatusEffects>();

            UIManager.battleRects["Image_PlayerPokemon"].SetSprite(currentBattle.playerTeam[currentBattle.playerTeamIndex].backTexture);

            ((UISlider)UIManager.battleRects["Slider_PlayerHealth"]).maxValue = currentBattle.playerTeam[index].hpStat;
            ((UISlider)UIManager.battleRects["Slider_PlayerHealth"]).SetValue(currentBattle.playerTeam[index].currentHealth);

            ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).maxValue = currentBattle.playerTeam[index].maxExp;
            ((UISlider)UIManager.battleRects["Slider_PlayerExp"]).SetValue(currentBattle.playerTeam[index].currentExp);

            ((UITextBox)UIManager.battleRects["Text_PlayerName"]).SetText(currentBattle.playerTeam[index].name.ToUpper());
            ((UITextBox)UIManager.battleRects["Text_PlayerLevel"]).SetText($"Lv{currentBattle.playerTeam[index].level}");

            ((UIButton)UIManager.battleRects["Button_Attack1"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack1"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack2"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack2"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack3"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack3"]).SetText("");
            ((UIButton)UIManager.battleRects["Button_Attack4"]).SetInteractable(false);
            ((UITextBox)UIManager.battleRects["Text_Attack4"]).SetText("");

            if (currentBattle.playerTeam[index].moveset.Length > 0)
            {
                ((UIButton)UIManager.battleRects["Button_Attack1"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack1"]).SetText(currentBattle.playerTeam[index].moveset[0].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 1)
            {
                ((UIButton)UIManager.battleRects["Button_Attack2"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack2"]).SetText(currentBattle.playerTeam[index].moveset[1].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 2)
            {
                ((UIButton)UIManager.battleRects["Button_Attack3"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack3"]).SetText(currentBattle.playerTeam[index].moveset[2].name);
            }
            if (currentBattle.playerTeam[index].moveset.Length > 3)
            {
                ((UIButton)UIManager.battleRects["Button_Attack4"]).SetInteractable(true);
                ((UITextBox)UIManager.battleRects["Text_Attack4"]).SetText(currentBattle.playerTeam[index].moveset[3].name);
            }
        }

        public static void Enemy_SwitchInPokemon(int index)
        {
            currentBattle.enemyTeamIndex = index;
            currentBattle.enemyTeam[index].volatileStatuses = new List<StatusEffects>();

            //currentBattle.enemyTeam[index].volatileStatuses.Add(new StatusEffects(Global.random.Next(2, 6), VolatileStatusConditions.Confusion));

            ((UISlider)UIManager.battleRects["Slider_EnemyHealth"]).maxValue = currentBattle.enemyTeam[index].hpStat;
            ((UISlider)UIManager.battleRects["Slider_EnemyHealth"]).SetValue(currentBattle.enemyTeam[index].currentHealth);

            ((UITextBox)UIManager.battleRects["Text_EnemyName"]).SetText(currentBattle.enemyTeam[index].name.ToUpper());
            ((UITextBox)UIManager.battleRects["Text_EnemyLevel"]).SetText($"Lv{currentBattle.enemyTeam[index].level}");

            Debug.WriteLine($"{currentBattle.enemyTeam[index].currentHealth} / {currentBattle.enemyTeam[index].hpStat}");

            UIManager.battleRects["Image_EnemyPokemon"].SetSprite(currentBattle.enemyTeam[currentBattle.enemyTeamIndex].frontTexture);
        }
    }
}
