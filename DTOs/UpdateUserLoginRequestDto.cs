using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs {
    public class UpdateUserLoginRequestDto {
        [Required(ErrorMessage = "New login is required")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Login must be between 3 and 40 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login can only contain latin letters and numbers")]
        public required string NewLogin { get; set; }
    }
}