namespace Study_Hub.Models
{
    public class RouterOptions
    {
        public string Type { get; set; } = "TPLink-ER7206";
        public string Host { get; set; } = null!;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string AddScriptPath { get; set; } = "/usr/local/bin/add_whitelist.sh";
        public string RemoveScriptPath { get; set; } = "/usr/local/bin/remove_whitelist.sh";
        public OmadaControllerOptions? OmadaController { get; set; }
    }

    public class OmadaControllerOptions
    {
        public bool Enabled { get; set; } = false;
        public string Url { get; set; } = "https://localhost:8043";
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = null!;
        public string SiteId { get; set; } = "Default";
    }
}

