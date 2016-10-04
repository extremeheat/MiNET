using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.Passive
{
    class TameableMob : PassiveMob, IOwnable
    {
        protected AI.GoalSit aiSit;

        public TameableMob(EntityType entityType, Worlds.Level level) : base()
        {
            
        }
    }
}
