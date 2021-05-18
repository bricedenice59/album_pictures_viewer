using System.ComponentModel.DataAnnotations.Schema;

namespace MyStreamingApp.Utils.Models
{
    [Table("Albums")]
    public class AlbumDto : DomainObject
    {
        public string Name { get; set; }

        public string AlbumPath { get; set; }

        public int ReleasedYear { get; set; }
    }
}