using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;




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
        private static Dictionary<Creep, float> CreepList = new Dictionary<Creep, float>();

        private static bool dataUpdated = false;
        private static string topLaneData;

        static Font Text;
        static float ScaleX, ScaleY;

        static void Main(string[] args)
        {
            Menu.AddItem(new MenuItem("PB.Enable", "Enable")).SetValue(true);
            Menu.AddToMainMenu();

            ScaleX = Drawing.Width / 1920f;
            ScaleY = Drawing.Height / 1080f;

            Game.OnUpdate += Game_OnUpdate;
            /*
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            */


        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.GameTime < 0) return;
            //Console.WriteLine(Game.GameTime);
            //Console.WriteLine(ObjectManager.LocalPlayer.Team);

            string path1 = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);


            Console.WriteLine(path1);

            
            //Environment.CurrentDirectory - Dota2 Directory
            //AppDomain.CurrentDomain.BaseDirectory - PlaySharp2 Directory

            

            //System.Security.Permissions.FileIOPermission fileIOPerm1 = new System.Security.Permissions.FileIOPermission(System.Security.Permissions.FileIOPermissionAccess.AllAccess, "C:\\Users\\Eternal\\Documents\\GitHub\\EnsageSharp\\Test\\Test\\TextFile1.txt");

            string path = Path.Combine(Environment.CurrentDirectory, @"\PathData\");

            //string readText = File.ReadAllText(@"C:\Users\Eternal\Documents\GitHub\EnsageSharp\Test\Test\TextFile1.txt");
            
            //Console.WriteLine(readText);
            //System.IO.File.ReadAllText(@"H:\Current Usage\Ensage\Scripts\CreepsPathData\RadiantTopLane2.txt");

            var me = ObjectManager.LocalHero;
            var myTeam = me.Team;
            var creeps = ObjectManager.GetEntities<Creep>().Where(creep =>
    (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
    || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege)
    && creep.Team != myTeam
    && creep.IsAlive
    && creep.IsVisible
    && creep.IsSpawned).ToList();

            //After 7:30 min mark creeps on offlanes do not get speed bonus.

            if (Game.GameTime > 450 && Game.GameTime < 451 && !dataUpdated)
            {
                var nowTeam = "Dire";
                if (myTeam == Team.Dire)
                {
                    nowTeam = "Radiant";                    
                }
                var topLaneData = System.IO.File.ReadAllText(@"H:\Current Usage\Ensage\Scripts\CreepsPathData\RadiantTopLane2.txt");



              
            }

        }
        /*
        private static void Drawing_OnEndScene(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }*/





        /*private static void OnDraw(EventArgs args)
        {
            if (!Menu.Item("PB.Enable").GetValue<bool>()) return;
            var myHero = ObjectManager.LocalHero;
            var myPosition = myHero.Position; // Vector3
            var mousePos = Game.MousePosition; // Vector3
            var mouseScreenPosition = Game.MouseScreenPosition; // Vector2
            var text = string.Format("{0} myPosition || {1} mousePos || {3} mouseScreenPosition"), myPosition.X, mousePos.X, mouseScreenPosition.X);
            Drawing.DrawText(text, new Vector2(22, 388), Color.GhostWhite, FontFlags.AntiAlias);

        }*/
    }
}
