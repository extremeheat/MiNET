using MiNET.Utils;
using MiNET.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiNET.Entities.AI
{
    public abstract class PathNavigator
    {
        public LivingEntity entity { get; protected set; }
        protected Level level;
        
        protected PathEntity currentPath;
        protected float speed;

        /// <summary>
        ///  <h1>MCP</h1>
        ///  The number of blocks (extra) +/- in each axis that get pulled out as 
        ///  cache for the pathfinder's search space
        /// </summary>
        //private unknown pathSearchRange;

        /// <summary>
        ///  <h1>MCP</h1>
        ///  Time, in number of ticks, following the current path
        /// </summary>
        private int totalTicks;
        private int ticksAtLastPos;

        private PlayerLocation lastCheckedPos;
        private Vector3 timeoutCachedNode = new Vector3();
        private long timeoutTimer;
        private long lastTimeoutCheck;
        private double timeoutLimit;
        private float maxDistanceToWaypoint = 0.5F;
        private bool tryUpdatePath;
        private long lastTimeUpdated;
        public Node AINode { get; protected set; }
        private BlockCoordinates targetPos;
        private readonly PathFinder pathFinder;

        public PathNavigator(LivingEntity entity, Level level)
        {
            this.entity = entity;
            this.level = level;
            //this.pathSearchRange = entity.GetFollowRange();
            this.pathFinder = this.CreatePathFinder();
        }

        abstract protected PathFinder CreatePathFinder();

        abstract public bool CanNativate();

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public float GetPathSearchRange()
        {
            return 20;
        }

        public bool CanUpdatePathOnTimeout()
        {
            return this.tryUpdatePath;
        }

        public void UpdatePath()
        {
            if (this.level.TickTime - this.lastTimeoutCheck > 20)
            {
                if (this.targetPos != null)
                {
                    this.currentPath = this.GetPathTo(this.targetPos);
                    this.lastTimeUpdated = this.level.TickTime;
                    this.tryUpdatePath = false;
                }
            } else
            {
                this.tryUpdatePath = true;
            }
        }

        public PathEntity GetPathTo(Vector3 location)
        {
            return this.GetPathTo(new BlockCoordinates((int)location.X, (int)location.Y, (int)location.Z));
        }

        public virtual PathEntity GetPathTo(BlockCoordinates location)
        {
            if (!this.CanNativate())
            {
                return null;
            } else if (this.currentPath != null && !this.currentPath.IsFinished() && location.Equals(this.targetPos))
            {
                return this.currentPath;
            } else
            {
                this.targetPos = location;
                float searchRange = this.GetPathSearchRange();

                BlockCoordinates blockCoords = new BlockCoordinates(this.entity.KnownPosition);
                //int var4 = (int)(searchRange + 8);
                //PathEntity var6 = this.pathFinder?.
                PathEntity path = this.pathFinder.FindPath(this.level, this.entity, this.targetPos, searchRange);
                return path;
            }
        }

        public virtual PathEntity GetPathToEntity(Entity entity)
        {
            if (!this.CanNativate())
            {
                return null;
            } else
            {
                BlockCoordinates bc = new BlockCoordinates(entity.KnownPosition);

                if (this.currentPath != null && this.currentPath.IsFinished() && bc.Equals(this.targetPos))
                {
                    return this.currentPath;
                } else
                {
                    this.targetPos = bc;
                    float searchRange = this.GetPathSearchRange();
                    BlockCoordinates blockCoords = new BlockCoordinates(this.entity.KnownPosition) + BlockCoordinates.Up;
                    PathEntity path = this.pathFinder.FindPath(this.level, this.entity, entity, searchRange);
                    return path;
                }
            }
        }

        public virtual bool TryToMoveTo(float x, float y, float z, float speed)
        {
            return this.SetPath(this.GetPathTo(new Vector3(x, y, z)), speed);
        }

        public virtual bool TryMoveToEntity(Entity entity, float speed)
        {
            PathEntity path = this.GetPathToEntity(entity);
            return path != null && this.SetPath(path, speed);
        }

        public bool SetPath(PathEntity path, float speed)
        {
            if (path == null)
            {
                this.currentPath = null;
                return false;
            } else
            {
                if (!path.IsSamePath(this.currentPath))
                {
                    this.currentPath = path;
                }

                this.RemoveSunnyPath();

                if (this.currentPath.PathLength == 0)
                {
                    return false;
                } else
                {
                    this.speed = speed;
                    PlayerLocation epos = this.GetEntityPosition();
                    this.ticksAtLastPos = this.totalTicks;
                    this.lastCheckedPos = epos;
                    return true;
                }
            }
        }

        public PathEntity GetPath()
        {
            return this.currentPath;
        }

        public virtual void OnNavigationTick()
        {
            ++this.totalTicks;

            if (this.tryUpdatePath)
            {
                this.UpdatePath();
            }

            if (this.HasPath())
            {
                if (this.CanNavigate())
                {
                    this.FollowPath();
                }
                else if (this.currentPath != null && this.currentPath.CurrentPath < this.currentPath.PathLength)
                {
                    PlayerLocation epos = this.GetEntityPosition();
                    Vector3 pathVector = this.currentPath.GetVectorFromPoint(this.entity, this.currentPath.CurrentPath);

                    if (epos.Y > pathVector.Y && !this.entity.IsOnGround() && (int)Math.Floor(epos.X) == (int)Math.Floor(pathVector.X))
                    {
                        this.currentPath.CurrentPath = this.currentPath.CurrentPath + 1;
                    }
                }

                if (this.HasPath())
                {
                    Vector3 epos = this.currentPath.GetPosition(this.entity);

                    if (epos != null)
                    {
                        BlockCoordinates bc = new BlockCoordinates(epos) + BlockCoordinates.Down;
                        BoundingBox bb = this.level.GetBlock(bc).GetBoundingBox();
                        Vector3 v3 = epos - new Vector3(0, 1 - bb.Max.Y, 0);
                        this.entity.MoveManager.MoveTo(v3.X, v3.Y, v3.Z, this.speed);
                    }
                }
            }
        }

        protected virtual void FollowPath()
        {
            PlayerLocation epos = this.GetEntityPosition();
            int pathLength = this.currentPath.PathLength;

            for (int pathIndex = this.currentPath.CurrentPath; pathIndex < this.currentPath.PathLength; ++pathIndex)
            {
                if (this.currentPath.Points[pathIndex].Y != (int)Math.Floor(epos.Y))
                {
                    pathLength = pathIndex;
                    break;
                }
            }

            this.maxDistanceToWaypoint = this.entity.Width > 0.75f ? (float)this.entity.Width / 2 : 0.75f - (float)this.entity.Width / 2f;

            Vector3 rpos = this.currentPath.GetCurrentPosition();

            if (Math.Abs(this.entity.KnownPosition.X - (rpos.X + 0.5f)) < this.maxDistanceToWaypoint 
                && Math.Abs(this.entity.KnownPosition.Z - (rpos.Z + 0.5f)) < this.maxDistanceToWaypoint && Math.Abs(this.entity.KnownPosition.Y - rpos.Y) < 1)
            {
                this.currentPath.CurrentPath = this.currentPath.CurrentPath + 1;
            }

            int width = (int)Math.Ceiling(this.entity.Width);
            int height = (int)Math.Ceiling(this.entity.Height);
            
            for (int pointIndex = pathLength - 1; pointIndex >= this.currentPath.CurrentPath; --pointIndex)
            {
                if (this.IsDirectPathBetweenPoints(epos.ToVector3(), this.currentPath.GetVectorFromPoint(this.entity, pointIndex), width, height, width))
                {
                    this.currentPath.CurrentPath = pointIndex;
                    break;
                }
            }

            this.CheckIfStuck(epos);
        }

        protected void CheckIfStuck(PlayerLocation pos)
        {
            if (this.totalTicks - this.ticksAtLastPos > 100)
            {
                if (pos.DistanceTo(this.lastCheckedPos) < 2.25f)
                {
                    this.ClearPathEntity();
                }


                this.ticksAtLastPos = this.totalTicks;
                this.lastCheckedPos = pos;

                if (this.currentPath != null && !this.currentPath.IsFinished())
                {
                    Vector3 rpos = this.currentPath.GetCurrentPosition();

                    if (rpos.Equals(this.timeoutCachedNode))
                    {
                        this.timeoutLimit += (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - this.lastTimeoutCheck;
                    } else
                    {
                        this.timeoutCachedNode = rpos;
                        double dist = pos.DistanceTo(new PlayerLocation(this.timeoutCachedNode));
                        this.timeoutLimit = this.entity.MovementSpeed > 0 ? dist / this.entity.MovementSpeed * 1000 : 0;
                    }

                    if (this.timeoutLimit> 0 && this.timeoutTimer > this.timeoutLimit * 3)
                    {
                        this.timeoutCachedNode = new Vector3();
                        this.timeoutTimer = 0;
                        this.timeoutLimit = 0;
                        this.ClearPathEntity();
                    }

                    this.lastTimeoutCheck = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
            }
        }

        public bool HasPath()
        {
            return this.currentPath != null || !this.currentPath.IsFinished();
        }

        public void ClearPathEntity()
        {
            this.currentPath = null;
        }

        protected abstract PlayerLocation GetEntityPosition();

        protected abstract bool CanNavigate();

        protected virtual void RemoveSunnyPath() { }

        protected abstract bool IsDirectPathBetweenPoints(Vector3 pos1, Vector3 pos2, int sizeX, int sizeY, int sizeZ);

        public bool CanEntityStandOn(BlockCoordinates bc)
        {
            return this.level.GetBlock(bc).IsSolid;
        }
    }
}
