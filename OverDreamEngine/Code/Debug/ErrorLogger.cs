using System;
using System.Diagnostics;
using System.IO;

namespace ODEngine
{
    public static class ErrorLogger
    {
        public static void Log(string text)
        {
            File.AppendAllText("Log.txt", "\r\n [" + DateTime.Now.ToString() + "]\r\n" + text + "\r\n");
        }

        public static void Log(Exception ex)
        {
            File.AppendAllText("Log.txt", "\r\n [" + DateTime.Now.ToString() + "]\r\n" + ex.Message + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n");
            Debug.Print(ex.ToString());
        }
    }
}