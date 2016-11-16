using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace BountyHunterSharp
{
    internal class Program
    {
        private static readonly Dictionary<Hero, double> HeroDamageDictionary = new Dictionary<Hero, double>();
        private static readonly Dictionary<Hero, string> HeroSpellDictionary = new Dictionary<Hero, string>();
        private static readonly Dictionary<Unit, double> UnitDamageDictionary = new Dictionary<Unit, double>();
        private static readonly Dictionary<Unit, string> UnitSpellDictionary = new Dictionary<Unit, string>();
        private static readonly Menu Menu = new Menu("BountyHunterSharp", "BountyHunterSharp", true, "npc_dota_hero_bounty_hunter", true);


        private static bool _killError = true;
        private static bool _killStealEnabled;

        private static Player _player;
        private static Hero _me;

        private static Unit vhero = new Unit();

        private static void Main()
        {

            Menu.AddToMainMenu();
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddItem(new MenuItem("Auto-Kill", "Auto-Kill")).SetValue(true).SetTooltip("Auto Kill Steal with Shuriken.");

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if ((!Menu.Item("PB.Enable").GetValue<bool>())) return;



            if (!_killStealEnabled)
            {
                if (!Game.IsInGame) return;

                _player = ObjectManager.LocalPlayer;
                _me = ObjectManager.LocalHero;

                if (_player == null || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_BountyHunter) return;

                _killStealEnabled = true;
                Console.Write("BH SHARP: Loaded!");
            }
            else if (!Game.IsInGame || _player == null || _me == null)
            {
                _killStealEnabled = false;
                Console.Write("BH SHARP: UnLoaded!");
                return;
            }

            if (!Utils.SleepCheck("BH ULT") || Game.IsPaused) return;
            Utils.Sleep(100, "BH ULT");

            if (_me == null) return;

            Kill(_me.Spellbook.Spell1, new double[] { 150, 225, 300, 375 }, 1, 400);

            /*
            if (_me.Spellbook.Spell1.Level > 0 && _me.Spellbook.Spell1.Cooldown == 0)
            {
                Console.WriteLine("Shuriken Ready");
            }

            else
            {
                Console.WriteLine("Cool Down:" + _me.Spellbook.Spell1.Cooldown);
            }
            */


        }

        private static void Kill(Ability ability, IReadOnlyList<double> damage, uint spellTargetType, uint? range = null, string abilityType = "normal", bool lsblock = true, bool throughSpellImmunity = false, IReadOnlyList<double> adamage = null)
        {
            if ((!Menu.Item("Auto-Kill").GetValue<bool>())) return;
            var spellLevel = (int)ability.Level - 1;
            if (ability.Level <= 0) return;

            double normalDamage = damage[spellLevel];
    
            var spellDamageType = ability.DamageType;
            var spellRange = range ?? (ability.CastRange + 50);
            var spellCastPoint = (float)(((_killError ? 0 : ability.GetCastPoint(ability.Level)) + Game.Ping) / 1000);

            var enemies = ObjectManager.GetEntities<Hero>().Where(enemy => enemy.Team == _me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0 && enemy.HasModifier("modifier_bounty_hunter_track")).ToList();
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

            if ((!Menu.Item("PB.Enable").GetValue<bool>())) return;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsVisible && hero.IsAlive && !hero.IsIllusion()).ToList();
            foreach (var enemy in enemies)
            {
                if ((!Menu.Item("Auto-Kill").GetValue<bool>())) continue;
                
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
        }
    }
}


