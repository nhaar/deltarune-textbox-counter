void OutputCode (string code)
{
    File.WriteAllText(FilePath + "/../test.txt", code);
}

void Append (string codeName, string code)
{
    Data.Code.ByName(codeName).AppendGML(code, Data);
}

void Replace (string codeName, string text, string replacement)
{
    OutputCode(replacement);
    ReplaceTextInGML(codeName, text, replacement);
}

void Place (string codeName, string preceding, string placement)
{
    OutputCode(placement);
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

UndertaleGameObject CreateObject (string objectName)
{
    var obj = new UndertaleGameObject();
    obj.Name = new UndertaleString("obj_textbox_counter");
    Data.GameObjects.Add(obj);
    Data.Strings.Add(obj.Name);

    return obj;
}

void CreateFunction (string functionName, string code, int argCount = 0, bool isScript = false)
{
    if (isScript)
    {
        var script = new UndertaleScript();
        var function = new UndertaleFunction();
        function.Name = new UndertaleString(functionName);
        script.Name = new UndertaleString(functionName);
        var scriptCode = new UndertaleCode();
        scriptCode.Name = new UndertaleString(functionName);
        scriptCode.ReplaceGML(code, Data);
        Data.Strings.Add(script.Name);
        Data.Code.Add(scriptCode);
        script.Code = scriptCode;
        Data.Scripts.Add(script);
        Data.Strings.Add(scriptCode.Name);
        Data.Strings.Add(function.Name);
        Data.Functions.Add(function);
    }
    else
    {
        List<string> args = new();
        for (int i = 0; i < argCount; i++)
        {
            args.Add($"argument{i}");
        }

        ImportGMLString(
        functionName,
        @$"function {functionName} ({String.Join(", ", args)})
        {{
            {code}
        }}"
        );
    }
}