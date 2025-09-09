namespace FZ.Constant
{
    public class EmailSettings
    {
        public string Mail { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool EnableSSL { get; set; } // map với appsettings
        public string? BaseUrl { get; set; }
    }
}
