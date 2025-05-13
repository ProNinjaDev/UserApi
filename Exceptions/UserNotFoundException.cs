namespace UserApi.Exceptions {
    public class UserNotFoundException : Exception
        {
            public UserNotFoundException(string identifier)
                : base($"User with identifier '{identifier}' not found")
            {
            }

            public UserNotFoundException(string identifier, Exception innerException)
                : base($"User with identifier '{identifier}' not found", innerException)
            {
            }
        }
}