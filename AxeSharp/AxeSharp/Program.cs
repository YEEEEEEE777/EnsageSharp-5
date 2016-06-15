using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;

using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.AbilityInfo;

namespace AxeSharp
{
    class Program
    {
        private static readonly Menu Menu = new Menu("AxeSharp", "AxeSharp", true, "npc_dota_hero_axe", true);

        private static Item agh;
        private static Hero target;
        private static Hero me;

        private const int agroDistance = 300;


        static void Main(string[] args)
        {

            Menu.AddToMainMenu();
            Events.OnLoad += Events_OnLoad;
            Events.OnClose += Events_OnClose;
            Game.OnUpdate += Game_OnUpdate;
            //Game.OnUpdate += Killsteal;



        }

        private static Hero GetLowHpHeroInDistance(Hero me, float maxDistance)
        {
            var enemies = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.IsAlive && !enemy.IsIllusion && enemy.Team != me.Team && (getULtDamage(me) > (enemy.Health - 5))).ToList();

            Hero target = getHeroInDistance(me, enemies, maxDistance);

            return target;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;
            if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Axe)
            {
                return;
            }
            Ability ult = me.Spellbook.SpellR;
            Hero target = null;
                               
            if (ult != null && ult.Level > 0)
            {
                target = GetLow
            }
                
                    
                    
                    
                    target != null && target.Health <= )
        }

        private static void Events_OnClose(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Events_OnLoad(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
