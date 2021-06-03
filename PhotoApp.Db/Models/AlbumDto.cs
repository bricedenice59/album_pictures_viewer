using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Albums")]
    public class AlbumDto : DomainObject
    {
        public string Path { get; set; }
    }
}
