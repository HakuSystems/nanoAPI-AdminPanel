namespace nanoAPIAdminPanel.Auth
{
    public class LoginBase<T>
    {
        public string message { get; set; }
        public T Data { get; set; }
    }
}