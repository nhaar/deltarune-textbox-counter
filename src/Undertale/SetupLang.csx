// This script setups the lang files for first time use (theoretically will not change over time)

#load "..\Lib\DecompileContext.csx"
#load "..\Lib\ExportJson.csx"
#load "..\Lib\JsonExclusive.csx"

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

ExportLang();

var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en.json"));
var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja.json"));

GetLanguageExclusive();

void GetLanguageExclusive ()
{
    var exclusive = GetJsonExclusive(langEN, langJP);
    File.WriteAllLines(Path.Combine(langFolder, "only_en.txt"), exclusive[0]);
    File.WriteAllLines(Path.Combine(langFolder, "only_ja.txt"), exclusive[1]);
}

void ExportLang ()
{
    if (!Directory.Exists(langFolder))
    {
        Directory.CreateDirectory(langFolder);
    }
    

    TextDataExtract("gml_Script_textdata_en", "global.text_data_en", "lang_en.json");
    TextDataExtract("gml_Script_textdata_ja", "global.text_data_ja", "lang_ja.json");
}

void TextDataExtract (string codeName, string textData, string fileName)
{
    var content = Decompiler.Decompile(Data.Code.ByName(codeName), DECOMPILE_CONTEXT.Value);
    Dictionary<string, string> result = new();
    var lines = content.Split("\n");
    foreach (string line in lines)
    {
        if (line.Contains("ds_map_add"))
        {
            var textId = Regex.Match(line, @$"(?<=ds_map_add\({Regex.Escape(textData)}, "")[\d\w_]+(?="")").Value;
            var text = Regex.Match(line, @"(?<=, "")(\\""|[^""])*(?=""\)\s*$)").Value;
            result[textId] = text;
        }
    }
    
    ExportJson(result, Path.Combine(langFolder, fileName));
}

ExportLang();