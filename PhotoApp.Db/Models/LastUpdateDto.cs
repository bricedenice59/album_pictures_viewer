using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("LastUpdate")]
    public class LastUpdateDto : DomainObject
    {
        public DateTime UpdateTime { get; set; }
    }
}