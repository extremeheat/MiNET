using System.Numerics;

namespace MiNET.Sounds
{
    class ZombieAttackDoorSound : Sound
    {
        public ZombieAttackDoorSound(Vector3 position, int pitch) : base((short)LevelEventType.SoundZombieDoorHit, position, pitch)
        {

        }
    }
}
