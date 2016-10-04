using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalDoorInteract : Goal
    {
        protected LivingEntity entity;
        protected Utils.BlockCoordinates blockPos = Utils.BlockCoordinates.One;

        protected Blocks.WoodenDoor doorBlock;

        bool hasStoppedDoorInteraction;
        float eposX;
        float eposZ;

        public GoalDoorInteract(LivingEntity entity)
        {
            this.entity = entity;

            if (!(entity.GetNavigator() is PathNavigatorGround))
            {
                throw new ArgumentException("Mob must have a ground navigator for GoalDoorInteract");
            }
        }

        public override bool ShouldExecute()
        {
            if (!this.entity.checkHorizontalCollisions)
            {
                return false;
            } else
            {
                PathNavigatorGround navigatorGround = (PathNavigatorGround)this.entity.GetNavigator();
                PathEntity path = navigatorGround.GetPath();

                if (path != null && !path.IsFinished() && navigatorGround.AINode.CanEnterDoors)
                {
                    for (int i = 0; i < Math.Min(path.CurrentPath + 2, path.PathLength); ++i)
                    {
                        PathPoint pathpoint = path.Points[i];
                        this.blockPos = new Utils.BlockCoordinates(pathpoint.X, pathpoint.Y + 1, pathpoint.Z);

                        if (this.entity.KnownPosition.DistanceToSquared(new PlayerLocation(this.blockPos)) <= 2.25)
                        {
                            var block = this.entity.Level.GetBlock(this.blockPos);
                            this.doorBlock = block is Blocks.WoodenDoor ? (Blocks.WoodenDoor)block : null;

                            if (this.doorBlock != null)
                            {
                                return true;
                            }
                        }
                    }

                    this.blockPos = new BlockCoordinates(this.entity.KnownPosition) + BlockCoordinates.Up;
                    var block2 = this.entity.Level.GetBlock(this.blockPos);
                    this.doorBlock = block2 is Blocks.WoodenDoor ? (Blocks.WoodenDoor)block2 : null;
                    return this.doorBlock != null;
                } else
                {
                    return false;
                }
            }
        }

        public override bool ContinueExecuting()
        {
            return !this.hasStoppedDoorInteraction;
        }

        public override void Execute()
        {
            this.hasStoppedDoorInteraction = false;
            this.eposX = ((float)this.blockPos.X + 0.5f) - this.entity.KnownPosition.X;
            this.eposZ = ((float)this.blockPos.Z + 0.5f) - this.entity.KnownPosition.Z;
        }

        public override void UpdateGoal()
        {
            float f = (float)((double)((float)this.blockPos.X + 0.5F) - this.entity.KnownPosition.X);
            float f1 = (float)((double)((float)this.blockPos.Z + 0.5F) - this.entity.KnownPosition.Z);
            float f2 = this.eposX * f + this.eposZ * f1;

            if (f2 < 0.0F)
            {
                this.hasStoppedDoorInteraction = true;
            }
        }
    }
}
