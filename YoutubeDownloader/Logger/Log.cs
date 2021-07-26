using System.IO;

namespace YoutubeDownloader.Logger
{
    class Log
    {
        public static void Ging(string me)
        {
            File.AppendAllLines("log", new string[] { me });
        }
    }
}
