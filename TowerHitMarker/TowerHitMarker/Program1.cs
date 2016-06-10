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

        private static void Main()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();

        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (!Game.IsInGame)
            {
                return;
            }
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;

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

            if (player == null || player.Team == Team.Observer || me.MinimumDamage >= 120)
            {
                return;
            }


            foreach (var creep in creeps)
            {

                if ((creep.IsAlive && creep.IsVisible) //Check creep is alive and visible
                    && (((creep.IsMelee && (creep.Health % 98.2142857 > damage * (1 - creep.DamageResist))) 
                    || ((creep.IsRanged && creep.AttackRange == 690) && (creep.Health % 160 > damage * (1 - creep.DamageResist)))
                    || ((creep.IsRanged && creep.AttackRange != 690) && (creep.Health % 110 > damage * (1 - creep.DamageResist))))
                    || creep.Health < damage * (1 - creep.DamageResist)))
                                        
                {
                    var start = HUDInfo.GetHPbarPosition(creep) + new Vector2(HUDInfo.GetHPBarSizeX(creep) / 2 - 5, HUDInfo.GetHpBarSizeY(creep) - 10);
                    var size = new Vector2(15, 15);
                    //var text = string.Format("{0} - {1} - {2}", creep.UnitType, creep.UnitState, creep.AttackRange); //Debugging Purposes
                    //Drawing.DrawText(text, start, Color.White, FontFlags.None);
                    Drawing.DrawRect(start, new Vector2(size.Y, size.X), Drawing.GetTexture("materials/ensage_ui/other/active_coin.vmat"));
                }


            }


        }

    }
}