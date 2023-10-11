using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Xml;

static class Global
{
    public static ConcurrentDictionary<string, List<ParallelAction>> ParallelFiles = new ConcurrentDictionary<string, List<ParallelAction>>();
}

using (XmlReader reader = XmlReader.Create(ScriptPath + "/../parallel.xml"))
{
    var currentFile = "";
    while (reader.Read())
    {
        if (reader.NodeType == XmlNodeType.Element && reader.Name == "file")
        {
            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "name":
                        {
                            reader.Read();
                            currentFile = reader.Value;
                            Console.WriteLine(currentFile);
                            Global.ParallelFiles[currentFile] = new List<ParallelAction>();
                            break;
                        }
                        case "destructure":
                        {
                            var thisVariable = "";
                            var thisDimension = "0";
                            do
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "variable":
                                        {
                                            reader.Read();
                                            thisVariable = reader.Value;
                                            break;
                                        }
                                        case "dim":
                                        {
                                            reader.Read();
                                            thisDimension = reader.Value;
                                            break;
                                        }
                                    }
                                }
                                reader.Read();
                            }
                            while (reader.Name != "destructure");
                            Global.ParallelFiles[currentFile].Add(new ParallelAction(thisVariable, ParallelActionType.Destructure, thisDimension));
                            break;
                        }
                        case "init":
                        {
                            reader.Read();
                            var line = reader.Value;
                            Global.ParallelFiles[currentFile].Add(new ParallelAction(line, ParallelActionType.Initialization));
                            break;
                        }
                        case "exchange":
                        {
                            reader.Read();
                            var line = reader.Value;
                            Global.ParallelFiles[currentFile].Add(new ParallelAction(line, ParallelActionType.Exchange));
                            break;
                        }
                        case "draw":
                        {
                            var thisLine = "";
                            var thisVariable = "";
                            do
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "line":
                                        {
                                            reader.Read();
                                            thisLine = reader.Value;
                                            break;
                                        }
                                        case "variable":
                                        {
                                            reader.Read();
                                            thisVariable = reader.Value;
                                            break;
                                        }
                                    }
                                }
                                reader.Read();
                            }
                            while (reader.Name != "draw");
                            Global.ParallelFiles[currentFile].Add(new ParallelAction(thisLine, ParallelActionType.Append, thisVariable));
                            break;
                        }
                    }
                }
                reader.Read();
            }
            while (reader.Name != "file");
        }
    }
}

EnsureDataLoaded();

// needed for decompiling the code entries
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));


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
/// Replace text in a code entry
/// </summary>
/// <param name="codeName"></param>
/// <param name="text"></param>
/// <param name="replacement"></param>
void Replace (string codeName, string text, string replacement)
{
    // File.WriteAllText(FilePath + "/../test.txt", replacement);
    ReplaceTextInGML(codeName, text, replacement);
}

/// <summary>
/// Place a text inside a code entry
/// </summary>
/// <param name="codeName"></param>
/// <param name="preceding"></param>
/// <param name="placement"></param>
void Place (string codeName, string preceding, string placement)
{
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

/// <summary>
/// Generate GML code to draw text at a position
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="text"></param>
/// <returns></returns>
string DrawText (string x, string y, string text)
{
    return $"draw_text((__view_get((0 << 0), 0) + {x}), (__view_get((1 << 0), 0) + {y}), {text})";
}

// set up the basic text counter stuff
Append("gml_Object_obj_time_Create_0", @$"
global.msg_id[100] = """";
directory_create(""textstuff"");

global.read_text[12666] = 0;
global.read_total = 0;

var read = file_text_open_read(""textstuff/text.txt"");
if (read == -1)
{{
    var file = file_text_open_append(""textstuff/text.txt"");
    file_text_close(file);
}}
else
{{
    for (var i = 0; i < 12667; i++)
    {{
        var line = file_text_read_string(read);
        if (line == """")
            break;
        global.read_total++;
        global.read_text[i] = line;
        file_text_readln(read);
    }}
    file_text_close(read);
}}
");

ImportGMLFile(ScriptPath + "/../append_text_line.gml");

// always add when it is created
Append("gml_Object_obj_writer_Create_0", "append_text_line(global.msg_id[0], global.msg[0]);");

// adding it when next message is called
Place("gml_GlobalScript_scr_nextmsg", "mystring = nstring[msgno]", "append_text_line(global.msg_id[msgno], global.msg[msgno]);");

