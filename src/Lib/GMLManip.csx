/// <summary>
/// Output test code to a file for debugging purposes
/// </summary>
/// <param name="code"></param>
void OutputCode (string code)
{
    File.WriteAllText(FilePath + "/../test.txt", code);
}

/// <summary>
/// Append to the end of a code entry
/// </summary>
/// <param name="codeName"></param>
/// <param name="code"></param>
void Append (string codeName, string code)
{
    Data.Code.ByName(codeName).AppendGML(code, Data);
}

/// <summary>
/// Replace inside a code entry
/// </summary>
/// <param name="codeName"></param>
/// <param name="text"></param>
/// <param name="replacement"></param>
void Replace (string codeName, string text, string replacement)
{
    OutputCode(replacement);
    ReplaceTextInGML(codeName, text, replacement);
}

/// <summary>
/// Place inside a code entry
/// </summary>
/// <param name="codeName"></param>
/// <param name="preceding"></param>
/// <param name="placement"></param>
void Place (string codeName, string preceding, string placement)
{
    OutputCode(placement);
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

/// <summary>
/// Create an object
/// </summary>
/// <param name="objectName"></param>
/// <returns></returns>
UndertaleGameObject CreateObject (string objectName)
{
    var obj = new UndertaleGameObject();
    obj.Name = new UndertaleString("obj_textbox_counter");
    Data.GameObjects.Add(obj);
    Data.Strings.Add(obj.Name);

    return obj;
}

/// <summary>
/// Create a function
/// </summary>
/// <param name="functionName"></param>
/// <param name="code"></param>
/// <param name="argCount"></param>
/// <param name="isScript">If creating with a `function` call or as a script</param>
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