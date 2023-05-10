using SDL2;

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
            Magma,
            Ash
        }

        public static readonly Types[] ParticleTypeArray = new Types[]
        {
            Types.Block, Types.Sand,
            Types.RedSand, Types.Water, Types.Lava,
            Types.Steam, Types.Magma, Types.Ash
        };

        public static readonly Dictionary<Types, SDL.SDL_Color> Colors = new()
        {
            { Types.Air, new() { r = 0, g = 0, b = 0, a = 255 } },
            { Types.Block, new() { r = 127, g = 127, b = 127, a = 255 } },
            { Types.Water, new() { r = 0, g = 21, b = 255, a = 255 } },
            { Types.Sand, new() { r = 252, g = 193, b = 53, a = 255 } },
            { Types.RedSand, new() { r = 212, g = 115, b = 55, a = 255 } },
            { Types.Lava, new() { r = 255, g = 32, b = 32, a = 255 } },
            { Types.Steam, new() { r = 240, g = 240, b = 240, a = 255 } },
            { Types.Magma, new() { r = 119, g = 0, b = 0, a = 255 } },
            { Types.Ash, new() { r = 176, g = 176, b = 176, a = 255 } },
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

        public static readonly Dictionary<Types, string> FriendlyNames = new()
        {
            { Types.Air, "Air" },
            { Types.Block, "Block" },
            { Types.Water, "Water" },
            { Types.Sand, "Sand" },
            { Types.RedSand, "Red Sand" },
            { Types.Lava, "Lava" },
            { Types.Steam, "Steam" },
            { Types.Magma, "Magma Block" },
            { Types.Ash, "Ash" }
        };
    }
}
