﻿namespace AxeSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Objects;
    using SharpDX;
    using SharpDX.Direct3D9;

    internal class Program
    {
        private static readonly MenuItem EnableKeyItem =
            new MenuItem("Enable", "Enable").SetValue(true);
        private static readonly Dictionary<string, bool> unitsdict = new Dictionary<string, bool>
            {
                { "roshan_spell_block", true },
                { "visage_summon_familiars", true },
                { "lone_druid_spirit_bear", true },
                { "item_flying_courier", true },
                {"axe_culling_blade", true }
        };
        private static readonly MenuItem KillKeyItem =
            new MenuItem("xKill", "Kill:").SetValue(new AbilityToggler(unitsdict));
        private static readonly MenuItem PotentialDmgItem =
            new MenuItem("Show Potential Damage", "Show Potential Damage").SetValue(true).SetTooltip("Show potential damage of helix and items if enemy is called.");
        private static readonly Dictionary<string, bool> itemsDict = new Dictionary<string, bool>
            {
                { "item_dagon_5", true },
                { "item_shivas_guard", true },
                { "item_black_king_bar", true },
                { "item_blade_mail", true },
                {"item_crimson_guard", true },
                {"item_pipe", true },
                {"item_hood_of_defiance", true },
                {"item_lotus_orb", true },
                {"item_mjollnir", true },
        };
        private static readonly MenuItem ItemKeyItem =
    new MenuItem("Item", "Item").SetValue(new AbilityToggler(itemsDict));
        private static readonly MenuItem UltRangeItem = new MenuItem("Dunk Range", "Dunk Range").SetValue(new Slider(75, 500, 0));
        private static readonly MenuItem NumJumpItem = new MenuItem("# of Enemies to Jump", "# of Enemies to Jump").SetValue(new Slider(3, 5, 1));
        private static readonly MenuItem EnableJumpKeyItem =
            new MenuItem("Enable Jump", "Enable Jump").SetValue(true);


        private static readonly MenuItem TauntKeyItem =
            new MenuItem("Taunt", "Taunt").SetValue(new KeyBind('F', KeyBindType.Press)).SetTooltip("Press to become fire truck.");

        private static readonly MenuItem JumpItem =
            new MenuItem("Jump", "Jump").SetValue(new KeyBind('D', KeyBindType.Press)).SetTooltip("Hold to blink combo into enemies.");




        private static readonly Dictionary<Hero, double> HeroDamageDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Hero, double> HeroSpinDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Unit, double> UnitDamageDictionary = new Dictionary<Unit, double>();
        private static readonly Dictionary<Unit, string> UnitSpellDictionary = new Dictionary<Unit, string>();

        private static readonly Dictionary<Hero, Vector3> HeroJumpPosDictionary = new Dictionary<Hero, Vector3>();
        private static readonly Dictionary<Hero, int> HeroEnemyNearbyDictionary = new Dictionary<Hero, int>();




        private static readonly Menu Menu = new Menu("AxeSharp", "AxeSharp", true, "npc_dota_hero_axe", true);

        private static readonly List<double> Damage = new List<double>(new double[] { 250, 325, 400 });
        private static readonly List<double> Adamage = new List<double>(new double[] { 300, 425, 550 });
        private static readonly List<double> TauntDur = new List<double>(new double[] { 2, 2.4, 2.8, 3.2 });
        private static readonly int[] DagonDmg = new int[5] { 400, 500, 600, 700, 800 };
        private static double creepMaxHP = 550;



        private static bool _killError = true;
        private static bool _killStealEnabled;
        private static bool _comboInUse = false;

        private static Player _player;
        private static Hero _me;

        private static Item dagon, mjol, lotus, blademail, shiva, blink, pipe, crimson, hood, bkb, aether;
        //private static Ability call = _me.Spellbook.Spell1, hunger = _me.Spellbook.Spell2, helix = _me.Spellbook.Spell3, dunk = _me.Spellbook.Spell4;

        private static Unit vhero;// = new Unit();



        private static void Main()
        {
            Menu.AddToMainMenu();
            Menu.AddItem(EnableKeyItem);
            Menu.AddItem(KillKeyItem);
            Menu.AddItem(ItemKeyItem);
            Menu.AddItem(PotentialDmgItem);
            Menu.AddItem(TauntKeyItem);
            Menu.AddItem(UltRangeItem);
            Menu.AddItem(EnableJumpKeyItem);
            Menu.AddItem(JumpItem);
            Menu.AddItem(NumJumpItem);
            //Menu.AddItem(new MenuItem("Anti-Detection", "Anti-Detection").SetValue(true)).SetTooltip("Moves cursor during culling blade to prevent replay detection.");
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Player.OnExecuteOrder += Player_OnExecuteOrder;
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_killStealEnabled)
            {
                if (!Game.IsInGame) return;

                _player = ObjectManager.LocalPlayer;
                _me = ObjectManager.LocalHero;

                if (_player == null || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Axe) return;

                _killStealEnabled = true;
                Console.WriteLine("[AxeSharp: Loaded!]");
            }

            else if (!Game.IsInGame || _player == null || _me == null)
            {
                _killStealEnabled = false;
                UnitSpellDictionary.Clear();
                UnitDamageDictionary.Clear();
                HeroDamageDictionary.Clear();
                HeroSpinDictionary.Clear();
                UnitDamageDictionary.Clear();
                UnitSpellDictionary.Clear();
                HeroJumpPosDictionary.Clear();
                HeroEnemyNearbyDictionary.Clear();
                Console.WriteLine("[AxeSharp: UnLoaded!]");
                return;
            }

            if (Game.IsPaused || _me == null) return; //!Utils.SleepCheck("AXE ULT")
                                                      //Utils.Sleep(10, "AXE ULT");

            if (!Menu.Item("Enable").GetValue<bool>()) return;
            Calc();
            DunkHero(_me.Spellbook.Spell4, Damage, "normal", Adamage);
            Dunk(_me.Spellbook.Spell4, Damage, "normal", Adamage);
            Jump();
            Taunt();
            ManaCheck();


            //MouseEmulation.Hello();
            /*
            Point x;            
            MouseEmulation.GetCursorPos(out x);
            Console.WriteLine(x);
            */
        }


        private static void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (_comboInUse) args.Process = false;
        }

        private static void FindItem()
        {
            dagon = _me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            mjol = _me.FindItem("item_mjollnir");
            lotus = _me.FindItem("item_lotus_orb");
            blademail = _me.FindItem("item_blade_mail");
            shiva = _me.FindItem("item_shivas_guard");
            blink = _me.FindItem("item_blink");
            pipe = _me.FindItem("item_pipe");
            crimson = _me.FindItem("item_crimson_guard");
            hood = _me.FindItem("item_hood_of_defiance");
            bkb = _me.FindItem("item_black_king_bar");
        }


        private static uint ManaCheck()
        {
            var call = _me.Spellbook.Spell1;
            var dunk = _me.Spellbook.Spell4;

            uint manaCost = 0;
            FindItem();
            if (_me.IsAlive)
            {
                //Spells
                if (call.Cooldown <= 0 && call.Level > 0)
                    manaCost += call.ManaCost;
                if (dunk.Cooldown <= 0 && dunk.Level > 0)
                    manaCost += dunk.ManaCost;

                //Dagon
                if (dagon != null && dagon.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled("item_dagon_5"))
                    manaCost += dagon.ManaCost;

                //Items
                if (blademail != null && blademail.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(blademail.Name))
                    manaCost += blademail.ManaCost;
                if (mjol != null && mjol.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(mjol.Name))
                    manaCost += mjol.ManaCost;
                if (lotus != null && lotus.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(lotus.Name))
                    manaCost += lotus.ManaCost;
                if (crimson != null && crimson.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(crimson.Name))
                    manaCost += crimson.ManaCost;
                if (pipe != null && pipe.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(pipe.Name))
                    manaCost += pipe.ManaCost;
                if (hood != null && hood.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(hood.Name))
                    manaCost += hood.ManaCost;
                if (shiva != null && shiva.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(shiva.Name))
                    manaCost += shiva.ManaCost;

                if (bkb != null && bkb.Cooldown <= 0 && ItemKeyItem.GetValue<AbilityToggler>().IsEnabled(bkb.Name))
                    manaCost += bkb.ManaCost;


                //Console.WriteLine(manaCost);

                return manaCost;
            }

            return manaCost;
        }


        private static void Execute(Hero target, Vector3 blinkPos)
        {
            FindItem();
            var call = _me.Spellbook.Spell1;
            var dunk = _me.Spellbook.Spell4;
            var mana = _me.Mana;

            if (Game.IsKeyDown(JumpItem.GetValue<KeyBind>().Key) && !Game.IsChatOpen)
            { 

            //Console.WriteLine("X: " + blinkPos.X + " Y: " + blinkPos.Y + " Z: " + blinkPos.Z);

            if (blademail != null && blademail.Cooldown <= 0 && _me.Mana - blademail.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                blademail.UseAbility();
            if (mjol != null && mjol.Cooldown <= 0 && _me.Mana - mjol.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                mjol.UseAbility(_me);
            if (shiva != null && shiva.Cooldown <= 0 && _me.Mana - shiva.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                shiva.UseAbility();
            if (pipe != null && pipe.Cooldown <= 0 && _me.Mana - pipe.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                pipe.UseAbility();
            if (hood != null && hood.Cooldown <= 0 && _me.Mana - hood.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                hood.UseAbility();
            if (lotus != null && lotus.Cooldown <= 0 && _me.Mana - lotus.ManaCost >= call.ManaCost + dunk.ManaCost) //Add buttons
                lotus.UseAbility(_me);
            if (crimson != null && crimson.Cooldown <= 0) //Add buttons
                crimson.UseAbility();
            if (bkb != null && bkb.Cooldown <= 0) //Add buttons
                bkb.UseAbility();

            /*
            if (Utils.SleepCheck("console"))
            {
                Console.WriteLine("==================================================================");
                Console.WriteLine("Mjol: " + mjol.ManaCost ?? "Fuck");
                Console.WriteLine("Shiva: " + (shiva.ManaCost ?? "Fuck"));
                Console.WriteLine("Pipe: " + (pipe.ManaCost ?? "Fuck"));
                Console.WriteLine("Hood: " + (hood.ManaCost ?? "Fuck"));
                Console.WriteLine("Lotus: " + (lotus.ManaCost ?? "Fuck"));
                Console.WriteLine("==================================================================");
                Utils.Sleep(1200, "console");
            }
            */



            _comboInUse = true;
            _me.Move(blinkPos);
            if (!(_me.Distance2D(target) <= 300)) blink.UseAbility(blinkPos);
            call.UseAbility();
            if (dagon != null && dagon.Cooldown <= 0 && _me.Mana - dagon.ManaCost >= call.ManaCost + dunk.ManaCost)
                dagon.UseAbility(target);
            DelayAction.Add(400 - Game.Ping, SetFalse);
            }
        }

        private static void SetFalse()
        {
            _comboInUse = false;
        }


        private static void Jump()
        {
            var blink = _me.FindItem("item_blink");
            var aether = _me.FindItem("item_aether_lens");
            var range = (aether != null) ? 1400 : 1200;
            var call = _me.Spellbook.Spell1;

            if (Menu.Item("Enable Jump").GetValue<bool>())
            {
                IEnumerable<Hero> enemyHeroesWithinRange = ObjectManager.GetEntities<Hero>().Where(x => x.IsAlive && x.Team != _me.Team && x.Health > 0 && x.IsVisible && x.Distance2D(_me) <= range);

                foreach (Hero hero in enemyHeroesWithinRange)
                {
                    var alliesWithin300 = enemyHeroesWithinRange.Where(x => x.Distance2D(hero) <= 300);
                    int nearbyEnemies = alliesWithin300.Count();
                    var avgX = alliesWithin300.Sum(x => x.Position.X) / alliesWithin300.Count();
                    var avgY = alliesWithin300.Sum(x => x.Position.Y) / alliesWithin300.Count();
                    var avgZ = alliesWithin300.Sum(x => x.Position.Z) / alliesWithin300.Count();

                    Vector3 midPoint = new Vector3(avgX, avgY, avgZ);

                    HeroEnemyNearbyDictionary.Add(hero, nearbyEnemies);
                    HeroJumpPosDictionary.Add(hero, midPoint);
                }

                var mostEnemies = HeroEnemyNearbyDictionary.Values.Max();
                var heroWithMostEnemy = HeroEnemyNearbyDictionary.OrderByDescending(x => x.Value).FirstOrDefault();



            }



                    
                    if (nearbyEnemies >= Menu.Item("# of Enemies to Jump").GetValue<Slider>().Value)
                    {
                        Vector3 blinkPos;
                        HeroJumpPosDictionary.TryGetValue(hero, out blinkPos);
                        //double hpPercent = Convert.ToDouble(_me.Health) / Convert.ToDouble(_me.MaximumHealth);
                        if (blink.Cooldown <= 0 && call.Cooldown <= 0 && Utils.SleepCheck("Jump") && _me.Mana >= call.ManaCost && _me.IsAlive)
                        {
                            Execute(hero, blinkPos);
                            Utils.Sleep(3000, "Jump");
                            HeroJumpPosDictionary.Clear();
                            HeroEnemyNearbyDictionary.Clear();
                            break;
                        }
                        HeroJumpPosDictionary.Clear();
                        HeroEnemyNearbyDictionary.Clear();
                    }
                    HeroJumpPosDictionary.Clear();
                    HeroEnemyNearbyDictionary.Clear();
                }
            }
        }




        private static void Calc()
        {
            var call = _me.Spellbook.Spell1;
            var helix = _me.Spellbook.Spell3;

            if (!(call.Level > 0) && !(helix.Level > 0)) return;

            var holdDur = TauntDur[Convert.ToInt32(_me.Spellbook.Spell1.Level - 1)];
            //var regen = _me.HealthRegeneration; // ADD THIS IN SOMEDAY.






            //Damage depending on Heroes.
            var enemies = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0);
            foreach (Hero enemy in enemies)
            {


                var damageAmp = (_me.HasItem(ClassID.CDOTA_Item_Aether_Lens) ? ((_me.TotalIntelligence / 16) * 0.01) + 0.05 : ((_me.TotalIntelligence / 16) * 0.01));
                //Initialize.
                var dagon = _me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
                var shiva = _me.FindItem("item_shivas_guard");
                var mjol = _me.FindItem("item_mjollnir");
                var blademail = _me.FindItem("item_blade_mail");
                var enemyRadiance = enemy.FindItem("item_radiance");
                double mjolDmg = 0;
                double shivaDmg = 0;
                double reflectedDmg = 0;
                double spinDamage = 0;
                double dagonDmg = 0;


                //Damage done by Axe right-click during Call.
                var attacksPS = _me.AttacksPerSecond;
                var numAttacks = Math.Round(holdDur * attacksPS);
                var mainAtkDamage = _me.DamageAverage + _me.BonusDamage;
                var finalAtkDamage = mainAtkDamage * (1 - enemy.DamageResist);
                var rclickDamage = finalAtkDamage * numAttacks;

                //Damage done by Axe Helix during Call.
                var helixSpinDmg = Math.Floor(helix.GetDamage(helix.Level - 1) * (1 + damageAmp));
                var helixCD = helix.GetCooldown((helix.Level - 1));
                var MaxNumSpins = Math.Floor((holdDur / helixCD));

                var creeps = ObjectManager.GetEntities<Unit>().Where(creep => creep.Team == _me.GetEnemyTeam() && creep.IsAlive && creep.Health > 0 && creep.Distance2D(enemy) <= 500); //Add support for neutrals.
                var aCreep = creeps.FirstOrDefault(creep => creep.UnitType == 1152 && creep.IsMelee);

                var spinsBeforeDeath = Math.Ceiling(creepMaxHP / helixSpinDmg);

                if (aCreep != null) creepMaxHP = aCreep.MaximumHealth;

                //Spins by Creeps.
                int creepCount = creeps.Count() - 1; //Remove the hero itself.
                var creepNumAtks = Math.Round(holdDur) * creepCount * (_me.HasItem(ClassID.CDOTA_Item_Shivas_Guard) ? 0.55 : 1);
                var creepSpins = (creepNumAtks / 4); //4 is the Most Probable N.
                var trueCreepSpins = Math.Min(spinsBeforeDeath, creepSpins);

                //Spins by Heroes.
                var heroAPS = enemy.AttacksPerSecond;
                var heroNumAttacks = Math.Round(holdDur * heroAPS);
                var heroSpins = (heroNumAttacks / 4); //4 is the Most Probable N.

                //Total Spins and Total Damage Done by Axe Helix During Call.
                var totalSpins = Math.Min(trueCreepSpins + heroSpins, MaxNumSpins);
                spinDamage = totalSpins * helixSpinDmg;



                //Damage done by Shiva
                if (shiva != null && _me.Mana >= 100 && shiva.Cooldown <= 0)
                {
                    shivaDmg = 200 * (1 + damageAmp) * (1 - enemy.MagicDamageResist);
                }

                //Damage done by Dagon                
                if (dagon != null && _me.Mana >= dagon.ManaCost && dagon.Cooldown <= 0)
                {
                    dagonDmg = DagonDmg[dagon.Level - 1] * (1 + damageAmp) * (1 - enemy.MagicDamageResist);
                }

                //Damage done by Mjolnir.                
                if (mjol != null && _me.Mana >= 50 && mjol.Cooldown <= 0)
                {
                    var mjolPassiveDmg = 150 * (1 - enemy.MagicDamageResist) * (1 + damageAmp);
                    var mjolActiveDmg = 200 * (1 - enemy.MagicDamageResist) * (1 + damageAmp);
                    var mjolPassiveDmgTotal = mjolPassiveDmg * (numAttacks / 3); //3 hits  from self
                    var mjolActiveDmgTotal = mjolActiveDmg * totalSpins; //4hits from enemy so same as totalSpins                        
                    mjolDmg = mjolPassiveDmgTotal + mjolActiveDmgTotal;
                }

                //Damage done by Axe blademail during Call.        
                if (blademail != null && blademail.Cooldown <= 0)
                {
                    var heroMainAtkDamage = enemy.DamageAverage + enemy.BonusDamage;
                    var heroResist = enemy.DamageResist;
                    var heroFinalAtkDmg = heroMainAtkDamage * (1 - heroResist);

                    var heroAtkPS = enemy.AttacksPerSecond;
                    var heroNumAtks = Math.Round(holdDur * heroAtkPS);


                    //Reflected Radiance Damage.
                    var enemyDamageAmp = (enemy.HasItem(ClassID.CDOTA_Item_Aether_Lens) ? ((enemy.TotalIntelligence / 16) * 0.01) + 0.05 : ((enemy.TotalIntelligence / 16) * 0.01));
                    var radianceDmg = (enemyRadiance == null ? 0 : 50 * (1 + enemyDamageAmp) * (1 - enemy.MagicDamageResist) * holdDur);


                    //Damage done by BladeMail
                    reflectedDmg = heroFinalAtkDmg * heroNumAtks + radianceDmg + shivaDmg;

                    //Consider radiance burn damage?
                    //Consider medusa mana shield/enchantress skin/slark stacks/meepo slow/spectre desolate/weaver shukuchi/clinkz arrow/drow arrow slow/huskar/OD/spectre?/alchemist acid/ursa fury swipes/enemy crit, brew, mort, daedelus, crystalus, bloodthorn, chaos crit/centaur return/anti mage mana burn/ember guard/veno poison sting/viper skin/silencer glaives/necrophos passive/ion shell/

                    //Add support for creep prediction
                    // DOES NOT SUPPORT EVASION
                }

                double totalDamage = reflectedDmg + spinDamage + rclickDamage + mjolDmg + shivaDmg + dagonDmg;

                if (!HeroSpinDictionary.TryGetValue(enemy, out totalDamage))
                {
                    HeroSpinDictionary.Add(enemy, totalDamage);
                    totalDamage = reflectedDmg + spinDamage + rclickDamage + mjolDmg + shivaDmg + dagonDmg;

                }
                else
                {
                    HeroSpinDictionary.Remove(enemy);

                    totalDamage = reflectedDmg + spinDamage + rclickDamage + mjolDmg + shivaDmg + dagonDmg;
                    HeroSpinDictionary.Add(enemy, totalDamage);
                }



            }
        }

        private static void Dunk(Ability ability, IReadOnlyList<double> damage, string abilityType = "normal", IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("visage_summon_familiars") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("item_flying_courier")) return;

            var spellLevel = (int)ability.Level - 1; // base 0 index system
            if (ability.Level <= 0) return;

            double normalDamage = _me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = ability.DamageType;
            var spellRange = (ability.CastRange + Menu.Item("Dunk Range").GetValue<Slider>().Value);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000); // This should always be 0 since _killerror never changes.

            var bears = ObjectManager.GetEntities<Unit>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.ClassID == ClassID.CDOTA_Unit_SpiritBear);
            var roshans = ObjectManager.GetEntities<Unit>().Where(enemy => enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.ClassID == ClassID.CDOTA_Unit_Roshan);
            var couriers = ObjectManager.GetEntities<Unit>().Where(x => x.IsAlive && x.Team != _me.Team && x.ClassID.Equals(ClassID.CDOTA_Unit_Courier));
            var familiars = ObjectManager.GetEntities<Unit>().Where(x => x.IsAlive && x.Team != _me.Team && x.ClassID == ClassID.CDOTA_Unit_VisageFamiliar);

            IEnumerable<Unit> enemies = Enumerable.Empty<Unit>();

            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear")) enemies = enemies.Union(bears);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block")) enemies = enemies.Union(roshans);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("visage_summon_familiars")) enemies = enemies.Union(familiars);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("item_flying_courier")) enemies = enemies.Union(couriers);

            foreach (var enemy in enemies)
            {
                double spellDamage = normalDamage;

                var damageDone = (float)spellDamage;

                double damageNeeded;


                if (!UnitDamageDictionary.TryGetValue(enemy, out damageNeeded))
                {
                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration;
                    UnitDamageDictionary.Add(enemy, damageNeeded);
                    UnitSpellDictionary.Add(enemy, ability.Name);

                }
                else
                {
                    UnitDamageDictionary.Remove(enemy);
                    UnitSpellDictionary.Remove(enemy);



                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration;
                    UnitDamageDictionary.Add(enemy, damageNeeded);
                    UnitSpellDictionary.Add(enemy, ability.Name);
                }


                if (_me.IsChanneling()) return;

                if (!(damageNeeded < 0) || !(_me.Distance2D(enemy) < spellRange) || !MeCanSurvive(enemy, _me, ability, damageDone)) continue;


                CastSpell(ability, enemy);
                break;
            }
        }


        private static void DunkHero(Ability ability, IReadOnlyList<double> damage, string abilityType = "normal", IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) return;
            var spellLevel = (int)ability.Level - 1;
            if (ability.Level <= 0) return;

            double normalDamage = _me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = ability.DamageType;
            var spellRange = (ability.CastRange + Menu.Item("Dunk Range").GetValue<Slider>().Value);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);


            var enemies = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0);

            foreach (var enemy in enemies)
            {
                double spellDamage = normalDamage;

                var damageDone = (float)spellDamage;

                double damageNeeded;

                if (!HeroDamageDictionary.TryGetValue(enemy, out damageNeeded))
                {
                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration + MorphMustDie(enemy, spellCastPoint);
                    HeroDamageDictionary.Add(enemy, damageNeeded);

                }
                else
                {
                    HeroDamageDictionary.Remove(enemy);


                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration + MorphMustDie(enemy, spellCastPoint);

                    HeroDamageDictionary.Add(enemy, damageNeeded);

                }
                if (_me.IsChanneling()) return;

                if (!(damageNeeded < 0) || !(_me.Distance2D(enemy) < spellRange || !MeCanSurvive(enemy, _me, ability, damageDone))) continue;


                CastSpell(ability, enemy);
                break;
            }
        }



        private static bool MeCanSurvive(Unit enemy, Hero me, Ability spell, double damageDone)
        {
            return (me.IsMagicImmune() || (NotDieFromSpell(spell, enemy, me) && NotDieFromLotusOrb(enemy, me, damageDone)));
        }

        private static bool NotDieFromLotusOrb(Unit enemy, Unit me, double damageDone)
        {
            return !(enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_lotus_orb_active") != null && me.Health < damageDone);
        }

        private static bool NotDieFromSpell(Ability spell, Unit enemy, Hero me)
        {
            if (me.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_pugna_nether_ward_aura") == null)
                return true;
            return !(me.Health < me.DamageTaken((spell.ManaCost * (float)1.75), DamageType.Magical, enemy));
        }

        private static void CastSpell(Ability spell, Unit target)
        {
            if (spell.Cooldown > 0) return;

            if (target.ClassID == ClassID.CDOTA_Unit_VisageFamiliar)
            {
                if (Utils.SleepCheck("ks") && CanBeCasted(spell) && _me.CanCast() && !target.IsInvul())
                {
                    spell.UseAbility(target);
                    //vhero = target;
                    Utils.Sleep(50, "ks");
                }
            }
            //spell.CanBeCasted

            //!target.IsLinkensProtected()

            //CanBeCasted(spell)

            else if (Utils.SleepCheck("ks") && CanBeCasted(spell) && _me.CanCast() && !target.HasModifier("modifier_skeleton_king_reincarnation_scepter_active") && !(target.ClassID == ClassID.CDOTA_Unit_Roshan && target.Spellbook.Spell1.Cooldown < 1) && !target.IsInvul())
            {
                spell.UseAbility(target);
                vhero = target;
                DelayAction.Add(300 - Game.Ping, CancelUlt);
                Utils.Sleep(50, "ks");
            }


        }

        private static void CancelUlt()
        {
            var dmg = Damage[Convert.ToInt32(_me.Spellbook.Spell4.Level - 1)];

            if (_me.HasItem(ClassID.CDOTA_Item_UltimateScepter))
            {
                dmg = Adamage[Convert.ToInt32(_me.Spellbook.Spell4.Level - 1)];
            }

            if (vhero.Health > dmg || vhero.IsLinkensProtected() || vhero.HasModifier("modifier_skeleton_king_reincarnation_scepter_active"))
            {
                _me.Stop();
                _me.Attack(vhero);
                //vhero = null; //new Unit();
            }
        }


        private static bool CanBeCasted(Ability ability)
        {
            return ability != null && ability.Cooldown.Equals(0) && ability.Level > 0 && _me.Mana > ability.ManaCost;
        }

        private static float MorphMustDie(Hero target, float value)
        {
            if (target.ClassID != ClassID.CDOTA_Unit_Hero_Morphling) return 0;

            var morphLevel = target.Spellbook.Spell3.Level;
            if (morphLevel <= 0) return 0;

            uint[] morphGain = { 38, 76, 114, 190 };
            if (target.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_morphling_morph_agi") != null && target.Strength > 1)
                return value * (0 - morphGain[morphLevel - 1] + 1);
            if (target.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_morphling_morph_str") != null && target.Agility > 1)
                return value * morphGain[morphLevel - 1];
            return 0;
        }



        private static void Game_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _player == null || _me == null)
                return;

            if ((!Menu.Item("Enable").GetValue<bool>())) return;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsVisible && hero.IsAlive && !hero.IsIllusion()).ToList();
            var units = ObjectManager.GetEntities<Unit>().Where(unit => unit.IsVisible && unit.IsAlive && !unit.IsIllusion() && (unit.ClassID == ClassID.CDOTA_Unit_SpiritBear || unit.ClassID == ClassID.CDOTA_Unit_Roshan || unit.ClassID == ClassID.CDOTA_Unit_VisageFamiliar || unit.ClassID == ClassID.CDOTA_Unit_Courier)).ToList();

            var enemiesAndUnits = enemies.Union(units);

            foreach (var enemy in enemies)
            {
                if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) continue;
                Vector2 screenPos;
                var enemyPos = enemy.Position + new Vector3(0, 0, enemy.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos + new Vector2(-51, -40);
                double damageNeeded;
                double totalDamage;


                if (!HeroDamageDictionary.TryGetValue(enemy, out damageNeeded) || !HeroSpinDictionary.TryGetValue(enemy, out totalDamage)) continue;

                var percent = (totalDamage / damageNeeded) >= 2 ? 1000 : Math.Abs(Math.Round(100 * (totalDamage / damageNeeded) * 0.75));

                string text = "KS: " + string.Format("{0}", (int)damageNeeded) + string.Format(" | {0}", (int)totalDamage);

                if (!Menu.Item("Show Potential Damage").GetValue<bool>())
                {
                    text = "KS: " + string.Format("{0}", (int)damageNeeded);
                }

                var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 150), FontFlags.None);
                var textPos = start + new Vector2(51 - textSize.X / 1, -textSize.Y / 1 + 2);
                Drawing.DrawText(text, "Arial", textPos, new Vector2(20, 0), ColorChoice(damageNeeded, totalDamage), FontFlags.DropShadow);
            }

            foreach (var unit in units)
            {
                if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block") && (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear"))) return;
                Vector2 screenPos;
                var enemyPos = unit.Position + new Vector3(0, 0, unit.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos + new Vector2(-51, -40);
                double damageNeeded;
                string spell;
                if (!UnitDamageDictionary.TryGetValue(unit, out damageNeeded) || !UnitSpellDictionary.TryGetValue(unit, out spell)) continue;

                var text = "KS:  " + string.Format("{0}", (int)damageNeeded);
                var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 150), FontFlags.None);
                var textPos = start + new Vector2(51 - textSize.X / 1, -textSize.Y / 1 + 2);
                Drawing.DrawText(text, "Arial", textPos, new Vector2(20, 0), damageNeeded < 0 ? Color.Red : Color.White, FontFlags.DropShadow);
            }
        }

        private static Color ColorChoice(double damageNeeded, double totalDamage)
        {
            if (damageNeeded <= 0)
                return Color.Red;
            else if (damageNeeded <= 0.5 * totalDamage)
                return Color.Orange;
            else if (damageNeeded <= totalDamage)
                return Color.Yellow;
            else if (damageNeeded <= 1.2 * totalDamage)
                return Color.LightGreen;
            else
                return Color.White;
        }









        private static void Taunt()
        {
            var hunger = _me.Spellbook.Spell2;
            IEnumerable<Unit> closeEnemies1 = ObjectManager.GetEntities<Unit>().Where(x => x.Team != _me.Team && x.Distance2D(_me) <= hunger.CastRange && x.IsAlive && x.Health > 0 && x.IsVisible && !x.IsMagicImmune());
            IEnumerable<Hero> closeEnemies2 = ObjectManager.GetEntities<Hero>().Where(x => x.Team != _me.Team && x.Distance2D(_me) <= hunger.CastRange && x.IsAlive && x.Health > 0 && x.IsVisible && !x.IsMagicImmune());

            Hero closeEnemy2 = closeEnemies2.FirstOrDefault();
            Unit closeEnemy1 = closeEnemies1.FirstOrDefault();




            if (Game.IsKeyDown(TauntKeyItem.GetValue<KeyBind>().Key) && Utils.SleepCheck("Taunt") && hunger.Level >= 1)
            {
                if (closeEnemy1 == null && closeEnemy2 == null) return;
                else if (closeEnemy2 == null) hunger.UseAbility(closeEnemy1);
                else if (closeEnemy1 == null) Console.WriteLine("hi");// hunger.UseAbility(closeEnemy2);

                DelayAction.Add(Convert.ToInt32(hunger.FindCastPoint() / 2), _me.Stop);
                Utils.Sleep(100, "Taunt");
            }


        }


    }


}



