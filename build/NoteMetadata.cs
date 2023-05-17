using YamlDotNet.Serialization;

public class NoteMetadata
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; }
    [YamlMember(Alias = "title")]
    public string Title { get; set; }

    public string FileNameId { get; set; }
}