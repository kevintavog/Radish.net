using System;
using NLog;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Radish.Support
{
    public class Preferences
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static public Preferences Instance { get; private set; }


        //  Properties perisisted between sessions
        public string FindAPhotoHost { get; set; }


        // Properties derived from other properties
        public string PreferencesFolder { get { return Path.GetDirectoryName(Filename); } }


        private string Filename { get; set; }

        public void Save()
        {
            Save(Filename);
        }

        static public Preferences Load(string filename)
        {
            Instance = new Preferences();
            Instance.Filename = filename;
            Instance.FindAPhotoHost = "";
            if (File.Exists(filename))
            {
                try
                {
                    dynamic json = JObject.Parse(File.ReadAllText(filename));

                    Instance.FindAPhotoHost = json.FindAPhotoHost;
                    if (Instance.FindAPhotoHost == null) Instance.FindAPhotoHost = "";
                }
                catch (Exception e)
                {
                    logger.Error("Exception loading preferences (using defaults): {0}", e);
                }
            }
            else
            {
                try
                {
                    Save(filename);
                }
                catch (Exception e)
                {
                    logger.Error("Exception saving preferences to '{0}': {1}", filename, e);
                }
            }
            return Instance;
        }

        static private void Save(string filename)
        {
            var prefs = new
            {
                Instance.FindAPhotoHost,
            };

            File.WriteAllText(filename, JsonConvert.SerializeObject(prefs));
        }
    }
}
