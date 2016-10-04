using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathNavigatorClimber : PathNavigatorGround
    {
        private BlockCoordinates targetCoords;

        public PathNavigatorClimber(LivingEntity entity, Worlds.Level level) : base(entity, level) { }

        public override PathEntity GetPathTo(BlockCoordinates bc)
        {
            this.targetCoords = bc;
            return base.GetPathTo(bc);
        }

        public override PathEntity GetPathToEntity(Entity entity)
        {
            this.targetCoords = new BlockCoordinates(entity.KnownPosition);
            return base.GetPathToEntity(entity);
        }

        public override bool TryToMoveTo(float x, float y, float z, float speed)
        {
            
            return base.TryToMoveTo(x, y, z, speed);
        }

        public override bool TryMoveToEntity(Entity entity, float speed)
        {
            return base.TryMoveToEntity(entity, speed);
        }

        public override void OnNavigationTick()
        {
            if (this.HasPath())
            {
                base.OnNavigationTick();
            } else
            {
                if (this.targetCoords != default(BlockCoordinates))
                {
                    double widthSq = this.entity.Width * this.entity.Width;

                    if (this.entity.KnownPosition.DistanceToSquaredToCenter(this.targetCoords) >= widthSq && 
                        (this.entity.KnownPosition.Y <= this.targetCoords.Y || this.entity.KnownPosition.DistanceToSquaredToCenter(
                            new BlockCoordinates(this.targetCoords.X, (int)Math.Floor(this.entity.KnownPosition.Y), this.targetCoords.Z)) >= widthSq)) {
                        this.entity.MoveManager.MoveTo(this.targetCoords.X, this.targetCoords.Y, this.targetCoords.Z, this.speed);
                    } else
                    {
                        this.targetCoords = default(BlockCoordinates);
                    }
                }
            }
        }
    }
}
