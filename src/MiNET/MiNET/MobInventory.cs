using MiNET.Entities;
using MiNET.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET
{
    public class MobInventory
    {
        Mob InventoryHolder;

        public Item Boots { get; set; }
        public Item Leggings { get; set; }
        public Item Chest { get; set; }
        public Item Helmet { get; set; }

        public Item HeldItem { get; set; }

        public MobInventory(Mob mob)
        {
            this.InventoryHolder = mob;
        }

        public bool CanPickupItem()
        {
            if (HeldItem == null)
            {
                return true;
            }
            return false;
        }
    }
}
