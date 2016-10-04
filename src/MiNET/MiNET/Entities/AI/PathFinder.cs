using MiNET.Utils;
using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathFinder
    {
        private Path path = new Path();

        private readonly List<PathPoint> blockedPoints = new List<PathPoint>();

        private PathPoint[] pathOptions = new PathPoint[32];
        private Node aiNode;

        public PathFinder(Node aiNode)
        {
            this.aiNode = aiNode;
        }

        public PathEntity FindPath(Level level, LivingEntity livingEntity, Entity entity, float maxDistance)
        {
            return FindPath(level, livingEntity, entity.KnownPosition.X, entity.GetBoundingBox().Min.Y, entity.KnownPosition.Z, maxDistance);
        }

        public PathEntity FindPath(Level level, LivingEntity livingEntity, BlockCoordinates blockCoords, float maxDistance)
        {
            return FindPath(level, livingEntity, (double)blockCoords.X + 0.5, (double)blockCoords.Y + 0.5, (double)blockCoords.Z + 0.5, maxDistance);
        }

        public PathEntity FindPath(Level level, LivingEntity livingEntity, double x, double y, double z, float maxDistance)
        {
            this.path.ClearPath();
            this.aiNode.Initialize(level, livingEntity);
            PathPoint pathpoint = this.aiNode.GetStart();
            PathPoint pathpoint2 = this.aiNode.GetPathPointAt((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z));
            PathEntity path = this.FindPath(pathpoint, pathpoint2, maxDistance);
            this.aiNode.PostProcess();
            return path;
        }

        // Finds a path from point A to point B
        private PathEntity FindPath(PathPoint pointA, PathPoint pointB, float maxDistance)
        {
            pointA.TotalPathDistance = 0;
            pointA.DistanceToNext = pointA.DistanceManhattan(pointB);
            pointA.DistanceToTarget = pointA.DistanceToNext;
            this.path.ClearPath();
            this.blockedPoints.Clear();
            this.path.AddPoint(pointA);
            PathPoint pathpoint = pointA;
            int i = 0;

            while (!this.path.IsEmpty() && i < 200)
            {
                ++i;
                PathPoint pathpoint2 = this.path.Dequeue();

                if (pathpoint.Equals(pointB))
                {
                    pathpoint = pointB;
                    break;
                }

                if (pathpoint2.DistanceManhattan(pointB) < pathpoint.DistanceManhattan(pointB))
                {
                    pathpoint = pathpoint2;
                }

                pathpoint2.Visited = true;

                int pathIndex = this.aiNode.FindPathOptions(this.pathOptions, pathpoint2, pointB, maxDistance);

                for (int k = 0; k < pathIndex; ++k)
                {
                    PathPoint pathpoint3 = this.pathOptions[k];
                    float dist = pathpoint2.DistanceManhattan(pathpoint3);
                    pathpoint3.DistanceFromOrigin = pathpoint2.DistanceFromOrigin + dist;
                    pathpoint3.Cost = dist + pathpoint3.CostMalus;
                    float distPlusCost = pathpoint2.TotalPathDistance + pathpoint3.Cost;

                    if (pathpoint3.DistanceFromOrigin < maxDistance && (!pathpoint3.IsAssigned() || distPlusCost < pathpoint3.TotalPathDistance))
                    {
                        pathpoint3.Previous = pathpoint2;
                        pathpoint3.TotalPathDistance = distPlusCost;
                        pathpoint3.DistanceToNext = pathpoint3.DistanceManhattan(pointB) + pathpoint3.CostMalus;

                        if (pathpoint3.IsAssigned())
                        {
                            this.path.ChangeDistance(pathpoint3, pathpoint3.TotalPathDistance + pathpoint3.DistanceToNext);
                        }
                        else
                        {
                            pathpoint3.DistanceToTarget = pathpoint3.TotalPathDistance + pathpoint3.DistanceToNext;
                            this.path.AddPoint(pathpoint3);
                        }
                    }
                }
            }

            if (pathpoint == pointA)
            {
                return null;
            } else
            {
                PathEntity path = this.CreateEntityPath(pointA, pathpoint);
                return path;
            }
        }

        private PathEntity CreateEntityPath(PathPoint start, PathPoint end)
        {
            int i = 1;

            for (PathPoint pathpoint = end; pathpoint.Previous != null; pathpoint = pathpoint.Previous)
            {
                ++i;
            }

            PathPoint[] pathpoint2 = new PathPoint[i];
            PathPoint pathpoint3 = end;
            --i;

            for (pathpoint2[i] = end; pathpoint3.Previous != null; pathpoint2[i] = pathpoint3)
            {
                pathpoint3 = pathpoint3.Previous;
                --i;
            }

            return new PathEntity(pathpoint2);
        }
    }
}
