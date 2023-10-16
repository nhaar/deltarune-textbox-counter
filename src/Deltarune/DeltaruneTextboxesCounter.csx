#load "..\Lib\DecompileContext.csx"

using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

// very odd try catch hook that needs to be removed or modtool can't compile it
Replace(
"gml_Object_obj_tensionbar_Draw_0",
@"@@try_hook@@(2224, 2272)
if (global.tensionselect >= 0)
    shit = 1
@@try_unhook@@()",
""
);

async Task SetupSystem
(
    string[] textReturningFunctions,
    string initiatorObject,
    Dictionary<string, int[]> drawFunctions,
    Dictionary<string, int[]> stringFunctions,
    string[] exceptionEntries
)
{

    // a character pattern that can never used in a string (needs to be 13 char to account for possible names)
    string delimiter = "_K_K_K_K_K_K_";

    var mainObj = new UndertaleGameObject();
    mainObj.Persistent = true;
    mainObj.Name = new UndertaleString("obj_textbox_counter");
    Data.GameObjects.Add(mainObj);
    Data.Strings.Add(mainObj.Name);

    
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

    ImportGMLString(
    "counter_draw_text",
    @"function counter_draw_text(argument0, argument1, argument2)
    {
        draw_set_font(fnt_main);
        draw_set_color(c_white);
        var xpos = argument0
        var ypos = argument1
        var str = argument2
        draw_text((__view_get((0 << 0), 0) + xpos), (__view_get((1 << 0), 0) + ypos), str)
    }"
    );

    mainObj.EventHandlerFor(EventType.Draw, Data).ReplaceGML(@$"
    counter_draw_text(20, 0, ""Text total: "" + string(global.read_total));
    ", Data);

    Append(
    initiatorObject,
    @"if (!instance_exists(obj_textbox_counter))
        instance_create(0, 0, obj_textbox_counter);"
    );

    ImportGMLString(
    "burn_text_id",
    @$"function burn_text_id(argument0, argument1, argument2)
    {{
        var str = argument0
        var localized_string_id = argument1
        var suffix = argument2
        if (is_undefined(suffix))
            suffix = """"
        var delimiter = ""{delimiter}""
        return (str + delimiter + localized_string_id + suffix + delimiter);
    }}"
    );

    
    ImportGMLString(
    "append_text_line.gml",
    @"function append_text_line (argument0)
    {
        var line = argument0;
        if (!ds_map_exists(global.read_text_map, line))
        {
            show_debug_message(""Appended: "" + line);
            var file = file_text_open_append(""textstuff/text.txt"");
            ds_map_add(global.read_text_map, line, 1);
            global.read_total++;
            file_text_write_string(file, line + ""\n"");
            file_text_close(file);
        }
    }"
    );

    
    ImportGMLString(
    "clean_text_string",
    @$"function clean_text_string(argument0, argument1)
    {{
        var str = argument0
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
    }}"
    );
        
    foreach (string drawFunction in drawFunctions.Keys)
    {
        CreateNewFunction(drawFunction, drawFunctions[drawFunction], true);
    }

    foreach (string stringFunction in stringFunctions.Keys)
    {
        CreateNewFunction(stringFunction, stringFunctions[stringFunction], false);
    }

    UndertaleCode[] AllCode = Data.Code.Where(c => c.ParentEntry == null).ToArray();
    List<UndertaleCode> toUpdate = new();
    ConcurrentDictionary<string, string> updatedCode = new();

    
    SetProgressBar(null, "Replacing Functions", 0, AllCode.Length);
    StartProgressBarUpdater();
    for (int i = 0; i < 5; i++)
    {
        try
        {
            await Parallel.ForEachAsync(AllCode, async (code, cancellationToken) => ReplaceDrawFunctions(code, textReturningFunctions, exceptionEntries, drawFunctions, stringFunctions, updatedCode, toUpdate));
            break;
        }
        catch
        {
        }
    }    
    await StopProgressBarUpdater();

    foreach (UndertaleCode code in toUpdate)
    {
        Console.WriteLine(code.Name.Content);
        Console.WriteLine("");
        OutputCode(updatedCode[code.Name.Content]);
        code.ReplaceGML(updatedCode[code.Name.Content], Data);

    }
}

await SetupSystem
(
    new[] { "stringset", "scr_84_get_lang_string_ch1", "stringsetloc", "stringsetsubloc" },
    @"gml_Object_obj_CHAPTER_SELECT_Create_0",
    new()
    {
        { "draw_text_shadow", new[] { 2, 3 } },
        { "draw_text_transformed", new[] { 2, 6 } },
        { "draw_text_width", new[] { 2, 4 } },
        { "draw_text", new[] { 2, 3 } },
        // required for colored monster names
        { "draw_text_colour", new[] { 2, 8 } },
        // initially required for the "HIT" text in punchout
        { "draw_text_ext", new[] { 2, 5 } },
        // initially required for the in-queen battle text like "Defenseless"
        { "draw_text_ext_transformed", new[] { 2, 8 } },
        { "window_set_caption", new[] { 0, 1 } }
    },
    new()
    {
        // initially required for displaying enemy names with proper width in battle
        { "string_width", new[] { 0, 1 } },
        // these two were initially required for spelling bee
        { "string_length", new[] { 0, 1 } },
        { "string_char_at", new[] { 0, 2 } }
    },
    new[]
    {
        "gml_GlobalScript_scr_asterskip_ch1",
        "gml_Object_obj_writer_ch1_Draw_0"
    }
);
UseDebug();

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
    msgnext(str, localized_string_id)
}"
);

Replace(
"gml_GlobalScript_stringsetsubloc",
@"function stringsetsubloc() //gml_Script_stringsetsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var format_string = argument[0]
    var localized_format_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_format_string_id)
    var str = substringargs(format_string, 1, args)
    return stringset(str);
}",
@"function stringsetsubloc() //gml_Script_stringsetsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var format_string = argument[0]
    var localized_format_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_format_string_id)
    var str = substringargs(format_string, 1, args)
    return stringset(str, localized_format_string_id);
}"
);

Replace(
"gml_GlobalScript_stringsetloc",
@"function stringsetloc(argument0, argument1) //gml_Script_stringsetloc
{
    var str = argument0
    if (!is_english())
        str = scr_84_get_lang_string(argument1)
    return stringset(str);
}",
@"function stringsetloc(argument0, argument1) //gml_Script_stringsetloc
{
    var str = argument0
    if (!is_english())
        str = scr_84_get_lang_string(argument1)
    return stringset(str, argument1);
}"
);

Replace(
"gml_GlobalScript_msgsetloc",
@"function msgsetloc(argument0, argument1, argument2) //gml_Script_msgsetloc
{
    var msg_index = argument0
    var str = argument1
    var localized_string_id = argument2
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgset(msg_index, str)
}",
@"function msgsetloc(argument0, argument1, argument2) //gml_Script_msgsetloc
{
    var msg_index = argument0
    var str = argument1
    var localized_string_id = argument2
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    msgset(msg_index, str, localized_string_id)
}"
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
    c_msgnext(str, localized_string_id)
}"
);

Replace(
"gml_GlobalScript_c_msgsetloc",
@"function c_msgsetloc(argument0, argument1, argument2) //gml_Script_c_msgsetloc
{
    var msg_index = argument0
    var english = argument1
    var localized_string_id = argument2
    var str = english
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgset(msg_index, str)
}",
@"function c_msgsetloc(argument0, argument1, argument2) //gml_Script_c_msgsetloc
{
    var msg_index = argument0
    var english = argument1
    var localized_string_id = argument2
    var str = english
    if (!is_english())
        str = scr_84_get_lang_string(localized_string_id)
    c_msgset(msg_index, str, localized_string_id)
}"
);

Replace(
"gml_GlobalScript_msgnextsubloc",
@"function msgnextsubloc() //gml_Script_msgnextsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var format_string = argument[0]
    var localized_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_string_id)
    var str = substringargs(format_string, 1, args)
    msgnext(str)
}",
@"function msgnextsubloc() //gml_Script_msgnextsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var format_string = argument[0]
    var localized_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_string_id)
    var str = substringargs(format_string, 1, args)
    msgnext(str, localized_string_id)
}"
);

Replace(
@"gml_GlobalScript_msgsetsubloc",
@"function msgsetsubloc() //gml_Script_msgsetsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var msg_index = argument[0]
    var format_string = argument[1]
    var localized_format_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_format_string_id)
    var str = substringargs(format_string, 2, args)
    msgset(msg_index, str)
}
",
@"function msgsetsubloc() //gml_Script_msgsetsubloc
{
    var len = argument_count
    for (var i = 0; i < len; i++)
        args[i] = argument[i]
    var msg_index = argument[0]
    var format_string = argument[1]
    var localized_format_string_id = argument[(len - 1)]
    if (!is_english())
        format_string = scr_84_get_lang_string(localized_format_string_id)
    var str = substringargs(format_string, 2, args)
    msgset(msg_index, str, localized_format_string_id)
}
"
);

Replace(
@"gml_GlobalScript_msgnext",
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

//

ImportGMLString(
"deltarune_burn_text_id",
@"
function deltarune_burn_text_id(argument0, argument1, argument2)
{
    var str = argument0
    var localized_string_id = argument1
    var chapter = argument2
    var suffix
    if (!is_undefined(localized_string_id))
        suffix = ""_ch"" + string(chapter)
    return burn_text_id(str, localized_string_id, suffix)
}
");

//

Replace(
"gml_GlobalScript_scr_84_get_lang_string_ch1",
@"function scr_84_get_lang_string_ch1(argument0) //gml_Script_scr_84_get_lang_string_ch1
{
    return ds_map_find_value(global.lang_map, argument0);
}",
@"function scr_84_get_lang_string_ch1(argument0) //gml_Script_scr_84_get_lang_string_ch1
{
    return deltarune_burn_text_id(ds_map_find_value(global.lang_map, argument0), argument0, 1);
}"
);

Replace(
@"gml_GlobalScript_msgset",
@"function msgset(argument0, argument1) //gml_Script_msgset
{
    global.msgno = argument0
    global.msg[argument0] = argument1
}",
@"function msgset(argument0, argument1, argument2) //gml_Script_msgset
{
    global.msgno = argument0
    global.msg[argument0] = deltarune_burn_text_id(argument1, argument2, 2)
}"
);

Replace(
"gml_GlobalScript_stringset",
@"function stringset(argument0) //gml_Script_stringset
{
    return argument0;
}",
@"function stringset(argument0, argumen1) //gml_Script_stringset
{
    return deltarune_burn_text_id(argument0, argument1, 2);
}"
);

Replace(
"gml_GlobalScript_c_msgnext",
@"function c_msgnext(argument0) //gml_Script_c_msgnext
{
    c_cmd(""msgnext"", argument0, 0, 0, 0)
}",
@"function c_msgnext(argument0) //gml_Script_c_msgnext
{
    c_cmd(""msgnext"", argument0, argument1, 0, 0)
}"
);

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
"gml_GlobalScript_scr_cutscene_commands",
@"  if (_c == ""msgset"")
        msgset(command_arg1[i], command_arg2[i])
    if (_c == ""msgnext"")
        msgnext(command_arg1[i])",
@"  if (_c == ""msgset"")
        msgset(command_arg1[i], command_arg2[i], command_arg3[i])
    if (_c == ""msgnext"")
        msgnext(command_arg1[i], command_arg2[i])"
);

// take care of everything in obj_writer

// ch1
Place(
"gml_Object_obj_writer_ch1_Create_0",
"specfade = 1",
"global.msg[0] = clean_text_string(global.msg[0])"
);

Replace(
"gml_GlobalScript_scr_nextmsg_ch1",
"mystring = nstring[msgno]",
"mystring = clean_text_string(nstring[msgno])"
);

// ch2
Place(
"gml_Object_obj_writer_Create_0",
"miniface_drawn = 0",
"global.msg[0] = clean_text_string(global.msg[0])"
);

Replace(
"gml_GlobalScript_scr_nextmsg",
"mystring = nstring[msgno]",
"mystring = clean_text_string(nstring[msgno])"
);

// go to every draw function and replace it!


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

void ReplaceDrawFunctions (UndertaleCode code, string[] textReturningFunctions, string[] exceptionEntries, Dictionary<string, int[]> drawFunctions, Dictionary<string, int[]> stringFunctions, ConcurrentDictionary<string, string> updatedCode, List<UndertaleCode> toUpdate)
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
                code.Instructions[i].Function = new UndertaleInstruction.Reference<UndertaleFunction>(Data.Functions.ByName($"gml_Script_new_{functionName}"));
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
        var codeContent = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
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

void CreateNewFunction (string functionName, int[] argsInfo, bool append)
{
    var argNames = new List<string>();
    for (int i = 0; i < argsInfo[1]; i++) {
        argNames.Add($"argument{i}");
    }
    var argString = $"({String.Join(", ", argNames)})";
    var cancelAppend = append ? "" : ", true";
    var callString = argString.Replace($"argument{argsInfo[0]}", $"clean_text_string(argument{argsInfo[0]}{cancelAppend})");
    var newFunction = $"new_{functionName}";
    ImportGMLString(newFunction, @$"
    function {newFunction}{argString}
    {{
        return {functionName}{callString};
    }}
    ");
}

void UseDebug ()
{
    // debug mode ch1
    Data.Code.ByName("gml_GlobalScript_scr_debug_ch1").ReplaceGML(@"
    function scr_debug_ch1() { return true; }
    ", Data);

    // enable debug mode ch2
    Replace("gml_GlobalScript_scr_gamestart", "global.debug = false", "global.debug = true");
    Replace("gml_Object_obj_cutscene_master_Draw_64", "global.debug == true", "0");
}