using System;
using System.Collections;
using System.Numerics;
using log4net;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using System.Collections.Generic;
using MiNET.Blocks;

namespace MiNET.Entities
{
	public class Entity
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (Entity));


		public Level Level { get; set; }

		public int EntityTypeId { get; private set; }
		public long EntityId { get; set; }
		public bool IsSpawned { get; set; }

		public DateTime LastUpdatedTime { get; set; }
		public PlayerLocation KnownPosition { get; set; }
		public Vector3 Velocity { get; set; }

		public HealthManager HealthManager { get; set; }

		public string NameTag { get; set; }

		public bool NoAi { get; set; }
		public bool HideNameTag { get; set; }
		public bool Silent { get; set; }

		public long Age { get; set; }
		public double Height { get; set; } = 1.80;
		public double Width { get; set; } = 0.6;
		public double Length { get; set; } = 0.6;
		public double Drag { get; set; }
		public double Gravity { get; set; }
		public int Data { get; set; }

		public Entity(int entityTypeId, Level level)
		{
			Height = 1;
			Width = 1;
			Length = 1;
			Gravity = 0.08;
			Drag = 0.02;

			EntityId = EntityManager.EntityIdUndefined;
			Level = level;
			EntityTypeId = entityTypeId;
			KnownPosition = new PlayerLocation();
			HealthManager = new HealthManager(this);
		}

		public virtual MetadataDictionary GetMetadata()
		{
			MetadataDictionary metadata = new MetadataDictionary();
			metadata[0] = new MetadataByte(GetDataValue());
			metadata[2] = new MetadataString(NameTag ?? string.Empty);
			metadata[3] = new MetadataByte(!HideNameTag);
			metadata[4] = new MetadataByte(Silent);
			metadata[7] = new MetadataInt(0); // Potion Color
			metadata[8] = new MetadataByte(0); // Potion Ambient
			metadata[15] = new MetadataByte(NoAi);
			metadata[16] = new MetadataByte(0); // Player flags
			//metadata[17] = new MetadataIntCoordinates(0, 0, 0);
			metadata[23] = new MetadataLong(-1); // Leads EID (target or holder?)
			metadata[24] = new MetadataByte(0); // Leads on/off
			return metadata;
		}

		public bool IsSneaking { get; set; }
		public bool IsRiding { get; set; }
		public bool IsSprinting { get; set; }
		public bool IsInAction { get; set; }
		public bool IsInvisible { get; set; }

		public byte GetDataValue()
		{
			BitArray bits = new BitArray(8);
			bits[0] = HealthManager.IsOnFire;
			bits[1] = IsSneaking; // Sneaking
			bits[2] = IsRiding; // Riding
			bits[3] = IsSprinting; // Sprinting
			bits[4] = IsInAction; // Action
			bits[5] = IsInvisible; // Invisible
			bits[6] = false; // Unused
			bits[7] = false; // Unused

			byte[] bytes = new byte[1];
			bits.CopyTo(bytes, 0);
			return bytes[0];
		}

		public virtual void OnTick()
		{
			Age++;

			HealthManager.OnTick();
		}

        public Entity CheckEntityCollide(Vector3 position, Vector3 direction)
        {
            Ray2 ray = new Ray2
            {
                x = position,
                d = Vector3.Normalize(direction)
            };

            var players = Level.GetSpawnedPlayers().OrderBy(player => Vector3.Distance(position, player.KnownPosition.ToVector3()));
            foreach (var entity in players)
            {
                if (entity == Shooter) continue;

                if (Intersect(entity.GetBoundingBox(), ray))
                {
                    if (ray.tNear > direction.Length()) break;

                    Vector3 p = ray.x + new Vector3((float)ray.tNear) * ray.d;
                    KnownPosition = new PlayerLocation((float)p.X, (float)p.Y, (float)p.Z);
                    return entity;
                }
            }

            var entities = Level.Entities.Values.OrderBy(entity => Vector3.Distance(position, entity.KnownPosition.ToVector3()));
            foreach (Entity entity in entities)
            {
                if (entity == Shooter) continue;
                if (entity == this) continue;
                if (entity is Projectile) continue;

                if (Intersect(entity.GetBoundingBox(), ray))
                {
                    if (ray.tNear > direction.Length()) break;

                    Vector3 p = ray.x + new Vector3((float)ray.tNear) * ray.d;
                    KnownPosition = new PlayerLocation(p.X, p.Y, p.Z);
                    return entity;
                }
            }

            return null;
        }

        public bool CheckBlockCollide(BoundingBox bb)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        Block block = Level.GetBlock(bb.X + x, bb.Y + y, coords.Z + z);

                        if (bb.Intersects(GetBoundingBox()))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckBlockCollide(PlayerLocation location)
        {
            var bbox = GetBoundingBox();
            var pos = location.ToVector3();

            var coords = new BlockCoordinates(
                (int)Math.Floor(KnownPosition.X),
                (int)Math.Floor((bbox.Max.Y + bbox.Min.Y) / 2.0),
                (int)Math.Floor(KnownPosition.Z));

            Dictionary<double, Block> blocks = new Dictionary<double, Block>();

            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        Block block = Level.GetBlock(coords.X + x, coords.Y + y, coords.Z + z);
                        if (block is Air) continue;

                        BoundingBox blockbox = block.GetBoundingBox() + 0.3f;
                        if (blockbox.Intersects(GetBoundingBox()))
                        {
                            //if (!blockbox.Contains(KnownPosition.ToVector3())) continue;

                            if (block is FlowingLava || block is StationaryLava)
                            {
                                HealthManager.Ignite(1200);
                                continue;
                            }

                            if (!block.IsSolid) continue;

                            blockbox = block.GetBoundingBox();

                            var midPoint = blockbox.Min + new Vector3(0.5f);
                            blocks.Add(Vector3.Distance((pos - Velocity), midPoint), block);
                        }
                    }
                }
            }

            if (blocks.Count == 0) return false;

            var firstBlock = blocks.OrderBy(pair => pair.Key).First().Value;

            BoundingBox boundingBox = firstBlock.GetBoundingBox();
            if (!SetIntersectLocation(boundingBox, KnownPosition))
            {
                // No real hit
                return false;
            }

            // Use to debug hits, makes visual impressions (can be used for paintball too)
            var substBlock = new Stone { Coordinates = firstBlock.Coordinates };
            Level.SetBlock(substBlock);
            // End debug block

            Velocity = Vector3.Zero;
            return true;
        }

        public virtual void SpawnEntity()
		{
			Level.AddEntity(this);

			IsSpawned = true;
		}

		public virtual void SpawnToPlayers(Player[] players)
		{
			var addEntity = McpeAddEntity.CreateObject();
			addEntity.entityType = (byte) EntityTypeId;
			addEntity.entityId = EntityId;
			addEntity.x = KnownPosition.X;
			addEntity.y = KnownPosition.Y;
			addEntity.z = KnownPosition.Z;
			addEntity.yaw = KnownPosition.Yaw;
			addEntity.pitch = KnownPosition.Pitch;
			addEntity.metadata = GetMetadata();
			addEntity.speedX = (float) Velocity.X;
			addEntity.speedY = (float) Velocity.Y;
			addEntity.speedZ = (float) Velocity.Z;
			Level.RelayBroadcast(players, addEntity);
		}

		public virtual void DespawnEntity()
		{
			Level.RemoveEntity(this);
		}

		public virtual void DespawnFromPlayers(Player[] players)
		{
			McpeRemoveEntity mcpeRemoveEntity = McpeRemoveEntity.CreateObject();
			mcpeRemoveEntity.entityId = EntityId;
			Level.RelayBroadcast(players, mcpeRemoveEntity);
		}

		public virtual void BroadcastSetEntityData()
		{
			McpeSetEntityData mcpeSetEntityData = McpeSetEntityData.CreateObject();
			mcpeSetEntityData.entityId = EntityId;
			mcpeSetEntityData.metadata = GetMetadata();
			Level?.RelayBroadcast(mcpeSetEntityData);
		}

		public BoundingBox GetBoundingBox()
		{
			var pos = KnownPosition;
			double halfWidth = Width/2;

			return new BoundingBox(new Vector3((float) (pos.X - halfWidth), pos.Y, (float) (pos.Z - halfWidth)), new Vector3((float) (pos.X + halfWidth), (float) (pos.Y + Height), (float) (pos.Z + halfWidth)));
		}

		public byte GetDirection()
		{
			return DirectionByRotationFlat(KnownPosition.Yaw);
		}

		public static byte DirectionByRotationFlat(float yaw)
		{
			byte direction = (byte) ((int) Math.Floor((yaw*4F)/360F + 0.5D) & 0x03);
			switch (direction)
			{
				case 0:
					return 1; // West
				case 1:
					return 2; // North
				case 2:
					return 3; // East
				case 3:
					return 0; // South 
			}
			return 0;
		}

		public virtual void Knockback(Vector3 velocity)
		{
			Velocity += velocity;
		}


		public virtual Item[] GetDrops()
		{
			return new Item[] {};
		}
	}
}