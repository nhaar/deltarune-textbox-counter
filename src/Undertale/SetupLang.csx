// This script setups the lang files for first time use (theoretically will not change over time)

#load "..\Lib\DecompileContext.csx"
#load "..\Lib\JsonUtils.csx"
#load "UndertaleUtils.csx"

ExportLang();

var langEN = GetUndertaleLang(Lang.EN);
var langJP = GetUndertaleLang(Lang.JP);

GetLanguageExclusive();

void GetLanguageExclusive ()
{
    var exclusive = GetJsonExclusive(langEN, langJP);
    foreach (Lang lang in Enum.GetValues(typeof(lang)))
    {
        var langName = GetUndertaleLangName(lang);
        var langIndex = lang == Lang.EN ? 0 : 1;
        WriteWithComments(Path.Combine(langFolder, $"only_{langName}.txt"), exclusive[langIndex], langEN, langJP);
    }
}

void ExportLang ()
{
    if (!Directory.Exists(langFolder))
    {
        Directory.CreateDirectory(langFolder);
    }

    foreach (Lang lang in Enum.GetValues(typeof(lang)))
    {
        var langName = GetUndertaleLangName(lang);
        TextDataExtract($"gml_Script_textdata_{langName}", $"global.text_data_{langName}", $"lang_{langName}.json");
    }
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