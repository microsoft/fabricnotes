using YamlDotNet.Serialization;

public class NoteMetadata
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; }
    [YamlMember(Alias = "title")]
    public string Title { get; set; }

    [YamlMember(Alias = "imgalt")]
    public string ImageDescription { get; set; }

    // When true, the note belongs to the new series and is highlighted
    // differently in the UI than the original Fabric Notes.
    [YamlMember(Alias = "new")]
    public bool IsNew { get; set; }

    public string FileNameId { get; set; }
}