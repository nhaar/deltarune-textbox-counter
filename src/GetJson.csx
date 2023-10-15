using System.Text.Json;

/// <summary>
/// Load the JSON for a file as a dictionary
/// </summary>
/// <returns></returns>
// for some reason, the read only spans and utf8jsonreader only will work inside a function and not in the global scope
Dictionary<string, string> GetJsonAsDict (string filePath)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(filePath);
    var reader = new Utf8JsonReader(fileBytes);
    Dictionary<string, string> json = new();

    var lastProperty = "";
    while (reader.Read())
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.PropertyName:
                lastProperty = reader.GetString();
                break;
            case JsonTokenType.String:
                json[lastProperty] = reader.GetString();
                break;
        }
    }
    return json;
}