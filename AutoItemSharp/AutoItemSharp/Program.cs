using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace AutoItemSharp
{
    class Program
    {
        private static readonly Menu Menu = new Menu("AutoItemSharp", "AutoItemSharp", true, "item_iron_talon", true);

        private static readonly Dictionary<string, bool> itemsDict = new Dictionary<string, bool>
            {   { "item_bottle", true },
                { "item_iron_talon", true },
                { "item_hand_of_midas", true },
                { "item_phase_boots", true }
        };
        private static readonly MenuItem ItemKeyItem =
new MenuItem("Item", "Item").SetValue(new AbilityToggler(itemsDict));
        private static readonly MenuItem EnableKeyItem =
new MenuItem("Enable", "Enable").SetValue(true);

        private static readonly MenuItem PhaseDistanceItem = new MenuItem("Phase Distance", "Phase Distance").SetValue(new Slider(750, 500, 1000));



        private static Hero me;
        private static Player player;

        private static void Main(string[] args)
        {
            Menu.AddToMainMenu();
            Menu.AddItem(EnableKeyItem);
            Menu.AddItem(ItemKeyItem);
            Menu.AddItem(PhaseDistanceItem);

            Game.OnUpdate += Game_OnUpdate;
            Player.OnExecuteOrder += Player_OnExecuteOrder;
        }

        private static void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (me.Distance2D(args.TargetPosition) >= Menu.Item("Phase Distance").GetValue<Slider>().Value)
            {
                AutoPhase();
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            player = ObjectManager.LocalPlayer;
            me = ObjectManager.LocalHero;

            if ((!Menu.Item("Enable").GetValue<bool>()) || !Game.IsInGame || player == null || me == null || Game.IsChatOpen || Game.IsPaused) return;

            AutoMidas();
            AutoTalon();
            AutoBottle();

        }

        private static void AutoBottle()
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_bottle"))
            {
                Item bottle = me.FindItem("item_bottle");

                if (bottle != null)
                {
                    IEnumerable<Hero> allies = ObjectManager.GetEntities<Hero>().Where(x => x.Team == me.Team && x.IsAlive && (x.Health < x.MaximumHealth || x.Mana < x.MaximumMana) && x.Distance2D(me) <= 600 && x.IsVisible && x.IsSpawned && !x.HasModifier("modifier_bottle_regeneration"));
                    Unit lowestHPAlly = allies.MinOrDefault(x => x.Health);
                                        

                    if (bottle.Cooldown <= 0 && Utils.SleepCheck("bottle") && lowestHPAlly != null && me.HasModifier("modifier_fountain_aura_buff"))
                    {
                        bottle.UseAbility(lowestHPAlly);
                        Utils.Sleep(250, "bottle");
                    }

                }
            }
        }

        private static void AutoTalon()
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_iron_talon"))
            {
                Item talon = me.FindItem("item_iron_talon");

                if (talon != null)
                {
                    IEnumerable<Unit> monsters = ObjectManager.GetEntities<Unit>().Where(x => x.Team != me.Team && x.IsAlive && !x.IsAncient && x.Distance2D(me) <= 600 && x.IsVisible && x.Health > 0 && !x.IsMagicImmune() && x.BaseMovementSpeed > 0 && x.IsSpawned);// && x.UnitType != "Building");
                    Unit highestHPCreep = monsters.MaxOrDefault(x => x.Health);

                    if (talon.Cooldown <= 0 && monsters.Count() > 0 && Utils.SleepCheck("Talon") && highestHPCreep.Distance2D(me) <= 350 && MoreThanAnAttack(highestHPCreep) && highestHPCreep != null)
                    {
                        talon.UseAbility(highestHPCreep);
                        Utils.Sleep(250, "Talon");
                        me.Attack(highestHPCreep);
                    }
                }
            }
        }

        private static bool MoreThanAnAttack(Unit unit)
        {
            var unitArmor = 1 - ((0.06 * unit.Armor) / (1 + 0.06 * Math.Abs(unit.Armor)));
            if (unit.Health >= (me.DamageAverage + me.BonusDamage) * (1 - unit.DamageResist) * unitArmor) return true;
            else return false;
        }

        private static void AutoMidas()
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                Item midas = me.FindItem("item_hand_of_midas");

                if (midas != null)
                {
                    IEnumerable<Unit> monsters = ObjectManager.GetEntities<Unit>().Where(x => x.Team != me.Team && x.IsAlive && !x.IsAncient && x.Distance2D(me) <= 800 && x.IsVisible && x.Health > 0 && !x.IsMagicImmune() && x.BaseMovementSpeed > 0 && x.IsSpawned);
                    Unit highestHPCreep = monsters.MaxOrDefault(x => x.Health);

                    if (midas.Cooldown <= 0 && monsters.Count() > 0 && Utils.SleepCheck("Midas") && highestHPCreep.Distance2D(me) <= 600 && MoreThanAnAttack(highestHPCreep) && highestHPCreep != null)
                    {
                        midas.UseAbility(highestHPCreep);
                        Utils.Sleep(250, "Midas");
                    }
                }
            }
        }

        private static void AutoPhase()
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                Item phase = me.FindItem("item_phase_boots");

                if (phase != null && phase.Cooldown <= 0 && Utils.SleepCheck("Phase") && me.IsMoving)
                {
                    phase.UseAbility();
                    Utils.Sleep(250, "Phase");
                }
            }

        }



    }
}
