using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;


namespace MMRSharp
{
    class Program
    {
        private static Dictionary<uint, string> MMRDict = new Dictionary<uint, string>();
        private static Dictionary<uint, string> WinRateDict = new Dictionary<uint, string>();
        private static Dictionary<uint, string> BestHeroesDict = new Dictionary<uint, string>();

        static private bool _isExecutedFirst = false;
        static private bool _isPressedFirst = false;
        static private bool _isDisplayedFirst = false;

        private static SharpDX.Direct3D9.Font textFont;
        private static readonly Menu Menu = new Menu("MMRSharp", "MMRSharp", true);

        private static int hText = 27;
        private static int wText = 10;

        //Colors
        private static SharpDX.Color playerColor0 = new SharpDX.Color(56, 117, 234);
        private static SharpDX.Color playerColor1 = new SharpDX.Color(111, 247, 197);
        private static SharpDX.Color playerColor2 = new SharpDX.Color(174, 11, 180);
        private static SharpDX.Color playerColor3 = new SharpDX.Color(241, 238, 35);
        private static SharpDX.Color playerColor4 = new SharpDX.Color(240, 109, 18);
        private static SharpDX.Color playerColor5 = new SharpDX.Color(237, 137, 187);
        private static SharpDX.Color playerColor6 = new SharpDX.Color(162, 179, 83);
        private static SharpDX.Color playerColor7 = new SharpDX.Color(111, 209, 236);
        private static SharpDX.Color playerColor8 = new SharpDX.Color(9, 117, 41);
        private static SharpDX.Color playerColor9 = new SharpDX.Color(151, 106, 15);


