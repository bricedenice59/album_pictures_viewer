using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoApp.Utils.Models
{
    public class TreeviewViewModel
    {
        public class PhotoModel
        {
            public string Title { get; set; }

            public string Date { get; set; }

            public byte[] Thumbnail { get; set; }

            public double Filesize { get; set; }

            public string PhotoExif { get; set; }

            public string ImgDataURL { get; set; }
        }

        public TreeviewUtils.AlbumFolder AlbumsFolders { get; set; }

        public List<PhotoModel> PhotosList { get; set; }
    }
}
