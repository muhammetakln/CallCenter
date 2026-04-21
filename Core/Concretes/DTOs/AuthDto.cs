using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Concretes.DTOs
{
    public class LoginDto
    {
        [Required, Display(Name = "Email Address", Prompt = "Email Address"), EmailAddress]
        public string Email { get; set; } = null!;
        [Required, Display(Name = "Password", Prompt = "Password"), DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Display(Name = "Remember Me", Prompt = "Remember Me")]
        public bool RememberMe { get; set; }
    }
    public class RegisterDto
    {
        [Required, Display(Name = "Email Address", Prompt = "Email Address"), EmailAddress]
        public string Email { get; set; } = null!;

        [Required, Display(Name = "First Name", Prompt = "Last Name")]
        public string FirstName { get; set; } = null!;
        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;
        [Required, Display(Name = "Password", Prompt = "Password"), DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required, Display(Name = "Confirm Password", Prompt = "Confirm Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
   

    public class ChangePasswordDto
    {
        [Required, Display(Name = "Previous Password", Prompt = "Password"), DataType(DataType.Password)]
        public string PreviousPassword { get; set; } = null!;
        [Required, Display(Name = "New Password", Prompt = "New Password"), DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;
        [Required, Display(Name = "Confirm Password", Prompt = "Confirm Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        
       
        [Required, Display(Name = "New Password", Prompt = "New Password"), DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;
        [Required, Display(Name = "Confirm Password", Prompt = "Confirm Password"), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
        [Required]
        public string AuthToken { get; set; } = null!;
    }
}
