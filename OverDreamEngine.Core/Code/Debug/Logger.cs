using System;
using System.Diagnostics;
using ODEngine.Core;

namespace ODEngine
{
    public static class Logger
    {
        public static void Log(string text)
        {
            if (!FileManager.SystemExists("Log.txt"))
            {
                FileManager.WriteAllText("Log.txt", "Log created [" + DateTime.Now.ToString() + "]" + Environment.NewLine, System.Text.Encoding.UTF8);
            }

            Console.WriteLine(text);
            FileManager.AppendAllText("Log.txt", Environment.NewLine + " [" + DateTime.Now.ToString() + "]" + Environment.NewLine + text + Environment.NewLine, System.Text.Encoding.UTF8);
        }

        public static void Log(Exception ex)
        {
            if (!FileManager.SystemExists("Log.txt"))
            {
                FileManager.WriteAllText("Log.txt", "Log created [" + DateTime.Now.ToString() + "]" + Environment.NewLine, System.Text.Encoding.UTF8);
            }
            
            FileManager.AppendAllText("Log.txt", Environment.NewLine +" [" + DateTime.Now.ToString() + "]" + Environment.NewLine + ex.Message + Environment.NewLine + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine, System.Text.Encoding.UTF8);
            Debug.Print(ex.ToString());
        }
    }
}