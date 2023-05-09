using SDL2;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ParticleGame
{
    public static class ParticleGame
    {
        public static readonly Random RNG = new();

        private static readonly Point[] Adjacent = new Point[4] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        private static readonly ulong performanceFrequency = SDL.SDL_GetPerformanceFrequency();

        public static int Mod(int a, int b)
        {
            // Needed because C#'s % operator finds the remainder, not modulo
            return ((a % b) + b) % b;
        }

        public static void StartGame()
        {
            _ = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            _ = SDL_ttf.TTF_Init();

            _ = SDL.SDL_SetHintWithPriority(SDL.SDL_HINT_RENDER_DRIVER, "direct3d11", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
            IntPtr window = SDL.SDL_CreateWindow("Particle Game", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 500, 500, 0);
            IntPtr screen = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            IntPtr renderTexture = SDL.SDL_CreateTexture(screen, SDL.SDL_PIXELFORMAT_ARGB8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 500, 500);
            IntPtr pixels = Marshal.AllocHGlobal(500 * 500 * 4);

            ParticleField particleField = new(500, 500);

            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    particleField[x, y] = new ParticleData(ParticleTypes.Types.Air, new Point(x, y));
                }
            }

            int currentParticleIndex = 1;
            int brushSize = 5;

            Point previousMousePos = new(-1, -1);

            ulong renderStart = 0;
            ulong renderEnd = 0;
            float frameTime;

            bool quit = false;

            bool mouseLeftDown = false;
            bool mouseRightDown = false;

            // Game loop
            while (!quit)
            {
                frameTime = (renderEnd - renderStart) / (float)performanceFrequency;
                renderStart = SDL.SDL_GetPerformanceCounter();
                ParticleTypes.Types currentParticleType = ParticleTypes.ParticleTypeArray[currentParticleIndex];
                _ = SDL.SDL_RenderClear(screen);

                while (SDL.SDL_PollEvent(out SDL.SDL_Event evn) != 0)
                {
                    if (evn.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                        break;
                    }
                    else if (evn.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                    {
                        _ = SDL.SDL_GetMouseState(out int x, out int y);
                        Point mousePos = new(x, y);
                        if (evn.button.button == SDL.SDL_BUTTON_LEFT)
                        {
                            mouseLeftDown = true;
                            FieldOperations.BrushDraw(particleField, mousePos, currentParticleType, brushSize);
                        }
                        else if (evn.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            mouseRightDown = true;
                            FieldOperations.BrushDraw(particleField, mousePos, ParticleTypes.Types.Air, brushSize);
                        }
                        else
                        {
                            continue;
                        }
                        previousMousePos = mousePos;
                    }
                    else if (evn.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
                    {
                        previousMousePos = new Point(-1, -1);
                        if (evn.button.button == SDL.SDL_BUTTON_LEFT)
                        {
                            mouseLeftDown = false;
                        }
                        else if (evn.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            mouseRightDown = false;
                        }
                    }
                    else if (evn.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                    {
                        if (previousMousePos == new Point(-1, -1))
                        {
                            continue;
                        }
                        _ = SDL.SDL_GetMouseState(out int x, out int y);
                        Point mousePos = new(x, y);
                        if (mouseLeftDown)
                        {
                            FieldOperations.BrushLine(particleField, previousMousePos, mousePos, currentParticleType, brushSize);
                        }
                        else if (mouseRightDown)
                        {
                            FieldOperations.BrushLine(particleField, previousMousePos, mousePos, ParticleTypes.Types.Air, brushSize);
                        }
                        else
                        {
                            continue;
                        }
                        previousMousePos = mousePos;
                    }
                    else if (evn.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        if (evn.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFTBRACKET)
                        {
                            currentParticleIndex = Mod(currentParticleIndex - 1, ParticleTypes.ParticleTypeArray.Length);
                        }
                        else if (evn.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHTBRACKET)
                        {
                            currentParticleIndex = Mod(currentParticleIndex + 1, ParticleTypes.ParticleTypeArray.Length);
                        }
                    }
                }

                int particles = 0;
                int awakeParticles = 0;
                HashSet<Point> queue = new(500 * 500);
                List<Point> points = new(500 * 500);
                for (int x = 0; x < 500; x++)
                {
                    for (int y = 0; y < 500; y++)
                    {
                        ParticleData data = particleField[x, y];
                        if (ParticleProcessors.Processors.ContainsKey(data.ParticleType))
                        {
                            particles++;
                            if (data.Awake)
                            {
                                Point position = new(x, y);
                                _ = queue.Add(position);
                                points.Add(position);
                                awakeParticles++;
                            }
                        }
                    }
                }
                // Iterating particles randomly provides more variation in physics
                points.Sort((x, y) => RNG.Next());

                while (queue.Count > 0)
                {
                    Point position = points[^1];
                    points.RemoveAt(points.Count - 1);
                    if (!queue.Remove(position))
                    {
                        continue;
                    }
                    int x = position.X;
                    int y = position.Y;
                    ParticleData data = particleField[x, y];
                    if (data.ParticleType == ParticleTypes.Types.Air)
                    {
                        continue;
                    }

                    Point newPos = ParticleProcessors.Processors[data.ParticleType](position, particleField, data);
                    if (newPos == new Point(-1, -1))
                    {
                        // Destroy particle
                        particleField[x, y] = new ParticleData(ParticleTypes.Types.Air, position);
                    }
                    else
                    {
                        // If not destroying particle, move to new location
                        particleField[x, y] = particleField[newPos.X, newPos.Y];
                        if (queue.Remove(newPos))
                        {
                            _ = queue.Add(position);
                            points.Add(position);
                        }
                        particleField[newPos.X, newPos.Y] = data;
                        bool allSameAdjacent = true;
                        // Check particles for interactions and to see whether to fall asleep
                        foreach (Point adj in Adjacent)
                        {
                            Point otherPos = new(newPos.X + adj.X, newPos.Y + adj.Y);
                            if (otherPos.X >= 0 && otherPos.Y >= 0
                                && otherPos.X < 500 && otherPos.Y < 500)
                            {
                                ParticleData otherData = particleField[otherPos.X, otherPos.Y];
                                ParticleTypes.Types otherType = otherData.ParticleType;
                                if (otherType != data.ParticleType)
                                {
                                    allSameAdjacent = false;
                                }
                                // Only run if particle moved
                                if (newPos != position)
                                {
                                    // Relative to this particle's original position
                                    // If moving toward a particle of the same type, make sure it is awake
                                    otherData.Awake = true;
                                    if (ParticleInteractions.Interactions.ContainsKey((data.ParticleType, otherType)))
                                    {
                                        ParticleInteractions.Interactions[(data.ParticleType, otherType)](newPos, otherPos, particleField);
                                    }
                                    else if (ParticleInteractions.Interactions.ContainsKey((otherType, data.ParticleType)))
                                    {
                                        ParticleInteractions.Interactions[(otherType, data.ParticleType)](otherPos, newPos, particleField);
                                    }
                                    if (particleField[newPos.X, newPos.Y].ParticleType == ParticleTypes.Types.Air)
                                    {
                                        // Interaction deleted current particle
                                        break;
                                    }
                                }
                            }
                            // Only run if particle moved
                            if (newPos != position)
                            {
                                Point originalAdjacentPos = new(position.X + adj.X, position.Y + adj.Y);
                                if (originalAdjacentPos.X >= 0 && originalAdjacentPos.Y >= 0
                                    && originalAdjacentPos.X < 500 && originalAdjacentPos.Y < 500)
                                {
                                    ParticleData originalAdjacentData = particleField[originalAdjacentPos.X, originalAdjacentPos.Y];
                                    // Relative to this particle's original position
                                    // If moving away from a particle of the same type, make sure it is awake
                                    originalAdjacentData.Awake = true;
                                }
                            }
                        }
                        if (allSameAdjacent)
                        {
                            // Particles surrounded by particles of the same type don't need to run physics
                            data.Awake = false;
                        }
                        data.PreviousPosition = position;
                    }
                    data.Age += frameTime;
                }

                _ = SDL.SDL_LockTexture(renderTexture, IntPtr.Zero, out pixels, out int pitch);
                for (int x = 0; x < 500; x++)
                {
                    for (int y = 0; y < 500; y++)
                    {
                        int offset = (pitch * y) + (x * 4);
                        (byte, byte, byte) color = particleField.ParticleColors[x, y];
                        Marshal.WriteByte(pixels + offset, color.Item3);
                        Marshal.WriteByte(pixels + offset + 1, color.Item2);
                        Marshal.WriteByte(pixels + offset + 2, color.Item1);
                        Marshal.WriteByte(pixels + offset + 3, 255);
                    }
                }

                SDL.SDL_UnlockTexture(renderTexture);
                _ = SDL.SDL_RenderCopy(screen, renderTexture, IntPtr.Zero, IntPtr.Zero);
                SDL.SDL_RenderPresent(screen);

                Console.Write($"\r{1 / frameTime:000.00} FPS  Awake Particles: {awakeParticles:000000}  Total Particles: {particles:000000}");
                Console.Out.Flush();

                while ((float)performanceFrequency / (SDL.SDL_GetPerformanceCounter() - renderStart) > 60)
                {
                    // Cap at 60fps
                    SDL.SDL_Delay(1);
                }

                renderEnd = SDL.SDL_GetPerformanceCounter();
            }

            Marshal.FreeHGlobal(pixels);
            SDL.SDL_DestroyTexture(renderTexture);
            SDL.SDL_DestroyRenderer(screen);
            SDL.SDL_DestroyWindow(window);
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }
    }
}
