using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Sounds
{
    class ZombieBreakDoorSound : Sound
    {
        public ZombieBreakDoorSound(Vector3 position, int pitch) : base((short) LevelEventType.SoundZombieBreakDoor, position, pitch)
        {

        }
    }
}
