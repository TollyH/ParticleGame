using SDL2;
using System.Drawing;

namespace ParticleGame
{
    public static class ParticleGame
    {
        private static Point[] Adjacent = new Point[4] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        private static readonly ulong performanceFrequency = SDL.SDL_GetPerformanceFrequency();

        public static void StartGame()
        {
            _ = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            _ = SDL_ttf.TTF_Init();

            _ = SDL.SDL_SetHintWithPriority(SDL.SDL_HINT_RENDER_DRIVER, "direct3d11", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
            IntPtr window = SDL.SDL_CreateWindow("Particle Game", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 500, 500, 0);
            IntPtr screen = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            ulong renderStart = 0;
            ulong renderEnd = 0;
            float frameTime;

            bool quit = false;

            // Game loop
            while (!quit)
            {
                frameTime = (renderEnd - renderStart) / (float)performanceFrequency;
                renderStart = SDL.SDL_GetPerformanceCounter();
                _ = SDL.SDL_RenderClear(screen);

                while (SDL.SDL_PollEvent(out SDL.SDL_Event evn) != 0)
                {
                    if (evn.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        quit = true;
                        break;
                    }
                }

                SDL.SDL_RenderPresent(screen);

                Console.Write($"\r{1 / frameTime:000.00} FPS");
                Console.Out.Flush();

                while ((float)performanceFrequency / (SDL.SDL_GetPerformanceCounter() - renderStart) > 60)
                {
                    // Cap at 60fps
                    SDL.SDL_Delay(1);
                }

                renderEnd = SDL.SDL_GetPerformanceCounter();
            }

            SDL.SDL_DestroyRenderer(screen);
            SDL.SDL_DestroyWindow(window);
            SDL_ttf.TTF_Quit();
            SDL.SDL_Quit();
        }
    }
}
