using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Models
{
    [Table("Document")]

    public class Document
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string DocumentName { get; set; }
        public string ApplicationUserId { get; set; }
        public int EstablishmentId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
