using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalAvoidEntity : Goal
    {
        //private readonly canBeSeen;

        protected LivingEntity entity;
        private float farSpeed;
        private float nearSpeed;
        protected LivingEntityBase closestLivingEntity;
        private double avoidDistance;

        private PathEntity pathEntity;

        private PathNavigator pathNavigator;
        private LivingEntityBase entityToAvoid;

        public GoalAvoidEntity(LivingEntity entity, LivingEntityBase entityToAvoid, double distanceToAvoid, float farSpeed, float nearSpeed)
        {
            this.entity = entity;
            this.entityToAvoid = entityToAvoid;
            this.avoidDistance = distanceToAvoid;
            this.farSpeed = farSpeed;
            this.nearSpeed = nearSpeed;
            this.pathNavigator = entity.GetNavigator();
        }

        public override bool ShouldExecute()
        {
            /*List<Entity>*/Entity[] entitiesInArea = this.entity.Level.GetNearbyEntities(this.entity.KnownPosition, this.avoidDistance, typeof(LivingEntityBase));

            if (entitiesInArea.Count() == 0)
            {
                return false;
            } else
            {
                this.closestLivingEntity = (LivingEntityBase)entitiesInArea[0];
                var bc = PathRandomPositionGenerator.FindRandomTargetBlock(this.entity, 16, 7, new Utils.BlockCoordinates(this.closestLivingEntity.KnownPosition));

                if (!bc.HasValue)
                {
                    return false;
                } else if (this.closestLivingEntity.KnownPosition.DistanceToSquared(bc.Value.X, bc.Value.Y, bc.Value.Z) < this.closestLivingEntity.KnownPosition.DistanceToSquared(this.entity.KnownPosition))
                {
                    return false;
                } else
                {
                    this.pathEntity = this.pathNavigator.GetPathTo(bc.Value);
                    return this.pathEntity != null;
                }
            }
        }

        public override bool ContinueExecuting()
        {
            return this.pathNavigator.HasPath();
        }

        public override void Execute()
        {
            this.pathNavigator.SetPath(this.pathEntity, this.farSpeed);
        }

        public override void ResetGoal()
        {
            this.closestLivingEntity = null;
        }

        public override void UpdateGoal()
        {
            if (this.entity.KnownPosition.DistanceToSquared(this.closestLivingEntity.KnownPosition) < 49)
            {
                this.entity.GetNavigator().SetSpeed(this.nearSpeed);
            } else
            {
                this.entity.GetNavigator().SetSpeed(this.farSpeed);
            }
        }
    }
}
