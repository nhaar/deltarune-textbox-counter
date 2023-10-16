#load "..\Lib\GMLManip.csx"
#load "..\Lib\DecompileContext.csx"

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

/// <summary>
/// Setup the text counting system
/// </summary>
/// <param name="textReturningFunctions">
/// Functions that return the text strings
/// </param>
/// <param name="initiatorCode">Code entry to initiate the object</param>
/// <param name="drawFunctions">
/// Data of draw functions that need to be replaced
/// </param>
/// <param name="stringFunctions">
/// Data of the string functions that need to be replaced
/// </param>
/// <param name="exceptionEntries">
/// Data of the code entries that need not be replaced
/// </param>
/// <param name="isOldGMS">
/// If version < GMS 2.3
/// </param>
/// <returns></returns>
async Task SetupSystem
(
    string[] textReturningFunctions,
    string initiatorCode,
    Dictionary<string, int[]> drawFunctions,
    Dictionary<string, int[]> stringFunctions,
    string[] exceptionEntries,
    bool isOldGMS
)
{
    // a character pattern that can never used in a string (needs to be 13 char to account for possible names)
    string delimiter = "_K_K_K_K_K_K_";

    var mainObj = CreateObject("obj_textbox_counter");
    mainObj.Persistent = true;
    
    mainObj.EventHandlerFor(EventType.Create, Data).ReplaceGML(
    @$"
    directory_create(""textstuff"");

    global.read_text_map = ds_map_create();
    global.read_total = 0;

    var read = file_text_open_read(""textstuff/text.txt"");
    if (read == -1)
    {{
        var file = file_text_open_append(""textstuff/text.txt"");
        file_text_close(file);
    }}
    else
    {{
        while (true)
        {{
            var line = file_text_read_string(read);
            if (line == """")
                break;
            global.read_total++;
            ds_map_add(global.read_text_map, line, 1);
            file_text_readln(read);
        }}
        file_text_close(read);
    }}", Data);

    mainObj.EventHandlerFor(EventType.Draw, Data).ReplaceGML(@$"
    counter_draw_text(20, 0, ""Text total: "" + string(global.read_total));
    ", Data);

    Append(
    initiatorCode,
    @"if (!instance_exists(obj_textbox_counter))
        instance_create(0, 0, obj_textbox_counter);"
    );


    CreateFunction(
    "burn_text_id",
    @$"
    var str = argument0
    var localized_string_id = argument1
    var suffix = argument2
    if (is_undefined(suffix))
        suffix = """"
    var delimiter = ""{delimiter}""
    return (str + delimiter + localized_string_id + suffix + delimiter);
    ",
    3,
    isOldGMS
    );
    
    CreateFunction(
    "append_text_line",
    @"
    var line = argument0;
    if (!ds_map_exists(global.read_text_map, line))
    {
        show_debug_message(""Appended: "" + line);
        var file = file_text_open_append(""textstuff/text.txt"");
        ds_map_add(global.read_text_map, line, 1);
        global.read_total++;
        file_text_write_string(file, line);
        file_text_writeln(file);
        file_text_close(file);
    }",
    1,
    isOldGMS
    );
    
    CreateFunction(
    "clean_text_string",
    @$"
    var str = argument0
    if (!is_string(str))
        return str;
    var cancel_append = argument1
    var delimiter = ""{delimiter}""

    var start_index = string_pos(delimiter, str)
    var before_delimiter, rest, end_index, after_delimiter, between_delimiters
    while (start_index > 0)
    {{
        before_delimiter = string_copy(str, 1, (start_index - 1))
        rest = string_copy(str, (start_index + string_length(delimiter)), string_length(str))
        end_index = string_pos(delimiter, rest)
        between_delimiters = string_copy(rest, 1, end_index - 1)
        if (cancel_append != true)
            append_text_line(between_delimiters)
        after_delimiter = string_copy(rest, (end_index + string_length(delimiter)), string_length(str))
        str = before_delimiter + after_delimiter
        start_index = string_pos(delimiter, str)
    }}
    return str;
    ",
    2,
    isOldGMS
    );
        
    foreach (string drawFunction in drawFunctions.Keys)
    {
        CreateNewFunction(drawFunction, drawFunctions[drawFunction], true, isOldGMS);
    }


    foreach (string stringFunction in stringFunctions.Keys)
    {
        CreateNewFunction(stringFunction, stringFunctions[stringFunction], false, isOldGMS);
    }

    UndertaleCode[] AllCode = Data.Code.Where(c => c.ParentEntry == null).ToArray();
    List<UndertaleCode> toUpdate = new();
    ConcurrentDictionary<string, string> updatedCode = new();

    
    SetProgressBar(null, "Replacing Functions", 0, AllCode.Length);
    StartProgressBarUpdater();
    for (int i = 0; i < 5; i++)
    {
        Console.WriteLine(i);
        try
        {
            await Parallel.ForEachAsync(AllCode, async (code, cancellationToken) => ReplaceFunctions(code, textReturningFunctions, exceptionEntries, drawFunctions, stringFunctions, isOldGMS, updatedCode, toUpdate));
            break;
        }
        catch (System.Exception e)
        {
            if (i == 4)
                throw e;
        }
    }    

    Console.WriteLine("GEGEJGEO");
    await StopProgressBarUpdater();

    

    foreach (UndertaleCode code in toUpdate)
    {
        if (code.Name.Content != "gml_Script_scr_namingscreen")
        {    
            Console.WriteLine(code.Name.Content);
            Console.WriteLine("");
            OutputCode(updatedCode[code.Name.Content]);
            code.ReplaceGML(updatedCode[code.Name.Content], Data);
        }

    }
}

/// <summary>
/// Replace the functions across all files
/// </summary>
/// <param name="code"></param>
/// <param name="textReturningFunctions"></param>
/// <param name="exceptionEntries"></param>
/// <param name="drawFunctions"></param>
/// <param name="stringFunctions"></param>
/// <param name="isOldGMS"></param>
/// <param name="updatedCode"></param>
/// <param name="toUpdate"></param>
void ReplaceFunctions
(
    UndertaleCode code,
    string[] textReturningFunctions,
    string[] exceptionEntries,
    Dictionary<string, int[]> drawFunctions,
    Dictionary<string, int[]> stringFunctions,
    bool isOldGMS,
    ConcurrentDictionary<string, string> updatedCode,
    List<UndertaleCode> toUpdate
)
{
    var update = false;
    bool containsComp = false;
    bool containsStrFunction = false;

    var newFunctions = drawFunctions.Keys.Union(stringFunctions.Keys).ToList();
    // replace function names in assembly for old ones
    for (int i = 0; i < code.Instructions.Count; i++) 
    {
        if (code.Instructions[i].Kind == UndertaleInstruction.Opcode.Call)
        {
            var functionName = code.Instructions[i].Function.ToString();
            if (textReturningFunctions.Contains(functionName.Replace("gml_Script_", "")))
            {
                containsStrFunction = true;
            }
            else if
            (
                newFunctions.Contains(functionName) &&
                code.Name.Content != $"gml_GlobalScript_{functionName}" &&
                code.Name.Content != $"new_{functionName}" &&
                code.Name.Content != "clean_text_string" // avoid (infinite) circular call
            )
            {
                var newName = $"new_{functionName}";
                if (!isOldGMS) newName = "gml_Script_" + newName;
                code.Instructions[i].Function = new UndertaleInstruction.Reference<UndertaleFunction>(Data.Functions.ByName(newName));
            }
        }
        else if
        (
            code.Instructions[i].Kind == UndertaleInstruction.Opcode.Cmp &&
            (code.Instructions[i].Type1 == UndertaleInstruction.DataType.Variable ||code.Instructions[i].Type1 == UndertaleInstruction.DataType.String) &&
            (code.Instructions[i].Type2 == UndertaleInstruction.DataType.Variable ||code.Instructions[i].Type2 == UndertaleInstruction.DataType.String)
        )
        {
            containsComp = true;
        }
    }

    var possibleException = (containsComp && containsStrFunction);
    var isException = exceptionEntries.Contains(code.Name.Content);
    if (possibleException || isException)
    {        
        var codeContent = Decompile(code);
        if (possibleException)
        {
            foreach (string function in textReturningFunctions)
            {
                var operators = new[] { "==", "!=" };

                foreach (string o in operators)
                {
                    var lines = Regex.Split(codeContent, @$"(?={o}\s{function}\()");
                    var newLines = new List<string>();
                    newLines.Add(lines[0]);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        newLines.Add(AddClearToFunction(lines[i], function));
                    }
                    codeContent = String.Join("", newLines);
                }
            }
        }
        else if (isException)
        {
            codeContent = AddAutoClear(codeContent, textReturningFunctions);
        }

        updatedCode[code.Name.Content] = codeContent;
        toUpdate.Add(code);
    }
    IncrementProgressParallel();
}

/// <summary>
/// Add a clear text string to a function call
/// </summary>
/// <param name="line"></param>
/// <param name="function"></param>
/// <returns></returns>
string AddClearToFunction (string line, string function)
{
    int start = line.IndexOf(function);
    int j = start + function.Length;
    int depth = 0;
    do
    {
        char c = line[j];
        if (c == '(')
            depth++;
        else if (c == ')')
            depth--;
        j++;
    }
    while (depth > 0);
    return line.Substring(0, start) + "clean_text_string(" + line.Substring(start, j - start) + ", 1)" + line.Substring(j) + "\n";
}

/// <summary>
/// Add a clear text string to all functions that return text strings
/// </summary>
/// <param name="content"></param>
/// <param name="textReturningFunctions"></param>
/// <param name="startLine"></param>
/// <param name="endLine"></param>
/// <returns></returns>
string AddAutoClear (string content, string[] textReturningFunctions, int startLine = 0, int endLine = 0)
{
    var lines = content.Split("\n");
    if (endLine == 0)
        endLine = lines.Length;
    var finalContent = "";
    for (int i = 0; i < lines.Length; i++)
    {
        var newLine = "";
        var line = lines[i];
        if (i > startLine && i < endLine)
        {
            bool found = false;
            foreach (string function in textReturningFunctions)
            {
                if (Regex.IsMatch(line, @$"\b{function}\b"))
                {
                    finalContent += AddClearToFunction(line, function);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                finalContent += line + "\n";
            }
        }
        else
            finalContent += lines[i] + "\n";
    }
    return finalContent;
}

/// <summary>
/// Create a new function that calls the old one with a clear text string
/// </summary>
/// <param name="functionName"></param>
/// <param name="argsInfo"></param>
/// <param name="append"></param>
/// <param name="isOldGMS"></param>
void CreateNewFunction (string functionName, int[] argsInfo, bool append, bool isOldGMS)
{
    var argNames = new List<string>();
    for (int i = 0; i < argsInfo[1]; i++) {
        argNames.Add($"argument{i}");
    }
    var argString = $"({String.Join(", ", argNames)})";
    var cancelAppend = append ? "" : ", true";
    var callString = argString.Replace($"argument{argsInfo[0]}", $"clean_text_string(argument{argsInfo[0]}{cancelAppend})");
    var newFunction = $"new_{functionName}";
    CreateFunction(
    newFunction,
    $"return {functionName}{callString};",
    argsInfo[1],
    isOldGMS
    );
}
