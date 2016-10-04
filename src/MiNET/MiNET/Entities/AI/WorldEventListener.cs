using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    public class WorldEventListener
    {

        List<PathNavigator> navigators = new List<PathNavigator>();

        public void OnBlockBreak(BlockCoordinates bc)
        {
            UpdatePathNavigators(bc);
        }

        public void OnBlockPlace(BlockCoordinates bc)
        {
            UpdatePathNavigators(bc);
        }

        public void OnBlockUpdate(BlockCoordinates bc)
        {
            UpdatePathNavigators(bc);
        }

        private void UpdatePathNavigators(BlockCoordinates bc)
        {
            foreach (var navigator in this.navigators)
            {
                if (navigator != null && !navigator.CanUpdatePathOnTimeout())
                {
                    PathEntity pathEntity = navigator.GetPath();

                    if (pathEntity != null && !pathEntity.IsFinished() && pathEntity.PathLength != 0)
                    {
                        PathPoint pathpoint = navigator.GetPath().GetFinalPathPoint();

                        double distanceSq = bc.DistanceToSquared(new BlockCoordinates((int)(pathpoint.X + navigator.entity.KnownPosition.X / 2),
                            (int)(pathpoint.Y + navigator.entity.KnownPosition.Y / 2), (int)(pathpoint.Z + navigator.entity.KnownPosition.Z / 2)));

                        int k = (pathEntity.PathLength - pathEntity.CurrentPath) * (pathEntity.PathLength - pathEntity.CurrentPath);

                        if (distanceSq < k)
                        {
                            navigator.UpdatePath();
                        }
                    }
                }
            }
        }
    }
}