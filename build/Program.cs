using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Stubble.Core.Builders;
using YamlDotNet.Serialization;




IDeserializer YamlDeserializer =
        new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

MarkdownPipeline Pipeline
        = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .Build();


Console.WriteLine("Fabric Notes Generator");

string outputfolder = "dist";

Console.WriteLine("Creating output folder");
if (Directory.Exists(outputfolder))
{
    Console.WriteLine($"Deleting existing {outputfolder}");
    Directory.Delete(outputfolder, true);
}
Directory.CreateDirectory(outputfolder);

// 1. Copy build/public folder to outputfolder
Console.WriteLine($"Copying build/public folder to {outputfolder}");
DirectoryCopy("build/public", outputfolder, true);

Directory.CreateDirectory(Path.Combine(outputfolder, "images", "notes"));

// 2. Listing all notes

// 3. For each note - For each markdown file in notes folder
// Read markdown file

var allNotes = Directory.GetFiles("notes", "*.md", SearchOption.AllDirectories).ToList();

var allNotesMetadata = new List<NoteMetadata>();

Console.WriteLine($"Discovered {allNotes.Count} notes");

foreach (var note in allNotes)
{
    var document = Markdown.Parse(File.ReadAllText(note), Pipeline);
    var block = document
        .Descendants<YamlFrontMatterBlock>()
        .FirstOrDefault();

    if (block == null)
    {
        Console.WriteLine($"Note {Path.GetFileNameWithoutExtension(note)} has no metadata. Skipping");
        continue;
    }

    var noteMetadata = GetNoteMetadata<NoteMetadata>(block);

    // Copy image
    var noteFileNameId = Path.GetFileNameWithoutExtension(note);
    noteMetadata.FileNameId = noteFileNameId;
    File.Copy(Path.Combine("notes", $"{noteFileNameId}.png"), Path.Combine(outputfolder, "images", "notes", $"{noteFileNameId}.png"), true);

    allNotesMetadata.Add(noteMetadata);
}

// 4. Generate index.html
var stubble = new StubbleBuilder().Build();

var templateContext = new
{
    UpdateDate = DateTime.Now.ToString("dddd dd MMMM yyyy HH:mm:ss"),
    Notes = allNotesMetadata,
    IsProductionBuild = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true"
};

using (StreamReader streamReader = new StreamReader(Path.Combine("build", "templates", "index.html"), Encoding.UTF8))
{
    var output = stubble.Render(streamReader.ReadToEnd(), templateContext);
    File.WriteAllText(Path.Combine(outputfolder, "index.html"), output);
}

Console.WriteLine("Generation done");

T GetNoteMetadata<T>(YamlFrontMatterBlock block)
{
    var yaml =
           block
           // this is not a mistake
           // we have to call .Lines 2x
           .Lines // StringLineGroup[]
           .Lines // StringLine[]
           .OrderByDescending(x => x.Line)
           .Select(x => $"{x}\n")
           .ToList()
           .Select(x => x.Replace("---", string.Empty))
           .Where(x => !string.IsNullOrWhiteSpace(x))
           .Aggregate((s, agg) => agg + s);

    return YamlDeserializer.Deserialize<T>(yaml);
}

void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
{
    // Get the subdirectories for the specified directory.
    DirectoryInfo dir = new DirectoryInfo(sourceDirName);

    if (!dir.Exists)
    {
        throw new DirectoryNotFoundException(
            "Source directory does not exist or could not be found: "
            + sourceDirName);
    }

    DirectoryInfo[] dirs = dir.GetDirectories();
    // If the destination directory doesn't exist, create it.
    if (!Directory.Exists(destDirName))
    {
        Directory.CreateDirectory(destDirName);
    }

    // Get the files in the directory and copy them to the new location.
    FileInfo[] files = dir.GetFiles();
    foreach (FileInfo file in files)
    {
        string temppath = Path.Combine(destDirName, file.Name);
        file.CopyTo(temppath, false);
    }

    // If copying subdirectories, copy them and their contents to new location.
    if (copySubDirs)
    {
        foreach (DirectoryInfo subdir in dirs)
        {
            string temppath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopy(subdir.FullName, temppath, copySubDirs);
        }
    }
}
