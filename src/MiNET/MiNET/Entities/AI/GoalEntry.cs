using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    struct GoalEntry
    {
        public readonly Goal goal;
        public readonly int priority;
        public bool executable;

        public GoalEntry(int priority, Goal goal)
        {
            this.priority = priority;
            this.goal = goal;
            this.executable = false;
        }

        public void SetExecutable(bool val)
        {
            this.executable = val;
        }
    }
}
