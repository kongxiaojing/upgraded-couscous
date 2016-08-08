using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; namespace ARAMDetFull.Champions
{
    class Singed : Champion
    {

        public Singed()
        {

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Rod_of_Ages),
                            new ConditionalItem(ItemId.Mercurys_Treads),
                            new ConditionalItem(ItemId.Randuins_Omen),
                            new ConditionalItem(ItemId.Rylais_Crystal_Scepter),
                            new ConditionalItem(ItemId.Liandrys_Torment),
                            new ConditionalItem(ItemId.Banshees_Veil),
                        },
                startingItems = new List<ItemId>
                        {
                            ItemId.Catalyst_the_Protector
                        }
            };
        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.LSIsReady())
                return;
            if (!player.LSHasBuff("Poison Trail"))
            {
                Q.Cast();
            }
        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.LSIsReady() || player.LSUnderTurret(true) || MapControl.fightIsOn() == null)
                return;
            W.Cast(target.Position);
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.LSIsReady())
                return;
            E.CastOnUnit(target);

        }

        public override void useR(Obj_AI_Base target)
        {
            if (!R.LSIsReady() || player.HealthPercent>70 )
                return;
            R.Cast();
        }

        public override void setUpSpells()
        {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 1175);
            E = new Spell(SpellSlot.E, 125);
            R = new Spell(SpellSlot.R, 250);
            
            W.SetSkillshot(0.5f, 350, 700, false, SkillshotType.SkillshotCircle);
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(550);
            if (tar != null) useQ(tar); else if (player.LSHasBuff("Poison Trail")) {if(Q.LSIsReady())Q.Cast();}
            tar = ARAMTargetSelector.getBestTarget(E.Range);
            if (tar != null) useE(tar);
            tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null) useW(tar);
            tar = ARAMTargetSelector.getBestTarget(300);
            if (tar != null) useR(tar);
        }

        public static bool EnemyInRange(int numOfEnemy, float range)
        {
            return LeagueSharp.Common.Utility.LSCountEnemysInRange(ObjectManager.Player, (int)range) >= numOfEnemy;
        }

    }
}
