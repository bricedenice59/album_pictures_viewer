using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Photos")]
    public class PhotoDto : DomainObject
    {
        public string Title { get; set; }

        public string AlbumPath { get; set; }

        public string Date { get; set; }

        public byte[] Thumbnail { get; set; }
    }
}
