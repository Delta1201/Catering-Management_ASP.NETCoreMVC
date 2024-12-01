using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class FunctionType
    {
        public int ID { get; set; }

        public string Summary => Name;

        [Display(Name = "Function Type")]
        [Required(ErrorMessage = "You cannot leave the function type blank.")]
        [StringLength(120, ErrorMessage = "Function type cannot be more than 120 characters long.")]
        public string Name { get; set; }

        public ICollection<Function> Functions { get; set; } = new HashSet<Function>();
    }
}
