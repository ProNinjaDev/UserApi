using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs {
    public class CreateUserRequestDto {

        [Required(ErrorMessage = "Login is required")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Login must be between 3 and 40 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login can only contain latin letters and numbers")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 80 characters")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 100 characters")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s]*$", ErrorMessage = "Name can only contain latin and cyrillic letters and spaces")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Range(0, 2, ErrorMessage = "Gender must be 0, 1 or 2")]
        public int Gender { get; set; }

        [Required(ErrorMessage = "Birtday is required")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Admin status is required")]
        public bool Admin { get; set; }
    }
}