using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalBreakDoor : GoalDoorInteract
    {
        private int breakingTime;
        private int previousBreakingTime = -1;

        public GoalBreakDoor(LivingEntity entity) : base(entity) { }

        public override bool ShouldExecute()
        {
            if (!base.ShouldExecute())
            {
                return false;
            } else if (/* is mob greifing enabled */false) // TODO: Implement mob greifing toggle
            {
                return false;
            } else
            {
                var block = this.doorBlock;
                return !block.IsOpen();
            }
        }

        public override bool ContinueExecuting()
        {
            double dist = this.entity.KnownPosition.DistanceToSquared(new Utils.PlayerLocation(this.blockPos));
            bool canContinue = false;

            if (this.breakingTime <= 240)
            {
                var doorblock = this.doorBlock;

                if (!doorBlock.IsOpen() && dist < 4)
                {
                    canContinue = true;
                    return canContinue;
                }
            }

            return canContinue;
        }

        public override void ResetGoal()
        {
            base.ResetGoal();
            // see todo below
            //@this.theEntity.worldObj.sendBlockBreakProgress(this.theEntity.getEntityId(), this.doorPosition, -1);
        }

        public override void UpdateGoal()
        {
            base.UpdateGoal();

            if (this.entity.Level.Random.Next(20) == 0)
            {
                var attackDoorSound = new Sounds.ZombieAttackDoorSound(this.blockPos, 0);
                attackDoorSound.Spawn(this.entity.Level);
            }

            this.breakingTime++;

            int i = (int)((float)this.breakingTime / 240.0F * 10.0F);

            if (i != this.previousBreakingTime)
            {
                //TODO(Future): Once MCPE supports broadcasting block break progresses, do this.
                //@this.theEntity.worldObj.sendBlockBreakProgress(this.theEntity.getEntityId(), this.blockPos, i);
                this.previousBreakingTime = i;
            }
            //http://wiki.vg/Protocol

            if (this.breakingTime == 240 && this.entity.Level.Difficulty == Worlds.Difficulty.Hard)
            {
                this.entity.Level.SetAir(this.blockPos.X, this.blockPos.Y, this.blockPos.Z);
                var breakDoorSound = new Sounds.ZombieBreakDoorSound(this.blockPos, 0);
                breakDoorSound.Spawn(this.entity.Level);
                var blockDestroyParticle = new Sounds.Sound((short)LevelEventType.ParticleDestroy, this.blockPos);
                blockDestroyParticle.Spawn(this.entity.Level);
            }
        }
    }
}
