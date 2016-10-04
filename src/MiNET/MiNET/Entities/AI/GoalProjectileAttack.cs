using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalProjectileAttack : Goal
    {
        private readonly LivingEntity entity;

        private LivingEntity host;
        private Mob attackTarget;

        private int rangedAttackTime;
        private float entityMoveSpeed;

        private int seenTime;

        private int maxRangedAttackTime;
        private float attackRadius;
        private float maxAttackDistance;

        public GoalProjectileAttack(LivingEntity attacker, float entityMoveSpeed, int rangedAttackTime, int maxRangedAttackTime, float maxAttackDistance)
        {
            if (!(attacker is IRangedAttacker))
            {
                throw new ArgumentException("Attacker mob must implement IRangedAttacker");
            }
            this.entity = attacker;
            this.entityMoveSpeed = entityMoveSpeed;
            this.rangedAttackTime = rangedAttackTime;
            this.maxRangedAttackTime = maxRangedAttackTime;
            this.attackRadius = maxAttackDistance;
            this.maxAttackDistance = maxAttackDistance * maxAttackDistance;

            SetMutexBits(3);
        }

        //public GoalArrowAttack(dynamic unknown1, double entityMoveSpeed, int attackTime, float attackDistance) : this(unknown1, entityMoveSpeed, attackTime, attackTime, attackDistance) {}

        public override bool ShouldExecute()
        {
            Mob attackTarget = this.entity.GetAttackTarget();
            if (attackTarget == null)
            {
                return false;
            } else
            {
                this.attackTarget = attackTarget;
                return true;
            }
        }

        public override bool ContinueExecuting()
        {
            return this.ShouldExecute() || this.entity.GetNavigator().HasPath();
        }

        public void ResetTask()
        {
            this.attackTarget = null;
            this.seenTime = 0;
            this.rangedAttackTime = -1;
        }

        public void UpdateTask()
        {
            double distanceToAttacker = Math.Pow(this.entity.KnownPosition.DistanceTo(new Utils.PlayerLocation(this.attackTarget.KnownPosition.X, (float)this.attackTarget.GetBoundingBox().Min.Y, this.attackTarget.KnownPosition.Z)), 2);
            bool canSee = entity.CanSeeEntity(this.attackTarget);
            /*var entities = entity.Level.GetVisibleEntities(entity.KnownPosition, 20, 10);

            foreach (var entity in entities)
            {
                if (entity == this.attackTarget)
                {
                    canSee = true;
                }
            }*/

            if (canSee)
            {
                ++this.seenTime;
            }
            else
            {
                this.seenTime = 0;
            }

            if (distanceToAttacker <= this.maxAttackDistance && this.seenTime >= 20)
            {
                this.entity.GetNavigator().ClearPathEntity();
            }
            else
            {
                this.entity.GetNavigator().TryMoveToEntity(this.attackTarget, this.entityMoveSpeed);
            }

            this.entity.LookManager.SetLookingPosition(this.attackTarget, 30, 30);

            if (--this.rangedAttackTime == 0)
            {
                if (distanceToAttacker > (double)this.maxAttackDistance || !canSee)
                {
                    return;
                }

                float f = (float)Math.Sqrt(distanceToAttacker) / this.attackRadius;

                float clamped = f < 0.1f ? 0.1f : (f > 1.0f ? 1.0f : f);
                ((IRangedAttacker)this.entity).AttackEntityWithRangedAttack(this.attackTarget, clamped);
                this.rangedAttackTime = Math.Floor(f * (float)(this.maxRangedAttackTime - this.attackIntervalMin) + (float)this.attackIntervalMin);
            }
            else if (this.rangedAttackTime < 0)
            {
                float f2 = (float)Math.Sqrt(distanceToAttacker) / this.attackRadius;
                this.rangedAttackTime = Math.Floor(f2 * (float)(this.maxRangedAttackTime - this.attackIntervalMin) + (float)this.attackIntervalMin);
            }
        }
    }
}
