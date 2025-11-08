using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static RealProject.CoroutineManager;

namespace RealProject
{
    internal class PokeBattleInstance
    {
        public PokemonInstance[] playerTeam;
        public int playerTeamIndex;

        public PokemonInstance[] enemyTeam;
        public int enemyTeamIndex;

        public bool isWildBattle;
        public int aiLevel;

        public int runAttempts;

        public bool isAnimating;

        public bool playerCanAct, enemyCanAct;

        public string enemyPrefix;

        public Vector2 battleTextPos = new Vector2(11 * Global.pixelsPerUnit, 141 * Global.pixelsPerUnit);

        public int damageJustCalculated;
        public bool simulation = false;

        public bool battleOver;

        public PokeBattleInstance(PokemonInstance[] plTeam, PokemonInstance[] enTeam, bool wildBattle, int aiLevel) 
        {
            playerTeam = plTeam;
            enemyTeam = enTeam;

            playerTeamIndex = 0;
            enemyTeamIndex = 0;

            battleOver = false;

            isWildBattle = wildBattle;
            this.aiLevel = aiLevel;

            if (isWildBattle)
                enemyPrefix = "Wild";
            else
                enemyPrefix = "Foe";
        }

        public CoroutineInstance TakeDamage(bool enemy, int damage)
        {
            if(enemy)
            {
                enemyTeam[enemyTeamIndex].currentHealth -= damage;
                enemyTeam[enemyTeamIndex].currentHealth = MathHelper.Clamp(enemyTeam[enemyTeamIndex].currentHealth, 0, enemyTeam[enemyTeamIndex].hpStat);

                if (!simulation)
                {
                    isAnimating = true;
                    return CoroutineManager.Start(AnimateSlider((UISlider)UIManager.battleRects["Slider_EnemyHealth"], enemyTeam[enemyTeamIndex].currentHealth));
                    //((UISlider)UIManager.battleRects["Slider_EnemyHealth"]).SetValue(enemyTeam[enemyTeamIndex].currentHealth);
                }

                return null;
            }
            else
            {
                playerTeam[playerTeamIndex].currentHealth -= damage;
                playerTeam[playerTeamIndex].currentHealth = MathHelper.Clamp(playerTeam[playerTeamIndex].currentHealth, 0, playerTeam[playerTeamIndex].hpStat);

                if (!simulation)
                {
                    isAnimating = true;
                    return CoroutineManager.Start(AnimateSlider((UISlider)UIManager.battleRects["Slider_PlayerHealth"], playerTeam[playerTeamIndex].currentHealth));
                    //((UISlider)UIManager.battleRects["Slider_PlayerHealth"]).SetValue(playerTeam[playerTeamIndex].currentHealth);
                }

                return null;
            }
        }

        public CoroutineInstance CheckDeath(PokemonInstance poke, bool isPlayer)
        {
            if (poke.currentHealth <= 0)
            {
                return CoroutineManager.Start(PokeBattleManager.FaintSequence(poke, isPlayer));
            }

            return null;
        }

        public IEnumerator<object> AnimateSlider(UISlider slider, float targetVal)
        {
            while(slider.value > targetVal + 0.2f || slider.value < targetVal - 0.2f)
            {
                slider.SetValue(MathHelper.Lerp(slider.value, targetVal, Global.deltaTime * 3 / Global.battleSpeed));

                yield return 1 / 60f;
            }

            slider.SetValue(targetVal);

            isAnimating = false;
        }
    }
}
