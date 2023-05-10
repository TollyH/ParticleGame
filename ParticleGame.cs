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

        /// <summary>
        /// Calls SDL_RenderCopy with a calculated width and height for the provided texture.
        /// </summary>
        public static int DrawTextureAtPosition(IntPtr renderer, IntPtr texture, Point position)
        {
            _ = SDL.SDL_QueryTexture(texture, out _, out _, out int w, out int h);
            SDL.SDL_Rect textureRect = new() { x = position.X, y = position.Y, w = w, h = h };
            return SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref textureRect);
        }

        public static void StartGame()
        {
            _ = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            _ = SDL_ttf.TTF_Init();

            IntPtr fontLarge = SDL_ttf.TTF_OpenFont(@"C:\Windows\Fonts\tahomabd.ttf", 22);
            IntPtr fontSmall = SDL_ttf.TTF_OpenFont(@"C:\Windows\Fonts\tahomabd.ttf", 16);

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

            bool blockReplacement = false;
            bool physics = true;

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
                            FieldOperations.BrushDraw(particleField, mousePos, currentParticleType, brushSize,
                                blockReplacement ? null : ParticleTypes.Types.Air);
                        }
                        else if (evn.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            mouseRightDown = true;
                            FieldOperations.BrushDraw(particleField, mousePos, ParticleTypes.Types.Air, brushSize,
                                blockReplacement ? null : currentParticleType);
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
                            FieldOperations.BrushLine(particleField, previousMousePos, mousePos, currentParticleType, brushSize,
                                blockReplacement ? null : ParticleTypes.Types.Air);
                        }
                        else if (mouseRightDown)
                        {
                            FieldOperations.BrushLine(particleField, previousMousePos, mousePos, ParticleTypes.Types.Air, brushSize,
                                blockReplacement ? null : currentParticleType);
                        }
                        else
                        {
                            continue;
                        }
                        previousMousePos = mousePos;
                    }
                    else if (evn.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        // Subtracting 1 from array length and adding 1 to mod results prevents 0 (Air) from being selected
                        switch (evn.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_LEFTBRACKET:
                                currentParticleIndex--;
                                if (currentParticleIndex <= 0)
                                {
                                    currentParticleIndex = ParticleTypes.ParticleTypeArray.Length - 1;
                                }
                                break;
                            case SDL.SDL_Keycode.SDLK_RIGHTBRACKET:
                                currentParticleIndex++;
                                if (currentParticleIndex >= ParticleTypes.ParticleTypeArray.Length)
                                {
                                    currentParticleIndex = 1;
                                }
                                break;
                            case SDL.SDL_Keycode.SDLK_TAB:
                                blockReplacement = !blockReplacement;
                                break;
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                physics = !physics;
                                break;
                            case SDL.SDL_Keycode.SDLK_EQUALS:
                                brushSize++;
                                break;
                            case SDL.SDL_Keycode.SDLK_MINUS:
                                brushSize--;
                                if (brushSize < 1)
                                {
                                    brushSize = 1;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                int particles = 0;
                int awakeParticles = 0;
                if (physics)
                {
                    HashSet<Point> queue = new(500 * 500);
                    List<Point> points = new(500 * 500);
                    for (int x = 0; x < 500; x++)
                    {
                        for (int y = 0; y < 500; y++)
                        {
                            ParticleData data = particleField[x, y];
                            if (data.ParticleType != ParticleTypes.Types.Air && ParticleProcessors.Processors.ContainsKey(data.ParticleType))
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
                                    // Run particle interactions with adjacent particles if one exists
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
                                    // Only run if particle moved
                                    if (newPos != position)
                                    {
                                        // Relative to this particle's original position
                                        // If moving toward a particle of the same type, make sure it is awake
                                        otherData.Awake = true;
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
                            if (allSameAdjacent && !ParticleTypes.CannotSleep.Contains(data.ParticleType))
                            {
                                // Particles surrounded by particles of the same type don't need to run physics
                                data.Awake = false;
                            }
                            data.PreviousPosition = position;
                        }
                        data.Age += frameTime;
                    }
                }

                _ = SDL.SDL_LockTexture(renderTexture, IntPtr.Zero, out pixels, out int pitch);
                for (int x = 0; x < 500; x++)
                {
                    for (int y = 0; y < 500; y++)
                    {
                        int offset = (pitch * y) + (x * 4);
                        SDL.SDL_Color color = particleField.ParticleColors[x, y];
                        Marshal.WriteByte(pixels + offset, color.b);
                        Marshal.WriteByte(pixels + offset + 1, color.g);
                        Marshal.WriteByte(pixels + offset + 2, color.r);
                        Marshal.WriteByte(pixels + offset + 3, color.a);
                    }
                }

                SDL.SDL_UnlockTexture(renderTexture);
                _ = SDL.SDL_RenderCopy(screen, renderTexture, IntPtr.Zero, IntPtr.Zero);

                IntPtr selectedTypeTextSfc = SDL_ttf.TTF_RenderUTF8_Blended(fontLarge,
                    ParticleTypes.FriendlyNames[currentParticleType], ParticleTypes.Colors[currentParticleType]);
                IntPtr selectedTypeText = SDL.SDL_CreateTextureFromSurface(screen, selectedTypeTextSfc);
                _ = DrawTextureAtPosition(screen, selectedTypeText, new Point(10, 10));

                IntPtr brushSizeTextSfc = SDL_ttf.TTF_RenderUTF8_Blended(fontSmall,
                    $"Brush Size: {brushSize}", Colors.White);
                IntPtr brushSizeText = SDL.SDL_CreateTextureFromSurface(screen, brushSizeTextSfc);
                _ = DrawTextureAtPosition(screen, brushSizeText, new Point(500 - 130, 10));

                IntPtr blockReplacementTextSfc = SDL_ttf.TTF_RenderUTF8_Blended(fontSmall,
                    $"Block Replacement/Power Erase: {(blockReplacement ? "On" : "Off")}", Colors.White);
                IntPtr blockReplacementText = SDL.SDL_CreateTextureFromSurface(screen, blockReplacementTextSfc);
                _ = DrawTextureAtPosition(screen, blockReplacementText, new Point(10, 500 - 35));

                IntPtr physicsTextSfc = SDL_ttf.TTF_RenderUTF8_Blended(fontSmall,
                    $"Physics: {(physics ? "On" : "Off")}", Colors.White);
                IntPtr physicsText = SDL.SDL_CreateTextureFromSurface(screen, physicsTextSfc);
                _ = DrawTextureAtPosition(screen, physicsText, new Point(500 - 105, 500 - 35));

                SDL.SDL_FreeSurface(selectedTypeTextSfc);
                SDL.SDL_DestroyTexture(selectedTypeText);
                SDL.SDL_FreeSurface(blockReplacementTextSfc);
                SDL.SDL_DestroyTexture(blockReplacementText);
                SDL.SDL_FreeSurface(physicsTextSfc);
                SDL.SDL_DestroyTexture(physicsText);

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

            SDL_ttf.TTF_CloseFont(fontLarge);
            SDL_ttf.TTF_CloseFont(fontSmall);
            Marshal.FreeHGlobal(pixels);
            SDL.SDL_DestroyTexture(renderTexture);
            SDL.SDL_DestroyRenderer(screen);
            SDL.SDL_DestroyWindow(window);
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }
    }
}
