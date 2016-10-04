using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class GoalBeg : Goal
    {
        private readonly Passive.Wolf entity;
        private Player player;
        //private Worlds.Level level;
        private float minPlayerDistance;
        private int timeoutCounter;

        public GoalBeg(Passive.Wolf wolf, float minDistance)
        {
            this.entity = wolf;
            this.minPlayerDistance = minDistance;
            this.SetMutexBits(2);
        }

        public override bool ShouldExecute()
        {
            var nearbyPlayers = this.entity.Level.GetNearbyEntities(this.entity.KnownPosition, this.minPlayerDistance, typeof(Player));
            var closestPlayer = nearbyPlayers[0] is Player ? nearbyPlayers[0] : null;
        }
    }
}
