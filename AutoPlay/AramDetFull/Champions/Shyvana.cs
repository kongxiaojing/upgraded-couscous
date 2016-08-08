using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

using EloBuddy; namespace ARAMDetFull.Champions
{
    class Shyvana : Champion
    {

        public Shyvana()
        {
            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                {
                    new ConditionalItem(ItemId.Blade_of_the_Ruined_King),
                    new ConditionalItem(ItemId.Mercurys_Treads, ItemId.Ninja_Tabi, ItemCondition.ENEMY_AP),
                    new ConditionalItem(ItemId.Spirit_Visage, ItemId.Randuins_Omen, ItemCondition.ENEMY_AP),
                    new ConditionalItem(ItemId.Sunfire_Cape),
                    new ConditionalItem(ItemId.Ravenous_Hydra_Melee_Only),
                    new ConditionalItem(ItemId.Banshees_Veil),
                },
                startingItems = new List<ItemId>
                {
                    ItemId.Vampiric_Scepter,ItemId.Boots_of_Speed
                }
            };
        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.LSIsReady())
                return;
            Q.CastOnUnit(target);
        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.LSIsReady())
                return;
            if (safeGap(target))
                W.CastOnUnit(target);
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.LSIsReady() || W.LSIsReady())
                return;
            E.Cast(target);
        }

        public override void useR(Obj_AI_Base target)
        {
            return;
            if (!R.LSIsReady())
                return;
            if (player.Path.Length > 0 && player.Path[player.Path.Length - 1].LSDistance(player.Position) > 2500)
            {
                R.Cast(player.Path[player.Path.Length - 1]);
            }


        }

        public override void setUpSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.E, 2500);
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(600);
            if (tar == null)
                return;
            useQ(tar);
            useW(tar);
            useE(tar);
            useR(tar);
        }
    }
}
