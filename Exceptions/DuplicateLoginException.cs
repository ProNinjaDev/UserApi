namespace UserApi.Exceptions {
    public class DuplicateLoginException : Exception {
        public DuplicateLoginException(string login)
         : base($"Login '{login}' already exists") {}
        
        public DuplicateLoginException(string login, Exception innerException)
         : base($"Login '{login}' already exists", innerException) {}
    }
}