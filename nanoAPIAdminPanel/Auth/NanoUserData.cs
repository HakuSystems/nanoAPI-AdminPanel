namespace nanoAPIAdminPanel.Auth
{
    public class NanoUserData
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Permission { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPremium { get; set; }
    }
}