        //Result {\"name\":\"fake\",\"id\":0,\"localized_name\":\"Fake\"},
        private static readonly Hero[] resultArray = JsonConvert.DeserializeObject<Result>("{\"heroes\":[{\"name\":\"fake1\",\"id\":0,\"localized_name\":\"fake1\"},{\"name\":\"antimage\",\"id\":1,\"localized_name\":\"Anti-Mage\"},{\"name\":\"axe\",\"id\":2,\"localized_name\":\"Axe\"},{\"name\":\"bane\",\"id\":3,\"localized_name\":\"Bane\"},{\"name\":\"bloodseeker\",\"id\":4,\"localized_name\":\"Bloodseeker\"},{\"name\":\"crystal_maiden\",\"id\":5,\"localized_name\":\"Crystal Maiden\"},{\"name\":\"drow_ranger\",\"id\":6,\"localized_name\":\"Drow Ranger\"},{\"name\":\"earthshaker\",\"id\":7,\"localized_name\":\"Earthshaker\"},{\"name\":\"juggernaut\",\"id\":8,\"localized_name\":\"Juggernaut\"},{\"name\":\"mirana\",\"id\":9,\"localized_name\":\"Mirana\"},{\"name\":\"morphling\",\"id\":10,\"localized_name\":\"Morphling\"},{\"name\":\"nevermore\",\"id\":11,\"localized_name\":\"Shadow Fiend\"},{\"name\":\"phantom_lancer\",\"id\":12,\"localized_name\":\"Phantom Lancer\"},{\"name\":\"puck\",\"id\":13,\"localized_name\":\"Puck\"},{\"name\":\"pudge\",\"id\":14,\"localized_name\":\"Pudge\"},{\"name\":\"razor\",\"id\":15,\"localized_name\":\"Razor\"},{\"name\":\"sand_king\",\"id\":16,\"localized_name\":\"Sand King\"},{\"name\":\"storm_spirit\",\"id\":17,\"localized_name\":\"Storm Spirit\"},{\"name\":\"sven\",\"id\":18,\"localized_name\":\"Sven\"},{\"name\":\"tiny\",\"id\":19,\"localized_name\":\"Tiny\"},{\"name\":\"vengefulspirit\",\"id\":20,\"localized_name\":\"Vengeful Spirit\"},{\"name\":\"windrunner\",\"id\":21,\"localized_name\":\"Windranger\"},{\"name\":\"zuus\",\"id\":22,\"localized_name\":\"Zeus\"},{\"name\":\"kunkka\",\"id\":23,\"localized_name\":\"Kunkka\"},{\"name\":\"fake2\",\"id\":24,\"localized_name\":\"fake2\"},{\"name\":\"lina\",\"id\":25,\"localized_name\":\"Lina\"},{\"name\":\"lion\",\"id\":26,\"localized_name\":\"Lion\"},{\"name\":\"shadow_shaman\",\"id\":27,\"localized_name\":\"Shadow Shaman\"},{\"name\":\"slardar\",\"id\":28,\"localized_name\":\"Slardar\"},{\"name\":\"tidehunter\",\"id\":29,\"localized_name\":\"Tidehunter\"},{\"name\":\"witch_doctor\",\"id\":30,\"localized_name\":\"Witch Doctor\"},{\"name\":\"lich\",\"id\":31,\"localized_name\":\"Lich\"},{\"name\":\"riki\",\"id\":32,\"localized_name\":\"Riki\"},{\"name\":\"enigma\",\"id\":33,\"localized_name\":\"Enigma\"},{\"name\":\"tinker\",\"id\":34,\"localized_name\":\"Tinker\"},{\"name\":\"sniper\",\"id\":35,\"localized_name\":\"Sniper\"},{\"name\":\"necrolyte\",\"id\":36,\"localized_name\":\"Necrophos\"},{\"name\":\"warlock\",\"id\":37,\"localized_name\":\"Warlock\"},{\"name\":\"beastmaster\",\"id\":38,\"localized_name\":\"Beastmaster\"},{\"name\":\"queenofpain\",\"id\":39,\"localized_name\":\"Queen of Pain\"},{\"name\":\"venomancer\",\"id\":40,\"localized_name\":\"Venomancer\"},{\"name\":\"faceless_void\",\"id\":41,\"localized_name\":\"Faceless Void\"},{\"name\":\"skeleton_king\",\"id\":42,\"localized_name\":\"Skeleton King\"},{\"name\":\"death_prophet\",\"id\":43,\"localized_name\":\"Death Prophet\"},{\"name\":\"phantom_assassin\",\"id\":44,\"localized_name\":\"Phantom Assassin\"},{\"name\":\"pugna\",\"id\":45,\"localized_name\":\"Pugna\"},{\"name\":\"templar_assassin\",\"id\":46,\"localized_name\":\"Templar Assassin\"},{\"name\":\"viper\",\"id\":47,\"localized_name\":\"Viper\"},{\"name\":\"luna\",\"id\":48,\"localized_name\":\"Luna\"},{\"name\":\"dragon_knight\",\"id\":49,\"localized_name\":\"Dragon Knight\"},{\"name\":\"dazzle\",\"id\":50,\"localized_name\":\"Dazzle\"},{\"name\":\"rattletrap\",\"id\":51,\"localized_name\":\"Clockwerk\"},{\"name\":\"leshrac\",\"id\":52,\"localized_name\":\"Leshrac\"},{\"name\":\"furion\",\"id\":53,\"localized_name\":\"Nature\'s Prophet\"},{\"name\":\"life_stealer\",\"id\":54,\"localized_name\":\"Lifestealer\"},{\"name\":\"dark_seer\",\"id\":55,\"localized_name\":\"Dark Seer\"},{\"name\":\"clinkz\",\"id\":56,\"localized_name\":\"Clinkz\"},{\"name\":\"omniknight\",\"id\":57,\"localized_name\":\"Omniknight\"},{\"name\":\"enchantress\",\"id\":58,\"localized_name\":\"Enchantress\"},{\"name\":\"huskar\",\"id\":59,\"localized_name\":\"Huskar\"},{\"name\":\"night_stalker\",\"id\":60,\"localized_name\":\"Night Stalker\"},{\"name\":\"broodmother\",\"id\":61,\"localized_name\":\"Broodmother\"},{\"name\":\"bounty_hunter\",\"id\":62,\"localized_name\":\"Bounty Hunter\"},{\"name\":\"weaver\",\"id\":63,\"localized_name\":\"Weaver\"},{\"name\":\"jakiro\",\"id\":64,\"localized_name\":\"Jakiro\"},{\"name\":\"batrider\",\"id\":65,\"localized_name\":\"Batrider\"},{\"name\":\"chen\",\"id\":66,\"localized_name\":\"Chen\"},{\"name\":\"spectre\",\"id\":67,\"localized_name\":\"Spectre\"},{\"name\":\"ancient_apparition\",\"id\":68,\"localized_name\":\"Ancient Apparition\"},{\"name\":\"doom_bringer\",\"id\":69,\"localized_name\":\"Doom\"},{\"name\":\"ursa\",\"id\":70,\"localized_name\":\"Ursa\"},{\"name\":\"spirit_breaker\",\"id\":71,\"localized_name\":\"Spirit Breaker\"},{\"name\":\"gyrocopter\",\"id\":72,\"localized_name\":\"Gyrocopter\"},{\"name\":\"alchemist\",\"id\":73,\"localized_name\":\"Alchemist\"},{\"name\":\"invoker\",\"id\":74,\"localized_name\":\"Invoker\"},{\"name\":\"silencer\",\"id\":75,\"localized_name\":\"Silencer\"},{\"name\":\"obsidian_destroyer\",\"id\":76,\"localized_name\":\"Outworld Devourer\"},{\"name\":\"lycan\",\"id\":77,\"localized_name\":\"Lycanthrope\"},{\"name\":\"brewmaster\",\"id\":78,\"localized_name\":\"Brewmaster\"},{\"name\":\"shadow_demon\",\"id\":79,\"localized_name\":\"Shadow Demon\"},{\"name\":\"lone_druid\",\"id\":80,\"localized_name\":\"Lone Druid\"},{\"name\":\"chaos_knight\",\"id\":81,\"localized_name\":\"Chaos Knight\"},{\"name\":\"meepo\",\"id\":82,\"localized_name\":\"Meepo\"},{\"name\":\"treant\",\"id\":83,\"localized_name\":\"Treant Protector\"},{\"name\":\"ogre_magi\",\"id\":84,\"localized_name\":\"Ogre Magi\"},{\"name\":\"undying\",\"id\":85,\"localized_name\":\"Undying\"},{\"name\":\"rubick\",\"id\":86,\"localized_name\":\"Rubick\"},{\"name\":\"disruptor\",\"id\":87,\"localized_name\":\"Disruptor\"},{\"name\":\"nyx_assassin\",\"id\":88,\"localized_name\":\"Nyx Assassin\"},{\"name\":\"naga_siren\",\"id\":89,\"localized_name\":\"Naga Siren\"},{\"name\":\"keeper_of_the_light\",\"id\":90,\"localized_name\":\"Keeper of the Light\"},{\"name\":\"wisp\",\"id\":91,\"localized_name\":\"Wisp\"},{\"name\":\"visage\",\"id\":92,\"localized_name\":\"Visage\"},{\"name\":\"slark\",\"id\":93,\"localized_name\":\"Slark\"},{\"name\":\"medusa\",\"id\":94,\"localized_name\":\"Medusa\"},{\"name\":\"troll_warlord\",\"id\":95,\"localized_name\":\"Troll Warlord\"},{\"name\":\"centaur\",\"id\":96,\"localized_name\":\"Centaur Warrunner\"},{\"name\":\"magnataur\",\"id\":97,\"localized_name\":\"Magnus\"},{\"name\":\"shredder\",\"id\":98,\"localized_name\":\"Timbersaw\"},{\"name\":\"bristleback\",\"id\":99,\"localized_name\":\"Bristleback\"},{\"name\":\"tusk\",\"id\":100,\"localized_name\":\"Tusk\"},{\"name\":\"skywrath_mage\",\"id\":101,\"localized_name\":\"Skywrath Mage\"},{\"name\":\"abaddon\",\"id\":102,\"localized_name\":\"Abaddon\"},{\"name\":\"elder_titan\",\"id\":103,\"localized_name\":\"Elder Titan\"},{\"name\":\"legion_commander\",\"id\":104,\"localized_name\":\"Legion Commander\"},{\"name\":\"techies\",\"id\":105,\"localized_name\":\"Techies\"},{\"name\":\"ember_spirit\",\"id\":106,\"localized_name\":\"Ember Spirit\"},{\"name\":\"earth_spirit\",\"id\":107,\"localized_name\":\"Earth Spirit\"},{\"name\":\"abyssal_underlord\",\"id\":108,\"localized_name\":\"Abyssal Underlord\"},{\"name\":\"terrorblade\",\"id\":109,\"localized_name\":\"Terrorblade\"},{\"name\":\"phoenix\",\"id\":110,\"localized_name\":\"Phoenix\"},{\"name\":\"oracle\",\"id\":111,\"localized_name\":\"Oracle\"},{\"name\":\"winter_wyvern\",\"id\":112,\"localized_name\":\"Winter Wyvern\"},{\"name\":\"arc_warden\",\"id\":113,\"localized_name\":\"Arc Warden\"},{\"name\":\"monkey_king\",\"id\":114,\"localized_name\":\"Monkey King\"}]}").heroes.ToArray();
        //private static readonly  resultArray = result.heroes.ToArray();

