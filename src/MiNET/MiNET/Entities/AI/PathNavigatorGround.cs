using MiNET.Utils;
using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathNavigatorGround : PathNavigator
    {

        public bool ShouldAvoidSun;

        public PathNavigatorGround(LivingEntity entity, Level level) : base(entity, level)
        {
            
        }

        protected override PathFinder CreatePathFinder()
        {
            this.AINode = new NodeWalk();
            this.AINode.CanEnterDoors = true;
            return new PathFinder(this.AINode);
        }

        protected override bool CanNavigate()
        {
            return this.entity.IsOnGround() || (this.CanSwim() && this.entity.IsInLiquid()) || this.entity.IsRiding;
        }

        protected override PlayerLocation GetEntityPosition()
        {
            return this.entity.KnownPosition;
        }

        public override PathEntity GetPathTo(BlockCoordinates bc)
        {
            if (this.level.GetBlock(bc) is Blocks.Air)
            {
                BlockCoordinates blockCoords;

                for (blockCoords = bc + BlockCoordinates.Down; this.level.GetBlock(blockCoords) is Blocks.Air; blockCoords = blockCoords + BlockCoordinates.Down)
                {

                }

                if (blockCoords.Y > 0)
                {
                    return base.GetPathTo(blockCoords + BlockCoordinates.Up);
                }

                while (blockCoords.Y < this.entity.Height && this.level.GetBlock(blockCoords) is Blocks.Air)
                {
                    blockCoords = blockCoords + BlockCoordinates.Up;
                }

                bc = blockCoords;
            }

            if (!this.level.GetBlock(bc).IsSolid)
            {
                return base.GetPathTo(bc);
            } else
            {
                BlockCoordinates bc2;

                for (bc2 = bc + BlockCoordinates.Up; bc2.Y < 128 && this.level.GetBlock(bc2).IsSolid; bc2 = bc + BlockCoordinates.Up)
                {

                }

                return base.GetPathTo(bc2);
            }
        }

        public override PathEntity GetPathToEntity(Entity entity)
        {
            BlockCoordinates bc = new BlockCoordinates(entity.KnownPosition);
            return this.GetPathTo(bc);
        }

        private int GetSafeYValue()
        {
            if (this.entity.IsInWater() && this.CanSwim())
            {
                int y = (int)this.entity.GetBoundingBox().Min.Y;
                var bc = new BlockCoordinates((int)Math.Floor(this.entity.KnownPosition.X), y, (int)Math.Floor(this.entity.KnownPosition.Z));
                var block = this.level.GetBlock(bc);

                int i = 0;

                while (block is Blocks.FlowingWater || block is Blocks.StationaryWater)
                {
                    ++y;
                    block = this.level.GetBlock(new BlockCoordinates((int)Math.Floor(this.entity.KnownPosition.X), y, (int)Math.Floor(this.entity.KnownPosition.Z)));
                    ++i;

                    if (i > 16)
                    {
                        return (int)this.entity.GetBoundingBox().Min.Y;
                    }
                }

                return y;
            } else
            {
                return (int)(this.entity.GetBoundingBox().Min.Y + 0.5);
            }
        }

        protected override void RemoveSunnyPath()
        {
            base.RemoveSunnyPath();

            for (int i = 0; i < this.currentPath.PathLength; ++i)
            {
                PathPoint pathPoint = this.currentPath.Points[i];
                PathPoint pathPoint2 = i + 1 < this.currentPath.PathLength ? this.currentPath.Points[i + 1] : null;
                var block = this.level.GetBlock(new BlockCoordinates(pathPoint.X, pathPoint.Y, pathPoint.Z));

                // TODO: Implement Cauldron
                /*if (block is Blocks.Cauldron)
                {
                    // TODO: Check if this actually needs to be Cloned
                    this.currentPath.Points[i] = pathPoint;

                    if (pathPoint2 != null && pathPoint.Y >= pathPoint2.Y)
                    {
                        this.currentPath.Points[i + 1] = pathPoint2;
                    }
                }*/
            }

            if (this.ShouldAvoidSun)
            {
                // TODO(Important): Do a check for block light, requires level changes from 'level-fixes' branch
                if (/*CanSeeSky()*/false)
                {
                    return;
                }

                for (int j = 0; j < this.currentPath.PathLength; ++j)
                {
                    PathPoint pathpoint3 = this.currentPath.Points[j];

                    if (/*CanSeeSky(new BlockCoordinates())*/false)
                    {
                        this.currentPath.PathLength = j - 1;
                        return;
                    }
                }
            }
        }

        protected override bool IsDirectPathBetweenPoints(Vector3 pos1, Vector3 pos2, int sizeX, int sizeY, int sizeZ)
        {
            int x = (int)Math.Floor(pos1.X);
            int z = (int)Math.Floor(pos1.Z);
            float distX = pos2.X - pos1.X;
            float distZ = pos2.Z - pos2.Z;
            // x^2 + z^2
            float dist = distX * distX + distZ * distZ;

            if (dist < 1.0E-8D)
            {
                return false;
            } else
            {
                float dist2 = 1 / (float)Math.Sqrt(dist);
                distX = distX * dist2;
                distZ = distZ * dist2;
                sizeX = sizeX + 2;
                sizeZ = sizeZ + 2;

                if (!this.IsSafeToStandAt(x, (int)pos1.Y, z, sizeX, sizeY, sizeZ, pos1, distX, distZ))
                {
                    return false;
                } else
                {
                    sizeX = sizeX - 2;
                    sizeZ = sizeZ - 2;
                    float f_distX = 1f / Math.Abs(distX);
                    float f_distY = 1f / Math.Abs(distZ);
                    float fX = x - pos1.X;
                    float fZ = z - pos1.Z;

                    if (distX >= 0)
                    {
                        ++fX;
                    }

                    if (distZ >= 0.0D)
                    {
                        ++fZ;
                    }

                    fX = fX / distX;
                    fZ = fZ / distZ;
                    int k = distX < 0 ? -1 : 1;
                    int l = distZ < 0 ? -1 : 1;
                    int i1 = (int)Math.Floor(pos2.X);
                    int j1 = (int)Math.Floor(pos2.Z);
                    int k1 = i1 - x;
                    int l1 = j1 - z;

                    while (k1 * k > 0 || l1 * l > 0)
                    {
                        if (fX < fZ)
                        {
                            fX += f_distX;
                            x += k;
                            k1 = i1 - x;
                        }
                        else
                        {
                            fZ += f_distY;
                            z += l;
                            l1 = j1 - z;
                        }

                        if (!this.IsSafeToStandAt(x, (int)pos1.Y, z, sizeX, sizeY, sizeZ, pos1, distX, distZ))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        private bool IsSafeToStandAt(int x, int y, int z, int sizeX, int sizeY, int sizeZ, Vector3 pos, float distX, float distZ)
        {
            int minX = x - sizeX / 2;
            int minZ = z - sizeZ / 2;

            if (this.IsPositionClear(minX, y, minZ, sizeX, sizeY, sizeZ, pos, distX, distZ))
            {
                return false;
            } else
            {
                for (int X = minX; X < minX + sizeX; ++X)
                {
                    for (int Z = minZ; minZ < minZ + sizeZ; ++Z)
                    {
                        float dist = X + 0.5f - pos.X;
                        float dist2 = Z + 0.5f - pos.Z;

                        if ((dist * distX + dist2 * distZ) >= 0)
                        {
                            NodeType nodeType = this.AINode.GetNodeType(this.level, X, y - 1, Z, this.entity, sizeX, sizeY, sizeZ, true, true);

                            if (nodeType == NodeType.Water)
                            {
                                return false;
                            }

                            if (nodeType == NodeType.Lava)
                            {
                                return false;
                            }

                            if (nodeType == NodeType.Open)
                            {
                                return false;
                            }

                            nodeType = this.AINode.GetNodeType(this.level, X, y, Z, this.entity, sizeX, sizeY, sizeZ, true, true);
                            float priority = this.entity.GetPathPriority(nodeType);

                            if (priority < 0 || priority >= 8)
                            {
                                return false;
                            }

                            if (nodeType == NodeType.DamageFire || nodeType == NodeType.DamageFire || nodeType == NodeType.DamageOther)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }

        private bool IsPositionClear(int x, int y, int z, int sizeX, int sizeY, int sizeZ, Vector3 pos, float distX, float distZ)
        {
            foreach (var bc in BlockCoordinates.GetBlocksBetween(new BlockCoordinates(x, y, z), new BlockCoordinates(x + sizeX - 1, y + sizeY - 1, z + sizeZ - 1)))
            {
                float distx = bc.X + 0.5f - pos.X;
                float distz = bc.Z + 0.5f - pos.Z;

                if (distx * distX + distz * distZ >= 0)
                {
                    var block = this.level.GetBlock(bc);

                    if (Blocks.Block.IsPassable(block))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanSwim()
        {
            return this.AINode.CanSwim;
        }
    }
}
