﻿using System;
using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiNET.Entities
{
    public class Mob : LivingEntity
    {
        public MobInventory Inventory { get; protected set; } = null;

        public Mob(int entityTypeId, Level level, bool hasInventory = false) : base(entityTypeId, level)
        {
            Width = Length = 0.6;
            Height = 1.80;

            if (hasInventory)
            {
                Inventory = new MobInventory(this);
            }
        }

        public Mob(EntityType mobTypes, Level level) : this((int)mobTypes, level)
        {
        }

        public override void OnTick()
        {
            base.OnTick();

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
        }

        private bool IsOnGround(PlayerLocation position)
        {
            PlayerLocation pos = (PlayerLocation)position.Clone();
            pos.Y -= 0.1f;
            Block block = Level.GetBlock(new BlockCoordinates(pos));

            return block.Id != 0; // Should probably test for solid
        }
    }
}