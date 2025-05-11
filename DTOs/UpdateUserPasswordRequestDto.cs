using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs {
    public class UpdateUserPasswordRequestDto {
        [Required(ErrorMessage = "New password is required")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 80 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public required string NewPassword { get; set; }
    }
}