using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;

using EloBuddy; namespace YasuoPro
{
    static class Extensions
    {
        internal static AIHeroClient Player = Helper.Yasuo;

        internal static bool IsDashable(this Obj_AI_Base unit, float range = 475)
        {
            if (!SpellSlot.E.LSIsReady() || unit == null || unit.Team == Player.Team || unit.LSDistance(Player) > range || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }

            if (Helper.GetBool("Misc.SafeE"))
            {
                var point = Helper.GetDashPos(unit);
                if (!EvadeYas.Program.IsSafe(point).IsSafe)
                {
                    return false;
                }
            }

            var minion = unit as Obj_AI_Minion;
            return !unit.LSHasBuff("YasuoDashWrapper") && (unit is AIHeroClient || minion.IsValidMinion());
        }

        internal static bool IsDashableFrom(this Obj_AI_Base unit, Vector2 fromPos, float range = 475)
        {
            if (unit == null || unit.Team == Player.Team || unit.LSDistance(fromPos) > range || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }

            if (Helper.GetBool("Misc.SafeE"))
            {
                var point = Helper.GetDashPos(unit);
                if (!EvadeYas.Program.IsSafe(point).IsSafe)
                {
                    return false;
                }
            }

            var minion = unit as Obj_AI_Minion;
            return !unit.LSHasBuff("YasuoDashWrapper") && (unit is AIHeroClient || minion.IsValidMinion());
        }


        internal static bool IsValidMinion(this Obj_AI_Minion minion, float range = 2000)
        {
            if (minion == null)
            {
                return false;
            }

            var name = minion.CharData.BaseSkinName.ToLower();
            return (Player.LSDistance(minion) <= range && minion.IsValid && minion.IsTargetable && !minion.IsInvulnerable && minion.IsVisible && minion.Team != Player.Team && minion.IsHPBarRendered && !MinionManager.IsWard(minion) && !name.Contains("gangplankbarrel"));
        }

        internal static bool IsValidAlly(this Obj_AI_Base unit, float range = 2000)
        {
            if (unit == null || unit.LSDistance(Player) > range || unit.Team != Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValidEnemy(this Obj_AI_Base unit, float range = 2000)
        {
            if (unit == null || !unit.IsHPBarRendered || unit.IsZombie || unit.LSDistance(Player) > range || unit.Team == Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector2.Distance(unit.ServerPosition.LSTo2D(), Helper.Yasuo.ServerPosition.LSTo2D()) <= range;
            }
            return false;
        }

        internal static bool PointUnderEnemyTurret(this Vector2 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Find(t => t.IsEnemy && Vector2.Distance(Point, t.Position.LSTo2D()) < 910f + Helper.Yasuo.BoundingRadius);
            return EnemyTurrets != null;
        }

        internal static bool PointUnderEnemyTurret(this Vector3 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy && Vector3.Distance(t.Position, Point) < 910f + Helper.Yasuo.BoundingRadius);
            return EnemyTurrets.Any();
        }

        internal static bool CanKill(this Obj_AI_Base @base, SpellSlot slot)
        {
            if (slot == SpellSlot.E)
            {
                return Helper.GetProperEDamage(@base) >= @base.Health;
            }
            return Player.LSGetSpellDamage(@base, slot) >= @base.Health;
        }

        internal static bool IsCloserWP(this Vector2 point, Obj_AI_Base target)
        {
            var wp = target.LSGetWaypoints();
            var lastwp = wp.LastOrDefault();
            var wpc = wp.Count();
            var midwpnum = wpc / 2;
            var midwp = wp[midwpnum];
            var plength = wp[0].LSDistance(lastwp);
            return (point.LSDistance(target.ServerPosition) < 0.95f * Player.LSDistance(target.ServerPosition) - Helper.Yasuo.BoundingRadius) || ((plength < Player.LSDistance(target.ServerPosition) * 1.2f && point.LSDistance(lastwp.To3D()) < Player.LSDistance(lastwp.To3D()) || point.LSDistance(midwp.To3D()) < Player.LSDistance(midwp)));
        }

        internal static bool IsCloser(this Vector2 point, Obj_AI_Base target)
        {
            if (Helper.GetBool("Combo.EAdvanced"))
            {
                return IsCloserWP(point, target);
            }
            return (point.LSDistance(target.ServerPosition) < 0.95f * Player.LSDistance(target.ServerPosition) - Helper.Yasuo.BoundingRadius);
        }

        internal static bool IsCloser(this Obj_AI_Base @base, Obj_AI_Base target)
        {
            return Helper.GetDashPos(@base).LSDistance(target.ServerPosition) < Player.LSDistance(target.ServerPosition);
        }

        internal static Vector3 WTS(this Vector3 vect)
        {
            return Drawing.WorldToScreen(vect).To3D();
        }


        //Menu Extensions

        internal static Menu AddSubMenu(this Menu menu, string disp)
        {
            return menu.AddSubMenu(new Menu(disp, Assembly.GetExecutingAssembly().GetName() + "." + disp));
        }

        internal static MenuItem AddBool(this Menu menu, string name, string displayname, bool @defaultvalue = true)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(@defaultvalue));
        }

        internal static MenuItem AddKeyBind(this Menu menu, string name, string displayname, uint key, KeyBindType type)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new KeyBind(key, type)));
        }

        internal static MenuItem AddCircle(this Menu menu, string name, string dname, float range, System.Drawing.Color col)
        {
            return menu.AddItem(new MenuItem(name, name).SetValue(new Circle(true, col, range)));
        }

        internal static MenuItem AddSlider(this Menu menu, string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new Slider(initial, min, max)));
        }

        internal static MenuItem AddSList(this Menu menu, string name, string displayname, string[] stringlist, int @default = 0)
        {
           return menu.AddItem(new MenuItem(name, displayname).SetValue(new StringList(stringlist, @default)));
        }

        internal static bool IsTargetValid(this AttackableUnit unit,
        float range = float.MaxValue,
        bool checkTeam = true,
        Vector3 from = new Vector3())
        {
            if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable ||
                unit.IsInvulnerable)
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;
            if (@base != null)
            {
                if (@base.LSHasBuff("kindredrnodeathbuff") && @base.HealthPercent <= 10)
                {
                    return false;
                }
            }

            if (checkTeam && unit.Team == ObjectManager.Player.Team)
            {
                return false;
            }

            var unitPosition = @base != null ? @base.ServerPosition : unit.Position;

            return !(range < float.MaxValue) ||
                   !(Vector2.DistanceSquared(
                       (@from.LSTo2D().LSIsValid() ? @from : ObjectManager.Player.ServerPosition).LSTo2D(),
                       unitPosition.LSTo2D()) > range * range);
        }

        internal static bool QCanKill(this Obj_AI_Base minion, bool isQ2 = false)
        {
            //var hpred =
            //  HealthPrediction.GetHealthPrediction(minion, 0, 500 + Game.Ping / 2);
            // return hpred < 0.95 * Player.LSGetSpellDamage(minion, SpellSlot.Q) && hpred > 0;
            var qspell = isQ2 ? Helper.Spells[Helper.Q2] : Helper.Spells[Helper.Q];
            var dmg = Player.LSGetSpellDamage(minion, SpellSlot.Q) / 1.3
                            >= HealthPrediction.GetHealthPrediction(
                                minion,
                                (int)(Player.LSDistance(minion) / qspell.Speed) * 1000,
                                (int)qspell.Delay * 1000);
            return dmg;
        }

        internal static bool ECanKill(this Obj_AI_Base minion)
        {
            var espell = Helper.Spells[Helper.E];
            return Helper.GetProperEDamage(minion) / 1.2
                            >= HealthPrediction.GetHealthPrediction(
                                minion,
                                (int)(Player.LSDistance(minion) / espell.Speed) * 1000,
                                (int)espell.Delay * 1000);
        }

        internal static bool isBlackListed(this AIHeroClient unit)
        {
            return !Helper.GetBool("ult" + unit.ChampionName);
        }

        internal static int MinionsInRange(this Obj_AI_Base unit, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Count(x => x.LSDistance(unit) <= range && x.NetworkId != unit.NetworkId && x.Team == unit.Team);
            return minions;
        }

        internal static int MinionsInRange(this Vector2 pos, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Count(x => x.LSDistance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }

        internal static int MinionsInRange(this Vector3 pos, float range)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(x => x.LSDistance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }


        internal static IEnumerable<Obj_AI_Minion> GetMinionsInRange(this Vector2 pos, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.LSIsValidTarget() && x.LSDistance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }

    }
}
