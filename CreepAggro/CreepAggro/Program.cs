using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

using SharpDX;

namespace CreepAggro
{
    class Program
    {
        private static readonly Menu Menu = new Menu("CreepAggro", "CreepAggro", true);

        private static readonly MenuItem AggroKeyItem =
            new MenuItem("aggroKey", "Aggro Key").SetValue(new KeyBind('N', KeyBindType.Press));
        private static readonly MenuItem UnaggroKeyItem =
            new MenuItem("unaggroKey", "Unaggro Key").SetValue(new KeyBind('B', KeyBindType.Press));

        private static readonly MenuItem AggroRangeEffectItem =
            new MenuItem("aggroRangeEffect", "Mark aggroable creeps").SetValue(true);

        private static readonly MenuItem AttackCooldown =
            new MenuItem("AttackCooldown", "AttackCooldown").SetValue(new Slider(1000, 0, 2000));

        private static readonly MenuItem AggroRange =
            new MenuItem("AggroRange", "Aggro Range").SetValue(true);

        

        private static readonly Dictionary<Unit, ParticleEffect> Effects = new Dictionary<Unit, ParticleEffect>();

        private static Vector3 _currentMovePosition = new Vector3(0, 0, 0);

        private static bool _isAttacking = false;
        private static Vector3 _facingDirection;

        public static void Main(string[] args)
        {
            AggroRangeEffectItem.ValueChanged += Item_ValueChanged;
            //AggroRange.ValueChanged += Item_ValueChanged; 

            Menu.AddItem(AggroRangeEffectItem);
            Menu.AddItem(AggroRange);
            Menu.AddItem(AggroKeyItem);
            Menu.AddItem(UnaggroKeyItem);
            Menu.AddItem(AttackCooldown);
            Menu.AddToMainMenu();

            Game.OnIngameUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            var player = ObjectManager.LocalPlayer;
            if (!Game.IsInGame || player == null || player.Team == Team.Observer) return;
            
            Hero me = ObjectManager.LocalHero;
            var creeps = ObjectManager.GetEntities<Creep>().ToList();

            //RangeEffect(me);

            if (Utils.SleepCheck("aggroDrawing"))
            {
                // apply range effect
                foreach (var creep in creeps)
                {
                    HandleEffect(creep, me);
                }

                Utils.Sleep(Game.Ping + 200, "aggroDrawing");
            }


        }

        // ReSharper disable once InconsistentNaming
        private static void Item_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            foreach (var particleEffect in Effects.Values)
            {
                particleEffect.Dispose();
            }
            Effects.Clear();
        }

        private static void Game_OnUpdate(System.EventArgs args)
        {
            var player = ObjectManager.LocalPlayer;
            if (!Game.IsInGame || player == null || player.Team == Team.Observer || Game.IsChatOpen)
            {
                return;
            }

            var me = ObjectManager.LocalHero;
            if (me == null || !me.IsAlive)
            {
                return;
            }

            // instantly cancel attack
            if (_isAttacking)
            {
                _isAttacking = false;
                me.Move(_facingDirection);
                me.Hold(true);
            }

            var creeps = ObjectManager.GetEntities<Creep>().ToList();

            if (Utils.SleepCheck("attackSleep"))
            {
                Unit target = null;

                // aggro                
                if (Game.IsKeyDown((AggroKeyItem.GetValue<KeyBind>().Key))) 
                {
                    target = GetHeroes(me).FirstOrDefault(x => x.Team != me.Team);
                }

                // unaggro
                if (Game.IsKeyDown((UnaggroKeyItem.GetValue<KeyBind>().Key)))
                {
                    target = GetHeroes(me).FirstOrDefault(x => x.Team == me.Team);
                }

                if (target != null)
                {
                    me.Attack(target);
                    _facingDirection = Prediction.InFront(me, 25);
                    _isAttacking = true;
                    Utils.Sleep(Game.Ping + AttackCooldown.GetValue<Slider>().Value, "attackSleep");
                }
            }
        }

        private static bool IsAggroable(Unit x, Hero me)
        {
            return x != null && x.IsValid && x.IsAlive && x.Team != me.Team
                && me.Distance2D(x) <= 500 && me.IsVisibleToEnemies;
        }

        private static List<Hero> GetHeroes(Hero me)
        {
            return ObjectManager.GetEntities<Hero>()
                .Where(x => x != null && x.IsValid && x.IsAlive && x.IsVisible)
                .OrderBy(me.Distance2D).ToList();
        }

        static void HandleEffect(Unit unit, Hero me)
        {
            if (IsAggroable(unit, me) && GetHeroes(me).Any())
            {
                ParticleEffect effect;
                if (!Effects.TryGetValue(unit, out effect) && AggroRangeEffectItem.GetValue<bool>())
                {
                    effect = unit.AddParticleEffect(@"particles\units\heroes\hero_beastmaster\beastmaster_wildaxe_glow.vpcf");
                    Effects.Add(unit, effect);
                }
            }
            else
            {
                ParticleEffect effect;
                if (Effects.TryGetValue(unit, out effect))
                {
                    effect.Dispose();
                    Effects.Remove(unit);
                }
            }
        }

        /*
        static void RangeEffect (Hero me)
        {
            if (AggroRange.GetValue<bool>())
            {
                ParticleEffect range;
                int aggroRangeDist = 500;
                range = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
                range.SetControlPoint(2, new Vector3(aggroRangeDist, 255, 0));
                range.SetControlPoint(1, new Vector3(0, 255, 0));
            }            
        }
        */



    }
}