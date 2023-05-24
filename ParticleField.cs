using SDL2;

namespace ParticleGame
{
    public class ParticleField : System.Collections.IEnumerable
    {
        public ParticleData[,] ParticleData { get; private set; }
        public SDL.SDL_Color[,] ParticleColors { get; private set; }
        public (DateTime Time, ParticleTypes.Types ParticleType) LastChange { get; private set; }

        public ParticleField(int xSize, int ySize)
        {
            ParticleData = new ParticleData[xSize, ySize];
            ParticleColors = new SDL.SDL_Color[xSize, ySize];
            LastChange = (DateTime.Now, ParticleTypes.Types.Air);
        }

        public ParticleData this[int x, int y]
        {
            get => ParticleData[x, y];
            set
            {
                ParticleData[x, y] = value;
                ParticleColors[x, y] = ParticleTypes.Colors[value.ParticleType];
                LastChange = (DateTime.Now, value.ParticleType);
            }
        }

        public int GetLength(int dimension)
        {
            return ParticleData.GetLength(dimension);
        }

        public void UpdatePoint(int x, int y)
        {
            ParticleTypes.Types type = ParticleData[x, y].ParticleType;
            ParticleColors[x, y] = ParticleTypes.Colors[type];
            LastChange = (DateTime.Now, type);
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return ParticleData.GetEnumerator();
        }
    }
}
