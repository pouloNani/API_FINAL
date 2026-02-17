using System;
using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace Api.DTOs;

public class RegisterDto
{
        public string? firstName { get; set; }
        public string? lastName { get; set; }

        public string? phoneNumber { get; set; }

        [Required]
        public string email {get;set;} = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
         public string ConfirmPassword { get; set; } = string.Empty;



        [Required]
        [RegularExpression("^(client|admin|owner)$", ErrorMessage = "Role must be 'client', 'admin', or 'owner'")]
        [EnumDataType(typeof(UserRole))]
        public UserRole role {get;set;} = UserRole.client;

        public Address? address {get;set;}
        

}
