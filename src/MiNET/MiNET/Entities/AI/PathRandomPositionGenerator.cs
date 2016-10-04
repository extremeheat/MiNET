using MiNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathRandomPositionGenerator
    {
        public static BlockCoordinates? FindRandomTarget(LivingEntity entity, int xz, int y)
        {
            return FindRandomTargetBlock(entity, xz, y, null);
        }

        public static BlockCoordinates? FindRandomTargetBlock(LivingEntity entity, int xz, int y, BlockCoordinates? targetCoord)
        {
            PathNavigator navigator = entity.GetNavigator();

            Random random = new Random();

            bool found = false;
            bool boolean2;
            int X = 0;
            int Y = 0;
            int Z = 0;

            if (entity.HasHome())
            {
                double dist = entity.HomeLocation.DistanceToSquared(new BlockCoordinates(entity.KnownPosition));
                double dist2 = entity.DistanceFromHome;
                boolean2 = dist < dist2 * dist2;
            } else
            {
                boolean2 = false;
            }

            for (int i = 0; i < 10; ++i)
            {
                int x2 = random.Next(2 * xz + 1) - xz;
                int y2 = random.Next(2 * y + 1) - y;
                int z2 = random.Next(2 * xz + 1) - xz;

                if (targetCoord == null || x2 * targetCoord?.X + x2 * targetCoord?.Z >= 0)
                {
                    if (entity.HasHome() && xz > 1)
                    {
                        if (entity.KnownPosition.X > entity.HomeLocation.X)
                        {
                            x2 -= random.Next(xz / 2);
                        } else
                        {
                            x2 += random.Next(xz / 2);
                        }

                        if (entity.KnownPosition.Z > entity.HomeLocation.Z)
                        {
                            x2 -= random.Next(xz / 2);
                        } else
                        {
                            x2 += random.Next(xz / 2);
                        }
                    }

                    BlockCoordinates blockCoords = new BlockCoordinates(new PlayerLocation(x2 + entity.KnownPosition.X, y2 + entity.KnownPosition.Y, z2 + entity.KnownPosition.Z));

                    if (!boolean2 || entity.IsHomeNearby(blockCoords) && navigator.CanEntityStandOn(blockCoords))
                    {
                        float pathWeight = entity.GetPathWeight(blockCoords);

                        if (pathWeight > -99999)
                        {
                            X = x2;
                            Y = y2;
                            Z = z2;
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                return new BlockCoordinates(new PlayerLocation(X + entity.KnownPosition.X, Y + entity.KnownPosition.Y, Z + entity.KnownPosition.Z));
            } else
            {
                return null;
            }
        }
    }
}
