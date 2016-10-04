using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Worlds;

namespace MiNET.Entities
{
    class AgeableMob : Mob
    {
        private static int AGE_BABY = -24000;
        private static int AGE_ADULT = 0;
        private static int BREEDING_AGE = 6000;

        public bool AgeLocked { get; set; }
        public int InLove { get; set; }

        private int Age;
        private int ForcedAge;
        private AgeableMob Parent;

        public AgeableMob(int entityTypeId, Level level): base(entityTypeId, level)
        {
            
        }

        public override void OnTick()
        {
            base.OnTick();

            if (AgeLocked)
            {
                SetScale();
            }
            else
            {
                int currentAge = Age;
                if (currentAge < AGE_ADULT)
                {
                    currentAge++;
                    SetAge(currentAge);
                }
                else if (currentAge > AGE_ADULT)
                {
                    currentAge--;
                    SetAge(currentAge);
                }
            }
        }

        public int GetAge()
        {
            return Age;
        }

        public void SetAge(int age)
        {
            Age = age;
            if (IsAdult()) SetScale();
        }

        public void SetBaby()
        {
            SetAge(AGE_BABY);
        }

        public void SetAdult()
        {
            SetAge(AGE_ADULT);
        }

        public bool IsAdult()
        {
            return Age >= AGE_ADULT;
        }

        public bool CanBreed()
        {
            return Age == AGE_ADULT;
        }

        public void SetCanBreed(bool breed)
        {
            if (breed)
            {
                SetAge(AGE_ADULT);
            } else if (IsAdult())
            {
                SetAge(BREEDING_AGE);
            }
        }

        public void SetScale()
        {
            SetScale(IsAdult() ? 1f : .5f);
        }

        public void SetScale(float scale)
        {
            Height = Height * scale;
            Width = Width * scale;
        }

        public AgeableMob CreateBaby()
        {
            throw new NotImplementedException();
            //var spawn = new this.entity
        }

        public AgeableMob GetParent()
        {
            return Parent;
        }

        public void SetParent(AgeableMob mob)
        {
            Parent = mob;
        }
    }
}
