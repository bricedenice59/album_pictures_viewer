using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Photos")]
    public class PhotoDto : DomainObject
    {
        public string Title { get; set; }

        public virtual AlbumDto Album { get; set; }

        public string Date { get; set; }

        public byte[] Thumbnail { get; set; }

        public double Filesize { get; set; }
        
        public string PhotoExif { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }
    }
}
