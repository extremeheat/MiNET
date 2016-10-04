using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathNavigatorSwimmer : PathNavigator
    {
        public PathNavigatorSwimmer(LivingEntity entity, Worlds.Level level): base(entity, level) { }

        protected override PathFinder CreatePathFinder()
        {
            return new PathFinder(new NodeSwim());
        }

        protected override bool CanNavigate()
        {
            return this.entity.IsInLiquid();
        }

        protected override PlayerLocation GetEntityPosition()
        {
            return new PlayerLocation(this.entity.KnownPosition.X, this.entity.KnownPosition.Y + this.entity.Height * 0.5f, this.entity.KnownPosition.Z);
        }

        protected override void FollowPath()
        {
            var epos = this.GetEntityPosition();
            double widthSq = this.entity.Width * this.entity.Width;

            if (epos.DistanceTo(new PlayerLocation(this.currentPath.GetVectorFromPoint(this.entity, this.currentPath.CurrentPath))) < widthSq)
            {
                this.currentPath.IncrementPathIndex();
            }

            for (int i = Math.Min(this.currentPath.CurrentPath + 6, this.currentPath.PathLength - 1); i > this.currentPath.CurrentPath; --i)
            {
                PlayerLocation point = new PlayerLocation(this.currentPath.GetVectorFromPoint(this.entity, i));

                if (point.DistanceTo(epos) <= 36 && this.IsDirectPathBetweenPoints(epos.ToVector3(), point.ToVector3(), 0, 0, 0)) {
                    this.currentPath.CurrentPath = i;
                    break;
                }
            }

            this.CheckIfStuck(epos);
        }

        public bool IsDirectPathBetweenPoints(Vector3 pos1, Vector3 pos2, int sizeX, int sizeY, int sizeZ)
        {
            var bb = new BoundingBox(pos1, new Vector3(pos2.X, pos2.Y * 0.5f, pos2.Z));
            this.entity.Level.GetCollidingBlocks(bb);
        }
    }
}
