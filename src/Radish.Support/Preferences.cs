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
        public string LastCopyToFolder { get; set; }
        public string BaseLocationLookup { get; set; }


        public override void FromJson(dynamic json)
        {
            FindAPhotoHost = json.FindAPhotoHost;
            LastCopyToFolder = json.LastCopyToFolder;
            BaseLocationLookup = json.BaseLocationLookup;
        }

        public override dynamic ToJson()
        {
            return new
            {
                FindAPhotoHost = FindAPhotoHost,
                LastCopyToFolder = LastCopyToFolder,
                BaseLocationLookup = BaseLocationLookup
            };
        }
    }
}
