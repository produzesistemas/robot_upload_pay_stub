using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Models
{
    [Table("Log")]

    public class Log
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
