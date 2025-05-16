using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs {
    public class UserCredentialsRequestDto {
        [Required(ErrorMessage = "Login is required")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Login must be between 3 and 40 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login can only contain latin letters and numbers")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 80 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public required string Password { get; set; }
        
    }
}