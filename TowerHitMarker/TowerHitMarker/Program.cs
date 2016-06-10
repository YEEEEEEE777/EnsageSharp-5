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
            var damage = (quellingBlade != null) ? (me.MinimumDamage * 1.4 + me.BonusDamage) : (me.MinimumDamage + me.BonusDamage);

            var creeps = ObjectManager.GetEntities<Unit>().Where(creep =>
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
                if (
                    (creep.IsAlive && creep.IsVisible)
                    && ((creep.Health % 110 > damage * (1 - creep.DamageResist) + 1)
                    || creep.Health < damage * (1 - creep.DamageResist) + 1)
                    )
                {
                    var start = HUDInfo.GetHPbarPosition(creep) + new Vector2(HUDInfo.GetHPBarSizeX(creep) / 2 - 4, HUDInfo.GetHpBarSizeY(creep) - 10);
                    var size = new Vector2(15, 15);
                    Drawing.DrawRect(start, new Vector2(size.Y, size.X), Drawing.GetTexture("materials/ensage_ui/other/active_coin.vmat"));



                }
            }


        }

    }
}