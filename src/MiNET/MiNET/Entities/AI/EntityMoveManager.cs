using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class EntityMoveManager
    {
        public enum Action
        {
            Wait,
            MoveTo,
            Strafe
        }

        // The entity that we manage
        protected readonly LivingEntity entity;

        protected double X;
        protected double Y;
        protected double Z;

        public float speed { get; protected set; }
        protected float moveForward;
        protected float moveStrafe;

        protected Action action = Action.Wait;

        public EntityMoveManager(LivingEntity entity)
        {
            this.entity = entity;
        }

        public bool IsUpdating()
        {
            return this.action == Action.MoveTo;
        }

        public void MoveTo(double x, double y, double z, float speed)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.speed = speed;
            this.action = Action.MoveTo;
        }

        public void Strafe(float forward, float strafe)
        {
            this.action = Action.Strafe;
            this.moveForward = forward;
            this.moveStrafe = strafe;
            this.speed = 0.25f;
        }

        public void OnTick()
        {
            if (this.action == Action.Strafe)
            {
                float defaultMovementSpeed = this.entity.MovementSpeed;
                float speed = (float)this.speed * defaultMovementSpeed;

                float f = (float)Math.Sqrt(this.moveForward * moveForward + this.moveStrafe * this.moveStrafe);

                if (f < 1)
                {
                    f = 1;
                }

                f = speed / f;
                float f2 = this.moveForward * f;
                float f3 = this.moveStrafe * f;

                float rotSine = (float)Math.Sin(this.entity.KnownPosition.HeadYaw * 0.017453292f);
                float rotCosine = (float)Math.Cos(this.entity.KnownPosition.HeadYaw * 0.017453292F);

                float f4 = f2 * rotCosine - f3 * rotSine;
                float f5 = f3 * rotCosine - f2 * rotSine;

                PathNavigator pathNavigator = this.entity.GetNavigator();

                if (pathNavigator != null)
                {
                    Node node = pathNavigator.AINode;

                    if (node != null && node.GetNodeType(this.entity.Level, (int)Math.Floor(this.entity.KnownPosition.X + f4), 
                        (int)Math.Floor(this.entity.KnownPosition.Y), (int)Math.Floor(this.entity.KnownPosition.Z + f5)) != NodeType.Walkable) {
                        this.moveForward = 1;
                        this.moveStrafe = 0;
                        speed = defaultMovementSpeed;
                    }
                }

                this.entity.AiMovementSpeed = speed;
                this.entity.AiMoveForward = this.moveForward;
                this.entity.AiMoveStrafing = this.moveStrafe;
                this.action = Action.Wait;
            } else if (this.action == Action.MoveTo)
            {
                this.action = Action.Wait;
                double distX = this.X - this.entity.KnownPosition.X;
                double distY = this.Y - this.entity.KnownPosition.Y;
                double distZ = this.Z - this.entity.KnownPosition.Z;
                double dist = distX * distX + distY * distY + distZ * distZ;

                if (dist < double.MaxValue)
                {
                    this.entity.AiMoveForward = 0f;
                    return;
                }

                float f9 = (float)(Math.Atan2(distZ, distX) * (180D / Math.PI))  - 90.0F;
                this.entity.KnownPosition.HeadYaw = this.LimitAngle(this.entity.KnownPosition.HeadYaw, f9, 90f);
                this.entity.AiMovementSpeed = this.speed * this.entity.AiMovementSpeed;

                if (distY > this.entity.StepHeight && distX * distX + distZ * distZ < Math.Max(1.0F, this.entity.Width))
                {
                    this.entity.JumpManager.IsJumping = true;
                }
            }
        }

        protected float LimitAngle(float yaw, float upperLimit, float lowerLimit)
        {
            float val = (upperLimit - lowerLimit) % 360f;

            if (val >= 180)
                val -= 360;

            if (val < -180)
                val += 360;

            if (val > lowerLimit)
                val = lowerLimit;

            if (val < -lowerLimit)
                val = -lowerLimit;

            float val2 = yaw + val;

            if (val2 < 0)
                val2 += 360f;
            else if (val > 360)
                val2 -= 360f;

            return val2;
        }
    }
}