        private static void Main()
        {
            /*
            int i = 0;
            foreach(var x in resultArray)
            {

                Console.WriteLine(x.localized_name);
                Console.WriteLine(x.id);
                Console.WriteLine(i);
                if (i != x.id) break;
                i++;
            }
            */
            Console.WriteLine("------------");




            MMRDict.Clear();
            WinRateDict.Clear();
            BestHeroesDict.Clear();
            Console.WriteLine("Dictionary Cleared.");

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
            Menu.Item("Hide/Show").SetValue<bool>(true);
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("Enable").GetValue<bool>()) return;

            if ((Game.IsInGame || Game.GameState.ToString() == "HeroSelection" || Game.GameState.ToString() == "StrategyTime"))
            {

                //Console.WriteLine(!_isExecutedFirst && Game.GameState.ToString() == "HeroSelection");

                if ((!_isExecutedFirst && _isPressedFirst))
                {
                    List<Player> players = ObjectManager.GetEntities<Player>().Where(x => x.Team != Team.Observer).OrderBy(x => x.Id).ToList(); // (x.Team == Team.Radiant || x.Team == Team.Dire)
                    Parallel.ForEach(players, player => Analyze(player));
                    _isExecutedFirst = true;
                }

                if (Game.GameTime >= -10.5 && Game.GameTime <= -9.5)
                {
                    Menu.Item("Hide/Show").SetValue<bool>(false);
                }

                return;
            }

