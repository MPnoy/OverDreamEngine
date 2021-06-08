using System;

namespace ODEngine.Core
{
    public class Program
    {
        public static void Init(string[] args)
        {
#if RELEASE
            try
            {
#endif
                FileManager.Delete("Log.txt");
                LoadAssemblies();
                ImageLoader.Init();
                Kernel.Init(args);
#if RELEASE
            }
            catch (Exception ex)
            {
                FileManager.WriteAllText("Crash.txt", ex.ToString(), System.Text.Encoding.UTF8);
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
                FileManager.WriteAllText("Crash.txt", ex.ToString(), System.Text.Encoding.UTF8);
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