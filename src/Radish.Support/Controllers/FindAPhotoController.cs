using Radish.Models;
using System.Collections.Generic;

namespace Radish.Controllers
{
    public class FindAPhotoController : MediaListController
    {
        public FindAPhotoController(IFileViewer fileViewer)
            : base(fileViewer)
        {
        }

        public void Set(IList<MediaMetadata> list)
        {
            SetList(list);
        }
    }
}

