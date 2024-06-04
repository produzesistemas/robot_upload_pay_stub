using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("Establishment")]
    public class Establishment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public bool Active { get; set; }
    }
}
