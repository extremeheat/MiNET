using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    class PathPoint
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public int Index = -1;

        public float TotalPathDistance;

        public float DistanceToNext;
        public float DistanceToTarget;

        public PathPoint Previous;

        public bool Visited;
        public float DistanceFromOrigin;
        public float Cost;
        public float CostMalus;
        public NodeType CurrentNodeType = NodeType.Blocked;

        public PathPoint(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public PathPoint(int v) : this(v, v, v) { }

        /// <summary>
		/// Calculates the distance between two BlockCoordinates objects.
		/// </summary>
		public double DistanceTo(PathPoint other)
        {
            return Math.Sqrt(Square(other.X - X) +
                             Square(other.Y - Y) +
                             Square(other.Z - Z));
        }

        /// <summary>
		/// Calculates the Manhattan distance between two BlockCoordinates objects.
        /// https://xlinux.nist.gov/dads/HTML/manhattanDistance.html
		/// </summary>
        public int DistanceManhattan(PathPoint other)
        {
            int x = Math.Abs(other.X - this.X);
            int y = Math.Abs(other.Y - this.Y);
            int z = Math.Abs(other.Z - this.Z);
            return x + y + z;
        }

        /// <summary>
        /// Calculates the square of a num.
        /// </summary>
        private int Square(int num)
        {
            return num * num;
        }

        /// <summary>
        /// Finds the distance of this Coordinate3D from BlockCoordinates.Zero
        /// </summary>
        public double Distance
        {
            get { return DistanceTo(Zero); }
        }

        public static PathPoint Min(PathPoint value1, PathPoint value2)
        {
            return new PathPoint(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z)
                );
        }

        public static PathPoint Max(PathPoint value1, PathPoint value2)
        {
            return new PathPoint(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z)
                );
        }

        public static bool operator !=(PathPoint a, PathPoint b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(PathPoint a, PathPoint b)
        {
            return a.Equals(b);
        }

        public static PathPoint operator +(PathPoint a, PathPoint b)
        {
            return new PathPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static PathPoint operator -(PathPoint a, PathPoint b)
        {
            return new PathPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static PathPoint operator -(PathPoint a)
        {
            return new PathPoint(-a.X, -a.Y, -a.Z);
        }

        public static PathPoint operator *(PathPoint a, PathPoint b)
        {
            return new PathPoint(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static PathPoint operator /(PathPoint a, PathPoint b)
        {
            return new PathPoint(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static PathPoint operator %(PathPoint a, PathPoint b)
        {
            return new PathPoint(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
        }

        public static PathPoint operator +(PathPoint a, int b)
        {
            return new PathPoint(a.X + b, a.Y + b, a.Z + b);
        }

        public static PathPoint operator -(PathPoint a, int b)
        {
            return new PathPoint(a.X - b, a.Y - b, a.Z - b);
        }

        public static PathPoint operator *(PathPoint a, int b)
        {
            return new PathPoint(a.X * b, a.Y * b, a.Z * b);
        }

        public static PathPoint operator /(PathPoint a, int b)
        {
            return new PathPoint(a.X / b, a.Y / b, a.Z / b);
        }

        public static PathPoint operator %(PathPoint a, int b)
        {
            return new PathPoint(a.X % b, a.Y % b, a.Z % b);
        }

        public static PathPoint operator +(int a, PathPoint b)
        {
            return new PathPoint(a + b.X, a + b.Y, a + b.Z);
        }

        public static PathPoint operator -(int a, PathPoint b)
        {
            return new PathPoint(a - b.X, a - b.Y, a - b.Z);
        }

        public static PathPoint operator *(int a, PathPoint b)
        {
            return new PathPoint(a * b.X, a * b.Y, a * b.Z);
        }

        public static PathPoint operator /(int a, PathPoint b)
        {
            return new PathPoint(a / b.X, a / b.Y, a / b.Z);
        }

        public static PathPoint operator %(int a, PathPoint b)
        {
            return new PathPoint(a % b.X, a % b.Y, a % b.Z);
        }

        public static explicit operator PathPoint(Utils.ChunkCoordinates a)
        {
            return new PathPoint(a.X, 0, a.Z);
        }

        public static implicit operator PathPoint(Vector3 a)
        {
            return new PathPoint((int)Math.Floor(a.X), (int)Math.Floor(a.Y), (int)Math.Floor(a.Z));
        }

        public static explicit operator PathPoint(Utils.PlayerLocation a)
        {
            return new PathPoint((int)Math.Floor(a.X), (int)Math.Floor(a.Y), (int)Math.Floor(a.Z));
        }

        public static implicit operator Vector3(PathPoint a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }

        public bool Equals(PathPoint other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PathPoint && Equals((PathPoint)obj);
        }

        public bool IsAssigned()
        {
            return this.Index >= 0;
        }

        public static readonly PathPoint Zero = new PathPoint(0);
        public static readonly PathPoint One = new PathPoint(1);

        public static readonly PathPoint Up = new PathPoint(0, 1, 0);
        public static readonly PathPoint Down = new PathPoint(0, -1, 0);
        public static readonly PathPoint Left = new PathPoint(-1, 0, 0);
        public static readonly PathPoint Right = new PathPoint(1, 0, 0);
        public static readonly PathPoint Backwards = new PathPoint(0, 0, -1);
        public static readonly PathPoint Forwards = new PathPoint(0, 0, 1);

        public static readonly PathPoint East = new PathPoint(1, 0, 0);
        public static readonly PathPoint West = new PathPoint(-1, 0, 0);
        public static readonly PathPoint North = new PathPoint(0, 0, -1);
        public static readonly PathPoint South = new PathPoint(0, 0, 1);


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public static int GetHashCode(int X, int Y, int Z)
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}, Z={Z}";
        }
    }
}
