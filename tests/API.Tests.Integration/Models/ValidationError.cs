namespace API.Tests.Integration.Models
{
    public class ValidationError
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
