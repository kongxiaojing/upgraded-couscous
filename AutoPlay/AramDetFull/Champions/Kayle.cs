using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;

using EloBuddy; namespace ARAMDetFull.Champions
{
    class Kayle : Champion
    {

        public Kayle()
        {
            Obj_AI_Base.OnProcessSpellCast += HealUltTrigger;

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Nashors_Tooth),
                            new ConditionalItem(ItemId.Berserkers_Greaves),
                            new ConditionalItem(ItemId.Guinsoos_Rageblade),
                            new ConditionalItem(ItemId.Rabadons_Deathcap),
                            new ConditionalItem(ItemId.Lich_Bane),
                            new ConditionalItem(ItemId.Zhonyas_Hourglass),
                        },
                startingItems = new List<ItemId>
                        {
                            ItemId.Fiendish_Codex,ItemId.Boots_of_Speed
                        }
            };
        }

        void HealUltTrigger(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var target = args.Target as AIHeroClient;
            var senderhero = sender as AIHeroClient;
            var senderturret = sender as Obj_AI_Turret;

            if (sender.IsAlly || (target == null) || !target.IsAlly)
            {
                return;
            }
            float setvaluehealth = 55;
            float setvalueult = 26;

            bool triggered = false;

            if (W.LSIsReady()  && (target.HealthPercent <= setvaluehealth))
            {
                HealUltManager(true, false, target);
                triggered = true;
            }
            if (R.LSIsReady()  && (target.HealthPercent <= setvaluehealth))
            {
                HealUltManager(false, true, target);
                triggered = true;
            }

            if (triggered)
            {
                return;
            }

            var damage = sender.LSGetSpellDamage(target, args.SData.Name);
            var afterdmg = ((target.Health - damage) / (target.MaxHealth)) * 100f;

            if (W.LSIsReady() && player.LSDistance(target) <= W.Range  && (target.HealthPercent <= setvaluehealth || ( afterdmg <= setvaluehealth)))
            {
                    HealUltManager(true, false, target);
            }

            if (R.LSIsReady() && player.LSDistance(target) <= R.Range && (target.HealthPercent <= setvalueult || (afterdmg <= setvalueult)) && (senderhero != null || senderturret != null || target.HealthPercent < 5f))
            {
                HealUltManager(false, true, target);
            }

        }

        void HealUltManager(bool forceheal = false, bool forceult = false, AIHeroClient target = null)
        {
            if (forceheal && target != null && W.LSIsReady() && player.LSDistance(target) <= W.Range)
            {
                W.CastOnUnit(target);
                return;
            }
            if (forceult && target != null && R.LSIsReady() && player.LSDistance(target) <= R.Range)
            {
                R.CastOnUnit(target);
                return;
            }

                var herolistheal = ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    h =>
                                        (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead &&
                                        h.HealthPercent <= 55 && player.LSDistance(h) <= R.Range).OrderByDescending(i => i == player).ThenBy(i => i);

                if (W.LSIsReady())
                {
                    if (herolistheal.Contains(player) && !player.LSIsRecalling() && !player.LSInFountain())
                    {
                        W.CastOnUnit(player);
                        return;
                    }
                    else if (herolistheal.Any())
                    {
                        var hero = herolistheal.FirstOrDefault();

                        if (player.LSDistance(hero) <= R.Range && !player.LSIsRecalling() && !hero.LSIsRecalling() && !hero.LSInFountain())
                        {
                            W.CastOnUnit(hero);
                            return;
                        }
                    }
                }
                
                var herolist = ObjectManager.Get<AIHeroClient>()
                    .Where(
                        h =>
                            (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead  &&
                            h.HealthPercent <=35 &&
                            player.LSDistance(h) <= R.Range && player.LSCountEnemiesInRange(500) > 0).OrderByDescending(i => i == player).ThenBy(i => i);

                if (R.LSIsReady())
                {
                    if (herolist.Contains(player))
                    {
                        R.CastOnUnit(player);
                        return;
                    }

                    else if (herolist.Any())
                    {
                        var hero = herolist.FirstOrDefault();

                        if (player.LSDistance(hero) <= R.Range)
                        {
                            R.CastOnUnit(hero);
                            return;
                        }
                    }
                }
        }

        public override void useQ(Obj_AI_Base target)
        {
            Q.CastOnUnit(target);
        }

        public override void useW(Obj_AI_Base target)
        {
        }

        public override void useE(Obj_AI_Base target)
        {
            if (E.LSIsReady())
                E.Cast();
        }


        public override void useR(Obj_AI_Base target)
        {
            if (!R.LSIsReady() || target == null)
                return;
        }

        public override void useSpells()
        {
            if(E.LSIsReady() && player.LSCountEnemiesInRange(650)>0)
                E.Cast();
            var tar = ARAMTargetSelector.getBestTarget(Q.Range);
            if (tar != null) useQ(tar);


        }

        public override void setUpSpells()
        {
            //Create the spells
            Q = new Spell(SpellSlot.Q, 650, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 525+250);
            R = new Spell(SpellSlot.R, 900);
        }

        public override void farm()
        {
            if (E.LSIsReady() && MinionManager.GetMinions(700).Count>2)
                E.Cast();
        }
    }
}
