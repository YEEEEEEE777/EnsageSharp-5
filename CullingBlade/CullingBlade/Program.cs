namespace CullingBlade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using SharpDX;


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
            new MenuItem("Show Potential Damage", "Show Potential Damage").SetValue(true).SetTooltip("Show potential damage output of helix and items if enemy is called.");

        private static readonly MenuItem UltRangeItem = new MenuItem("Dunk Range", "Dunk Range").SetValue(new Slider(75, 500, 0));


        private static readonly Dictionary<Hero, double> HeroDamageDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Hero, double> HeroSpinDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Unit, double> UnitDamageDictionary = new Dictionary<Unit, double>();


        private static readonly Dictionary<Hero, Vector3> HeroJumpPosDictionary = new Dictionary<Hero, Vector3>();
        private static readonly Dictionary<Hero, int> HeroEnemyNearbyDictionary = new Dictionary<Hero, int>();




        private static readonly Menu Menu = new Menu("CullingBlade", "CullingBlade", true, "npc_dota_hero_axe", true);




        private static readonly List<double> Damage = new List<double>(new double[] { 250, 325, 400 });
        private static readonly List<double> Adamage = new List<double>(new double[] { 300, 425, 550 });
        private static readonly List<double> TauntDur = new List<double>(new double[] { 2, 2.4, 2.8, 3.2 });
        private static readonly int[] DagonDmg = new int[5] { 400, 500, 600, 700, 800 };

        private static double creepMaxHP = 550;



        private static bool _killError = true;
        private static bool _killStealEnabled;
        private static bool _comboInUse = false;

        private static Player player;
        private static Hero me;

        private static Unit vhero;// = new Unit();






        private static void Main()
        {


            Menu.AddItem(EnableKeyItem);
            Menu.AddItem(PotentialDmgItem);
            Menu.AddItem(KillKeyItem);
            Menu.AddItem(UltRangeItem);
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;

        }


        private static void Game_OnUpdate(EventArgs args)
        {


            if (!_killStealEnabled)
            {



                if (!Game.IsInGame) return;
                player = ObjectManager.LocalPlayer;
                me = ObjectManager.LocalHero;
                if (player == null || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Axe) return;
                _killStealEnabled = true;
                Console.WriteLine("[CullingBlade: Loaded!]");
            }


            else if (!Game.IsInGame || player == null || me == null)
            {


                _killStealEnabled = false;

                UnitDamageDictionary.Clear();
                HeroDamageDictionary.Clear();
                HeroSpinDictionary.Clear();
                UnitDamageDictionary.Clear();

                HeroJumpPosDictionary.Clear();
                HeroEnemyNearbyDictionary.Clear();
                Console.WriteLine("[CullingBlade: UnLoaded!]");
                return;
            }

            if (Game.IsPaused || me == null) return;


            if (!Menu.Item("Enable").GetValue<bool>()) return;

            DamageCalculation();
            Dunk(Damage, Adamage);


        }


        private static void DamageCalculation()
        {
            Ability call = me.Spellbook.Spell1;
            Ability helix = me.Spellbook.Spell3;

            if (call == null || helix == null) return;
            if ((call.Level <= 0) || (helix.Level <= 0)) return;

            var holdDur = TauntDur[Convert.ToInt32(call.Level - 1)];

            //Damage depending on Heroes.
            var enemies = ObjectManager.GetEntitiesParallel<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0);
            foreach (Hero enemy in enemies)
            {


                var damageAmp = (me.HasItem(ClassID.CDOTA_Item_Aether_Lens) ? ((me.TotalIntelligence / 16) * 0.01) + 0.05 : ((me.TotalIntelligence / 16) * 0.01));
                //Initialize.
                var dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
                var shiva = me.FindItem("item_shivas_guard");
                var mjol = me.FindItem("item_mjollnir");
                var blademail = me.FindItem("item_blade_mail");
                var enemyRadiance = enemy.FindItem("item_radiance");
                double mjolDmg = 0;
                double shivaDmg = 0;
                double reflectedDmg = 0;
                double spinDamage = 0;
                double dagonDmg = 0;


                //Damage done by Axe right-click during Call.
                var attacksPS = me.AttacksPerSecond;
                var numAttacks = Math.Round(holdDur * attacksPS);
                var mainAtkDamage = me.DamageAverage + me.BonusDamage;
                var finalAtkDamage = mainAtkDamage * (1 - enemy.DamageResist);
                var rclickDamage = finalAtkDamage * numAttacks;

                //Damage done by Axe Helix during Call.
                var helixSpinDmg = Math.Floor(helix.GetDamage(helix.Level - 1) * (1 + damageAmp));
                var helixCD = helix.GetCooldown((helix.Level - 1));
                var MaxNumSpins = Math.Floor((holdDur / helixCD));

                var creeps = ObjectManager.GetEntitiesParallel<Unit>().Where(creep => creep.Team == me.GetEnemyTeam() && creep.IsAlive && creep.Health > 0 && creep.Distance2D(enemy) <= 500); //Add support for neutrals.
                var aCreep = creeps.FirstOrDefault(creep => creep.UnitType == 1152 && creep.IsMelee);

                var spinsBeforeDeath = Math.Ceiling(creepMaxHP / helixSpinDmg);

                if (aCreep != null) creepMaxHP = aCreep.MaximumHealth;

                //Spins by Creeps.
                int creepCount = creeps.Count() - 1; //Remove the hero itself.
                var creepNumAtks = Math.Round(holdDur) * creepCount * (me.HasItem(ClassID.CDOTA_Item_Shivas_Guard) ? 0.55 : 1);
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
                if (shiva != null && me.Mana >= 100 && shiva.Cooldown <= 0)
                {
                    shivaDmg = 200 * (1 + damageAmp) * (1 - enemy.MagicDamageResist);
                }

                //Damage done by Dagon                
                if (dagon != null && me.Mana >= dagon.ManaCost && dagon.Cooldown <= 0)
                {
                    dagonDmg = DagonDmg[dagon.Level - 1] * (1 + damageAmp) * (1 - enemy.MagicDamageResist);
                }

                //Damage done by Mjolnir.                
                if ((mjol != null && me.Mana >= 50 && mjol.Cooldown <= 0) || me.Modifiers.Any(modifier => modifier.Name == "modifier_item_mjollnir_static" && modifier.RemainingTime >= holdDur))
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
                    var heroTotalPhysicalDamage = enemy.DamageAverage + enemy.BonusDamage;
                    var heroResist = enemy.DamageResist;
                    var heroFinalAtkDmg = heroTotalPhysicalDamage * (1 - heroResist);
                    

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

        private static double MagicStick(Unit target)
        {
            var enemyStick = target.FindItem("item_magic_stick");
            var enemyWand = target.FindItem("item_magic_wand");

            if (enemyStick == null && enemyWand == null) return Convert.ToDouble(0);

            else
            {
                var mainHealingStick = enemyStick == null ? enemyWand : enemyStick;

                if (mainHealingStick.Cooldown > 0) return Convert.ToDouble(0);

                long enemyStickWandHealPerCharge = 15;
                long totalStickWandHeal = mainHealingStick.CurrentCharges * enemyStickWandHealPerCharge;
                //Console.WriteLine("Heal: " + totalStickWandHeal);
                return Convert.ToDouble(totalStickWandHeal);
            }
        }

        private static void Dunk(IReadOnlyList<double> damage, IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("visage_summon_familiars") && !Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("item_flying_courier")) return;

            var cullingBlade = me.Spellbook.Spell4;

            IEnumerable<Unit> enemies = Enumerable.Empty<Unit>();

            var bears = ObjectManager.GetEntitiesParallel<Unit>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.Name.Contains("druid_bear"));
            var roshans = ObjectManager.GetEntitiesParallel<Unit>().Where(enemy => enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.Name.Contains("roshan"));
            var couriers = ObjectManager.GetEntitiesParallel<Unit>().Where(x => x.IsAlive && x.Team != me.Team && x.Name.Contains("courier"));
            var familiars = ObjectManager.GetEntitiesParallel<Unit>().Where(x => x.IsAlive && x.Team != me.Team && x.Name.Contains("familiar"));
            var heroes = ObjectManager.GetEntitiesParallel<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0);

            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear")) enemies = enemies.Union(bears);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block")) enemies = enemies.Union(roshans);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("visage_summon_familiars")) enemies = enemies.Union(familiars);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("item_flying_courier")) enemies = enemies.Union(couriers);
            if (Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) enemies = enemies.Union(heroes);



            if (cullingBlade.Level <= 0)
            {
                foreach (var enemy in enemies)
                {


                    double damageNeeded;


                    if (!UnitDamageDictionary.TryGetValue(enemy, out damageNeeded))
                    {

                        damageNeeded = enemy.Health + MagicStick(enemy);
                        UnitDamageDictionary.Add(enemy, damageNeeded);
                    }

                    else
                    {
                        UnitDamageDictionary.Remove(enemy);
                        damageNeeded = enemy.Health + MagicStick(enemy);
                        UnitDamageDictionary.Add(enemy, damageNeeded);
                    }



                }

                return;
            }
            




            var spellLevel = (int)cullingBlade.Level - 1; // base 0 index system

            double normalDamage = me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = cullingBlade.DamageType;
            var spellRange = (cullingBlade.CastRange + Menu.Item("Dunk Range").GetValue<Slider>().Value);
            var spellCastPoint = (cullingBlade.GetCastPoint(cullingBlade.Level) + (Game.Ping / 1000));//(float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000); // This should always be 0 since _killerror never changes.

            //Console.WriteLine(Game.Ping/1000);
            //Console.WriteLine(ability.GetCastPoint(ability.Level));





            //Add support for priority and facing direction.
            foreach (var enemy in enemies)
            {

                //Console.WriteLine(enemy.UnitType);
                double spellDamage = normalDamage;

                var damageDone = (float)spellDamage;

                double damageNeeded;
                //Console.WriteLine(enemy.HealthRegeneration);

                if (!UnitDamageDictionary.TryGetValue(enemy, out damageNeeded))
                {

                    damageNeeded = enemy.Health - damageDone + (spellCastPoint * enemy.HealthRegeneration) + MagicStick(enemy);
                    UnitDamageDictionary.Add(enemy, damageNeeded);
                }

                else
                {
                    UnitDamageDictionary.Remove(enemy);
                    damageNeeded = enemy.Health - damageDone + (spellCastPoint * enemy.HealthRegeneration) + MagicStick(enemy);
                    UnitDamageDictionary.Add(enemy, damageNeeded);
                }


                //if (me.IsChanneling()) return;

                if (!(damageNeeded < 0) || !(me.Distance2D(enemy) < spellRange) || !meCanSurvive(enemy, me, cullingBlade, damageDone)) continue;


                CastSpell(cullingBlade, enemy);
                //break; //what is this for?? commented it to optimize.
            }

        }






        private static bool meCanSurvive(Unit enemy, Hero me, Ability spell, double damageDone)
        {
            return (me.IsMagicImmune() || (notDieFromSpell(spell, enemy, me) && notDieFromLotusOrb(enemy, me, damageDone)));
        }

        private static bool notDieFromLotusOrb(Unit enemy, Unit me, double damageDone)
        {
            return !(enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_lotus_orb_active") != null && me.Health < damageDone);
        }

        private static bool notDieFromSpell(Ability spell, Unit enemy, Hero me)
        {
            if (me.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_pugna_nether_ward_aura") == null)
                return true;
            return !(me.Health < me.DamageTaken((spell.ManaCost * (float)1.75), DamageType.Magical, enemy));
        }

        private static void CastSpell(Ability spell, Unit target)
        {
            if (spell.Cooldown > 0) return;

            if (target.Name.Contains("familiar"))
            {
                if (Utils.SleepCheck("ks") && CanBeCasted(spell) && me.CanCast() && !target.IsInvul())
                {
                    spell.UseAbility(target);
                    //vhero = target;
                    Utils.Sleep(50, "ks");
                }
            }
            //spell.CanBeCasted

            //!target.IsLinkensProtected()

            //CanBeCasted(spell)

            else if (Utils.SleepCheck("ks") && CanBeCasted(spell) && me.CanCast() && !target.HasModifier("modifier_skeleton_king_reincarnation_scepter_active") && !(target.Name.Contains("roshan") && target.Spellbook.Spell1.Cooldown < 1) && !target.IsInvul())
            {
                spell.UseAbility(target);

                vhero = target;

                DelayAction.Add((spell.GetCastPoint(spell.Level) * 1000) - Game.Ping, CancelUlt);

                Utils.Sleep(25, "ks");
            }


        }

        private static void CancelUlt()
        {

            var dmg = Damage[Convert.ToInt32(me.Spellbook.Spell4.Level - 1)];
            var agh = me.FindItem("item_ultimate_scepter");

            if (agh != null)
            {
                dmg = Adamage[Convert.ToInt32(me.Spellbook.Spell4.Level - 1)];
            }


            if (vhero.Health > dmg || vhero.IsLinkensProtected() || vhero.HasModifier("modifier_skeleton_king_reincarnation_scepter_active"))
            {
                me.Stop();
                me.Attack(vhero);
            }
        }


        private static bool CanBeCasted(Ability ability)
        {
            return ability != null && ability.Cooldown.Equals(0) && ability.Level > 0 && me.Mana > ability.ManaCost;
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
            if (!Game.IsInGame || player == null || me == null)
                return;

            if ((!Menu.Item("Enable").GetValue<bool>())) return;

            var enemyHeroes = ObjectManager.GetEntitiesParallel<Hero>().Where(hero => hero.IsVisible && hero.IsAlive && !hero.IsIllusion()).ToList();
            var enemyUnits = ObjectManager.GetEntities<Unit>().Where(unit => unit.IsVisible && unit.IsAlive && !unit.IsIllusion() && (unit.Name.Contains("druid_bear") || unit.Name.Contains("roshan") || unit.Name.Contains("familiar") || unit.Name.Contains("courier"))).ToList();
            //var enemiesAndUnits = enemies2.Union(units);

            foreach (var enemy in enemyHeroes)
            {
                if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) continue;
                Vector2 screenPos;
                var enemyPos = enemy.Position + new Vector3(0, 0, enemy.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos + new Vector2(-51, -40);
                double damageNeeded;
                double totalDamage;


                if (!UnitDamageDictionary.TryGetValue(enemy, out damageNeeded) || !HeroSpinDictionary.TryGetValue(enemy, out totalDamage)) continue;

                //if (UnitDamageDictionary.TryGetValue(enemy, out damageNeeded) && HeroSpinDictionary.TryGetValue(enemy, out totalDamage)) continue;


                ////var percent = (totalDamage / damageNeeded) >= 2 ? 1000 : Math.Abs(Math.Round(100 * (totalDamage / damageNeeded) * 0.75));

                string text = "KS: " + string.Format("{0}", (int)damageNeeded) + string.Format(" | ({0})", (int)totalDamage);

                if (!Menu.Item("Show Potential Damage").GetValue<bool>())
                {
                    text = "KS: " + string.Format("{0}", (int)damageNeeded);
                }

                var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 150), FontFlags.None);
                var textPos = start + new Vector2(51 - textSize.X / 1, -textSize.Y / 1 + 2);
                Drawing.DrawText(text, "Arial", textPos, new Vector2(20, 0), ColorChoice(damageNeeded, totalDamage), FontFlags.DropShadow);
            }



            foreach (var unit in enemyUnits)
            {
                if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block") && (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear"))) return;
                Vector2 screenPos;
                var enemyPos = unit.Position + new Vector3(0, 0, unit.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos + new Vector2(-51, -40);
                double damageNeeded;
                string spell;
                if (!UnitDamageDictionary.TryGetValue(unit, out damageNeeded)) continue;

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





    }
}





/*
       private static void dunkHero(Ability ability, IReadOnlyList<double> damage, IReadOnlyList<double> adamage = null)
       {
           if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) return;
           var spellLevel = (int)ability.Level - 1;

           if (ability.Level <= 0) return;

           double normalDamage = me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

           var spellDamageType = ability.DamageType;
           var spellRange = (ability.CastRange + Menu.Item("Dunk Range").GetValue<Slider>().Value);
           var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);

           var enemies = ObjectManager.GetEntitiesParallel<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0).ToList();



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
               if (me.IsChanneling()) return;

               if (!(damageNeeded < 0) || !(me.Distance2D(enemy) < spellRange || !meCanSurvive(enemy, me, ability, damageDone))) continue;


               castSpell(ability, enemy);
               break;
           }
       }
       */
