using System.ComponentModel.DataAnnotations;

namespace CateringManagement.Models
{
    public class Customer : Auditable
    {
        public int ID { get; set; }

        #region Summary Properties

        [Display(Name = "Customer")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }
        [Display(Name = "Customer")]
        public string Summary
        {
            get
            {
                
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper())
                        + " (" + CustomerCode + ")";
            }
        }
        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone[6..];
            }
        }

        #endregion

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(30, ErrorMessage = "Middle name cannot be more than 30 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        [Display(Name = "Company Name")]
        [StringLength(120, ErrorMessage = "Company name cannot be more than 120 characters long.")]
        public string CompanyName { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "You cannot leave the phone number blank.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "The phone number must be exactly 10 numeric digits.")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; }

        [Display(Name = "Customer Code")]
        [Required(ErrorMessage = "You cannot leave the customer code blank.")]
        [RegularExpression("(C|G|I|A)\\d{7}",
            ErrorMessage = "Invalid Customer Code: it must start with C, G, I or A followed by 7 numeric digits.")]
        public string CustomerCode { get; set; }

        public CustomerPhoto CustomerPhoto { get; set; }
        public CustomerThumbnail CustomerThumbnail { get; set; }

        public ICollection<Function> Functions { get; set; } = new HashSet<Function>();

        //public ICollection<CustomerFunction> CustomerFunctions { get; set; } = new HashSet<CustomerFunction>();

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? EMail { get; set; }

        public static implicit operator Customer(Function v)
        {
            throw new NotImplementedException();
        }
    }
}