            //Clear Dictionary when game is over.
            else if (Game.GameState.ToString() == "Init" && MMRDict.Any())
            {
                MMRDict.Clear();
                WinRateDict.Clear();
                BestHeroesDict.Clear();
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
            if (Drawing.Direct3DDevice9 == null || !(Menu.Item("Hide/Show").GetValue<bool>())) return;
            if (_isPressedFirst == false) return;

            //textFont.DrawText(null, ".", 1417, 817, SharpDX.Color.Yellow); //For Testing Purposes
            //Game.PrintMessage("<font color='#00aaff'>" + String.Format("{0, -2} | {1, -4} | {2, -3}% | {3, -4}", p.ID, mmr, wr, role), MessageType.ChatMessage);

            List<Player> players = ObjectManager.GetEntities<Player>().Where(x => x.Team != Team.Observer).ToList();

            if (_isDisplayedFirst)
            {
                //Panel Positions.
                int textPosX = 1435 + Menu.Item("BarPosX").GetValue<Slider>().Value;
                int textPosY = 850 + Menu.Item("BarPosY").GetValue<Slider>().Value;
                int horDist = 60;
                int boxDist = 15; //Distance of Box
                int vertDist = hText;

                //Header
                textFont.DrawText(null, "MMR | WIN % | BEST HERO", textPosX, textPosY - vertDist, SharpDX.Color.White);
                SharpDX.Color textColor = SharpDX.Color.White;

                foreach (Player player in players)
                {

                    //Player Color.
                    SharpDX.Color playerColor = SelectColor(player.Id);
                    string mmr;
                    string wr;
                    string bestHero;

                    if (player.Team == Team.Radiant)
                    {

                        //Radiant text position.
                        Vector2 startPosMMRR = new Vector2(textPosX, textPosY + player.TeamSlot * vertDist);
                        Vector2 startPosWRR = new Vector2(textPosX + 1 * horDist, textPosY + player.TeamSlot * vertDist);
                        Vector2 startPosRoleR = new Vector2(textPosX + 2 * horDist, textPosY + player.TeamSlot * vertDist);

                        if (BestHeroesDict.TryGetValue(player.PlayerSteamId , out bestHero))
                        {
                            //Console.WriteLine(BestHeroesDict[p.PlayerSteamId ][0]);
                            textFont.DrawText(null, bestHero, (int)startPosRoleR.X, (int)startPosRoleR.Y, textColor);
                        }

                        if (MMRDict.TryGetValue(player.PlayerSteamId , out mmr))
                        {
                            textFont.DrawText(null, "🔲", (int)startPosMMRR.X - boxDist, (int)startPosMMRR.Y, playerColor);
                            textFont.DrawText(null, mmr, (int)startPosMMRR.X, (int)startPosMMRR.Y, textColor);
                        }

                        if (WinRateDict.TryGetValue(player.PlayerSteamId , out wr))
                        {
                            textFont.DrawText(null, wr + "%", (int)startPosWRR.X, (int)startPosWRR.Y, textColor);
                        }





                    }

                    else if (player.Team == Team.Dire)
                    {

                        //Dire text position.
                        Vector2 startPosMMRD = new Vector2(textPosX + 5 * horDist, textPosY + player.TeamSlot * vertDist);
                        Vector2 startPosWRD = new Vector2(textPosX + 6 * horDist, textPosY + player.TeamSlot * vertDist);
                        Vector2 startPosRoleD = new Vector2(textPosX + 7 * horDist, textPosY + player.TeamSlot * vertDist);

                        if (BestHeroesDict.TryGetValue(player.PlayerSteamId , out bestHero))
                        {
                            //Console.WriteLine(BestHeroesDict[p.PlayerSteamId ][0]);
                            textFont.DrawText(null, bestHero, (int)startPosRoleD.X, (int)startPosRoleD.Y, textColor);
                        }

                        if (MMRDict.TryGetValue(player.PlayerSteamId , out mmr))
                        {
                            textFont.DrawText(null, "🔲", (int)startPosMMRD.X - boxDist, (int)startPosMMRD.Y, playerColor);
                            textFont.DrawText(null, mmr, (int)startPosMMRD.X, (int)startPosMMRD.Y, textColor);
                        }

                        if (WinRateDict.TryGetValue(player.PlayerSteamId , out wr))
                        {
                            textFont.DrawText(null, wr + "%", (int)startPosWRD.X, (int)startPosWRD.Y, textColor);
                        }
                    }

                    else return;
                }
                   return;

            }
        }

