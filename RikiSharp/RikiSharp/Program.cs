using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace RikiSharp
{
    internal class Program
    {
        private static readonly Dictionary<Hero, double> HeroDamageDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Hero, string> HeroSpellDictionary = new Dictionary<Hero, string>();
        private static readonly Dictionary<Unit, double> UnitDamageDictionary = new Dictionary<Unit, double>();
        private static readonly Dictionary<Unit, string> UnitSpellDictionary = new Dictionary<Unit, string>();
        private static readonly Menu Menu = new Menu("RikiSharp", "RikiSharp", true, "npc_dota_hero_riki", true);


        //private static readonly 

        private static bool _killError = true;


        private static Player _player = ObjectManager.LocalPlayer;
        private static Hero _me = ObjectManager.LocalHero;

        private static Unit vhero = new Unit();

        private static void Main()
        {
            var unitsdict = new Dictionary<string, bool>
            {
                { "roshan_spell_block", true },
                { "visage_summon_familiars", true },
                { "lone_druid_spirit_bear", true },
                { "item_flying_courier", true },
                {"axe_culling_blade", true }
            };

            Menu.AddToMainMenu();
            Menu.AddItem(new MenuItem("Enable", "Enable")).SetValue(true);
            Menu.AddItem(new MenuItem("xKill", "Kill:").SetValue(new AbilityToggler(unitsdict))).SetTooltip("Hi");


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            if (_me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Riki)
                return;

            if ((!Menu.Item("Enable").GetValue<bool>()) || !Game.IsInGame || _player == null || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Riki) return;


            if (!Utils.SleepCheck("RIKISHARP") || Game.IsPaused) return;
            Utils.Sleep(100, "RIKISHARP");

            DispelDust();
        }

        private static void DispelDust()
        {
            string[] VisionMods = new string[] { "modifier_item_dustofappearance", "modifier_bounty_hunter_track", "modifier_slardar_amplify_damage", "modifier_crystal_maiden_frostbite", "modifier_naga_siren_ensnare" };
            bool invisible = ObjectManager.LocalHero.HasModifier("modifier_invisible");

            bool hasModifiers = ObjectManager.LocalHero.HasModifiers(VisionMods, false);
            bool hasModifier = ObjectManager.LocalHero.HasModifier("modifier_bounty_hunter_track");

            if (!HaveDispelItem() || !hasModifiers || !invisible) return;

            var greaves = 

            if (_me.HasItem(ClassID.CDOTA_Item_Guardian_Greaves) && _me.)
            { }


            

            








        }

        private static bool HaveDispelItem()
        {
            if
                (_me.HasItem(ClassID.CDOTA_Item_Diffusal_Blade_Level2) ||
                _me.HasItem(ClassID.CDOTA_Item_Diffusal_Blade) ||
                _me.HasItem(ClassID.CDOTA_Item_MantaStyle) ||
                _me.HasItem(ClassID.CDOTA_Item_Guardian_Greaves))
                return true;

            else return false;



        }

        private static void DunkRoshan(Ability ability, IReadOnlyList<double> damage, uint spellTargetType, uint? range = null, string abilityType = "normal", bool lsblock = true, bool throughSpellImmunity = false, IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("roshan_spell_block")) return;

            var spellLevel = (int)ability.Level - 1; // base 0 index system
            if (ability.Level <= 0) return;

            double normalDamage;
            if (adamage == null)
                normalDamage = damage[spellLevel];
            else
                normalDamage = _me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = ability.DamageType;
            var spellRange = range ?? (ability.CastRange + 50);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);

            var enemies = ObjectManager.GetEntities<Unit>().Where(enemy => enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.ClassID == ClassID.CDOTA_Unit_Roshan).ToList();
            foreach (var enemy in enemies)
            {
                double spellDamage = normalDamage;

                var damageDone = enemy.DamageTaken((float)spellDamage, spellDamageType, _me, throughSpellImmunity);


                damageDone = (float)spellDamage;

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

                if (!(damageNeeded < 0) || !(_me.Distance2D(enemy) < spellRange)) continue;

                switch (spellTargetType)
                {
                    case 1:
                        if (enemy.Spellbook.Spell1.Cooldown > 1)
                        {
                            CastSpell(ability, enemy, _me, lsblock);
                            break;
                        }
                        else break;

                }
                break;
            }
        }

        private static void DunkBear(Ability ability, IReadOnlyList<double> damage, uint spellTargetType, uint? range = null, string abilityType = "normal", bool lsblock = true, bool throughSpellImmunity = false, IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("lone_druid_spirit_bear")) return;
            var spellLevel = (int)ability.Level - 1; // base 0 index system
            if (ability.Level <= 0) return;

            double normalDamage;
            if (adamage == null)
                normalDamage = damage[spellLevel];
            else
                normalDamage = _me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = ability.DamageType;
            var spellRange = range ?? (ability.CastRange + 50);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);

            var enemies = ObjectManager.GetEntities<Unit>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.ClassID == ClassID.CDOTA_Unit_SpiritBear).ToList();
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

                if (!(damageNeeded < 0) || !(_me.Distance2D(enemy) < spellRange)) continue;

                switch (spellTargetType)
                {
                    case 1:
                        CastSpell(ability, enemy, _me, lsblock);
                        break;
                }
                break;
            }
        }

        private static void DunkCourier()
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("item_flying_courier")) return;
            var couriers = ObjectManager.GetEntities<Courier>().Where(x => x.IsAlive && x.Team != _me.Team);
            var dunk = _me.Spellbook.Spell4;
            var spellRange = (dunk.CastRange + 150);

            foreach (var courier in couriers)
            {
                if (Utils.SleepCheck("ks") && CanBeCasted(dunk) && _me.CanCast() && (_me.Distance2D(courier) <= spellRange))
                {
                    _me.Spellbook.Spell4.UseAbility(courier);
                    Utils.Sleep(300, "ks");
                }
            }
        }

        private static void DunkFamiliar()
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("visage_summon_familiars")) return;

            var familiars = ObjectManager.GetEntities<Unit>().Where(x => x.IsAlive && x.Team != _me.Team && x.ClassID == ClassID.CDOTA_Unit_VisageFamiliar);
            var dunk = _me.Spellbook.Spell4;
            var spellRange = (dunk.CastRange + 150);

            foreach (var fam in familiars)
            {
                if (Utils.SleepCheck("ks") && CanBeCasted(dunk) && _me.CanCast() && (_me.Distance2D(fam) <= spellRange))
                {
                    _me.Spellbook.Spell4.UseAbility(fam);
                    Utils.Sleep(300, "ks");
                }
            }
        }

        private static void Kill(Ability ability, IReadOnlyList<double> damage, uint spellTargetType, uint? range = null, string abilityType = "normal", bool lsblock = true, bool throughSpellImmunity = false, IReadOnlyList<double> adamage = null)
        {
            if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) return;
            var spellLevel = (int)ability.Level - 1;
            if (ability.Level <= 0) return;

            double normalDamage;
            if (adamage == null)
                normalDamage = damage[spellLevel];
            else
                normalDamage = _me.AghanimState() ? adamage[spellLevel] : damage[spellLevel];

            var spellDamageType = ability.DamageType;
            var spellRange = range ?? (ability.CastRange + 50);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);

            var enemies = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0).ToList();
            foreach (var enemy in enemies)
            {
                double spellDamage = normalDamage;

                var damageDone = (float)spellDamage;

                double damageNeeded;

                if (!HeroDamageDictionary.TryGetValue(enemy, out damageNeeded))
                {
                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration + MorphMustDie(enemy, spellCastPoint);
                    HeroDamageDictionary.Add(enemy, damageNeeded);
                    HeroSpellDictionary.Add(enemy, ability.Name);
                }
                else
                {
                    HeroDamageDictionary.Remove(enemy);
                    HeroSpellDictionary.Remove(enemy);

                    damageNeeded = enemy.Health - damageDone + spellCastPoint * enemy.HealthRegeneration + MorphMustDie(enemy, spellCastPoint);

                    HeroDamageDictionary.Add(enemy, damageNeeded);
                    HeroSpellDictionary.Add(enemy, ability.Name);
                }
                if (_me.IsChanneling()) return;

                if (!(damageNeeded < 0) || !(_me.Distance2D(enemy) < spellRange || abilityType.Equals("global")) || !MeCanSurvive(enemy, _me, ability, damageDone)) continue;

                switch (spellTargetType)
                {
                    case 1:
                        CastSpell(ability, enemy, _me, lsblock);
                        break;
                }
                break;
            }
        }




        private static bool MeCanSurvive(Hero enemy, Hero me, Ability spell, double damageDone)
        {
            return (me.IsMagicImmune() || (NotDieFromSpell(spell, enemy, me) && NotDieFromLotusOrb(enemy, me, damageDone)));
        }

        private static bool NotDieFromLotusOrb(Unit enemy, Unit me, double damageDone)
        {
            return !(enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_lotus_orb_active") != null && me.Health < damageDone);
        }

        private static bool NotDieFromSpell(Ability spell, Hero enemy, Hero me)
        {
            if (me.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_pugna_nether_ward_aura") == null)
                return true;
            return !(me.Health < me.DamageTaken((spell.ManaCost * (float)1.75), DamageType.Magical, enemy));
        }

        private static void CastSpell(Ability spell, Unit target, Unit me, bool lsblock)
        {
            if (spell.Cooldown > 0) return;

            // OLD CODE
            /*if (Utils.SleepCheck("ks") && CanBeCasted(spell) && me.CanCast() && (target.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_sphere_target") == null)
            && (target.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_sphere") == null || target.FindItem("item_sphere").Cooldown > 0))*/


            if (Utils.SleepCheck("ks") && CanBeCasted(spell) && me.CanCast() && !target.IsLinkensProtected() && !target.HasModifier("modifier_skeleton_king_reincarnation_scepter_active"))
            {

                spell.UseAbility(target);
                vhero = target;
                DelayAction.Add(300 - Game.Ping, CancelUlt);
                Utils.Sleep(50, "ks");

            }
        }

        private static void CancelUlt()
        {
            var rDmg = new int[3] { 250, 325, 400 };

            if (_me.HasItem(ClassID.CDOTA_Item_UltimateScepter))
            {
                rDmg = new int[3] { 300, 425, 550 };
            }
            else
            {
                rDmg = new int[3] { 250, 325, 400 };
            }
            var damage = rDmg[_me.Spellbook.Spell4.Level - 1];

            if (vhero.Health > damage || vhero.IsLinkensProtected())
            {
                _me.Stop();
                _me.Attack(vhero);
                vhero = new Unit();
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

        private static void CheckProc()
        {

            var me = ObjectManager.LocalHero;



            var helix = me.Spellbook.Spell3;
            if (helix.Level <= 0) return;
            if (!me.HasModifier("modifier_axe_counter_helix"))
            {
                Console.WriteLine("Helix USED.");
            }

        }

        private static void Game_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || _player == null || _me == null)
                return;

            if ((!Menu.Item("Enable").GetValue<bool>())) return;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsVisible && hero.IsAlive && !hero.IsIllusion()).ToList();
            foreach (var enemy in enemies)
            {
                if (!Menu.Item("xKill").GetValue<AbilityToggler>().IsEnabled("axe_culling_blade")) continue;
                Vector2 screenPos;
                var enemyPos = enemy.Position + new Vector3(0, 0, enemy.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos + new Vector2(-51, -40);
                double damageNeeded;
                string spell;
                if (!HeroDamageDictionary.TryGetValue(enemy, out damageNeeded) || !HeroSpellDictionary.TryGetValue(enemy, out spell)) continue;

                var text = "KS:  " + string.Format("{0}", (int)damageNeeded);
                var textSize = Drawing.MeasureText(text, "Arial", new Vector2(10, 150), FontFlags.None);
                var textPos = start + new Vector2(51 - textSize.X / 1, -textSize.Y / 1 + 2);
                //Drawing.DrawRect(textPos - new Vector2(15, 0), new Vector2(10, 10), Drawing.GetTexture("materials/NyanUI/spellicons/" + spell + ".vmt"));
                Drawing.DrawText(text, "Arial", textPos, new Vector2(20, 0), damageNeeded < 0 ? Color.Red : Color.White, FontFlags.AntiAlias | FontFlags.DropShadow);
            }

            var units = ObjectManager.GetEntities<Unit>().Where(unit => unit.IsVisible && unit.IsAlive && !unit.IsIllusion() && (unit.ClassID == ClassID.CDOTA_Unit_SpiritBear || unit.ClassID == ClassID.CDOTA_Unit_Roshan)).ToList();
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
                //Drawing.DrawRect(textPos - new Vector2(15, 0), new Vector2(10, 10), Drawing.GetTexture("materials/NyanUI/spellicons/" + spell + ".vmt"));
                Drawing.DrawText(text, "Arial", textPos, new Vector2(20, 0), damageNeeded < 0 ? Color.Red : Color.White, FontFlags.AntiAlias | FontFlags.DropShadow);
            }
        }
    }
}
