using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class Room
    {
        public int ID { get; set; }
        #region Summary Properties

        [Display(Name = "Room")]
        public string Summary
        {
            get
            {
                return Name + " (Cap: " + Capacity.ToString() + ")"; ;
            }
        }
        #endregion

        [Display(Name = "Room")]
        [Required(ErrorMessage = "You cannot leave the room name blank.")]
        [StringLength(80, ErrorMessage = "Room name cannot be more than 80 characters long.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Room Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Room Capacity must be greater than zero")]
        public int Capacity { get; set; } = 1;

        public ICollection<FunctionRoom> FunctionRooms { get; set; } = new HashSet<FunctionRoom>();
    }
}
