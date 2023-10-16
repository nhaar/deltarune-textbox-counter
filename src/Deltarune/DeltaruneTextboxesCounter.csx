#load "..\Lib\DecompileContext.csx"
#load "..\Lib\GMLManip.csx"
#load "..\Lib\TextSystem.csx"

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