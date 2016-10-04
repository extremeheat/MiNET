using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    public enum NodeType 
    {
        Blocked = -1,
        Open = 0,
        Walkable = 0,
        Trapdoor = 0,
        Fence = -1,
        Lava = -1,
        Water = 8,
        Rail = 0,
        DangerFire = 8,
        DamageFire = 16,
        DangerCactus = 8,
        DamageCactus = 8,
        DamageOther = 8,
        DoorOpen = 0,
        DoorWoodClosed = -1,
        DoorIronClosed = -1
    }
}
