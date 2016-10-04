using fNbt;
using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities
{
    public class LivingEntityBase : Entity
    {
        public readonly float EyeHeight = 1.53f;

        public LivingEntityBase(int entityTypeId, Worlds.Level level) : base(entityTypeId, level)
        {

        }

        public bool IsOnGround(PlayerLocation position)
        {
            PlayerLocation pos = (PlayerLocation)position.Clone();
            pos.Y -= 0.1f;
            var block = Level.GetBlock(new BlockCoordinates(pos));

            return block.Id != 0; // Should probably test for solid
        }

        public bool IsOnGround()
        {
            return IsOnGround(KnownPosition);
        }

        /*public void OnEntityAttack(Entity sourceEntity, float damage)
        {
        }

        public void OnEntityAttack(Player sourcePlayer, Items.Item item)
        {
            double damage = item.GetDamage(); //Item Damage.
            if (IsFalling)
            {
                damage += Level.Random.Next((int)(damage / 2 + 2));

                McpeAnimate animate = McpeAnimate.CreateObject();
                animate.entityId = target.EntityId;
                animate.actionId = 4;
                Level.RelayBroadcast(animate);
            }

            Effect effect;
            if (Effects.TryGetValue(EffectType.Weakness, out effect))
            {
                damage -= (effect.Level + 1) * 4;
                if (damage < 0) damage = 0;
            }
            else if (Effects.TryGetValue(EffectType.Strength, out effect))
            {
                damage += (effect.Level + 1) * 3;
            }

            damage += CalculateDamageIncreaseFromEnchantments(itemInHand);

            player.HealthManager.TakeHit(this, (int)CalculatePlayerDamage(player, damage), DamageCause.EntityAttack);

        }

        protected virtual double CalculateEnchantmentDamageBoosts(Items.Item tool)
        {
            if (tool == null) return 0;
            if (tool.ExtraData == null) return 0;

            NbtList enchantings;
            if (!tool.ExtraData.TryGet("ench", out enchantings)) return 0;

            double increase = 0;
            foreach (NbtCompound enchanting in enchantings)
            {
                short level = enchanting["lvl"].ShortValue;

                if (level == 0) continue;

                short id = enchanting["id"].ShortValue;
                if (id == 9)
                {
                    increase += 1 + ((level - 1) * 0.5);
                }
            }

            return increase;
        }*/

        //public bool IsInLiquid()

        //public bool Entity
    }
}