namespace TodoApi.Options
{
    public class MaintenantanceOptions
    {
        public const string SectionName = "MaintenantanceMode";

        public bool IsInMaintenantanceMode { get; set; }
        public string MaintenantanceMessage { get; set; }
    }
}
