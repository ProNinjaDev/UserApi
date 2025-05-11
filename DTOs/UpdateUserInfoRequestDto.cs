using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs {
    public class UpdateUserInfoRequestDto {
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s]*$", ErrorMessage = "Name can only contain latin and cyrillic letters and spaces")]
        public string? Name { get; set; }

        [Range(0, 2, ErrorMessage = "Gender must be 0, 1 or 2")]
        public int? Gender { get; set; }

        public DateTime? Birthday { get; set; } // todo: проверить не мешает ли nullable для апдейта
    }
}