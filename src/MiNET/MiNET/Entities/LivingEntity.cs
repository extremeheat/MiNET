using MiNET.Blocks;
using MiNET.Effects;
using MiNET.Entities.AI;
using MiNET.Utils;
using MiNET.Worlds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities
{
    public abstract class LivingEntity : LivingEntityBase
    {
        private bool RemoveIfTooDistant;
        private bool PickupItems;

        // Grace period for hitting entities
        private int NoDamageTicks;

        public readonly EntityJumpManager JumpManager;
        public readonly EntityMoveManager MoveManager;
        public readonly EntityLookManager LookManager;

        // Generic tasks that the AI is expected to perform.
        // Looking around, wandering, etc.
        protected readonly Goals AIGoals;
        // Attacking tasks that the AI is expected to perform.
        // Zombies attack players and villagers, etc.
        protected readonly Goals ATAttackingGoals;

        private Mob AttackTarget;

        protected float[] InventoryDropHandChances = new float[2];

        private readonly Dictionary<NodeType, float> MapPathPriority = new Dictionary<NodeType, float>();

        public float StepHeight;
        public float MovementSpeed { get; set; } = 0.1f;
        public float Absorption { get; set; } = 0;
        public ConcurrentDictionary<EffectType, Effect> Effects { get; set; } = new ConcurrentDictionary<EffectType, Effect>();

        // AI
        protected PathNavigator navigator;
        public bool AiIsJumping;
        public float AiMoveStrafing;
        public float AiMoveForward;
        public float AiRandomYawVelocity;
        public float AiMovementSpeed;
        //public float AiYawOffset;

        public BlockCoordinates HomeLocation;
        public float DistanceFromHome;

        public LivingEntity(int entityTypeId, Level level) : base(entityTypeId, level)
        {
            this.LookManager = new EntityLookManager(this);
            this.MoveManager = new EntityMoveManager(this);
            this.JumpManager = new EntityJumpManager(this);
            this.navigator = this.CreateNavigator(level);
        }

        public LivingEntity(EntityType mobTypes, Level level) : this((int) mobTypes, level)
		{
        }


        protected virtual PathNavigator CreateNavigator(Level level)
        {
            return new PathNavigatorGround(this, level);
        }

        public Mob GetAttackTarget()
        {
            return AttackTarget;
        }

        public void SetAttackTarget(Mob attackTarget)
        {
            AttackTarget = attackTarget;
        }

        public bool CanAttack(Mob entity)
        {
            return !(entity is Hostile.Ghast);
        }

        public int GetMaximumFallHeight()
        {
            if (this.AttackTarget == null)
            {
                return 3;
            }
            else
            {
                int i = (int)(this.HealthManager.Health - this.HealthManager.MaxHealth * 0.33);
                i = (i - (3 - (byte)this.Level.Difficulty) * 4);

                if (i < 0)
                {
                    i = 0;
                }

                return i + 3;
            }
        }

        public void SetHome(BlockCoordinates location, int distance = -1)
        {
            this.HomeLocation = location;
            this.DistanceFromHome = distance;
        }

        public void RemoveHome()
        {
            this.DistanceFromHome = -1;
        }

        public bool HasHome()
        {
            return this.DistanceFromHome != -1;
        }

        public bool IsHomeNearby()
        {
            return this.IsHomeNearby(new BlockCoordinates(KnownPosition));
        }

        public bool IsHomeNearby(BlockCoordinates pos)
        {
            return this.DistanceFromHome == -1 ? true : this.HomeLocation.DistanceTo(pos) < this.DistanceFromHome * this.DistanceFromHome;
        }

        public virtual float GetPathWeight(BlockCoordinates loc)
        {
            return 0;
        }

        public override void OnTick()
        {
            base.OnTick();

            // BASE MOB TICKING
            if (Velocity.Length() > 0)
            {
                PlayerLocation oldPosition = (PlayerLocation)KnownPosition.Clone();
                bool onGroundBefore = IsOnGround(KnownPosition);

                KnownPosition.X += (float)Velocity.X;
                KnownPosition.Y += (float)Velocity.Y;
                KnownPosition.Z += (float)Velocity.Z;

                bool onGround = IsOnGround(KnownPosition);
                if (!onGroundBefore && onGround)
                {
                    KnownPosition.Y = (float)Math.Floor(oldPosition.Y);
                    Velocity = Vector3.Zero;
                }
                else
                {
                    Velocity *= (float)(1.0f - Drag);
                    if (!onGround)
                    {
                        Velocity -= new Vector3(0, (float)Gravity, 0);
                    }
                }
                LastUpdatedTime = DateTime.UtcNow;
            }
            else if (Velocity != Vector3.Zero)
            {
                Velocity = Vector3.Zero;
                LastUpdatedTime = DateTime.UtcNow;
            }
            // END BASE MOB TICKING

            /*if (IsSpawned && Level.Random.Next(1000) < LivingSoundTime++)
            {
                LivingSoundTime = -TalkingInterval;
                PlayLivingSound();
            }*/
        }

        protected void Jump()
        {
            Velocity = new Vector3(Velocity.X, 0.42f, Velocity.Z);

            if (IsEffectActive(EffectType.JumpBoost))
            {
                Velocity = new Vector3(Velocity.X, (Effects[EffectType.JumpBoost].Level + 1) * 0.1f, Velocity.Z);
            }

            if (IsSprinting)
            {
                float rot = KnownPosition.HeadYaw * 0.017453292F;
                Velocity = new Vector3((float)Math.Sin(rot) * 0.2f, Velocity.Y, (float)Math.Cos(rot) * 0.2f);
            }
        }

        public bool IsEffectActive(EffectType effect)
        {
            if (Effects.ContainsKey(effect))
                return true;
            return false;
        }

        protected void OnAITick()
        {
            Velocity = new Vector3(Velocity.X, Velocity.Y + 0.03f, Velocity.Z);
        }

        public bool IsInWater(dynamic loc = null)
        {
            var block = Level.GetBlock(loc ?? KnownPosition);
            if (block is StationaryWater || block is FlowingWater)
            {
                return true;
            }
            return false;
        }

        public bool IsInLiquid(dynamic loc = null)
        {
            var block = Level.GetBlock(loc ?? KnownPosition);
            if (block is Stationary || block is Flowing)
            {
                return true;
            }
            return false;
        }

        public void MoveEntity(float strafe, float forward)
        {
            if (IsInWater() && !IsRiding)
            {

            }
        }

        public void FaceEntity(Entity entity, float maxYawIncrease, float maxPitchIncrease)
        {
            float x = entity.KnownPosition.X - this.KnownPosition.X;
            float y;
            float z = entity.KnownPosition.Z - this.KnownPosition.Z;

            if (entity is LivingEntityBase)
            {
                LivingEntityBase entity2 = (LivingEntityBase)entity;
                y = entity2.KnownPosition.Y + entity2.EyeHeight - (this.KnownPosition.Y + this.EyeHeight);
            } else
            {
                y = (entity.GetBoundingBox().Min.Y + entity.GetBoundingBox().Max.Y) / 2.0f - (this.KnownPosition.Y + this.EyeHeight);
            }

            double dist = Math.Sqrt(x * x + z * z);
            double yawInc = Math.Atan2(z, x) * (180 / Math.PI) - 90;
            double pitchInc = (-(Math.Atan2(y, dist) * (180 / Math.PI)));
            this.KnownPosition.Pitch = this.UpdateRotation(this.KnownPosition.Pitch, (float)pitchInc, maxPitchIncrease);
            this.KnownPosition.Yaw = this.UpdateRotation(this.KnownPosition.Yaw, (float)yawInc, maxYawIncrease);
        }

        private float UpdateRotation(float angle, float targetAngle, float maxIncrease)
        {
            float val = (targetAngle - maxIncrease) % 360f;

            if (val > maxIncrease)
            {
                val = maxIncrease;
            }

            if (val < -maxIncrease)
            {
                val = -maxIncrease;
            }

            return angle + val;
        }


        // AI
        public float GetPathPriority(NodeType nodeType)
        {
            if (MapPathPriority.ContainsKey(nodeType))
                return MapPathPriority[nodeType];
            else
                return (float)nodeType;
        }

        public void SetPathPriority(NodeType nodeType, float priority)
        {
            this.MapPathPriority.Add(nodeType, priority);
        }

        public PathNavigator GetNavigator()
        {
            return this.navigator;
        }

        public List<Entity> GetCollidingEntities(Vector3 position, Vector3 direction)
        {
            List<Entity> found = new List<Entity>();
            Ray2 ray = new Ray2
            {
                x = position,
                d = Vector3.Normalize(direction)
            };

            var players = Level.GetSpawnedPlayers().OrderBy(player => Vector3.Distance(position, player.KnownPosition.ToVector3()));
            foreach (var entity in players)
            {
                if (Projectiles.Projectile.Intersect(entity.GetBoundingBox(), ray))
                {
                    if (ray.tNear > direction.Length()) break;

                    Vector3 p = ray.x + new Vector3((float)ray.tNear) * ray.d;
                    KnownPosition = new PlayerLocation((float)p.X, (float)p.Y, (float)p.Z);
                    found.Add(entity);
                }
            }

            var entities = Level.Entities.Values.OrderBy(entity => Vector3.Distance(position, entity.KnownPosition.ToVector3()));
            foreach (Entity entity in entities)
            {
                if (entity == this) continue;
                if (entity is Projectiles.Projectile) continue;

                if (Projectiles.Projectile.Intersect(entity.GetBoundingBox(), ray))
                {
                    if (ray.tNear > direction.Length()) break;

                    Vector3 p = ray.x + new Vector3((float)ray.tNear) * ray.d;
                    KnownPosition = new PlayerLocation(p.X, p.Y, p.Z);
                    found.Add(entity);
                }
            }

            return found;
        }

        public bool CanSeeEntity(Entity entity)
        {
            var entities = this.GetCollidingEntities(new Vector3(this.KnownPosition.X, this.KnownPosition.Y + this.EyeHeight, this.KnownPosition.Z),
                new Vector3(entity.KnownPosition.X, entity.KnownPosition.Y/* + entity.EyeHeight*/, entity.KnownPosition.Z));
            foreach (var e in entities)
            {
                if (e == entity)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
