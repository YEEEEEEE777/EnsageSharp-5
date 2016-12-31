using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;


using System.Net.Http;




using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;

using System.Net;
using System.IO;

using System.Drawing;

using SharpDX;
using SharpDX.Direct3D9;


namespace MMRSharp
{
    class Program
    {
        private static Dictionary<uint, string> MMRDict = new Dictionary<uint, string>();
        private static Dictionary<uint, string> WRDict = new Dictionary<uint, string>();
        private static Dictionary<uint, string> RoleDict = new Dictionary<uint, string>();

        static private bool _isExecutedFirst = false;
        static private bool _isPressedFirst = false;
        static private bool _isDisplayedFirst = false;

        private static SharpDX.Direct3D9.Font textFont;
        private static readonly Menu Menu = new Menu("MMRSharp", "MMRSharp", true);

        private static int hText = 27;
        private static int wText = 10;

        //Colors
        private static SharpDX.Color aColor0 = new SharpDX.Color(56, 117, 234);
        private static SharpDX.Color aColor1 = new SharpDX.Color(111, 247, 197);
        private static SharpDX.Color aColor2 = new SharpDX.Color(174, 11, 180);
        private static SharpDX.Color aColor3 = new SharpDX.Color(241, 238, 35);
        private static SharpDX.Color aColor4 = new SharpDX.Color(240, 109, 18);
        private static SharpDX.Color aColor5 = new SharpDX.Color(237, 137, 187);
        private static SharpDX.Color aColor6 = new SharpDX.Color(162, 179, 83);
        private static SharpDX.Color aColor7 = new SharpDX.Color(111, 209, 236);
        private static SharpDX.Color aColor8 = new SharpDX.Color(9, 117, 41);
        private static SharpDX.Color aColor9 = new SharpDX.Color(151, 106, 15);

        private static void Main()
        {

            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnEndScene += Drawing_OnEndScene;

            //Font for OnEndScene.
            textFont = new SharpDX.Direct3D9.Font(
                    Drawing.Direct3DDevice9,
                    new FontDescription
                    {
                        FaceName = "Segoe UI",
                        Height = hText,
                        OutputPrecision = FontPrecision.Raster,
                        Quality = FontQuality.ClearTypeNatural,
                        CharacterSet = FontCharacterSet.Hangul,
                        MipLevels = 3,
                        PitchAndFamily = FontPitchAndFamily.Modern,
                        Weight = FontWeight.Normal,
                        Width = wText,
                    });

            //Menu
            Menu.AddItem(new MenuItem("Enable", "Enable")).SetValue(true);
            //Menu.AddItem(new MenuItem("Allies Enable", "Allies Enable")).SetValue(true);
            //Menu.AddItem(new MenuItem("Enemies Enable", "Enemies Enable")).SetValue(true);
            Menu.AddItem(new MenuItem("Hide/Show", "Hide/Show")).SetValue(true).SetTooltip("Hide/Show MMR information.");
            Menu.AddItem(new MenuItem("Analyze", "Analyze").SetValue(new KeyBind(70, KeyBindType.Press)))
    .SetTooltip("Press to analyze MMR info.");


            //Settings SubMenu
            var settings = new Menu("Settings", "settings");
            settings.AddItem(new MenuItem("BarPosX", "Position X").SetValue(new Slider(0, -1500, 300)));
            settings.AddItem(new MenuItem("BarPosY", "Position Y").SetValue(new Slider(0, -930, 200)));
            //settings.AddItem(new MenuItem("Loading", "Loading").SetValue(new Slider(0, 0, 1000)).SetTooltip("Increase loading time if MMR info does not show."));
            //settings.AddItem(new MenuItem("BarSizeY", "Size").SetValue(new Slider(0, -10, 10)));
            Menu.AddSubMenu(settings);
            Menu.AddToMainMenu();
        }



        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("Enable").GetValue<bool>()) return;

            if ((Game.IsInGame || Game.GameState.ToString() == "Picking" || Game.GameState.ToString() == "Loaded"))
            {
                if (!_isExecutedFirst && _isPressedFirst)
                {
                    //Lookup();
                    Task t = new Task(Lookup);
                    t.Start();
                    _isExecutedFirst = true;


                }
                return;
            }

