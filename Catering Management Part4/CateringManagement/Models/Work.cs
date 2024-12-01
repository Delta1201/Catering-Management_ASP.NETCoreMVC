using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class Work
    {
        public int ID { get; set; }

        public int Points { get; set; }

        [Display(Name = "Function")]
        public int FunctionID { get; set; }
        public Function Function { get; set; }

        [Display(Name = "Worker")]
        public int WorkerID { get; set; }
        public Worker Worker { get; set; }
    }
}
