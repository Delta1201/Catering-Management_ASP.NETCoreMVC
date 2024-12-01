using System.ComponentModel.DataAnnotations;

namespace CateringManagement.ViewModels
{
    public class FunctionRevenueVM
    {
        public int ID { get; set; }

        [Display(Name = "Function Type")]
        public string Name { get; set; }

        [Display(Name = "Total Number")]
        public int TotalNumber { get; set; }

        [Display(Name = "Avg. Per Person Charge")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double AveragePPCharge { get; set; }

        [Display(Name = "Avg. Guar. No.")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageGuarNo { get; set; }

        [Display(Name = "Total Value")]
        [DataType(DataType.Currency)]
        public double TotalValue { get; set; }

        [Display(Name = "Avg. Value")]
        [DataType(DataType.Currency)]
        public double AvgValue { get; set; }

        [Display(Name = "Highest Value")]
        [DataType(DataType.Currency)]
        public double MaxValue { get; set; }

    }
}
