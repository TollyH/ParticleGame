using System.Collections.Immutable;
using System.Drawing;

namespace ParticleGame
{
    public static class ParticleInteractions
    {
        public delegate void Interaction(Point pos1, Point pos2, ParticleData[,] field);

        public static void LavaWaterInteraction(Point pos1, Point pos2, ParticleData[,] field)
        {
            field[pos1.X, pos1.Y].ParticleType = ParticleTypes.Types.Air;
            field[pos2.X, pos2.Y].ParticleType = ParticleTypes.Types.Steam;
            field[pos2.X, pos2.Y].Age = 0;
        }

        public static void WaterMagmaInteraction(Point pos1, Point pos2, ParticleData[,] field)
        {
            field[pos1.X, pos1.Y].ParticleType = ParticleTypes.Types.Steam;
            field[pos1.X, pos1.Y].Age = 0;
        }

        public static readonly ImmutableDictionary<(ParticleTypes.Types, ParticleTypes.Types), Interaction> Interactions =
            new Dictionary<(ParticleTypes.Types, ParticleTypes.Types), Interaction>()
            {
                { (ParticleTypes.Types.Lava, ParticleTypes.Types.Water), new(LavaWaterInteraction) },
                { (ParticleTypes.Types.Water, ParticleTypes.Types.Magma), new(WaterMagmaInteraction) }
            }.ToImmutableDictionary();
    }
}
