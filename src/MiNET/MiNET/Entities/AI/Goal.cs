using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    abstract class Goal
    {
        public int MutexBits { get; private set; }

        public abstract bool ShouldExecute();

        public virtual bool ContinueExecuting()
        {
            return this.ShouldExecute();
        }

        public bool IsInterruptable()
        {
            return true;
        }

        public abstract void Execute();

        public virtual void ResetGoal()
        {

        }

        public virtual void UpdateGoal()
        {

        }

        public void SetMutexBits(int mutexBits)
        {
            MutexBits = mutexBits;
        }
    }
}
