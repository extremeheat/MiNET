using MiNET.Worlds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    abstract class Node
    {
        protected Level level;
        protected LivingEntity entity;

        protected ConcurrentDictionary<int, PathPoint> pointMap = new ConcurrentDictionary<int, PathPoint>();

        public bool CanEnterDoors;
        public bool CanBreakDoors;
        public bool CanSwim;

        public virtual void Initialize(Level level, LivingEntity entity)
        {
            this.level = level;
            this.entity = entity;
        }

        public virtual void PostProcess()
        {
            this.level = null;
            this.entity = null;
        }

        protected PathPoint CreatePoint(int x, int y, int z)
        {
            int i = PathPoint.GetHashCode(x, y, z);
            PathPoint pathpoint = this.pointMap[i];

            if (pathpoint == null)
            {
                pathpoint = new PathPoint(x, y, z);
                this.pointMap[i] = pathpoint;
            }

            return pathpoint;
        }

        public abstract PathPoint GetStart();

        public abstract PathPoint GetPathPointAt(int x, int y, int z);

        public abstract int FindPathOptions(PathPoint[] pathOptions, PathPoint currentPoint, PathPoint targetPoint, float maxDistance);

        public abstract NodeType GetNodeType(Level level, int x, int y, int z, LivingEntity entityIn, int sizeX, int sizeY, int sizeZ, bool canBreakDoors, bool canEnterDoors);

        public abstract NodeType GetNodeType(Level level, int x, int y, int z);
    }
}
