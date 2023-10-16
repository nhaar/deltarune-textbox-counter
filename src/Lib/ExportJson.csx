using System.Text.Json;
using System.Text.Encodings.Web;

void ExportJson (Dictionary<string, string> json, string path)
{
    string jsonString = JsonSerializer.Serialize(json, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    File.WriteAllText(path, jsonString);
}