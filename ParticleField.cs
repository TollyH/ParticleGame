using SDL2;

namespace ParticleGame
{
    public class ParticleField : System.Collections.IEnumerable
    {
        public ParticleData[,] ParticleData { get; private set; }
        public SDL.SDL_Color[,] ParticleColors { get; private set; }

        public ParticleField(int xSize, int ySize)
        {
            ParticleData = new ParticleData[xSize, ySize];
            ParticleColors = new SDL.SDL_Color[xSize, ySize];
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

        public void UpdateColor(int x, int y)
        {
            ParticleColors[x, y] = ParticleTypes.Colors[ParticleData[x, y].ParticleType];
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return ParticleData.GetEnumerator();
        }
    }
}
