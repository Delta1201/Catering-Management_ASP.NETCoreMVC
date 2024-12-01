using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class Equipment
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Equipment")]
        public string Summary
        {
            get
            {
                return Name + " (Std. " + StandardCharge.ToString("c") + ")";
            }
        }
        #endregion

        [Display(Name = "Equipment")]
        [Required(ErrorMessage = "You cannot leave the equipment name blank.")]
        [StringLength(80, ErrorMessage = "Equipment name cannot be more than 80 characters long.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "You must enter the Standard Charge Per Unit.")]
        [Display(Name = "Standard Charge")]
        [DataType(DataType.Currency)]
        public double StandardCharge { get; set; }

        public ICollection<FunctionEquipment> FunctionEquipments { get; set; } = new HashSet<FunctionEquipment>();
    }
}
