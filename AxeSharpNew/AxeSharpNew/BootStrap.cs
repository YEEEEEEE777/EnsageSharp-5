namespace AxeSharpNewNew
{
    using System;

    using Ensage;
    using Ensage.Common;

    internal class Bootstrap
    {
        #region Fields

        private readonly AxeSharpNew axeSharpNew = new AxeSharpNew();

        #endregion

        #region Public Methods and Operators

        public void Initialize()
        {
            Events.OnLoad += OnLoad;
        }

        #endregion

        #region Methods

        private void Drawing_OnDraw(EventArgs args)
        {
            axeSharpNew.OnDraw();
        }

        private void Game_OnUpdate(EventArgs args)
        {
            axeSharpNew.OnUpdate();
        }

        private void OnClose(object sender, EventArgs e)
        {
            Events.OnClose -= OnClose;
            Game.OnIngameUpdate -= Game_OnUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
            Player.OnExecuteOrder -= Player_OnExecuteAction;
            axeSharpNew.OnClose();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (ObjectManager.LocalHero.ClassID != ClassID.CDOTA_Unit_Hero_Shredder)
            {
                return;
            }

            axeSharpNew.OnLoad();
            Events.OnClose += OnClose;
            Game.OnIngameUpdate += Game_OnUpdate;
            Player.OnExecuteOrder += Player_OnExecuteAction;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            axeSharpNew.OnExecuteAbilitiy(sender, args);
        }

        #endregion
    }
}