            //Clear Dictionary when game is over.
            else if (Game.GameState.ToString() == "NotInGame" && MMRDict.Any())
            {
                MMRDict.Clear();
                WRDict.Clear();
                RoleDict.Clear();
                _isExecutedFirst = false;
                _isPressedFirst = false;
                _isDisplayedFirst = false;
                Console.WriteLine("MMR Data Cleared.");
                return;
            }

            else return;
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen && !Game.IsInGame) return;
            try
            {
                if ((Menu.Item("Enable").GetValue<bool>()) && args.Msg == 0x0101 && args.WParam == Menu.Item("Analyze").GetValue<KeyBind>().Key) //0x0101 WM_KeyUP
                {
                    _isPressedFirst = true;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e + e.StackTrace);
            }
        }


        private static void Drawing_OnEndScene(EventArgs args)
        {
            Player player = ObjectManager.LocalPlayer;

            if (Drawing.Direct3DDevice9 == null || !(Menu.Item("Hide/Show").GetValue<bool>())) return;
            if (_isPressedFirst == false) return;

            //textFont.DrawText(null, ".", 1417, 817, SharpDX.Color.Yellow); //For Testing Purposes



            string mmr;
            string wr;
            string role;

            List<Player> players = ObjectManager.GetEntities<Player>().Where(x => x.Team != Team.Observer).ToList(); //(x.Team == Team.Radiant || x.Team == Team.Dire)

            /*
            if (_isExecutedFirst && Game.GameState.ToString() == "Loaded")
            {
                foreach (Player p in players)
                {
                    //Player Color.
                    SharpDX.Color playerColor = SelectColor(p.ID);
                    SharpDX.Color textColor = SharpDX.Color.White;

                    //Panel Positions.
                    int textPosX = 1407;
                    int textPosYR = 179;
                    int textPosYD = 440;
                    int horDist = 60;
                    int vertDist = hText + 7;

                    //Header
                    textFont.DrawText(null, "MMR", textPosX, textPosYR - vertDist, SharpDX.Color.White);

                    //Radiant text position.
                    Vector2 startPosMMRR = new Vector2(textPosX, textPosYR + p.TeamSlot * vertDist);

                    //Dire text position.
                    Vector2 startPosMMRD = new Vector2(textPosX, textPosYD + p.TeamSlot * vertDist);

                    if (p.Team == Team.Radiant)
                    {
                        if (MMRDict.TryGetValue(p.PlayerSteamID, out mmr))
                            textFont.DrawText(null, mmr, (int)startPosMMRR.X, (int)startPosMMRR.Y, textColor);
                    }

                    else if (p.Team == Team.Dire)
                    {
                        if (MMRDict.TryGetValue(p.PlayerSteamID, out mmr))
                            textFont.DrawText(null, mmr, (int)startPosMMRD.X, (int)startPosMMRD.Y, textColor);
                    }

                    else return;

                }
            }
            */

            //Game.PrintMessage("<font color='#00aaff'>" + String.Format("{0, -2} | {1, -4} | {2, -3}% | {3, -4}", p.ID, mmr, wr, role), MessageType.ChatMessage);

            if (_isDisplayedFirst && Game.GameState.ToString() != "Loaded")
            {
                foreach (Player p in players)
                {
                    
                    //Player Color.
                    SharpDX.Color playerColor = SelectColor(p.ID);
                    SharpDX.Color textColor = SharpDX.Color.White;

                    //Panel Positions.
                    int textPosX = 1435 + Menu.Item("BarPosX").GetValue<Slider>().Value;
                    int textPosY = 850 + Menu.Item("BarPosY").GetValue<Slider>().Value;
                    int horDist = 60;
                    int boxDist = 15; //Distance of Box
                    int vertDist = hText;

                    //Header
                    textFont.DrawText(null, "MMR | WIN % | ROLE", textPosX, textPosY - vertDist, SharpDX.Color.White);

                    //Radiant text position.
                    Vector2 startPosMMRR = new Vector2(textPosX, textPosY + p.TeamSlot * vertDist);
                    Vector2 startPosWRR = new Vector2(textPosX + 1 * horDist, textPosY + p.TeamSlot * vertDist);
                    Vector2 startPosRoleR = new Vector2(textPosX + 2 * horDist, textPosY + p.TeamSlot * vertDist);

                    //Dire text position.
                    Vector2 startPosMMRD = new Vector2(textPosX + 4 * horDist, textPosY + p.TeamSlot * vertDist);
                    Vector2 startPosWRD = new Vector2(textPosX + 5 * horDist, textPosY + p.TeamSlot * vertDist);
                    Vector2 startPosRoleD = new Vector2(textPosX + 6 * horDist, textPosY + p.TeamSlot * vertDist);

                    if (p.Team == Team.Radiant)
                    {
                        if (MMRDict.TryGetValue(p.PlayerSteamID, out mmr))
                            textFont.DrawText(null, "🔲", (int)startPosMMRR.X - boxDist, (int)startPosMMRR.Y, playerColor);
                        textFont.DrawText(null, mmr, (int)startPosMMRR.X, (int)startPosMMRR.Y, textColor);

                        if (WRDict.TryGetValue(p.PlayerSteamID, out wr))
                            textFont.DrawText(null, wr + "%", (int)startPosWRR.X, (int)startPosWRR.Y, textColor);

                        if (RoleDict.TryGetValue(p.PlayerSteamID, out role))
                            textFont.DrawText(null, role, (int)startPosRoleR.X, (int)startPosRoleR.Y, textColor);
                    }

                    else if (p.Team == Team.Dire)
                    {
                        if (MMRDict.TryGetValue(p.PlayerSteamID, out mmr))
                            textFont.DrawText(null, "🔲", (int)startPosMMRD.X - boxDist, (int)startPosMMRD.Y, playerColor);
                        textFont.DrawText(null, mmr, (int)startPosMMRD.X, (int)startPosMMRD.Y, textColor);

                        if (WRDict.TryGetValue(p.PlayerSteamID, out wr))
                            textFont.DrawText(null, wr + "%", (int)startPosWRD.X, (int)startPosWRD.Y, textColor);

                        if (RoleDict.TryGetValue(p.PlayerSteamID, out role))
                            textFont.DrawText(null, role, (int)startPosRoleD.X, (int)startPosRoleD.Y, textColor);
                    }

                    else return;


                }
                return;

            }
        }

        private static async void Lookup()
        {
            List<Player> players = ObjectManager.GetEntities<Player>().Where(x => x.Team != Team.Observer).OrderBy(x => x.ID).ToList(); // (x.Team == Team.Radiant || x.Team == Team.Dire)
            Console.WriteLine("Hi");
            foreach (var p in players)
            {
                string steamID = p.PlayerSteamID.ToString();
                var personalURL = "https://api.opendota.com/api/players/" + steamID + "/counts";
                using (HttpClient client = new HttpClient())


                using (HttpResponseMessage response = await client.GetAsync(personalURL))
                using (HttpContent content = response.Content)
                {

                    int position = 6; //Instantiated to 6 so default position is "N/A", number is arbitrary.
                    string role;



                    string data = await content.ReadAsStringAsync();

                    string mmrBegin = "<abbr title=\"Solo MMR\">";
                    string mmrEnd = "</small></span><span class=\"text";
                    string potMMRBegin = "<abbr title=\"MMR estimate based on available data from peer players. This is an estimate of the population mean MMR of the recent matches played by this user.\">";
                    string potMMREnd = "</small></span></h4></div><d"; //"</abbr></span></" OLD STRING.
                    string winRateBegin = "6.88</a></td><td class=\"rankable\">";
                    string winRateBegin2 = "</td><td class=\"rankable\"><div>";
                    string winRateEnd = "</td></tr></tbody></table></div></";
                    string winRateEnd2 = "</div>";

                    string roleBegin1 = "Safe</a></td><td class=\"rankable\">"; //safe
                    string roleBegin2 = "Mid</a></td><td class=\"rankable\">"; //mid
                    string roleBegin3 = "Off</a></td><td class=\"rankable\">"; //off            
                    string roleBegin4 = "Jungle</a></td><td class=\"rankable\">"; //jungle
                    string roleEnd = "</td>";

                    string mmr = StripHTML(ExtractString(data, mmrBegin, mmrEnd));
                    string potMMR = StripHTML(ExtractString(data, potMMRBegin, potMMREnd));
                    string winRate = TruncateorNAWR(StripHTML(ExtractString(ExtractString(data, winRateBegin, winRateEnd), winRateBegin2, winRateEnd2)));

                    mmr = MMRNotAvailable(mmr); //MMR N/A set to "".     
                    potMMR = MMRNotAvailable(potMMR); //potMMR N/A set to "".                          
                    string trueMMR = CompareMMR(mmr, potMMR); //True MMR is the larger of mmr and potMMR else N/A.

                    string role1 = StripHTML(ExtractString(data, roleBegin1, roleEnd));
                    string role2 = StripHTML(ExtractString(data, roleBegin2, roleEnd));
                    string role3 = StripHTML(ExtractString(data, roleBegin3, roleEnd));
                    string role4 = StripHTML(ExtractString(data, roleBegin4, roleEnd));

                    List<string> maxValueString = new List<string> { role1, role2, role3, role4 };
                    List<int> maxValue = new List<int>();

                    foreach (string s in maxValueString)
                    {
                        int j;
                        Int32.TryParse(s, out j);
                        maxValue.Add(j);
                    }

                    position = TruePosition(winRate, maxValue);

                    switch (position)
                    {
                        case 0:
                            role = "[1/5]";
                            break;
                        case 1:
                            role = "[2]";
                            break;
                        case 2:
                            role = "[3]";
                            break;
                        case 3:
                            role = "[4]";
                            break;
                        default:
                            role = "N/A";
                            break;
                    }

                    MMRDict.Add(p.PlayerSteamID, trueMMR);
                    WRDict.Add(p.PlayerSteamID, winRate);
                    RoleDict.Add(p.PlayerSteamID, role);

                    if (data != null)
                    {
                        Console.WriteLine("--------------------------");
                        Console.WriteLine("Name:" + " " + p.Name);
                        Console.WriteLine("True MMR:" + " " + trueMMR);
                        //Console.WriteLine("Potential MMR:" + potMMR);
                        //Console.WriteLine("MMR:" + " " + mmr);
                        Console.WriteLine("Win:" + " " + winRate + "%");
                        Console.WriteLine("Role:" + " " + role);
                        Console.WriteLine("ID:" + " " + p.ID);
                    }


                }

            }
            _isDisplayedFirst = true;
        }







        private static int TruePosition(string winRate, List<int> list)
        {
            if (winRate == "") return 6;
            else return list.IndexOf(list.Max());
        }

        private static string TruncateorNAWR(string wr)
        {
            return Math.Truncate(Decimal.Parse(wr)).ToString();
        }

        private static string MMRNotAvailable(string mmr)
        {
            if (mmr == "N/A")
            {
                return "";
            }
            else return mmr;
        }


        private static string CompareMMR(string mmr, string potmmr)
        {
            string trueMMR = "N/A";

            if (mmr == "" && potmmr == "") return trueMMR;

            else if (mmr == "" ^ potmmr == "")
            {
                if (mmr == "") return potmmr;
                else return mmr;
            }

            else
            {
                if (Int32.Parse(mmr) >= Int32.Parse(potmmr)) return mmr;
                else return potmmr;
            }
        }


        public static string StripHTML(string html)
        {
            string htmlgarbage = @"<(.|\n)*?>";
            string spaces = @"(?<=>)\s+?(?=<)";
            string removehtml = Regex.Replace(html, htmlgarbage, string.Empty);
            string removedspaces = Regex.Replace(removehtml, spaces, string.Empty).Trim();
            return removedspaces;
        }

        public static string ExtractString(string s, string start, string end)
        {
            if (s.Contains(start) && s.Contains(end) && start.Length > 0)
            {
                int startIndex = s.IndexOf(start) + start.Length;
                int endIndex = s.IndexOf(end, startIndex);

                return s.Substring(startIndex, endIndex - startIndex);
            }

            return "0";
        }

        private static SharpDX.Color SelectColor(int pid)
        {
            switch (pid)
            {
                case 0:
                    return aColor0;
                case 1:
                    return aColor1;
                case 2:
                    return aColor2;
                case 3:
                    return aColor3;
                case 4:
                    return aColor4;
                case 5:
                    return aColor5;
                case 6:
                    return aColor6;
                case 7:
                    return aColor7;
                case 8:
                    return aColor8;
                case 9:
                    return aColor9;
                default:
                    return SharpDX.Color.White;
            }
        }



    }
}

