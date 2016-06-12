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

namespace Test
{
    internal class Program
    {
        private static readonly Menu Menu = new Menu("Test", "Test", true);
        static void Main(string[] args)
        {
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;
            var myHero = ObjectManager.LocalHero;
            var myPosition = myHero.Position; // Vector3
            var mousePos = Game.MousePosition; // Vector3
            var mouseScreenPosition = Game.MouseScreenPosition; // Vector2
            var text = string.Format("{0} myPosition || {1} mousePos || {3} mouseScreenPosition"), myPosition.X, mousePos.X, mouseScreenPosition.X);
            Drawing.DrawText(text, new Vector2(22, 388), Color.GhostWhite, FontFlags.AntiAlias);

        }
    }
}
