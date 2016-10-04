using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class NodeSwim : Node
    {
        public override PathPoint GetStart()
        {
            return this.CreatePoint((int)Math.Floor(this.entity.GetBoundingBox().Min.X), 
                (int)Math.Floor(this.entity.GetBoundingBox().Min.Y + 0.5), (int)Math.Floor(this.entity.GetBoundingBox().Min.Z));
        }

        public override PathPoint GetPathPointAt(int x, int y, int z)
        {
            return this.CreatePoint((int)Math.Floor(x - (this.entity.Width / 2)), (int)Math.Floor(y + 0.5d), (int)Math.Floor(z - (this.entity.Width / 2)));
        }

        public override int FindPathOptions(PathPoint[] pathOptions, PathPoint currentPoint, PathPoint targetPoint, float maxDistance)
        {
            int i = 0;

            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
            {
                PathPoint pathPoint = this.GetWaterNode(
                    currentPoint.X + BlockFaceUtil.GetFrontOffsetX(face),
                    currentPoint.Y + BlockFaceUtil.GetFrontOffsetY(face),
                    currentPoint.Z + BlockFaceUtil.GetFrontOffsetZ(face));

                if (pathPoint != null && !pathPoint.Visited && pathPoint.DistanceTo(targetPoint) < maxDistance)
                {
                    pathOptions[i++] = pathPoint;
                }
            }

            return i;
        }

        public override NodeType GetNodeType(Level level, int x, int y, int z, LivingEntity entityIn, int sizeX, int sizeY, int sizeZ, bool canBreakDoors, bool canEnterDoors)
        {
            return NodeType.Water;
        }

        public override NodeType GetNodeType(Level level, int x, int y, int z)
        {
            return NodeType.Water;
        }

        private PathPoint GetWaterNode(int x, int y, int z)
        {
            NodeType nodeType = this.IsFree(x, y, z);
            return nodeType == NodeType.Water ? this.CreatePoint(x, y, z) : null;
        }

        private NodeType IsFree(int x, int y, int z)
        {
            for (int X = x; X < x + this.entity.Width + 1; ++X)
            {
                for (int Y = y; Y < y + this.entity.Height + 1; ++Y)
                {
                    for (int Z = z; Z < z + this.entity.Width + 1; ++Z)
                    {
                        var block = this.level.GetBlock(X, Y, Z);

                        if (block is Blocks.StationaryWater)
                        {
                            return NodeType.Blocked;
                        }
                    }
                }
            }

            return NodeType.Water;
        }

    }
}
