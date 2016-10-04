using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class Path
    {
        private PathPoint[] pathPoints = new PathPoint[128];

        private int count;

        public PathPoint AddPoint(PathPoint point)
        {
            if (point.Index >= 0)
            {
                throw new InvalidOperationException("Cannot add a point to a path with an index below 0");
            } else
            {
                if (this.count == this.pathPoints.Length)
                {
                    PathPoint[] pathPoint = new PathPoint[this.count << 1];
                    this.pathPoints = new PathPoint[this.count << 1];
                }

                this.pathPoints[this.count] = point;
                point.Index = this.count;
                this.SortBack(this.count++);
                return point;
            }
        }

        public void ClearPath()
        {
            this.count = 0;
        }

        public PathPoint Dequeue()
        {
            PathPoint pathPoint = this.pathPoints[0];
            this.pathPoints[0] = this.pathPoints[--this.count];
            this.pathPoints[this.count] = null;

            if (this.count > 0)
            {
                this.SortForward(0);
            }

            pathPoint.Index = -1;
            return pathPoint;
        }

        public void ChangeDistance(PathPoint point, float distance)
        {
            float distToTarget = point.DistanceToTarget;
            point.DistanceToTarget = distance;

            if (distance < distToTarget)
            {
                this.SortBack(point.Index);
            }
            else
            {
                this.SortForward(point.Index);
            }
        }

        private void SortBack(int index)
        {
            PathPoint pathpoint = this.pathPoints[index];
            int i;

            for (float f = pathpoint.DistanceToTarget; index > 0; index = i)
            {
                i = index - 1 >> 1;
                PathPoint pathpoint2 = this.pathPoints[i];

                if (f >= pathpoint2.DistanceToTarget)
                {
                    break;
                }

                this.pathPoints[index] = pathpoint2;
                pathpoint2.Index = index;
            }

            this.pathPoints[index] = pathpoint;
            pathpoint.Index = index;
        }

        /**
         * Sorts a point to the right
         */
        private void SortForward(int index)
        {
            PathPoint pathpoint = this.pathPoints[index];
            float distToTarget = pathpoint.DistanceToTarget;

            while (true)
            {
                int i = 1 + (index << 1);
                int j = i + 1;

                if (i >= this.count)
                {
                    break;
                }

                PathPoint pathpoint2 = this.pathPoints[i];
                float distToTarget2 = pathpoint2.DistanceToTarget;
                PathPoint pathpoint3;
                float f2;

                if (j >= this.count)
                {
                    pathpoint3 = null;
                    f2 = float.MaxValue;
                }
                else
                {
                    pathpoint3 = this.pathPoints[j];
                    f2 = pathpoint2.DistanceToTarget;
                }

                if (distToTarget2 < f2)
                {
                    if (distToTarget2 >= distToTarget)
                    {
                        break;
                    }

                    this.pathPoints[index] = pathpoint2;
                    pathpoint2.Index = index;
                    index = i;
                }
                else
                {
                    if (f2 >= distToTarget)
                    {
                        break;
                    }

                    this.pathPoints[index] = pathpoint2;
                    pathpoint2.Index = index;
                    index = j;
                }
            }

            this.pathPoints[index] = pathpoint;
            pathpoint.Index = index;
        }

        public bool IsEmpty()
        {
            return this.count == 0;
        }
    }
}
