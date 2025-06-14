namespace lunchin.Optimizely.Cloud.Extensions.MasterLanguage;

public class SwitchLanguageModel
{
    public int ContentId { get; set; }

    public string? TargetLanguage { get; set; }

    public bool ProcessChildren { get; set; }

    public bool SwitchOnly { get; set; }
}
