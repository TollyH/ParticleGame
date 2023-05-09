using System.Drawing;

namespace ParticleGame
{
    public static class ParticleInteractions
    {
        public delegate void Interaction(Point pos1, Point pos2, ParticleField field);

        public static void LavaWaterInteraction(Point pos1, Point pos2, ParticleField field)
        {
            field[pos1.X, pos1.Y].ParticleType = ParticleTypes.Types.Air;
            field[pos2.X, pos2.Y].ParticleType = ParticleTypes.Types.Steam;
            field.UpdateColor(pos1.X, pos1.Y);
            field.UpdateColor(pos2.X, pos2.Y);
            field[pos2.X, pos2.Y].Age = 0;
        }

        public static void WaterMagmaInteraction(Point pos1, Point pos2, ParticleField field)
        {
            field[pos1.X, pos1.Y].ParticleType = ParticleTypes.Types.Steam;
            field.UpdateColor(pos1.X, pos1.Y);
            field[pos1.X, pos1.Y].Age = 0;
        }

        public static readonly Dictionary<(ParticleTypes.Types, ParticleTypes.Types), Interaction> Interactions = new()
        {
            { (ParticleTypes.Types.Lava, ParticleTypes.Types.Water), new(LavaWaterInteraction) },
            { (ParticleTypes.Types.Water, ParticleTypes.Types.Magma), new(WaterMagmaInteraction) }
        };
    }
}
