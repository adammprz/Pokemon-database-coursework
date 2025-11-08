using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    static class EnemyBattleAI
    {
        public static MoveInstance ChooseMove(int aiLevel, PokeBattleInstance currentBattleState)
        {
            MoveInstance moveChosen = null;

            if(!HasAvailableMove(currentBattleState))
            {
                //return struggle move
            }

            switch (aiLevel)
            {
                case 0:
                    moveChosen = RandomAI(currentBattleState);
                    break;
                case 1:
                    moveChosen = MostDamageAI(currentBattleState);
                    break;
                case 2:
                    moveChosen = MiniMax(currentBattleState, 1, false);
                    break;
            }

            return moveChosen;
        }

        static bool HasAvailableMove(PokeBattleInstance currentBattleState)
        {
            PokemonInstance enemy = currentBattleState.enemyTeam[currentBattleState.enemyTeamIndex];

            foreach (var v in enemy.moveset)
            {
                if(v.currentPP > 0)
                    return true;
            }

            return false;
        }

        static MoveInstance RandomAI(PokeBattleInstance currentBattleState)
        {
            MoveInstance moveChosen = null;

            for (int i = 0; i < 10; i++)
            {
                int rand = Global.random.Next(0, currentBattleState.enemyTeam[currentBattleState.enemyTeamIndex].moveset.Length);

                moveChosen = currentBattleState.enemyTeam[currentBattleState.enemyTeamIndex].moveset[rand];

                if (moveChosen.currentPP > 0)
                    i = 100;
            }

            if (moveChosen == null)
            {
                Debug.WriteLine("Couldn't find a suitable move.");
                moveChosen = DatabaseManager.GetBaseMoveInstance(2);
            }

            return moveChosen;
        }

        static MoveInstance MostDamageAI(PokeBattleInstance currentBattleState)
        {
            MoveInstance moveChosen = null;
            int mostDamage = -1;

            PokemonInstance enemy = currentBattleState.enemyTeam[currentBattleState.enemyTeamIndex];

            foreach (var v in enemy.moveset)
            {
                if(v.currentPP == 0)
                    continue;

                int damage = PokeBattleManager.CalculateAttack(enemy, currentBattleState.playerTeam[currentBattleState.playerTeamIndex], v, false);

                if(damage > mostDamage)
                {
                    mostDamage = damage;
                    moveChosen = v;
                } else if(damage == mostDamage)
                {
                    if(v.currentPP > moveChosen.currentPP)
                        moveChosen = v;
                }
            }

            return moveChosen;
        }

        static MoveInstance MiniMax(PokeBattleInstance currentBattleState, int depth, bool gambler)
        {
            MoveInstance moveChosen = null;
            bool aiMove = !PokeBattleManager.IsPlayerFaster();
            int topVal = Int32.MinValue;

            PokemonInstance playerPoke = currentBattleState.playerTeam[currentBattleState.playerTeamIndex];
            PokemonInstance playerPokeCopy = playerPoke.DuplicatePokemon();
            PokemonInstance enemyPoke = currentBattleState.enemyTeam[currentBattleState.enemyTeamIndex];
            PokemonInstance enemyPokeCopy = enemyPoke.DuplicatePokemon();

            foreach (var move in enemyPokeCopy.moveset)
            {
                if (move.currentPP > 0)
                {
                    PokeBattleInstance performedMove = new PokeBattleInstance([playerPokeCopy], [enemyPokeCopy], currentBattleState.isWildBattle, currentBattleState.aiLevel);
                    performedMove.simulation = true;
                    performedMove.TakeDamage(false, PokeBattleManager.CalculateAttack(enemyPokeCopy, playerPokeCopy, move, false));
                    MiniMaxNode temp = new MiniMaxNode(performedMove);

                    //perform the move first on the current battle state
                    int val = TraverseMiniMax(new MiniMaxNode(currentBattleState), 0, depth, false);

                    if(val >  topVal)
                    {
                        topVal = val;
                        moveChosen = move;
                    }
                    else if(val == topVal)
                    {
                        if (Global.random.Next(0, 2) == 0)
                            moveChosen = move;
                    }
                }
            }

            return moveChosen;
        }

        static int TraverseMiniMax(MiniMaxNode currentNode, int currentDepth, int maxDepth, bool aiMove)
        {
            int value = 0;
            PokeBattleInstance thisNodesState = currentNode.nodeBattleState;

            PokemonInstance playerPoke = thisNodesState.playerTeam[thisNodesState.playerTeamIndex];
            PokemonInstance playerPokeCopy = playerPoke.DuplicatePokemon();
            PokemonInstance enemyPoke = thisNodesState.enemyTeam[thisNodesState.enemyTeamIndex];
            PokemonInstance enemyPokeCopy = enemyPoke.DuplicatePokemon();

            currentDepth++;

            if (currentDepth >= maxDepth * 2 || playerPoke.currentHealth <= 0 || enemyPoke.currentHealth <= 0)
                return (EvaluateBattleState(currentNode.nodeBattleState) * maxDepth * 2 / currentDepth);

            if (aiMove)
            {
                foreach(var move in enemyPokeCopy.moveset)
                {
                    if (move.currentPP > 0)
                    {
                        PokeBattleInstance performedMove = new PokeBattleInstance([playerPokeCopy], [enemyPokeCopy], thisNodesState.isWildBattle, thisNodesState.aiLevel);
                        performedMove.simulation = true;
                        performedMove.TakeDamage(false, PokeBattleManager.CalculateAttack(enemyPokeCopy, playerPokeCopy, move, false));
                        MiniMaxNode temp = new MiniMaxNode(performedMove);
                        //apply statues
                        int v = TraverseMiniMax(temp, currentDepth, maxDepth, !aiMove);
                        Debug.WriteLine($"enemy {move.name} has a value of {v} at depth {currentDepth}");

                        if (v > value)
                            value = v;
                    }
                }
                //foreach enemy move check their pp --
                //perform the move --
                //check value returned --
                //traverse each possibility --
                //if either KO, return instead. --
                //value = TraverseMiniMax() --
                //basically what goes on in the minimax function lol
            }
            else
            {
                foreach (var move in playerPokeCopy.moveset)
                {
                    if (move.currentPP > 0)
                    {
                        PokeBattleInstance performedMove = new PokeBattleInstance([playerPokeCopy], [enemyPokeCopy], thisNodesState.isWildBattle, thisNodesState.aiLevel);
                        performedMove.simulation = true;
                        performedMove.TakeDamage(true, PokeBattleManager.CalculateAttack(playerPokeCopy, enemyPokeCopy, move, false));
                        MiniMaxNode temp = new MiniMaxNode(performedMove);
                        //apply statues
                        int v = TraverseMiniMax(temp, currentDepth, maxDepth, !aiMove);
                        //Debug.WriteLine($"player {move.name} has a value of {v} at depth {currentDepth}");

                        if (v > value)
                            value = v;
                    }
                }
            }

            return value;
        }

        public static int EvaluateBattleState(PokeBattleInstance stateToEvaluate)
        {
            float value = 0;

            PokemonInstance playerPoke = stateToEvaluate.playerTeam[stateToEvaluate.playerTeamIndex];
            PokemonInstance enemyPoke = stateToEvaluate.enemyTeam[stateToEvaluate.enemyTeamIndex];

            Debug.WriteLine(enemyPoke.currentHealth);

            if (enemyPoke.currentHealth <= 0)
            {
                int counter = stateToEvaluate.enemyTeam.Length;
                foreach (var child in stateToEvaluate.enemyTeam)
                {
                    if (child.currentHealth > 0)
                        counter--;
                }

                if(counter <= 0)
                    return Int32.MinValue / 100;
            }

            if (playerPoke.currentHealth <= 0)
            {
                int counter = stateToEvaluate.playerTeam.Length;
                foreach (var child in stateToEvaluate.playerTeam)
                {
                    if (child.currentHealth > 0)
                        counter--;
                }

                if (counter <= 0)
                    return Int32.MinValue / 100;
            }

            value += ((float)enemyPoke.currentHealth / enemyPoke.hpStat) * 100;
            value -= ((float)playerPoke.currentHealth / playerPoke.hpStat) * 100;
            //Debug.WriteLine(value);

            // account for stat changes
            value -= playerPoke.accModifier * 2f;
            value -= playerPoke.evaModifier * 4.5f;
            value -= playerPoke.atkModifier * 3.5f;
            value -= playerPoke.spaModifier * 3.5f;
            value -= playerPoke.defModifier * 3.5f;
            value -= playerPoke.spdModifier * 3.5f;
            value -= playerPoke.speModifier * 2.75f;
            //Debug.WriteLine(value);

            value += enemyPoke.accModifier * 2f;
            value += enemyPoke.evaModifier * 4.5f;
            value += enemyPoke.atkModifier * 4.5f;//value += 10 * MathHelper.SquareRoot(Math.Abs(enemyPoke.atkModifier)) * Math.Sign(enemyPoke.atkModifier);
            value += enemyPoke.spaModifier * 3.5f;
            value += enemyPoke.defModifier * 3.5f;
            value += enemyPoke.spdModifier * 3.5f;
            value += enemyPoke.speModifier * 2.75f;
            //Debug.WriteLine(value);

            //if (playerPoke.statusCondition.volatileCondition != VolatileStatusConditions.Empty)
            //    value += 30;

            PrimaryStatusConditions playerCondition = playerPoke.statusCondition.primaryCondition;

            if (playerCondition == PrimaryStatusConditions.Burn || playerCondition == PrimaryStatusConditions.Poison || playerCondition == PrimaryStatusConditions.Paralysis)
                value += 20;
            else if (playerCondition == PrimaryStatusConditions.Sleep || playerCondition == PrimaryStatusConditions.Toxic)
                value += 40;
            else if (playerCondition == PrimaryStatusConditions.Freeze)
                value += 60;

            PrimaryStatusConditions enemyCondition = enemyPoke.statusCondition.primaryCondition;

            if (enemyCondition == PrimaryStatusConditions.Burn || enemyCondition == PrimaryStatusConditions.Poison || enemyCondition == PrimaryStatusConditions.Paralysis)
                value -= 20;
            else if (enemyCondition == PrimaryStatusConditions.Sleep || enemyCondition == PrimaryStatusConditions.Toxic)
                value -= 40;
            else if (enemyCondition == PrimaryStatusConditions.Freeze)
                value -= 60;

            return (int)value;
        }
    }

    class MiniMaxNode
    {
        public List<MiniMaxNode> children = new List<MiniMaxNode>();

        public PokeBattleInstance nodeBattleState;
        public int nodeValue;

        public MiniMaxNode(PokeBattleInstance state)
        {
            nodeBattleState = state;
        }
    }
}
