using System.Drawing;

namespace ParticleGame
{
    public class ParticleData
    {
        public ParticleTypes.Types ParticleType { get; set; }
        public Point PreviousPosition { get; set; }
        public float Age { get; set; }
        public bool Awake { get; set; }

        public ParticleData(ParticleTypes.Types particleType, Point previousPosition)
        {
            ParticleType = particleType;
            PreviousPosition = previousPosition;
            Age = 0;
            Awake = true;
        }
    }
}
