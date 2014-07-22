using System.IO;

namespace Radish.Utilities
{
    public class JheadInvoker : ProcessInvoker
    {
        override protected string ProcessName { get { return "jhead"; } }
        override protected string ProcessPath 
        { 
            get 
            { 
                // Mac specific path to exiftool
                return "/users/goatboy/tools/jhead";
            }
        }

        override protected bool CheckProcessPath(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            Invoke("-V");
            return true;
        }
    }
}
