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
        private static Hero vhero;

        private static int[] rDmg = new int[3] { 250, 325, 400 };


        static void Main()
        {

            Menu.AddToMainMenu();
            Events.OnLoad += Events_OnLoad;
            Events.OnClose += Events_OnClose;
            Game.OnUpdate += Game_OnUpdate;
            //Game.OnUpdate += Killsteal;



        }

        public static void cancelult()
        {
            me = ObjectManager.LocalHero;
            if (me.HasItem(ClassID.CDOTA_Item_UltimateScepter))
            {
                rDmg = new int[3] { 300, 425, 550 };
            }
            else
            {
                rDmg = new int[3] { 250, 325, 400 };
            }
            
            var damage = Math.Floor(rDmg[me.Spellbook.Spell4.Level - 1]);
            if (Menu.Item("stealEdmg").GetValue<bool>() && me.Distance2D(vhero) < 1200)
                damage = damage + eDmg[me.Spellbook.Spell3.Level] * 0.01 * vhero.Health * (1 - vhero.MagicDamageResist);
            if (vhero.NetworkName == "CDOTA_Unit_Hero_Spectre" && vhero.Spellbook.Spell3.Level > 0)
            {
                damage =
                    Math.Floor(rDmg[me.Spellbook.Spell4.Level - 1] *
                               (1 - (0.10 + vhero.Spellbook.Spell3.Level * 0.04)) * (1 - vhero.MagicDamageResist));
                if (Menu.Item("stealEdmg").GetValue<bool>() && me.Distance2D(vhero) < 1150)
                    damage = damage + eDmg[me.Spellbook.Spell3.Level] * 0.01 * vhero.Health * (1 - vhero.MagicDamageResist);
            }
            if (vhero.NetworkName == "CDOTA_Unit_Hero_SkeletonKing" &&
                vhero.Spellbook.SpellR.CanBeCasted())
                damage = 0;
            if (vhero.NetworkName == "CDOTA_Unit_Hero_Tusk" &&
                vhero.Spellbook.SpellW.CooldownLength - 3 > vhero.Spellbook.SpellQ.Cooldown)
                damage = 0;
            if (lens) damage = damage * 1.08;
            var kunkkarum = vhero.Modifiers.Any(x => x.Name == "modifier_kunkka_ghost_ship_damage_absorb");
            if (kunkkarum) damage = damage * 0.5;
            if (momd) damage = damage * 1.3;
            var unkillabletarget1 = vhero.Modifiers.Any(
                x => x.Name == "modifier_abaddon_borrowed_time" || x.Name == "modifier_dazzle_shallow_grave" ||
                     x.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison" ||
                     x.Name == "modifier_puck_phase_shift" ||
                     x.Name == "modifier_brewmaster_storm_cyclone" || x.Name == "modifier_eul_cyclone" ||
                     x.Name == "modifier_item_aegis" || x.Name == "modifier_slark_shadow_dance" || x.Name == "modifier_ember_spirit_flame_guard" ||
                     x.Name == "modifier_abaddon_aphotic_shield" || x.Name == "modifier_phantom_lancer_doppelwalk_phase" ||
                     x.Name == "modifier_shadow_demon_disruption" || x.Name == "modifier_nyx_assassin_spiked_carapace" ||
                     x.Name == "modifier_templar_assassin_refraction_absorb" || x.Name == "modifier_necrolyte_reapers_scythe" ||
                     x.Name == "modifier_storm_spirit_ball_lightning" || x.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability" ||
                     x.Name == "modifier_ember_spirit_fire_remnant" || x.Name == "modifier_snowball_movement" || x.Name == "modifier_snowball_movement_friendly");

            if (vhero.Health > damage || !vhero.IsAlive || vhero.IsIllusion) me.Stop();
            vhero = null;
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
