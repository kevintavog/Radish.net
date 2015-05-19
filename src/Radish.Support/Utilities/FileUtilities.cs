using System.IO;
using NLog;

namespace Radish.Support.Utilities
{
    static public class FileUtilities
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        static public void Copy(string sourceFile, string destinationFile)
        {
            logger.Info("Copy '{0}' to {1}", sourceFile, destinationFile);
            File.Copy(sourceFile, destinationFile, false);

            File.SetLastWriteTime(destinationFile, File.GetLastWriteTime(sourceFile));
        }
    }
}
