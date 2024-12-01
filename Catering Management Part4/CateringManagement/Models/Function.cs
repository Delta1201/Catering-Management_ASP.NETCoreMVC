using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class Function : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Function")]
        public string Summary
        {
            get
            {
                return (string.IsNullOrEmpty(Name) ? (!string.IsNullOrEmpty(LobbySign) ? LobbySign : "TBA") : Name); ;
            }
        }

        [Display(Name = "Estimated Value")]
        public string EstimatedValue
        {
            get
            {
                // Returns the function's Estimated Value (BaseCharge plus SOCAN fee plus the Guaranteed Number times the PerPersonCharge.) formatted as currency
                return (BaseCharge + SOCAN + (GuaranteedNumber * PerPersonCharge)).ToString("c");
            }
        }

        [Display(Name = "Date")]
        public string StartDateSummary
        {
            get
            {
                return StartTime.ToString("f");
            }
        }
        [Display(Name = "Start")]
        public string TimeSummary
        {
            get
            {
                return StartTime.ToString("h:mm tt") + " to " + EndTimeSummary;
            }
        }

        [Display(Name = "End")]
        public string EndTimeSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "Unknown";
                }
                else
                {
                    string endtime = EndTime.GetValueOrDefault().ToString("h:mm tt");
                    TimeSpan difference = ((TimeSpan)(EndTime - StartTime));
                    int days = difference.Days;
                    if (days > 0)
                    {
                        return endtime + " (" + days + " day" + (days > 1 ? "s" : "") + " later)";
                    }
                    else
                    {
                        return endtime;
                    }
                }
            }
        }
        [Display(Name = "Duration")]
        public string DurationSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "";
                }
                else
                {
                    TimeSpan d = ((TimeSpan)(EndTime - StartTime));
                    string duration = "";
                    if (d.Minutes > 0) //Show the minutes if there are any
                    {
                        duration = d.Minutes.ToString() + " min";
                    }
                    if (d.Hours > 0) //Show the hours if there are any
                    {
                        duration = d.Hours.ToString() + " hr" + (d.Hours > 1 ? "s" : "")
                            + (d.Minutes > 0 ? ", " + duration : ""); //Put a ", " between hours and minutes if there are both
                    }
                    if (d.Days > 0) //Show the days if there are any
                    {
                        duration = d.Days.ToString() + " day" + (d.Days > 1 ? "s" : "")
                            + (d.Hours > 0 || d.Minutes > 0 ? ", " + duration : ""); //Put a ", " between days and hours/minutes if there are any
                    }
                    //Changed from this approach to avoid possibly having a comma at the end of the string
                    //duration = (d.Days > 0 ? d.Days.ToString() + " day" + (d.Days > 1 ? "s, " : ", ") : "") +
                    //    (d.Hours > 0 ? d.Hours.ToString() + " hr" + (d.Hours > 1 ? "s, " : ", ") : "") +
                    //    (d.Minutes > 0 ? d.Minutes.ToString() + " min" : "");
                    return duration;
                }
            }
        }
        #endregion

        [StringLength(300, ErrorMessage = "Name cannot be more than 300 characters long.")]
        public string Name { get; set; }

        [Display(Name = "Lobby Sign")]
        [StringLength(120, ErrorMessage = "Lobby sign cannot be more than 60 characters long.")]
        public string LobbySign { get; set; }

        [Required(ErrorMessage = "You must enter a start date and time for the Function.")]
        [Display(Name = "Start")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; } = DateTime.Today.AddHours(32);

        [Display(Name = "End")]
        [DataType(DataType.DateTime)]
        public DateTime? EndTime { get; set; } = DateTime.Today.AddHours(33);

        [Display(Name = "Setup Notes")]
        [StringLength(2000, ErrorMessage = "Only 2000 characters for notes.")]
        [DataType(DataType.MultilineText)]
        public string SetupNotes { get; set; }

        [Required(ErrorMessage = "You must enter the Base Charge.")]
        [Display(Name = "Base Charge")]
        [DataType(DataType.Currency)]
        public double BaseCharge { get; set; }

        [Required(ErrorMessage = "You must enter the Charge Per Person.")]
        [Display(Name = "Per Person")]
        [DataType(DataType.Currency)]
        public double PerPersonCharge { get; set; }

        [Required(ErrorMessage = "The Guaranteed Number is required.")]
        [Display(Name = "Guaranteed Number")]
        [Range(1, int.MaxValue, ErrorMessage = "Guaranteed number must be greater than zero")]
        public int GuaranteedNumber { get; set; } = 1;

        [Required(ErrorMessage = "You must enter a value for the SOCAN fee.  Use 0.00 if no fee is applicable.")]
        [Display(Name = "SOCAN")]
        [DataType(DataType.Currency)]
        public double SOCAN { get; set; } = 50.00;

        [Required(ErrorMessage = "Amount for the Deposit is required.")]
        [DataType(DataType.Currency)]
        public double Deposit { get; set; }
        public bool Alcohol { get; set; } = false;

        [Display(Name = "Deposit Paid")]
        public bool DepositPaid { get; set; } = false;

        [Display(Name = "No HST")]
        public bool NoHST { get; set; } = false;

        [Display(Name = "No Gratuity")]
        public bool NoGratuity { get; set; } = false;

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }//Added for concurrency

        // foreign keys
        [Display(Name = "Customer")]
        [Required(ErrorMessage = "You must select a Customer")]
        public int CustomerID { get; set; }
        public Customer Customer { get; set; }

        [Display(Name = "Function Type")]
        [Required(ErrorMessage = "You must select a Function Type")]
        public int FunctionTypeID { get; set; }

        [Display(Name = "Function Type")]
        public FunctionType FunctionType { get; set; }

        //Note: MealType is not required
        [Display(Name = "Meal Type")]
        public int? MealTypeID { get; set; }

        [Display(Name = "Meal Type")]
        public MealType MealType { get; set; }

        [Display(Name = "Function Rooms")]
        public ICollection<FunctionRoom> FunctionRooms { get; set; } = new HashSet<FunctionRoom>();

        [Display(Name = "Documents")]
        public ICollection<FunctionDocument> FunctionDocuments { get; set; } = new HashSet<FunctionDocument>();
        public ICollection<Work> Works { get; set; } = new HashSet<Work>();

        [Display(Name = "Function Equipment")]
        public ICollection<FunctionEquipment> FunctionEquipments { get; set; } = new HashSet<FunctionEquipment>();
        //public ICollection<CustomerFunction> CustomerFunctions { get; set; } = new HashSet<CustomerFunction>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Function Date cannot be before January 1st, 2018 because that is when the Hotel opened.
            if (StartTime < DateTime.Parse("2018-01-01"))
            {
                yield return new ValidationResult("Date cannot be before January 1st, 2018.", new[] { "StartTime" });
            }

            // Function Date cannot be more than 10 years in the future from the current date.
            if (StartTime > DateTime.Now.AddYears(10))
            {
                yield return new ValidationResult("Date cannot be more than 10 years in the future.", new[] { "StartTime" });
            }

            //Function cannot end before it starts
            if (EndTime < StartTime)
            {
                yield return new ValidationResult("Appointment cannot end before it starts.", new[] { "EndTime" });
            }
        }
    }
}
