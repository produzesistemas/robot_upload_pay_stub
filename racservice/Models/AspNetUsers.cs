
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("AspNetUsers")]
    public class AspNetUsers
    {
        public string Id { get; set; }
        public int Registration { get; set; }
        public string Email { get; set; }
    }
}
