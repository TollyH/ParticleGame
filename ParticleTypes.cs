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
            Types.Air, Types.Block, Types.Water,
            Types.Sand, Types.RedSand, Types.Lava,
            Types.Steam, Types.Magma
        };
    }
}
