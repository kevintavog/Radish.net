using System.IO;

namespace Radish.Utilities
{
    public class ExifToolInvoker : ProcessInvoker
    {
        override protected string ProcessName { get { return "ExifTool"; } }
        override protected string ProcessPath 
        { 
            get 
            {
                // Mac specific path to exiftool
                return "/usr/bin/exiftool";
            }
        }

        override protected bool CheckProcessPath(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            Invoke("-ver");
            return true;
        }
    }
}
