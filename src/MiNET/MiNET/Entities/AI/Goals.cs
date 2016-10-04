using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class Goals
    {

        private List<GoalEntry> goalEntries = new List<GoalEntry>();
        private List<GoalEntry> executingEntries = new List<GoalEntry>();

        private int tickCount;
        private int tickRate = 3;
        private int disabledControlFlags;

        public Goals()
        {

        }

        public void AddGoal(int priority, Goal goal)
        {
            this.goalEntries.Add(new GoalEntry(priority, goal));
        }

        public void RemoveGoal(Goal goal)
        {
            foreach (var goalEntry in this.goalEntries)
            {
                if (goal == goalEntry.goal)
                {
                    goalEntry.SetExecutable(false);
                    goalEntry.goal.ResetGoal();
                    this.executingEntries.Remove(goalEntry);
                }
            }
        }

        public void OnTick()
        {
            if (this.tickCount++ % this.tickRate == 0)
            {
                foreach (var goalEntry in this.goalEntries)
                {
                    if (goalEntry.executable)
                    {
                        if (!this.CanExecute(goalEntry) || !this.CanContinue(goalEntry))
                        {
                            goalEntry.SetExecutable(false);
                            goalEntry.goal.ResetGoal();
                            this.executingEntries.Remove(goalEntry);
                        }
                    } else if (this.CanContinue(goalEntry) && goalEntry.goal.ShouldExecute())
                    {
                        goalEntry.SetExecutable(true);
                        goalEntry.goal.Execute();
                        this.executingEntries.Add(goalEntry);
                    }
                }
            } else
            {
                foreach (var executingGoalEntry in this.executingEntries)
                {
                    if (!this.CanContinue(executingGoalEntry))
                    {
                        executingGoalEntry.SetExecutable(false);
                        executingGoalEntry.goal.ResetGoal();
                    }
                }
            }
        }

        private bool CanContinue(GoalEntry entry)
        {
            return entry.goal.ContinueExecuting();
        }

        private bool CanExecute(GoalEntry entry)
        {
            if (this.executingEntries.Count == 0)
            {
                return true;
            } else if (this.IsControlFlagDisabled(entry.goal.MutexBits))
            {
                return false;
            } else
            {
                foreach (var e in this.executingEntries)
                {
                    if (!e.Equals(entry))
                    {
                        if (entry.priority >= e.priority)
                        {
                            if (this.AreTasksCompatible(entry, e))
                                return false;
                        }
                    } else if (e.goal.IsInterruptable())
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool AreTasksCompatible(GoalEntry goalEntry1, GoalEntry goalEntry2)
        {
            return (goalEntry1.goal.MutexBits & goalEntry2.goal.MutexBits) == 0;
        }

        public bool IsControlFlagDisabled(int flag)
        {
            return (this.disabledControlFlags & flag) > 0;
        }

        public void DisableControlFlag(int flag)
        {
            this.disabledControlFlags |= flag;
        }

        public void EnableControlFlag(int flag)
        {
            this.disabledControlFlags &= ~flag; // blame notch
        }

        public void SetControlFlag(int flag, bool enable)
        {
            if (enable)
            {
                this.EnableControlFlag(flag);
            } else
            {
                this.DisableControlFlag(flag);
            }
        }
    }
}
