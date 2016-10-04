using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalArrowAttack : Goal
    {
        public readonly Hostile.Skeleton entity;
        private readonly float moveSpeedAmplifier;
        public int AttackCooldown;
        private readonly float maxAttackDistance;
        private int attackTime = -1;
        private int seeTime;
        private bool strafingClockwise;
        private bool strafingBackwards;
        private int strafingTime = -1;

        public GoalArrowAttack(Hostile.Skeleton skeleton, float speedAmplifier, int delay, float maxDistance)
        {
            this.entity = skeleton;
            this.moveSpeedAmplifier = speedAmplifier;
            this.AttackCooldown = delay;
            this.maxAttackDistance = maxDistance * maxDistance;
            this.SetMutexBits(3);
        }

        public override bool ShouldExecute()
        {
            return this.entity.GetAttackTarget() == null ? false : this.IsHoldingBow();
        }

        protected bool IsHoldingBow()
        {
            return this.entity.Inventory != null && this.entity.Inventory.HeldItem is Items.ItemBow;
        }

        public override bool ContinueExecuting()
        {
            return (this.ShouldExecute() || this.entity.GetNavigator().HasPath()) && this.IsHoldingBow();
        }

        /**
         * Execute a one shot task or start executing a continuous task
         */
        public override void Execute()
        {
            //this.entity.setSwingingArms(true);
        }

        /**
         * Resets the task
         */
        public override void ResetGoal()
        {
            //this.entity.setSwingingArms(false);
            this.seeTime = 0;
            this.attackTime = -1;
            //this.entity.resetActiveHand();
        }

        public override void UpdateGoal()
        {
            Mob attackTarget = this.entity.GetAttackTarget();

            if (attackTarget != null)
            {
                double dist = this.entity.KnownPosition.DistanceToSquared(new Utils.PlayerLocation(attackTarget.KnownPosition.X, attackTarget.GetBoundingBox().Min.Y, attackTarget.KnownPosition.Z));
                bool canSee = this.entity.CanSeeEntity(attackTarget);
                bool seen = this.seeTime > 0;

                if (canSee != seen)
                {
                    this.seeTime = 0;
                }

                if (canSee)
                {
                    ++this.seeTime;
                }
                else
                {
                    --this.seeTime;
                }

                if (dist <= (double)this.maxAttackDistance && this.seeTime >= 20)
                {
                    this.entity.GetNavigator().ClearPathEntity();
                    ++this.strafingTime;
                }
                else
                {
                    this.entity.GetNavigator().TryMoveToEntity(attackTarget, this.moveSpeedAmplifier);
                    this.strafingTime = -1;
                }

                if (this.strafingTime >= 20)
                {
                    if (this.entity.Level.Random.NextDouble() < 0.3)
                    {
                        this.strafingClockwise = !this.strafingClockwise;
                    }

                    if (this.entity.Level.Random.NextDouble() < 0.3)
                    {
                        this.strafingBackwards = !this.strafingBackwards;
                    }

                    this.strafingTime = 0;
                }

                if (this.strafingTime > -1)
                {
                    if (dist > (double)(this.maxAttackDistance * 0.75F))
                    {
                        this.strafingBackwards = false;
                    }
                    else if (dist < (double)(this.maxAttackDistance * 0.25F))
                    {
                        this.strafingBackwards = true;
                    }

                    this.entity.MoveManager.Strafe(this.strafingBackwards ? -0.5F : 0.5F, this.strafingClockwise ? 0.5F : -0.5F);
                    this.entity.FaceEntity(attackTarget, 30.0F, 30.0F);
                }
                else
                {
                    this.entity.LookManager.SetLookingPosition(attackTarget, 30.0F, 30.0F);
                }

                if (this.entity.isHandActive())
                {
                    if (!canSee && this.seeTime < -60)
                    {
                        this.entity.resetActiveHand();
                    }
                    else if (canSee)
                    {
                        int i = this.entity.getItemInUseMaxCount();

                        if (i >= 20)
                        {
                            this.entity.resetActiveHand();
                            this.entity.AttackEntityWithRangedAttack(attackTarget, ItemBow.getArrowVelocity(i));
                            this.attackTime = this.attackCooldown;
                        }
                    }
                }
                else if (--this.attackTime <= 0 && this.seeTime >= -60)
                {
                    this.entity.setActiveHand(EnumHand.MAIN_HAND);
                }
            }
        }
    }
}
