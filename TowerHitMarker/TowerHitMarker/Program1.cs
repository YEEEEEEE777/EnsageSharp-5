using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using Ensage.Menu;
using SharpDX;
using SharpDX.Direct3D9;



namespace TowerHitMarker
{
    internal class Program
    {

        private static readonly Menu Menu = new Menu("TowerHit Marker", "TowerHitMarker", true);
        private static readonly Dictionary<Unit, string> CreepsDictionary = new Dictionary<Unit, string>();


        private static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused)
                return;

            if (!Menu.Item("PB.Enable").GetValue<bool>())
                return;

            var player = ObjectManager.LocalPlayer;
            var me = player.Hero;
            var quellingBlade = me.FindItem(" item_quelling_blade ");
            var damage = (quellingBlade != null) ? (me.MinimumDamage * 1.4) : (me.MinimumDamage);
            var creeps = ObjectManager.GetEntities<Creep>().Where(creep =>
    (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
    || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege)
    && creep.Team != player.Team
    && creep.IsAlive
    && creep.IsVisible
    && creep.IsSpawned).ToList();

            if (player == null || player.Team == Team.Observer || me == null || me.MinimumDamage >= 120)
                return;

            foreach (var creep in creeps)
            {
                if (creep.IsAlive && creep.IsVisible)
                {
                    string creepType;
                    if (creep.Health > 0 && creep.Health < damage * (1 - creep.DamageResist) * ((creep.AttackRange == 690) ? 0.5 : 1)) //Is last hittable.
                    {
                        // if (!CreepsDictionary.TryGetValue(creep, out creepType) || creepType != "prime") continue; //Not a creep or not primed.
                        CreepsDictionary.Remove(creep); //Remove Primed Key from the creep and set it to active.
                        creepType = "active";
                        CreepsDictionary.Add(creep, creepType);
                    }
                    else if (((creep.IsMelee && (creep.Health % 98.2142857 > damage * (1 - creep.DamageResist)))
                            || ((creep.IsRanged && creep.AttackRange == 690) && (creep.Health % 165 > damage * (1 - creep.DamageResist) * 0.5))
                            || ((creep.IsRanged && creep.AttackRange != 690) && (creep.Health % 110 > damage * (1 - creep.DamageResist)))))
                    {
                        //if (CreepsDictionary.TryGetValue(creep, out creepType)) continue; //If it is a creep
                        CreepsDictionary.Remove(creep);
                        creepType = "prime";
                        CreepsDictionary.Add(creep, creepType);
                    }
                    else CreepsDictionary.Remove(creep);
                }
                else
                {
                    CreepsDictionary.Remove(creep);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
                return;

            var creeps = ObjectManager.GetEntities<Unit>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && creep.IsAlive
    && creep.IsVisible
    && creep.IsSpawned).ToList();
            foreach (var creep in creeps)
            {
                Vector2 screenPos;
                var enemyPos = creep.Position + new Vector3(0, 0, creep.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue; // If enemy position is not on screen continue.

                var start = HUDInfo.GetHPbarPosition(creep) + new Vector2(HUDInfo.GetHPBarSizeX(creep) / 2 - 5, HUDInfo.GetHpBarSizeY(creep) - 10);
                var size = new Vector2(15, 15);
                var greenText = Drawing.GetTexture("materials/vgui/hud/hud_timer_full.vmat");
                var coinText = Drawing.GetTexture("materials/ensage_ui/other/active_coin.vmat");
                var greyText2 = Drawing.GetTexture("materials/vgui/hud/minimap_creep.vmat");
                var coinsText = Drawing.GetTexture("materials/vgui/hud/gold.vmat");
                string creepType;

                if (!CreepsDictionary.TryGetValue(creep, out creepType)) continue; //If not creep continue.

                switch (creepType)
                {
                    case "active": Drawing.DrawRect(start, new Vector2(size.Y, size.X), greenText); break;
                    case "prime": Drawing.DrawRect(start, new Vector2(size.Y, size.X), greyText2); break;
                }
            }
        }

    }
}