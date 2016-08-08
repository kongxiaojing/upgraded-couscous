using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Hikigaya_Syndra
{
    static class Helper
    {
        public static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (onlyOrbs)
                return OrbManager.GetOrbToGrab((int)Program.W.Range);
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.LSIsValidTarget(Program.W.Range)))
                return minion.ServerPosition;
            return OrbManager.GetOrbToGrab((int)Program.W.Range);
        }

        public static void UseW(Obj_AI_Base grabObject, Obj_AI_Base enemy)
        {
            if (grabObject != null && Program.W.LSIsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                var gObjectPos = GetGrabableObjectPos(false);

                if (gObjectPos.LSTo2D().LSIsValid() && Environment.TickCount - Program.Q.LastCastAttemptT > Game.Ping + 150 
                    && Environment.TickCount - Program.E.LastCastAttemptT > 750 + Game.Ping && Environment.TickCount - Program.W.LastCastAttemptT > 750 + Game.Ping)
                {
                    var grabsomething = false;
                    if (enemy != null)
                    {
                        var pos2 = Program.W.GetPrediction(enemy, true);
                        if (pos2.Hitchance >= HitChance.High) grabsomething = true;
                    }
                    if (grabsomething || grabObject.IsStunned)
                    {
                        Program.W.Cast(gObjectPos);
                    }
                        
                }
            }
            if (enemy != null && Program.W.LSIsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
            {
                var pos = Program.W.GetPrediction(enemy, true);
                if (pos.Hitchance >= HitChance.High)
                {
                    Program.W.Cast(pos.CastPosition);
                }  
            }
        }
        public static void UseQe(Obj_AI_Base target)
        {
            if (!Program.Q.LSIsReady() || !Program.E.LSIsReady() || target == null) return;
            var sPos = Prediction.GetPrediction(target, Program.Q.Delay + Program.E.Delay).UnitPosition;
            if (ObjectManager.Player.LSDistance(sPos, true) > Math.Pow(Program.E.Range, 2))
            {
                var orb = ObjectManager.Player.ServerPosition + Vector3.Normalize(sPos - ObjectManager.Player.ServerPosition) * Program.E.Range;
                Program.Qe.Delay = Program.Q.Delay + Program.E.Delay + ObjectManager.Player.LSDistance(orb) / Program.E.Speed;
                var pos = Program.Qe.GetPrediction(target);
                if (pos.Hitchance >= HitChance.High)
                {
                    UseQe2(target, orb);
                }
            }
            else
            {
                Program.Q.Width = 40f;
                var pos = Program.Q.GetPrediction(target, true);
                Program.Q.Width = 125f;
                if (pos.Hitchance >= HitChance.VeryHigh)
                    UseQe2(target, pos.UnitPosition);
            }
        }

        public static void UseE(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }
            foreach (var orb in OrbManager.GetOrbs(true).Where(orb => orb.LSTo2D().LSIsValid() && ObjectManager.Player.LSDistance(orb, true) < Math.Pow(Program.E.Range, 2)))
            {
                var sp = orb.LSTo2D() + Vector2.Normalize(ObjectManager.Player.ServerPosition.LSTo2D() - orb.LSTo2D()) * 100f;
                var ep = orb.LSTo2D() + Vector2.Normalize(orb.LSTo2D() - ObjectManager.Player.ServerPosition.LSTo2D()) * 592;
                Program.Qe.Delay = Program.E.Delay + ObjectManager.Player.LSDistance(orb) / Program.E.Speed;
                Program.Qe.UpdateSourcePosition(orb);
                var pPo = Program.Qe.GetPrediction(target).UnitPosition.LSTo2D();
                if (pPo.LSDistance(sp, ep, true, true) <= Math.Pow(Program.Qe.Width + target.BoundingRadius, 2))
                {
                    Program.E.Cast(orb);
                }
            }
        }

        public static void UseQe2(Obj_AI_Base target, Vector3 pos)
        {
            if (target == null || !(ObjectManager.Player.LSDistance(pos, true) <= Math.Pow(Program.E.Range, 2)))
            {
                return;
            }
               
            var sp = pos + Vector3.Normalize(ObjectManager.Player.ServerPosition - pos) * 100f;
            var ep = pos + Vector3.Normalize(pos - ObjectManager.Player.ServerPosition) * 592;
            Program.Qe.Delay = Program.Q.Delay + Program.E.Delay + ObjectManager.Player.ServerPosition.LSDistance(pos) / Program.E.Speed;
            Program.Qe.UpdateSourcePosition(pos);
            var pPo = Program.Qe.GetPrediction(target).UnitPosition.LSTo2D().LSProjectOn(sp.LSTo2D(), ep.LSTo2D());

            if (!pPo.IsOnSegment ||
                !(pPo.SegmentPoint.LSDistance(target, true) <= Math.Pow(Program.Qe.Width + target.BoundingRadius, 2)))
            {
                return;
            }
                
            var delay = 280 - (int)(ObjectManager.Player.LSDistance(pos) / 2.5) + Program.Config.Item("q.e.delay").GetValue<Slider>().Value;
            LeagueSharp.Common.Utility.DelayAction.Add(Math.Max(0, delay), () => Program.E.Cast(pos));
            Program.Qe.LastCastAttemptT = Environment.TickCount;
            Program.Q.Cast(pos);
            UseE(target);
        }

        public static float TotalDamage(Obj_AI_Base enemy)
        {
            var damage = 0f;
            if (Program.Q.LSIsReady() && Program.Config.Item("q.combo").GetValue<bool>())
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    Program.Q.GetDamage(enemy));
            }
            if (Program.W.LSIsReady() && Program.Config.Item("w.combo").GetValue<bool>())
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    Program.W.GetDamage(enemy));
            }
            if (Program.E.LSIsReady() && Program.Config.Item("e.combo").GetValue<bool>())
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    Program.E.GetDamage(enemy));
            }
            if (Program.R.LSIsReady() && Program.Config.Item("r.combo").GetValue<bool>())
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    Program.R.GetDamage(enemy));
            }
            if (Program.IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Program.IgniteSlot) == SpellState.Ready &&
                Program.Config.Item("r.combo").GetValue<bool>())
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite));
            }
            return damage;
        }
        public static bool BuffCheck(Obj_AI_Base enemy)
        {
            if (enemy.LSHasBuff("UndyingRage") && Program.Config.Item("undy.tryn").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("JudicatorIntervention") && Program.Config.Item("undy.kayle").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("ZacRebirthReady") && Program.Config.Item("undy.zac").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("AttroxPassiveReady") && Program.Config.Item("undy.aatrox").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("Chrono Shift") && Program.Config.Item("undy.aatrox").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("Ferocious Howl") && Program.Config.Item("undy.alistar").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("Black Shield") && Program.Config.Item("undy.morgana").GetValue<bool>())
            {
                return true;
            }
            else if (enemy.LSHasBuff("Spell Shield") && Program.Config.Item("undy.sivir").GetValue<bool>())
            {
                return true;
            }
            return false;
        }
    }
}
