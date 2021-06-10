using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoApp.Utils.Models
{
    public class AlbumModelDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int NbPhotos { get; set; }

        public AlbumModelDto(int id, string path, int nbPhotos)
        {
            Id = id;
            Path = path;
            NbPhotos = nbPhotos;
        }
    }

    public class PhotosModelDto
    {
        public PhotosModelDto(List<PhotoDto> listPhotos)
        {
            ListPhotos = listPhotos;
        }

        public PhotosModelDto()
        {

        }

        public List<PhotoDto> ListPhotos { get; set; }
    }

    public class PhotoDto
    {
        public string Title { get; set; }

        public string Date { get; set; }

        public byte[] Thumbnail { get; set; }

        public double Filesize { get; set; }

        public string PhotoExif { get; set; }

        public string ImgDataURL { get; set; }
    }

    public class TreeviewViewModel
    {
        public PhotosModelDto PhotosModel { get; set; }

        public TreeviewUtils.AlbumFolder AlbumsFolders { get; set; }
    }
}
