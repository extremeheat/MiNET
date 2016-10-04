using MiNET.Utils;

namespace MiNET.Entities.AI
{
    class GoalWander : Goal
    {
        private readonly LivingEntity entity;

        private float X;
        private float Y;
        private float Z;

        private readonly float speed;
        private int executionChance;
        private bool mustUpdate;

        public GoalWander(LivingEntity entity, float speed) : this(entity, speed, 120) { }

        public GoalWander(LivingEntity entity, float speed, int chance)
        {
            this.entity = entity;
            this.speed = speed;
            this.executionChance = chance;
            this.SetMutexBits(1);
        }

        public override bool ShouldExecute()
        {
            if (!this.mustUpdate)
            {
                if (this.entity.Age >= 100)
                {
                    return false;
                }

                if (this.entity.Level.Random.Next(this.executionChance) != 0)
                {
                    return false;
                }
            }

            BlockCoordinates? blockCoords = PathRandomPositionGenerator.FindRandomTarget(this.entity, 10, 7);

            if (blockCoords == null)
            {
                return false;
            } else
            {
                BlockCoordinates bc = (BlockCoordinates)blockCoords;
                this.X = bc.X;
                this.Y = bc.Y;
                this.Z = bc.Z;
                this.mustUpdate = false;
                return true;
            }
        }

        public override bool ContinueExecuting()
        {
            return this.entity.GetNavigator().HasPath();
        }

        public override void Execute()
        {
            this.entity.GetNavigator().TryToMoveTo(this.X, this.Y, this.Z, this.speed);
        }

        public void MakeUpdate()
        {
            this.mustUpdate = true;
        }

        public void SetExecutionChance(int chance)
        {
            this.executionChance = chance;
        }
    }
}
