using MiNET.Worlds;
using MiNET.Items;

namespace MiNET.Entities.Hostile
{
	public class Skeleton : HostileMob, IRangedAttacker
	{
		public Skeleton(Level level) : base((int) EntityType.Skeleton, level)
		{
			Width = Length = 0.6;
			Height = 1.95;
            this.Inventory.HeldItem = new ItemBow();
		}

        public void AttackEntityWithRangedAttack(Mob target, float velocity)
        {
        }
    }
}