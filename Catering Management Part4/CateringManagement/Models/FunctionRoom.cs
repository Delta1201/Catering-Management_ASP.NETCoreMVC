using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class FunctionRoom
    {
        [Display(Name="Function")]
        public int FunctionID { get; set; }
        public Function Function { get; set; }

        [Display(Name = "Room")]
        public int RoomID { get; set; }
        public Room Room { get; set; }
    }
}
