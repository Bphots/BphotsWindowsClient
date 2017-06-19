namespace HotsBpHelper.Api.Security
{
    public class SecurityParameter
    {
        public string Timestamp { get; set; }

        public string Patch { get; set; }

        public string Nonce { get; set; }

        public string Sign { get; set; }
    }
}