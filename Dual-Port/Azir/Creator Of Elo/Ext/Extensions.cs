using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Azir_Creator_of_Elo
{
    static class Extensions
    {
        public static bool IsRunningOfYou(this AIHeroClient target)
        {
            var pos = Prediction.GetPrediction(target, 0.5f).UnitPosition;
            return Vector2.Distance(HeroManager.Player.ServerPosition.LSTo2D(), pos.LSTo2D()) > Vector2.Distance(HeroManager.Player.ServerPosition.LSTo2D(), target.ServerPosition.LSTo2D());
        }
    }
}
