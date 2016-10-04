namespace MiNET
{
	public enum BlockFace
	{
		Down = 0,
		Up = 1,
		East = 2,
		West = 3,
		North = 4,
		South = 5,
		None = 255
	}

    public class BlockFaceUtil
    {
        public static int GetFrontOffsetX(BlockFace blockFace)
        {
            if (blockFace != BlockFace.West && blockFace != BlockFace.East)
                return 0;
            return GetOffset(blockFace);
        }

        public static int GetFrontOffsetY(BlockFace blockFace)
        {
            if (blockFace != BlockFace.Down && blockFace != BlockFace.Up)
                return 0;
            return GetOffset(blockFace);
        }

        public static int GetFrontOffsetZ(BlockFace blockFace)
        {
            if (blockFace != BlockFace.North && blockFace != BlockFace.South)
                return 0;
            return GetOffset(blockFace);
        }

        public static int GetOffset(BlockFace blockFace)
        {
            switch (blockFace)
            {
                case BlockFace.Down:
                case BlockFace.North:
                case BlockFace.West:
                    return -1;
                case BlockFace.Up:
                case BlockFace.South:
                case BlockFace.East:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}