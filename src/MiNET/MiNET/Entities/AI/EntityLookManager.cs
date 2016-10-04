using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class EntityLookManager
    {
        public readonly LivingEntity entity;

        // delta pitch + yaw
        private float deltaLookYaw;
        private float deltaLookPitch;

        public bool IsLooking;

        public float X;
        public float Y;
        public float Z;

        public EntityLookManager(LivingEntity entity)
        {
            this.entity = entity;
        }

        public void SetLookingPosition(Entity entity, float deltaYaw, float deltaPitch)
        {
            this.X = entity.KnownPosition.X;

            if (entity is LivingEntity)
            {
                this.Y = entity.KnownPosition.Y + ((LivingEntity)entity).EyeHeight;
            } else
            {
                this.Y = (entity.GetBoundingBox().Min.Y + entity.GetBoundingBox().Max.Y) / 2;
            }

            this.Z = entity.KnownPosition.Z;
            this.deltaLookPitch = deltaPitch;
            this.deltaLookYaw = deltaYaw;
            this.IsLooking = true;
        }

        public void SetLookingPosition(float x, float y, float z, float deltaYaw, float deltaPitch)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.deltaLookPitch = deltaPitch;
            this.deltaLookYaw = deltaYaw;
            this.IsLooking = true;
        }

        public void OnTick()
        {
            this.entity.KnownPosition.Pitch = 0;

            if (this.IsLooking)
            {
                this.IsLooking = false;
                float x = this.X - this.entity.KnownPosition.X;
                float y = this.Y - (this.entity.KnownPosition.Y + this.entity.EyeHeight);
                float z = this.Z - this.entity.KnownPosition.Z;
                float dist = (float)Math.Sqrt(x * x + z * z);
                float f = (float)Math.Atan2(z, dist) * (180f / (float)Math.PI) - 90f;
                float f1 = (float)(-(Math.Atan2(y, dist) * (180D / Math.PI)));
                this.entity.KnownPosition.Pitch = this.UpdateRotation(this.entity.KnownPosition.Pitch, f1, this.deltaLookPitch);
                this.entity.KnownPosition.HeadYaw = this.UpdateRotation(this.entity.KnownPosition.HeadYaw, f, this.deltaLookYaw);
            } else
            {
                // TODO: Figure out issue with render yaw offset
                this.entity.KnownPosition.HeadYaw = this.UpdateRotation(this.entity.KnownPosition.HeadYaw, this.entity.KnownPosition.Yaw, 10f);
            }

            float val = (this.entity.KnownPosition.HeadYaw - this.entity.KnownPosition.Yaw) % 360f;

            if (val >= 180)
                val -= 360;

            if (val < -180)
                val += 360;

            if (this.entity.GetNavigator().HasPath())
            {
                if (val < -75)
                {
                    this.entity.KnownPosition.HeadYaw = this.entity.KnownPosition.Yaw - 75f;
                }

                if (val > 75)
                {
                    this.entity.KnownPosition.HeadYaw = this.entity.KnownPosition.Yaw + 75f;
                }
            }
        }

        private float UpdateRotation(float rot, float upper, float lower)
        {
            float val = (upper - lower) % 360f;

            if (val >= 180)
                val -= 360;

            if (val < -180)
                val += 360;

            if (val > lower)
                val = lower;

            if (val < -lower)
                val = -lower;

            return rot + val;
        }
    }
}
