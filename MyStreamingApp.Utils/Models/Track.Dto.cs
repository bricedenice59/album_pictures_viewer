using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStreamingApp.Utils.Models
{
    [Table("Tracks")]
    public class Track : DomainObject
    {
        public string Title { get; set; }

        public string FilePath { get; set; }

        public AlbumDto Album { get; set; }

        public int Year { get; set; }
    }
}
