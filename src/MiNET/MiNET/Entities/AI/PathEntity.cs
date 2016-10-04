using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathEntity
    {
        public readonly PathPoint[] Points;
        public PathPoint[] OpenPoints { get; } = new PathPoint[0];
        public PathPoint[] ClosedPoints { get; } = new PathPoint[0];
        public int CurrentPath;
        public int PathLength;

        public PathPoint Target { get; }

        public PathEntity(PathPoint[] points)
        {
            this.Points = points;
            this.PathLength = points.Length;
        }

        public void IncrementPathIndex()
        {
            this.CurrentPath++;
        }

        public bool IsFinished()
        {
            return this.CurrentPath >= this.PathLength;
        }

        public PathPoint GetFinalPathPoint()
        {
            return this.PathLength > 0 ? this.Points[this.PathLength - 1] : null;
        }

        public Vector3 GetVectorFromPoint(Entity entity, int pointIndex)
        {
            float x = this.Points[pointIndex].X + ((float)entity.Width + 1) * 0.5f;
            float y = this.Points[pointIndex].Y;
            float z = this.Points[pointIndex].Z + ((float)entity.Width + 1) * 0.5f;

            return new Vector3(x, y, z);
        }

        public Vector3 GetPosition(Entity entity)
        {
            return this.GetVectorFromPoint(entity, this.CurrentPath);
        }

        public Vector3 GetCurrentPosition()
        {
            PathPoint pathpoint = this.Points[this.CurrentPath];
            return new Vector3(pathpoint.X, pathpoint.Y, pathpoint.Z);
        }

        public bool IsSamePath(PathEntity otherPath)
        {
            if (otherPath == null)
            {
                return false;
            } else if (otherPath.PathLength != this.PathLength)
            {
                return false;
            } else
            {
                for (int i = 0; i < this.PathLength; ++i)
                {
                    if (this.Points[i].X != otherPath.Points[i].X || this.Points[i].Y != otherPath.Points[i].Y || this.Points[i].Z != otherPath.Points[i].Z)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
