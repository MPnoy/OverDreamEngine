using System;
using System.IO;

namespace ODEngine.Core
{
    public class Program
    {
        public static void Init()
        {
#if RELEASE
            try
            {
#endif
                File.Delete("Log.txt");
                LoadAssemblies();
                ImageLoader.Init();
                Kernel.Init();
#if RELEASE
            }
            catch (Exception ex)
            {
                File.WriteAllText("Crash.txt", ex.ToString());
                throw;
            }
#endif
        }

        public static void StartGame()
        {
#if RELEASE
            try
            {
#endif
                Kernel.StartGame();
                ImageLoader.Deinit();
#if RELEASE
            }
            catch (Exception ex)
            {
                File.WriteAllText("Crash.txt", ex.ToString());
                throw;
            }
#endif
        }

        private static void LoadAssemblies() // Загрузка неуправляемых зависимостей, во избежание их подгрузки во время игры
        {
            AppDomain.CurrentDomain.Load("System.Threading.ThreadPool");
        }

    }
}