        private static async void URLParse(Player player, string url, string type)
        {
            using (HttpClient client = new HttpClient())

            using (HttpResponseMessage urlResponse = await client.GetAsync(url))

            using (HttpContent URLContent = urlResponse.Content)
            {
                var urlJSONObject = await URLContent.ReadAsStringAsync();
                dynamic deserializedJSONObject = JsonConvert.DeserializeObject(urlJSONObject);

                if (type == "player")
                {
                    var soloCompetitiveRank = deserializedJSONObject["solo_competitive_rank"].ToString() == "" ? 0 : int.Parse(deserializedJSONObject["solo_competitive_rank"].ToString());
                    var mmrEstimate = deserializedJSONObject["mmr_estimate"]["estimate"].ToString() == "" ? 0 : int.Parse(deserializedJSONObject["mmr_estimate"]["estimate"].ToString());
                    var trueMMR = soloCompetitiveRank > mmrEstimate ? soloCompetitiveRank.ToString() : mmrEstimate.ToString();
                    MMRDict.Add(player.PlayerSteamId , trueMMR);

                    /*
                    Console.WriteLine("soloCompetitiveRank:" + soloCompetitiveRank);
                    Console.WriteLine("mmrEstimate:" + mmrEstimate);
                    Console.WriteLine("trueMMR: " + trueMMR);
                    */
                }

                else if (type == "heroes")
                {

                    string firstBestHeroID = deserializedJSONObject[0]["hero_id"];
                    /*
                    string secondBestHeroID = deserializedJSONObject[1]["hero_id"];
                    string thirdBestHeroID = deserializedJSONObject[2]["hero_id"];
                    var secondBestHeroName = resultArray[int.Parse(secondBestHeroID)].localized_name;//.heroes.Where(hero => hero.id == int.Parse(secondBestHeroID)).Select(hero => hero.localized_name).FirstOrDefault();
                    var thirdBestHeroName = resultArray[int.Parse(thirdBestHeroID)].localized_name; ;//.heroes.Where(hero => hero.id == int.Parse(thirdBestHeroID)).Select(hero => hero.localized_name).FirstOrDefault();
                    */



                    if (int.Parse(firstBestHeroID) != resultArray[int.Parse(firstBestHeroID)].id)
                    {
                        Console.WriteLine("firstbest: " + firstBestHeroID);
                        Console.WriteLine("firstbest parse: " + int.Parse(firstBestHeroID));
                        Console.WriteLine(resultArray[int.Parse(firstBestHeroID)].id);
                        Console.WriteLine("FUCK");
                    }

                    var firstBestHeroName = resultArray[int.Parse(firstBestHeroID)].localized_name; //.heroes.Where(hero => hero.id == int.Parse(firstBestHeroID)).Select(hero => hero.localized_name).FirstOrDefault();
                    BestHeroesDict.Add(player.PlayerSteamId , firstBestHeroName);

                    /*
                    Console.WriteLine("firstBestHero: " + firstBestHeroName);
                    Console.WriteLine("secondBestHero: " + secondBestHeroName);
                    Console.WriteLine("thirdBestHero: " + thirdBestHeroName);
                    */
                }

                else if (type == "wl")
                {
                    string win = deserializedJSONObject["win"];
                    string lose = deserializedJSONObject["lose"];
                    var winPercentage = Math.Round(((float.Parse(win) / (float.Parse(win) + float.Parse(lose))) * 100), 0).ToString();
                    WinRateDict.Add(player.PlayerSteamId , winPercentage);

                    /*
                    Console.WriteLine("win: " + win);
                    Console.WriteLine("lose: " + lose);
                    Console.WriteLine("win% : " + winPercentage);
                    */

                }

                else
                {
                    throw new Exception("This is not a type to be parsed!");
                }
            }
        }

