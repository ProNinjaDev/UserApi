namespace UserApi.DTOs {
    public class TokenResponseDto {
        public required string Token { get; init; }
        public required DateTime Expiration { get; init; }

    }
}