using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("LastUpdate")]
    public class LastUpdateDto : DomainObject
    {
        public string Name { get; set; }

        public string AlbumPath { get; set; }

        public int ReleasedYear { get; set; }
    }
}