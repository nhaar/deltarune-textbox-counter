using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

UseDebug();

// very odd try catch hook that needs to be removed or modtool can't compile it
Replace(
"gml_Object_obj_tensionbar_Draw_0",
@"@@try_hook@@(2224, 2272)
if (global.tensionselect >= 0)
    shit = 1
@@try_unhook@@()",
@""
);

Append(
"gml_Object_obj_time_Create_0",
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
}}
"
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

// a two character pattern that is never used (with this capitalization)
string Delimiter = "AJ";

ImportGMLString(
"burn_text_id",
@$"function burn_text_id(argument0, argument1)
{{
    var str = argument0
    var localized_string_id = argument1
    var delimiter = ""{Delimiter}""
    var suffix
    if (is_undefined(localized_string_id))
    {{
        suffix = """"
    }}
    else
    {{
        suffix = delimiter + localized_string_id + delimiter
    }}
    return (str + suffix);
}}"
);

//

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
    global.msg[argument0] = burn_text_id(argument1, argument2)
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
    return burn_text_id(argument0, argument1);
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

ImportGMLString(
"clean_text_string",
@$"function clean_text_string(argument0, argument1)
{{
    var str = argument0
    var cancel_append = argument1
    var delimiter = ""{Delimiter}""

    var start_index = string_pos(delimiter, str)
    var before_delimiter, rest, end_index, after_delimiter, between_delimiters
    while (start_index > 0)
    {{
        before_delimiter = string_copy(str, 1, (start_index - 1))
        rest = string_copy(str, (start_index + string_length(delimiter)), string_length(str))
        between_delimiters = string_copy(rest, 1, string_length(rest) - string_length(delimiter))
        if (cancel_append != true)
            append_text_line(between_delimiters)
        end_index = string_pos(delimiter, rest)
        after_delimiter = string_copy(rest, (end_index + string_length(delimiter)), string_length(str))
        str = before_delimiter + after_delimiter
        start_index = string_pos(delimiter, str)
    }}
    return str;
}}"
);

ImportGMLString(
"new_string_width",
@$"function new_string_width(argument0)
{{
    return string_width(clean_text_string(argument0, true));
}}"
);

// take care of everything in obj_writer

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
Dictionary<string, int[]> DrawFunctions = new()
{
    { "draw_text_shadow", new[] { 2, 3 } },
    { "draw_text_transformed", new[] { 2, 6 } },
    { "draw_text_width", new[] { 2, 4 } },
    { "draw_text", new[] { 2, 3 } },
};

Dictionary<string, int[]> StringFunctions = new()
{
    // initially required for displaying enemy names with proper width in battle
    { "string_width", new[] { 0, 1 } }
};
};

foreach (string drawFunction in DrawFunctions.Keys)
{
    CreateNewFunction(drawFunction, DrawFunctions[drawFunction], true);
}

foreach (string stringFunction in StringFunctions.Keys)
{
    CreateNewFunction(stringFunction, StringFunctions[stringFunction], false);
}

UndertaleCode[] Ch2Code = Data.Code.Where(c => c.ParentEntry == null && !c.Name.Content.Contains("ch1")).ToArray();
List<UndertaleCode> ToUpdate = new();
ConcurrentDictionary<string, string> UpdatedCode = new();


SetProgressBar(null, "Replacing Functions", 0, Ch2Code.Length);
StartProgressBarUpdater();
await ReplaceDrawFunctions();
await StopProgressBarUpdater();

Console.WriteLine(ToUpdate.Count);

foreach (UndertaleCode code in ToUpdate)
{
    Console.WriteLine(code.Name.Content);
    Console.WriteLine("");
    OutputCode(UpdatedCode[code.Name.Content]);
    code.ReplaceGML(UpdatedCode[code.Name.Content], Data);

}

async Task ReplaceDrawFunctions ()
{
    await Parallel.ForEachAsync(Ch2Code, async (Ch2Code, cancellationToken) => ReplaceDrawFunctions(Ch2Code));
}

void ReplaceDrawFunctions (UndertaleCode code)
{
    var codeContent = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
    var update = false;

    var newFunctions = DrawFunctions.Keys.Union(StringFunctions.Keys);
    foreach (string function in newFunctions)
    {   
        var functionRegex = new Regex($@"\b{function}\b");
        if (
            functionRegex.IsMatch(codeContent) &&
            code.Name.Content != $"gml_GlobalScript_{function}" &&
            code.Name.Content != $"new_{function}"
        )
        {
            update = true;
            codeContent = functionRegex.Replace(codeContent, $"new_{function}");
        }
    }

    var newStringWidthRegex = new Regex(@"\bstring_width\b");
    if (
        newStringWidthRegex.IsMatch(codeContent) &&
        code.Name.Content != "new_string_width"
    )
    {
        update = true;
        codeContent = newStringWidthRegex.Replace(codeContent, "new_string_width");
    }

    if (update)
    {
        UpdatedCode[code.Name.Content] = codeContent;
        ToUpdate.Add(code);
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
    // enable debug mode
    Replace("gml_GlobalScript_scr_gamestart", "global.debug = false", "global.debug = true");
    Replace("gml_Object_obj_cutscene_master_Draw_64", "global.debug == true", "0");

    ImportGMLString(
    "debug_draw_text",
    @"function debug_draw_text(argument0, argument1, argument2)
    {
        var xpos = argument0
        var ypos = argument1
        var str = argument2
        draw_text((__view_get((0 << 0), 0) + xpos), (__view_get((1 << 0), 0) + ypos), str)
    }"
    );

    Append("gml_Object_obj_time_Draw_0", @$"
    draw_set_color(c_white);
    debug_draw_text(20, 0, ""global.read_total: "" + string(global.read_total));
    ");
}