        private static async Task Analyze(Player player)
        {

            string steamID = player.PlayerSteamId .ToString();
            var playerHeroesURL = "https://api.opendota.com/api/players/" + steamID + "/heroes?date=30&win=1";
            var playerURL = "https://api.opendota.com/api/players/" + steamID;
            var playerWinLoseURL = "https://api.opendota.com/api/players/" + steamID + "/wl?date=30";
            //var playerCountsURL = "https://api.opendota.com/api/players/" + steamID + "/counts?date=30";

            Parallel.Invoke(
                () => URLParse(player, playerHeroesURL, "heroes"),
                () => URLParse(player, playerURL, "player"),
                 () => URLParse(player, playerWinLoseURL, "wl"));


            _isDisplayedFirst = true;

            //Console.WriteLine("------------------------------------");
            //Console.WriteLine("Name: " + player.Name);

        }


        private static SharpDX.Color SelectColor(int playerID)
        {
            switch (playerID)
            {
                case 0:
                    return playerColor0;
                case 1:
                    return playerColor1;
                case 2:
                    return playerColor2;
                case 3:
                    return playerColor3;
                case 4:
                    return playerColor4;
                case 5:
                    return playerColor5;
                case 6:
                    return playerColor6;
                case 7:
                    return playerColor7;
                case 8:
                    return playerColor8;
                case 9:
                    return playerColor9;
                default:
                    return SharpDX.Color.White;
            }
        }
    }
}

