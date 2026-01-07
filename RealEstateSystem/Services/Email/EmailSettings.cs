namespace RealEstateSystem.Services.Email
{
    public class EmailSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;

        // Gmail recommended: StartTLS on port 587
        public bool UseStartTls { get; set; } = true;

        // If you ever want SSL port 465, you can set this true and UseStartTls false.
        public bool UseSsl { get; set; } = false;

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "Real Estate System";

        // Where commission notifications go
        public string AdminNotificationEmail { get; set; } = "admin@gmail.com";
    }
}
