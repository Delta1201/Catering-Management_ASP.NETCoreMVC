using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class FunctionEquipment
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "The Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Equipment Quantity must be greater than zero")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "You must enter the Charge Per Unit.")]
        [Display(Name = "Per Unit")]
        [DataType(DataType.Currency)]
        public double PerUnitCharge { get; set; }

        [Display(Name = "Function")]
        public int FunctionID { get; set; }
        public Function Function { get; set; }

        [Display(Name = "Equipment")]
        public int EquipmentID { get; set; }
        public Equipment Equipment { get; set; }
    }
}
