using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace CateringManagement.Models
{
    public class FunctionDocument : UploadedFile
    {
        [Display(Name = "Function")]
        public int FunctionID { get; set; }

        public Function Function { get; set; }
    }
}
