using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class EntityJumpManager
    {
        public readonly LivingEntity entity;
        public bool IsJumping;

        public EntityJumpManager(LivingEntity entity)
        {
            this.entity = entity;
        }

        public void Jump()
        {
            this.entity.AiIsJumping = this.IsJumping;
            this.IsJumping = false;
        }
    }
}