// modifying functions to automatically track the text id
Replace("gml_GlobalScript_msgset", @"function msgset(argument0, argument1) //gml_Script_msgset
{
    global.msgno = argument0
    global.msg[argument0] = argument1
}", @"function msgset(argument0, argument1, argument2) //gml_Script_msgset
{
    global.msgno = argument0
    global.msg[argument0] = argument1
    global.msg_id[argument0] = argument2
}");

Replace("gml_GlobalScript_msgsetloc", @"function msgsetloc(argument0, argument1, argument2) //gml_Script_msgsetloc
{
    var msg_index = argument0
    var str = argument1
    var localized_string_id = argument2
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgset(msg_index, str)
}", @"function msgsetloc(argument0, argument1, argument2) //gml_Script_msgsetloc
{
    var msg_index = argument0
    var str = argument1
    var localized_string_id = argument2
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgset(msg_index, str, argument2)
}");

Replace(
"gml_GlobalScript_c_msgset",
@"function c_msgset(argument0, argument1) //gml_Script_c_msgset
{
    c_cmd(""msgset"", argument0, argument1, 0, 0)
}",
@"function c_msgset(argument0, argument1, argument2) //gml_Script_c_msgset
{
    c_cmd(""msgset"", argument0, argument1, argument2, 0)
}"
);

Replace(
"gml_GlobalScript_msgsetsubloc",
"msgset(msg_index, str)",
"msgset(msg_index, str, localized_format_string_id)"
);

Replace(
"gml_GlobalScript_scr_cutscene_commands",
@"    if (_c == ""msgset"")
        msgset(command_arg1[i], command_arg2[i])",
@"    if (_c == ""msgset"")
        msgset(command_arg1[i], command_arg2[i], command_arg3[i])"
);

Replace("gml_GlobalScript_c_msgsetloc", @"function c_msgsetloc(argument0, argument1, argument2) //gml_Script_c_msgsetloc
{
    var msg_index = argument0
    var english = argument1
    var localized_string_id = argument2
    var str = english
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgset(msg_index, str)
}
", @"function c_msgsetloc(argument0, argument1, argument2) //gml_Script_c_msgsetloc
{
    var msg_index = argument0
    var english = argument1
    var localized_string_id = argument2
    var str = english
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgset(msg_index, str, argument2)
}
");

Replace(
"gml_GlobalScript_msgnext",
@"function msgnext(argument0) //gml_Script_msgnext
{
    global.msgno++
    msgset(global.msgno, argument0)
}",
@"function msgnext(argument0, argument1) //gml_Script_msgnext
{
    global.msgno++
    msgset(global.msgno, argument0, argument1)
}"
);

Replace(
"gml_GlobalScript_msgnextloc",
@"function msgnextloc(argument0, argument1) //gml_Script_msgnextloc
{
    var str = argument0
    var localized_string_id = argument1
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgnext(str)
}",
@"function msgnextloc(argument0, argument1) //gml_Script_msgnextloc
{
    var str = argument0
    var localized_string_id = argument1
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgnext(str, argument1)
}"
);

Replace(
"gml_GlobalScript_msgnextsubloc",
"msgnext(str)",
"msgnext(str, localized_string_id)"
);

Replace(
"gml_GlobalScript_c_msgnext",
@"function c_msgnext(argument0) //gml_Script_c_msgnext
{
    c_cmd(""msgnext"", argument0, 0, 0, 0)
}",
@"function c_msgnext(argument0, argument1) //gml_Script_c_msgnext
{
    c_cmd(""msgnext"", argument0, argument1, 0, 0)
}"
);

Replace(
"gml_GlobalScript_scr_cutscene_commands",
@"    if (_c == ""msgnext"")
        msgnext(command_arg1[i])",
@"    if (_c == ""msgnext"")
        msgnext(command_arg1[i], command_arg2[i])"
);

Replace(
"gml_GlobalScript_c_msgnextloc",
@"function c_msgnextloc(argument0, argument1) //gml_Script_c_msgnextloc
{
    var str = argument0
    var localized_string_id = argument1
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgnext(str)
}",
@"function c_msgnextloc(argument0, argument1) //gml_Script_c_msgnextloc
{
    var str = argument0
    var localized_string_id = argument1
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgnext(str, argument1)
}"
);

void UseDebug ()
{
    // enable debug mode
    Replace("gml_GlobalScript_scr_gamestart", "global.debug = false", "global.debug = true");
    Replace("gml_Object_obj_cutscene_master_Draw_64", "global.debug == true", "0");

    // variables to print
    string[] watchVars =
    {
        "global.msg[0]",
        "global.msg_id[0]",
        "msgno",
        "global.msg_id[msgno]"
    };

    // start with line break just to not interefere with anything
    string code = @"
    draw_set_color(c_red);
    ";
    int i = 0;
    foreach (string watchVar in watchVars)
    {
        code += DrawText("20", (i * 25).ToString(), $"\"{watchVar}: \" + string({watchVar})");
        i++;
    }
    Append("gml_Object_obj_writer_Draw_0", code);
}

List<UndertaleCode> codeList = new List<UndertaleCode>();
foreach (UndertaleCode code in Data.Code)
{
    // for now only ch2 specific code is being searched
    // don't know why parent entry needs to be null but that is how it is in ExportAllCode.csx
    if (code.ParentEntry == null && !code.Name.Content.Contains("ch1"))
        codeList.Add(code);
}

// first going to see which ones will be replaced
// this done can be done in parallel to speed up the process
SetProgressBar(null, "Code Entry Formatting", 0, codeList.Count);
StartProgressBarUpdater();

var newCode = new ConcurrentDictionary<string, string>();
var toUpdate = new List<UndertaleCode>();

await MainReplace();
await StopProgressBarUpdater();

int i = 0;
int total = toUpdate.Count;
// now this one can't be done in parallel since it will lead to issues
foreach (UndertaleCode code in toUpdate)
{
    Console.WriteLine($"Replacing {code.Name.Content} ({i}/{total})");
    i++;
    code.ReplaceGML(newCode[code.Name.Content], Data);
}

UseDebug();

async Task MainReplace ()
{
    await Parallel.ForEachAsync(codeList, async (code, cancellationToken) => MainReplace(code));
}

/// <summary>
/// Add braces to every if, else if and else statement to make it safe for replacing
/// </summary>
/// <param name="code"></param>
/// <returns></returns>
string AddSafeBraces (string code)
{
    Regex ifRegex = new Regex(@"^[^\S\n]*(?:else )?if\s.*\n\s*[^{\s].*$", RegexOptions.Multiline);
    Regex elseRegex = new Regex(@"^.*else\n\s*[^{\s].*$", RegexOptions.Multiline);
    var safeIf = ifRegex.Replace(code, (match) =>
    {
        var value = match.Value;
        var condition = Regex.Match(value, @"(?<=if\s).*").Value;
        bool isElse = value.Contains("else if");
        var isElseString = isElse ? "else " : "";
        var statement = Regex.Match(value, @"(?<=\)\n\s*).*").Value;
        return @$"
        {isElseString} if ({condition})
        {{
        {statement}
        }}
        ";
    });

    return elseRegex.Replace(safeIf, (match) =>
    {
        var value = match.Value;
        var statement = Regex.Match(value, @"(?<=else\n\s*).*").Value;
        return @$"
        else
        {{
        {statement}
        }}
        ";
    });
}   

class Assignment
{
    public string LeftSide;

    public string RightSide;

    public Assignment (string assignmentString)
    {
        var assignment = assignmentString.Split('=');
        LeftSide = assignment[0].Trim();
        RightSide = assignment[1].Trim();
    }

    public Assignment (string leftSide, string rightSide)
    {
        LeftSide = leftSide;
        RightSide = rightSide;
    }

    public override string ToString ()
    {
        return $"{LeftSide} = {RightSide}";
    }
}

class AssignmentVariable
{
    public string Name;

    public string Brackets;

    public AssignmentVariable (string variableString)
    {
        if (variableString.Contains("["))
        {
            Name = Regex.Match(variableString, @"[\w\d_\.]+(?=\[)").Value;
            Brackets = variableString.Replace(Name, "");
        }
        else
        {
            Name = variableString;
            Brackets = "";
        }
    }

    public override string ToString ()
    {
        return $"{Name}{Brackets}";
    }
}

string PlaceInString (string str, string line, string placement)
{
    return str.Replace(line, $"{line}\n{placement}");
}

string AddParallelInitialization (string content, string originalLine)
{
    var assignment = new Assignment(originalLine);
    var leftSide = new AssignmentVariable(assignment.LeftSide);
    return PlaceInString(content, originalLine, @$"
    {leftSide.Name}_id{leftSide.Brackets} = 0;
    ");
}


string AddParallelExchange (string content, string originalLine)
{
    var assignment = new Assignment(originalLine);
    var leftSide = new AssignmentVariable(assignment.LeftSide);
    var rightSide = new AssignmentVariable(assignment.RightSide);
    var exchange = @$"
    {leftSide.Name}_id{leftSide.Brackets} = {rightSide.Name}_id{rightSide.Brackets};
    ";

    return PlaceInString(content, originalLine, exchange);
}

string AddLineAppend (string content, string drawLine, string drawVariable)
{
    var originalVariable = new AssignmentVariable(drawVariable);
    var newVariable = new AssignmentVariable($"{originalVariable.Name}_id{originalVariable.Brackets}");
    var appendCode = $"append_text_line({newVariable}, {originalVariable})";
    return PlaceInString(content, drawLine, appendCode);
}

enum ParallelActionType
{
    Initialization,
    Exchange,
    Append,
    Destructure
}

struct ParallelAction
{
    public string Arg1;

    public string Arg2;

    public ParallelActionType Type;

    public ParallelAction (string arg1, ParallelActionType type, string arg2 = "")
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Type = type;
    }
}

class StringsetlocCall
{
    public string Call;

    public string TextId;

    public StringsetlocCall (string call)
    {
        Call = call;
        TextId = Regex.Match(call, @"(?<=stringsetloc\(.*?"")[\d\w]*(?=""\))").Value;
    }
}

class StringsetlocPattern
{
    public string VariableName;

    public int ArrayDimension;

    public Regex Pattern;

    public StringsetlocPattern (string variableName, int arrayDimension = 0)
    {
        VariableName = variableName;
        var namePattern = VariableName.Replace(".", @"\.");
        ArrayDimension = arrayDimension;
        var bracketPattern = ArrayDimension > 0 ? @$"\[[\w\d_\.]+\]{{{ArrayDimension}}}" : "";
        Pattern = new Regex(@$"^\s*{namePattern}{bracketPattern}\s=\sstringsetloc\(.*?\)\s*$", RegexOptions.Multiline);
    }

    public bool IsMatch (string input)
    {
        return Pattern.IsMatch(input);
    }

    public string Replace (string input)
    {
        return Pattern.Replace(input, (match) => DestructureStringset(match.Value));
    }

    public string DestructureStringset (string assignmentLine)
    {
        var assignment = new Assignment(assignmentLine);
        var leftSide = new AssignmentVariable(assignment.LeftSide);
        var rightSide = new StringsetlocCall(assignment.RightSide);
        var newLeftSide = new AssignmentVariable($"{leftSide.Name}_id{leftSide.Brackets}");
        return $@"
        {assignmentLine}
        {new Assignment(newLeftSide.ToString(), $"\"{rightSide.TextId}\"")}
        ";
    }
}

void MainReplace (UndertaleCode code)
{
    var codeName = code.Name.Content;
    var content = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
    string replaced = AddSafeBraces(content);
    bool changed = false;

    void processAction (ParallelAction action)
    {
        switch (action.Type)
        {
            case ParallelActionType.Initialization:
            {
                changed = true;
                replaced = AddParallelInitialization(replaced, action.Arg1);
                break;
            }
            case ParallelActionType.Exchange:
            {
                changed = true;
                replaced = AddParallelExchange(replaced, action.Arg1);
                break;
            }
            case ParallelActionType.Append:
            {
                changed = true;
                replaced = AddLineAppend(replaced, action.Arg1, action.Arg2);
                break;
            }
            case ParallelActionType.Destructure:
            {
                var pattern = new StringsetlocPattern(action.Arg1, Int32.Parse(action.Arg2));
                if (pattern.IsMatch(replaced))
                {
                    changed = true;
                    replaced = pattern.Replace(replaced);
                }
                break;
            }
        }
    }

    if (Global.ParallelFiles.ContainsKey(codeName))
    {
        var parallelActions = Global.ParallelFiles[codeName];
        foreach (ParallelAction action in parallelActions)
        {
            processAction(action);
        }
    }

    foreach (ParallelAction action in Global.ParallelFiles["global"])
    {
        processAction(action);       
    }

    if (changed)
    {
        newCode[codeName] = replaced;
        toUpdate.Add(code);
    }
    IncrementProgressParallel();
}