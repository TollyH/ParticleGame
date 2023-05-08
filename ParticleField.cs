namespace ParticleGame
{
    public class ParticleField
    {
        public ParticleData[,] ParticleData { get; private set; }
        public (byte, byte, byte)[,] ParticleColors { get; private set; }

        public ParticleField(int xSize, int ySize)
        {
            ParticleData = new ParticleData[xSize, ySize];
            ParticleColors = new (byte, byte, byte)[xSize, ySize];
        }

        public ParticleData this[int x, int y]
        {
            get => ParticleData[x, y];
            set
            {
                ParticleData[x, y] = value;
                ParticleColors[x, y] = ParticleTypes.Colors[value.ParticleType];
            }
        }

        public int GetLength(int dimension)
        {
            return ParticleData.GetLength(dimension);
        }
    }
}
