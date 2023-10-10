using System.Threading;

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

/// <summary>
/// Generate GML code that adds a line to the textbox tracker
/// </summary>
/// <param name="line"></param>
/// <returns></returns>
string AddLine (string line)
{
    return @$"
    var line = {line};
    if (is_undefined(line))
    {{
        line = """";
        show_debug_message(""undefined line"" + global.msg[obj_writer.msgno]);
    }}
    else if (is_real(line))
    {{
        line = """";
        show_debug_message(""line not set"" + global.msg[obj_writer.msgno]);
    }}
    var file = file_text_open_append(""textstuff/text.txt"");
    var add = true;
    for (var i = 0; i < global.read_total; i++)
    {{
        if (global.read_text[i] == line)
        {{
            add = false;
            break;
        }}
    }}
    if (add)
    {{
        global.read_text[global.read_total] = line;
        global.read_total++;
        file_text_write_string(file, line + ""\n"");
    }}
    file_text_close(file);
    ";
}

// always add when it is created
Append("gml_Object_obj_writer_Create_0", AddLine("global.msg_id[0]"));

// adding it when next message is called
Place("gml_GlobalScript_scr_nextmsg", "mystring = nstring[msgno]", AddLine("global.msg_id[msgno]"));

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

SetProgressBar(null, "Code Entries", 0, codeList.Count);
StartProgressBarUpdater();

// single-threaded approach for now: multi-threated showed some issues
foreach (UndertaleCode code in codeList)
{
    await Task.Run(() => ReplaceGlobalMessages(code));
}


await StopProgressBarUpdater();


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

/// <summary>
/// Replace the <c>global.msg[] = stringsetloc()</c> statements with <c>global.msg_id[] = ""</c> in all files
/// </summary>
/// <param name="code"></param>
void ReplaceGlobalMessages (UndertaleCode code)
{
    var content = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);

    Regex regex = new Regex(@"^.*global\.msg\[\d+\] = stringsetloc\(.*\).*$", RegexOptions.Multiline);
    if (regex.IsMatch(content))
    {
        var replaced = regex.Replace(AddSafeBraces(content), (match) =>
        {
            var value = match.Value;
            var messageIndex = Regex.Match(value, @"(?<=global\.msg\[)\d+(?=\])").Value;
            var textId = Regex.Match(value, @"(?<=stringsetloc\(.*?"")[\d\w]*(?=""\))").Value;
            return @$"
            {value}
            global.msg_id[{messageIndex}] = ""{textId}"";
            ";
        });

        Replace(code.Name.Content, content, replaced);
    }
    IncrementProgressParallel();
}