
using System;
using System.IO;
using libutilscore.Logging;

namespace libutilscore.IO
{
    public static class LocalFileSystem
    {

        /// <summary>
        /// check whether file exists
        /// </summary>
        /// <param name="filePath">Absolute File Path</param>
        /// <returns></returns>
        public static bool IsFileExist(string filePath)
        {
            bool exist = File.Exists(filePath);
            Log.Logger.Info("Checking File {0} Exist? {1}", filePath, exist);
            return exist;
        }

        /// <summary>
        /// check whether directory exists
        /// </summary>
        /// <param name="dirPath">Absolute Directory Path</param>
        /// <returns></returns>
        public static bool IsDirectoryExist(string dirPath)
        {
            bool exist = Directory.Exists(dirPath);
            Log.Logger.Info("Checking Directory {0} Exist? {1}", dirPath, exist);
            return exist;
        }

    }
}
