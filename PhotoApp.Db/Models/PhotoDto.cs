using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Photos")]
    public class PhotoDto : DomainObject, IEquatable<PhotoDto>
    {
        public string Title { get; set; }

        public string AlbumPath { get; set; }

        public string Date { get; set; }

        public byte[] Thumbnail { get; set; }

        public double Filesize { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as PhotoDto);
        }

        public bool Equals(PhotoDto other)
        {
            return other != null &&
                   Title == other.Title &&
                   AlbumPath == other.AlbumPath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AlbumPath, Title);
        }
    }
}
