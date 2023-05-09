namespace ParticleGame
{
    public static class ParticleTypes
    {
        public enum Types
        {
            Air,
            Block,
            Water,
            Sand,
            RedSand,
            Lava,
            Steam,
            Magma
        }

        public static readonly Types[] ParticleTypeArray = new Types[]
        {
            Types.Air, Types.Block, Types.Sand,
            Types.RedSand, Types.Water, Types.Lava,
            Types.Steam, Types.Magma
        };

        public static readonly Dictionary<Types, (byte, byte, byte)> Colors = new()
        {
            {Types.Air, (0, 0, 0)},
            {Types.Block, (127, 127, 127)},
            {Types.Water, (0, 21, 255)},
            {Types.Sand, (252, 193, 53)},
            {Types.RedSand, (212, 115, 55)},
            {Types.Lava, (255, 32, 32)},
            {Types.Steam, (192, 192, 192)},
            {Types.Magma, (119, 0, 0)}
        };

        /// <summary>
        /// Contains particle types which will not automatically have their <see cref="ParticleData.Awake"/> attribute set to false.
        /// Add a particle type to this set if even non-moving particles of the type need to run their processor.
        /// </summary>
        public static readonly HashSet<Types> CannotSleep = new()
        {
            Types.Steam
        };

        /// <summary>
        /// Contains particle types which should, in most circumstances, not prevent other particles from moving through them.
        /// </summary>
        public static readonly HashSet<Types> Fluids = new()
        {
            Types.Air, Types.Water, Types.Lava, Types.Steam
        };
    }
}
