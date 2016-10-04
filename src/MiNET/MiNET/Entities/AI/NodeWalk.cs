using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class NodeWalk : Node
    {
        private float avoidsWater;

        public override void Initialize(Worlds.Level level, LivingEntity mob)
        {
            base.Initialize(level, mob);
            this.avoidsWater = mob.GetPathPriority(NodeType.Water);
        }

        public override void PostProcess()
        {
            this.entity.SetPathPriority(NodeType.Water, this.avoidsWater);
            base.PostProcess();
        }

        public override PathPoint GetStart()
        {
            int i;

            if (this.CanSwim && this.entity.IsInLiquid())
            {
                i = (int)this.entity.GetBoundingBox().Min.Y;
                var blockPos = new Utils.BlockCoordinates((int)Math.Floor(this.entity.KnownPosition.X), i, (int)Math.Floor(this.entity.KnownPosition.Z));

                // o.O
                for (var block = this.entity.Level.GetBlock(blockPos); block is Blocks.Flowing || block is Blocks.Stationary; block = this.entity.Level.GetBlock(blockPos))
                {
                    ++i;
                    blockPos.Y = i;
                }
            } else if (this.entity.IsOnGround())
            {
                i = (int)Math.Floor(this.entity.GetBoundingBox().Min.Y + 0.5d);
            } else
            {
                var blockpos = new BlockCoordinates(this.entity.KnownPosition);
                // TODO: Add an isPassable check allowing AIs to jump onto blocks such as layered snow
                for (; this.entity.Level.GetBlock(blockpos) is Blocks.Air; blockpos = blockpos + BlockCoordinates.Down)
                {

                }
                i = (blockpos + BlockCoordinates.Up).Y;
            }

            BlockCoordinates blockPos2 = new BlockCoordinates(this.entity.KnownPosition);
            NodeType nodeType = this.GetNodeType(this.entity, blockPos2.X, i, blockPos2.Z);

            if (this.entity.GetPathPriority(nodeType) < 0)
            {
                List<BlockCoordinates> list = new List<BlockCoordinates>();
                list.Add(new BlockCoordinates((int)this.entity.GetBoundingBox().Min.X, i, (int)this.entity.GetBoundingBox().Min.Z));
                list.Add(new BlockCoordinates((int)this.entity.GetBoundingBox().Min.X, i, (int)this.entity.GetBoundingBox().Max.Z));
                list.Add(new BlockCoordinates((int)this.entity.GetBoundingBox().Max.X, i, (int)this.entity.GetBoundingBox().Min.Z));
                list.Add(new BlockCoordinates((int)this.entity.GetBoundingBox().Max.X, i, (int)this.entity.GetBoundingBox().Max.Z));

                foreach (var blockPos3 in list)
                {
                    NodeType nodeType2 = this.GetNodeType(this.entity, blockPos3);

                    if (this.entity.GetPathPriority(nodeType2) > 0)
                    {
                        return this.CreatePoint(blockPos3.X, blockPos3.Y, blockPos3.Z);
                    }
                }
            }
            return this.CreatePoint(blockPos2.X, i, blockPos2.Z);
        }

        public override PathPoint GetPathPointAt(int x, int y, int z)
        {
            return this.CreatePoint((int)Math.Floor(x - this.entity.Width / 2), y, (int)Math.Floor(z - this.entity.Width / 2));
        }

        public override int FindPathOptions(PathPoint[] pathOptions, PathPoint currentPoint, PathPoint targetPoint, float maxDistance)
        {
            int i = 0;
            int maxStepHeight = 0;
            NodeType nodeType = this.GetNodeType(this.entity, currentPoint.X, currentPoint.Y + 1, currentPoint.Z);

            if (this.entity. GetPathPriority(nodeType) >= 0)
            {
                maxStepHeight = (int)Math.Floor(Math.Max(1, this.entity.StepHeight));
            }

            BlockCoordinates blockCoords = new BlockCoordinates(currentPoint.X, currentPoint.Y, currentPoint.Z) + BlockCoordinates.Down;

            float dist = currentPoint.Y - (1 - this.level.GetBlock(blockCoords).GetBoundingBox().Max.Y);
            PathPoint pathpoint = this.GetSafePoint(currentPoint.X, currentPoint.Y, currentPoint.Z + 1, maxStepHeight, dist, BlockFace.South);
            PathPoint pathpoint2 = this.GetSafePoint(currentPoint.X - 1, currentPoint.Y, currentPoint.Z, maxStepHeight, dist, BlockFace.West);
            PathPoint pathpoint3 = this.GetSafePoint(currentPoint.X + 1, currentPoint.Y, currentPoint.Z, maxStepHeight, dist, BlockFace.East);
            PathPoint pathpoint4 = this.GetSafePoint(currentPoint.X, currentPoint.Y, currentPoint.Z - 1, maxStepHeight, dist, BlockFace.North);

            if (pathpoint != null && pathpoint.Visited && pathpoint.DistanceTo(targetPoint) < maxDistance)
            {
                pathOptions[i++] = pathpoint;
            }

            if (pathpoint2 != null && pathpoint2.Visited && pathpoint2.DistanceTo(targetPoint) < maxDistance)
            {
                pathOptions[i++] = pathpoint2;
            }

            if (pathpoint3 != null && pathpoint3.Visited && pathpoint3.DistanceTo(targetPoint) < maxDistance)
            {
                pathOptions[i++] = pathpoint3;
            }

            if (pathpoint4 != null && pathpoint4.Visited && pathpoint4.DistanceTo(targetPoint) < maxDistance)
            {
                pathOptions[i++] = pathpoint4;
            }

            bool flag  = pathpoint  == null || pathpoint.CurrentNodeType  == NodeType.Open || pathpoint.CostMalus  != 0.0F;
            bool flag2 = pathpoint2 == null || pathpoint2.CurrentNodeType == NodeType.Open || pathpoint2.CostMalus != 0.0F;
            bool flag3 = pathpoint3 == null || pathpoint3.CurrentNodeType == NodeType.Open || pathpoint3.CostMalus != 0.0F;
            bool flag4 = pathpoint4 == null || pathpoint4.CurrentNodeType == NodeType.Open || pathpoint4.CostMalus != 0.0F;

            if (flag4 && flag2)
            {
                PathPoint pathpoint5 = this.GetSafePoint(currentPoint.X - 1, currentPoint.Y, currentPoint.Z - 1, maxStepHeight, dist, BlockFace.North);

                if (pathpoint5 != null && !pathpoint5.Visited && pathpoint5.DistanceTo(targetPoint) < maxDistance)
                {
                    pathOptions[i++] = pathpoint5;
                }
            }

            if (flag4 && flag3)
            {
                PathPoint pathpoint6 = this.GetSafePoint(currentPoint.X + 1, currentPoint.Y, currentPoint.Z - 1, maxStepHeight, dist, BlockFace.North);

                if (pathpoint6 != null && !pathpoint6.Visited && pathpoint6.DistanceTo(targetPoint) < maxDistance)
                {
                    pathOptions[i++] = pathpoint6;
                }
            }

            if (flag && flag2)
            {
                PathPoint pathpoint7 = this.GetSafePoint(currentPoint.X - 1, currentPoint.Y, currentPoint.Z + 1, maxStepHeight, dist, BlockFace.South);

                if (pathpoint7 != null && !pathpoint7.Visited && pathpoint7.DistanceTo(targetPoint) < maxDistance)
                {
                    pathOptions[i++] = pathpoint7;
                }
            }

            if (flag && flag3)
            {
                PathPoint pathpoint8 = this.GetSafePoint(currentPoint.X + 1, currentPoint.Y, currentPoint.Z + 1, maxStepHeight, dist, BlockFace.South);

                if (pathpoint8 != null && !pathpoint8.Visited && pathpoint8.DistanceTo(targetPoint) < maxDistance)
                {
                    pathOptions[i++] = pathpoint8;
                }
            }

            return i;

        }

        private PathPoint GetSafePoint(int x, int y, int z, int unknown, double distance, BlockFace facing)
        {
            PathPoint pathpoint = null;
            BlockCoordinates blockPos = new BlockCoordinates(x, y, z);
            BlockCoordinates blockPos2 = blockPos + BlockCoordinates.Down;

            float dist = y - (1 - this.level.GetBlock(blockPos2).GetBoundingBox().Max.Y);

            if (dist - distance > 1.125d)
            {
                return null;
            } else
            {
                NodeType nodeType = this.GetNodeType(this.entity, x, y, z);

                float priority = this.entity.GetPathPriority(nodeType);
                float dist2 = (float)this.entity.Width / 2f;

                if (priority >= 0)
                {
                    pathpoint = this.CreatePoint(x, y, z);
                    pathpoint.CurrentNodeType = nodeType;
                    pathpoint.CostMalus = Math.Max(pathpoint.CostMalus, priority);
                }

                if (nodeType == NodeType.Walkable)
                {
                    return pathpoint;
                } else
                {
                    if (pathpoint == null && unknown > 0 && nodeType != NodeType.Fence && nodeType != NodeType.Trapdoor)
                    {
                        pathpoint = this.GetSafePoint(x, y, z, unknown - 1, distance, facing);

                        if (pathpoint != null && (pathpoint.CurrentNodeType == NodeType.Open || pathpoint.CurrentNodeType == NodeType.Walkable) && this.entity.Width < 1)
                        {
                            float dist3 = (x - BlockFaceUtil.GetFrontOffsetX(facing)) + 0.5f;
                            float dist4 = (z - BlockFaceUtil.GetFrontOffsetZ(facing)) + 0.5f;
                            BoundingBox bb = new BoundingBox(
                                new Vector3(dist3 - dist, y + 0.001f, dist4 - dist),
                                new Vector3(dist3 + dist, y + (float)this.entity.Height, dist4 + dist)
                            );
                            BoundingBox bb2 = this.level.GetBlock(blockPos).GetBoundingBox();
                            BoundingBox bb3 = bb.OffsetBy(new Vector3(0f, bb2.Max.Y - 0.002f, 0));

                            if (this.entity.Level.CollidesWithBlock(bb3))
                            {
                                pathpoint = null;
                            }
                        }
                    }

                    if (nodeType == NodeType.Open)
                    {
                        BoundingBox bb4 = new BoundingBox(new Vector3(x - dist2 + 0.5f, y + 0.001f, z - dist2 + 0.5f), new Vector3(x + dist2 + 0.5f, y + (float)this.entity.Height, z + dist2 + 0.5f));
                        if (this.entity.Level.CollidesWithBlock(bb4))
                        {
                            return null;
                        }

                        if (this.entity.Width >= 1)
                        {
                            NodeType nodeType2 = this.GetNodeType(this.entity, x, y - 1, z);

                            if (nodeType2 == NodeType.Blocked)
                            {
                                pathpoint = this.CreatePoint(x, y, z);
                                pathpoint.CurrentNodeType = NodeType.Walkable;
                                pathpoint.CostMalus = Math.Max(pathpoint.CostMalus, priority);
                                return pathpoint;
                            }
                        }

                        int i = 0;

                        while (y > 0 && nodeType == NodeType.Open)
                        {
                            --y;

                            if (i++ >= this.entity.GetMaximumFallHeight())
                                return null;

                            nodeType = this.GetNodeType(this.entity, x, y, z);

                            priority = this.entity.GetPathPriority(nodeType);

                            if (nodeType != NodeType.Open && priority >= 0)
                            {
                                pathpoint = this.CreatePoint(x, y, z);
                                pathpoint.CurrentNodeType = nodeType;
                                pathpoint.CostMalus = Math.Max(pathpoint.CostMalus, priority);
                                break;
                            }

                            if (priority < 0)
                            {
                                return null;
                            }
                        }
                    }

                    return pathpoint;
                }
            }
        }

        public override NodeType GetNodeType(Worlds.Level level, int x, int y, int z, LivingEntity entityIn, int sizeX, int sizeY, int sizeZ, bool canBreakDoors, bool canEnterDoors)
        {
            NodeType nodeType = NodeType.Blocked;
            List<NodeType> nodeTypeList = new List<NodeType>();
            double dist = entityIn.Width / 2;
            BlockCoordinates blockPos = new BlockCoordinates(entityIn.KnownPosition);

            var width = sizeX; //entity.Width + 1;
            var height = sizeY; //entity.Height + 1;

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    for (int k = 0; k < width; ++k)
                    {
                        int x2 = i + x;
                        int y2 = j + y;
                        int z2 = k + z;

                        NodeType nodeType2 = this.GetNodeType(level, x2, y2, z2);

                        if (nodeType2 == NodeType.DoorWoodClosed && canBreakDoors && canEnterDoors)
                        {
                            nodeType2 = NodeType.Walkable;
                        }

                        if (nodeType2 == NodeType.DoorOpen && !canEnterDoors)
                        {
                            nodeType2 = NodeType.Blocked;
                        }

                        if (nodeType2 == NodeType.Rail && !(level.GetBlock(blockPos) is Blocks.Rail) && !(level.GetBlock(blockPos + BlockCoordinates.Down) is Blocks.Rail))
                        {
                            nodeType2 = NodeType.Fence;
                        }

                        if (i == 0 && j == 0 && k == 0)
                        {
                            nodeType = nodeType2;
                        }

                        nodeTypeList.Add(nodeType);
                    }
                }
            }

            if (nodeTypeList.Contains(NodeType.Fence))
            {
                return NodeType.Fence;
            } else
            {
                NodeType nodeType3 = NodeType.Blocked;

                foreach (var nodeType4 in nodeTypeList)
                {
                    if (entityIn.GetPathPriority(nodeType4) < 0)
                    {
                        return nodeType4;
                    }

                    if (entityIn.GetPathPriority(nodeType4) >= entityIn.GetPathPriority(nodeType3))
                    {
                        nodeType3 = nodeType4;
                    }
                }

                if (nodeType == NodeType.Open && entityIn.GetPathPriority(nodeType3) == 0)
                {
                    return NodeType.Open;
                } else
                {
                    return nodeType3;
                }
            }
        }

        private NodeType GetNodeType(LivingEntity entity, BlockCoordinates bc)
        {
            return this.GetNodeType(entity, bc.X, bc.Y, bc.Z);
        }

        private NodeType GetNodeType(LivingEntity entity, int x, int y, int z)
        {
            return this.GetNodeType(this.level, x, y, z, entity,
                (int)Math.Floor(entity.Width + 1), (int)Math.Floor(entity.Height + 1), (int)Math.Floor(entity.Width + 1), this.CanBreakDoors, this.CanEnterDoors);
        }

        public override NodeType GetNodeType(Worlds.Level level, int x, int y, int z)
        {
            NodeType nodeType = this.GetNodeTypeAt(level, x, y, z);
            
            if (nodeType == NodeType.Open && z >= 1)
            {
                var block = level.GetBlock(x, y - 1, z);
                NodeType nodeType2 = this.GetNodeTypeAt(level, x, y - 1, z);

                nodeType = nodeType2 != NodeType.Walkable && nodeType2 != NodeType.Open && nodeType2 != NodeType.Water && nodeType2 != NodeType.Lava
                    ? NodeType.Walkable : NodeType.Open;


                // TODO(Future): Minecraft 1.10 "magma" blocks are not implemented in MCPE as of 0.16
                if (nodeType2 == NodeType.DamageFire/* || block is Blocks.Magma*/)
                {
                    nodeType = NodeType.DamageFire;
                }

                if (nodeType2 == NodeType.DamageCactus)
                {
                    nodeType = NodeType.DamageCactus;
                }
            }

            if (nodeType == NodeType.Walkable)
            {
                for (int i = -1; i <= 1; ++i)
                {
                    for (int j = -1; i <= 1; ++i)
                    {
                        if (i != 0 || j != 0)
                        {
                            var block = level.GetBlock(i + x, y, i + z);
                            if (block is Blocks.Cactus)
                            {
                                nodeType = NodeType.DangerCactus;
                            }
                            else if (block is Blocks.Fire)
                            {
                                nodeType = NodeType.DangerFire;
                            }
                        }
                    }
                }
            }

            return nodeType;
        }

        private NodeType GetNodeTypeAt(Worlds.Level level, int x, int y, int z)
        {
            BlockCoordinates bc = new BlockCoordinates(x, y, z);
            var block = level.GetBlock(bc);

            // this is fucking hell

            /*if (block is Blocks.Air)
            {
                return NodeType.Open;
            } else if (!(block is Blocks.Trapdoor) && !(block is Blocks.IronTrapdoor) && !(block is Blocks.Waterlily))
            {
                if (block is Blocks.Fire)
                {
                    return NodeType.DamageFire;
                } else
                {
                    if (block is Blocks.Cactus)
                    {
                        return NodeType.DamageCactus;
                    } else
                    {
                        if 
                    }
                }
            }*/

            /*return block is Blocks.Air ? NodeType.Open : (!(block is Blocks.Trapdoor) && !(block is Blocks.IronTrapdoor) && !(block is Blocks.Waterlily) 
                ? (block is Blocks.Fire ? NodeType.DamageFire : (block is Blocks.Cactus
                    ? NodeType.DamageCactus : (block is Blocks.Door && block is Blocks.Wood
                    
                    )
                )
                )
            )*/
            // HACK: make a proper way to check if a door is open...
            // this was a nightmare to write

            return block is Blocks.Air ? NodeType.Open :
            (!(block is Blocks.Trapdoor) && !(block is Blocks.IronTrapdoor) && !(block is Blocks.Waterlily)
                ? (block is Blocks.Fire
                    ? NodeType.DamageFire : (block is Blocks.Cactus
                        ? NodeType.DamageCactus : (block is Blocks.WoodenDoor && (block.Metadata & 0x4) == 0
                            ? NodeType.DoorWoodClosed : (block is Blocks.IronDoor && (block.Metadata & 0x4) == 0
                                ? NodeType.DoorIronClosed : ((block is Blocks.WoodenDoor || block is Blocks.IronDoor) && (block.Metadata & 0x4) == 1
                                    ? NodeType.DoorOpen : (block is Blocks.Rail
                                    ? NodeType.Rail : (!(block is Blocks.Fence) && !(block is Blocks.CobblestoneWall) && !(block is Blocks.FenceGate) || (block.Metadata & 0x4) == 1
                                    ? (block is Blocks.FlowingWater || block is Blocks.StationaryWater
                                    ? NodeType.Water : (block is Blocks.FlowingLava || block is Blocks.StationaryLava
                                    ? NodeType.Lava : (block.IsSolid // HACK: See if this causes any issues; should be an isPassable() check
                                    ? NodeType.Open : NodeType.Blocked))) : NodeType.Fence))))))) : NodeType.Trapdoor);

        }
    }
}
