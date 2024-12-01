using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class CustomerPhoto
    {
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        [StringLength(255)]
        public string MimeType { get; set; }

        public int CustomerID { get; set; }
        public Customer Customer { get; set; }
    }
}
