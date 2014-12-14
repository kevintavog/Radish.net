using System;
using NLog;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Rangic.Utilities.Preferences;

namespace Radish.Support
{
    public class RadishPreferences : BasePreferences
    {
        public RadishPreferences()
        {
            FindAPhotoHost = "";
        }

        //  Properties perisisted between sessions
        public string FindAPhotoHost { get; set; }


        public override void FromJson(dynamic json)
        {
            FindAPhotoHost = json.FindAPhotoHost;
        }

        public override dynamic ToJson()
        {
            return new
            {
                FindAPhotoHost = FindAPhotoHost
            };
        }
    }
}
