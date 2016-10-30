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
        private static int cutRange;


        private static readonly string[] cutters = { "item_quelling_blade", "item_iron_talon", "item_bfury", "item_tango_single", "item_tango" };

        private static void Main(string[] args)
        {
            Menu.AddToMainMenu();
            Menu.AddItem(EnableKeyItem);
            Menu.AddItem(ItemKeyItem);
            Menu.AddItem(PhaseDistanceItem);

            Game.OnUpdate += Game_OnUpdate;
            Player.OnExecuteOrder += Player_OnExecuteOrder;

        }

        //BUGGY SINCE IF STOP OR HOLD IS PRESSED PHASE TRIGGERED.
        private static void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (me.Distance2D(args.TargetPosition) >= Menu.Item("Phase Distance").GetValue<Slider>().Value)
            {
                AutoPhase(me);
            }

            if (me.Name.Equals("npc_dota_hero_lone_druid") || me.Name.Equals("npc_dota_hero_rubick"))
            {
                var bear = ObjectManager.GetEntities<Unit>().FirstOrDefault(x => x.IsControllable && x.Name.Contains("npc_dota_lone_druid_bear") && x.IsAlive && !x.IsIllusion && x.IsSpawned && x.Health > 0);
                if (bear != null)
                {
                    if (bear.Distance2D(args.TargetPosition) >= Menu.Item("Phase Distance").GetValue<Slider>().Value) AutoPhase(bear);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            player = ObjectManager.LocalPlayer;
            me = ObjectManager.LocalHero;

            if ((!Menu.Item("Enable").GetValue<bool>()) || !Game.IsInGame || player == null || me == null || Game.IsChatOpen || Game.IsWatchingGame || Game.IsPaused || me.IsChanneling() || me.IsInvisible()) return;

            AutoMidas(me);
            AutoTalon(me);
            AutoBottle();
            AutoDeward(me);
            if (me.Name.Equals("npc_dota_hero_lone_druid") || me.Name.Equals("npc_dota_hero_rubick"))
            { 
                    Bear();
            }

        }

        private static void AutoBottle()
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_bottle"))
            {
                Item bottle = me.FindItem("item_bottle");



                if (bottle != null && bottle.CanBeCasted() && !me.IsInvisible() && !me.IsChanneling() && bottle.Cooldown <= 0)
                {
                    IEnumerable<Hero> allies = ObjectManager.GetEntities<Hero>().Where(x => x.Team == me.Team && x.IsAlive && (x.Health < x.MaximumHealth || x.Mana < x.MaximumMana) && x.Distance2D(me) <= 600 && x.IsVisible && x.IsSpawned && !x.HasModifier("modifier_bottle_regeneration") && !x.IsIllusion);
                    IEnumerable<Unit> fountains = ObjectManager.GetEntities<Unit>().Where(x => x.Name == "dota_fountain" && x.Team.Equals(me.Team));
                    Unit fountain = fountains.MinOrDefault(x => x.Distance2D(me));




                    if (Utils.SleepCheck("time") && Utils.SleepCheck("bottle") && me.HasModifier("modifier_fountain_aura_buff") && me.Distance2D(fountain) > 1300)
                    {
                        foreach (Hero ally in allies)
                        {
                            if (Utils.SleepCheck("bottle") && ally != null)
                            {
                                bottle.UseAbility(ally);

                                Utils.Sleep(450, "bottle");
                            }
                        }

                        if (me != null)
                        {
                            if (Utils.SleepCheck("bottle"))
                            {
                                bottle.UseAbility(me);

                                Utils.Sleep(250, "bottle");
                            }
                        }
                        Utils.Sleep(3500, "time");
                    }

                    else if (Utils.SleepCheck("bottle") && me.HasModifier("modifier_fountain_aura_buff"))
                    {
                        foreach (Hero ally in allies)
                        {
                            if (Utils.SleepCheck("bottle") && ally != null)
                            {
                                bottle.UseAbility(ally);

                                Utils.Sleep(450, "bottle");
                            }
                        }
                    }



                }
            }
        }

        private static void AutoTalon(Unit unit)
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_iron_talon"))
            {
                Item talon = unit.FindItem("item_iron_talon");

                if (talon != null && talon.CanBeCasted())
                {
                    IEnumerable<Creep> monsters = ObjectManager.GetEntities<Creep>().Where(x => x.Team != me.Team && x.IsAlive && !x.IsAncient && x.Distance2D(unit) <= 600 && x.IsVisible && x.Health > 0 && !x.IsMagicImmune() && x.BaseMovementSpeed > 0 && x.IsSpawned && !x.Name.Contains("npc_dota_necronomicon_warrior") && !x.Name.Contains("npc_dota_necronomicon_archer"));
                    Creep highestHPCreep = monsters.MaxOrDefault(x => x.Health);

                    if (talon.Cooldown <= 0 && monsters.Count() > 0 && Utils.SleepCheck("Talon") && highestHPCreep.Distance2D(unit) <= 350 && MoreThanAnAttack(highestHPCreep, unit) && highestHPCreep != null && unit.IsAlive)
                    {
                        talon.UseAbility(highestHPCreep);
                        Utils.Sleep(250, "Talon");
                        unit.Attack(highestHPCreep);
                    }
                }
            }
        }

        private static bool MoreThanAnAttack(Unit creep, Unit unit)
        {
            var unitArmor = 1 - ((0.06 * creep.Armor) / (1 + 0.06 * Math.Abs(creep.Armor)));
            if (creep.Health >= (unit.DamageAverage + unit.BonusDamage) * (1 - creep.DamageResist) * unitArmor) return true;
            else return false;
        }

        private static void AutoMidas(Unit unit)
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_hand_of_midas"))
            {
                Item midas = unit.FindItem("item_hand_of_midas");

                if (midas != null && midas.CanBeCasted())
                {
                    IEnumerable<Creep> monsters = ObjectManager.GetEntities<Creep>().Where(x => x.Team != me.Team && x.IsAlive && !x.IsAncient && x.Distance2D(unit) <= 800 && x.IsVisible && x.Health > 0 && !x.IsMagicImmune() && x.BaseMovementSpeed > 0 && !x.Name.Contains("npc_dota_necronomicon_warrior") && !x.Name.Contains("npc_dota_necronomicon_archer"));
                    Creep highestHPCreep = monsters.MaxOrDefault(x => x.Health);

                    if (midas.Cooldown <= 0 && monsters.Count() > 0 && Utils.SleepCheck("Midas") && highestHPCreep.Distance2D(unit) <= 600 && highestHPCreep != null && unit.IsAlive)
                    {
                        midas.UseAbility(highestHPCreep);
                        Utils.Sleep(250, "Midas");
                    }
                }
            }
        }

        private static void AutoPhase(Unit unit)
        {
            if (ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_phase_boots"))
            {
                Item phase = unit.FindItem("item_phase_boots");

                if (phase != null && phase.Cooldown <= 0 && Utils.SleepCheck("Phase") && unit.IsMoving && !unit.IsChanneling() && !unit.IsInvisible() && unit.IsAlive)
                {
                    phase.UseAbility();
                    Utils.Sleep(250, "Phase");
                }
            }

        }

        private static void Bear()
        {
            var bear = ObjectManager.GetEntities<Unit>().Where(x => x.IsControllable && x.Name.Contains("npc_dota_lone_druid_bear") && x.IsAlive && !x.IsIllusion && x.IsSpawned && x.Health > 0).FirstOrDefault();

            if ((me.Name.Equals("npc_dota_hero_lone_druid") || me.Name.Equals("npc_dota_hero_rubick")) && bear != null && bear.IsAlive)
            {
                AutoMidas(bear);
                AutoTalon(bear);
                AutoDeward(bear);
            }
        }

        private static void AutoDeward(Unit unit)
        {
            if (!unit.IsAlive) return;
            Item aether = unit.FindItem("item_aether_lens");


            if (aether == null)
            {
                cutRange = 450;
            }
            else
            {
                cutRange = 650;
            }

            Unit ward = ObjectManager.GetEntities<Unit>()
                .FirstOrDefault(
                    x =>
                        (x.ClassID == ClassID.CDOTA_NPC_Observer_Ward ||
                         x.ClassID == ClassID.CDOTA_NPC_Observer_Ward_TrueSight || x.ClassID == ClassID.CDOTA_NPC_TechiesMines || x.ClassID == ClassID.CDOTA_NPC_Treant_EyesInTheForest)
                        && x.Team != me.Team && me.NetworkPosition.Distance2D(x.NetworkPosition) < cutRange &&
                        x.IsVisible && x.IsAlive);
            var cutter = cutters.Select(x => unit.FindItem(x)).FirstOrDefault(x => x != null && x.CanBeCasted());

            if(ward != null && !unit.IsChanneling() && Utils.SleepCheck("cut") && cutter != null && !((cutter.Name == "item_tango_single" || cutter.Name == "item_tango") && ward.ClassID == ClassID.CDOTA_NPC_TechiesMines))
            {
                Console.WriteLine("Reached");
                cutter.UseAbility(ward);
                Utils.Sleep(250, "cut");
            }
        }




    }
}
