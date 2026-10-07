using System.ComponentModel.DataAnnotations;

namespace EmFren.Models
{
    public class RegisterUser
    {
        public string Username { get; set; } = string.Empty;


        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;


        
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    }
}
