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
            WaterPowered,
            Sand,
            RedSand,
            Lava,
            Steam,
            Magma,
            Ash,
            Wire,
            WirePowered,
            Battery,
            Light,
            LightPowered,
            Tap,
            TapPowered,
            Inverter,
            InverterPowered,
            Copper,
            CopperPowered,
            Steel,
            SteelPowered,
        }

        public static readonly Types[] ParticleTypeArray = new Types[]
        {
            Types.Block, Types.Sand,
            Types.RedSand, Types.Water, Types.Lava,
            Types.Steam, Types.Magma, Types.Ash,
            Types.Wire, Types.Battery, Types.Light,
            Types.Tap, Types.Inverter, Types.Copper,
            Types.Steel
        };

        public static readonly Dictionary<Types, SDL.SDL_Color> Colors = new()
        {
            { Types.Air, new() { r = 0, g = 0, b = 0, a = 255 } },
            { Types.Block, new() { r = 127, g = 127, b = 127, a = 255 } },
            { Types.Water, new() { r = 0, g = 21, b = 255, a = 255 } },
            { Types.WaterPowered, new() { r = 28, g = 138, b = 255, a = 255 } },
            { Types.Sand, new() { r = 252, g = 193, b = 53, a = 255 } },
            { Types.RedSand, new() { r = 212, g = 115, b = 55, a = 255 } },
            { Types.Lava, new() { r = 255, g = 32, b = 32, a = 255 } },
            { Types.Steam, new() { r = 240, g = 240, b = 240, a = 255 } },
            { Types.Magma, new() { r = 119, g = 0, b = 0, a = 255 } },
            { Types.Ash, new() { r = 176, g = 176, b = 176, a = 255 } },
            { Types.Wire, new() { r = 166, g = 25, b = 15, a = 255 } },
            { Types.WirePowered, new() { r = 250, g = 63, b = 50, a = 255 } },
            { Types.Battery, new() { r = 77, g = 112, b = 57, a = 255 } },
            { Types.Light, new() { r = 120, g = 120, b = 101, a = 255 } },
            { Types.LightPowered, new() { r = 242, g = 239, b = 44, a = 255 } },
            { Types.Tap, new() { r = 88, g = 88, b = 112, a = 255 } },
            { Types.TapPowered, new() { r = 88, g = 88, b = 112, a = 255 } },
            { Types.Inverter, new() { r = 176, g = 130, b = 245, a = 255 } },
            { Types.InverterPowered, new() { r = 80, g = 60, b = 110, a = 255 } },
            { Types.Copper, new() { r = 172, g = 79, b = 0, a = 255 } },
            { Types.CopperPowered, new() { r = 234, g = 135, b = 30, a = 255 } },
            { Types.Steel, new() { r = 137, g = 139, b = 143, a = 255 } },
            { Types.SteelPowered, new() { r = 193, g = 196, b = 201, a = 255 } },
        };

        /// <summary>
        /// Contains particle types which will not automatically have their <see cref="ParticleData.Awake"/> attribute set to false.
        /// Add a particle type to this set if even non-moving particles of the type need to run their processor.
        /// </summary>
        public static readonly HashSet<Types> CannotSleep = new()
        {
            Types.Steam, Types.TapPowered
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
            { Types.Ash, "Ash" },
            { Types.Wire, "Wire" },
            { Types.Battery, "Battery" },
            { Types.Light, "Light" },
            { Types.Tap, "Tap" },
            { Types.Inverter, "Inverter" },
            { Types.Copper, "Copper" },
            { Types.Steel, "Steel" },
        };

        /// <summary>
        /// Contains particle types which emit power to adjacent particles.
        /// </summary>
        public static readonly HashSet<Types> EmitsPower = new()
        {
            Types.Battery, Types.Inverter
        };

        /// <summary>
        /// Whether electrical power should spread through particles of this type.
        /// Note: power always spreads through particles of the same type.
        /// </summary>
        public static readonly HashSet<Types> ConductsPower = new()
        {
            Types.Water, Types.WaterPowered, Types.Wire, Types.WirePowered,
            Types.Copper, Types.CopperPowered, Types.Steel, Types.SteelPowered
        };

        /// <summary>
        /// Whether only un-conditional electrical power should spread through particles of this type.
        /// Contained types should also be in the <see cref="ConductsPower"/> set.
        /// Note: power always spreads through particles of the same type.
        /// </summary>
        public static readonly HashSet<Types> ConductsUnconditionalPower = new()
        {
            Types.Copper, Types.CopperPowered
        };

        /// <summary>
        /// Whether this type should not conduct power to emitters.
        /// Contained types should also be in the <see cref="ConductsPower"/> set.
        /// </summary>
        public static readonly HashSet<Types> WillNotPowerEmitters = new()
        {
            Types.Steel, Types.SteelPowered
        };

        /// <summary>
        /// Maps unpowered particle types to their powered counterparts (if applicable).
        /// </summary>
        public static readonly Dictionary<Types, Types> PoweredStates = new()
        {
            { Types.Water, Types.WaterPowered },
            { Types.Wire, Types.WirePowered },
            { Types.Light, Types.LightPowered },
            { Types.Tap, Types.TapPowered },
            { Types.Inverter, Types.InverterPowered },
            { Types.Copper, Types.CopperPowered },
            { Types.Steel, Types.SteelPowered },
        };

        /// <summary>
        /// Maps powered particle types to their unpowered counterparts (if applicable).
        /// </summary>
        public static readonly Dictionary<Types, Types> UnpoweredStates = new()
        {
            { Types.WaterPowered, Types.Water },
            { Types.WirePowered, Types.Wire },
            { Types.LightPowered, Types.Light },
            { Types.TapPowered, Types.Tap },
            { Types.InverterPowered, Types.Inverter },
            { Types.CopperPowered, Types.Copper },
            { Types.SteelPowered, Types.Steel },
        };

        /// <summary>
        /// Stores power emitters that only emit when unpowered.
        /// </summary>
        public static readonly HashSet<Types> EmitsWhenUnpowered = EmitsPower.Where(
            x => PoweredStates.ContainsKey(x) && !EmitsPower.Contains(PoweredStates[x])).ToHashSet();
    }
}
