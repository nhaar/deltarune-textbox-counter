#load "..\Lib\DecompileContext.csx"
#load "..\Lib\GMLManip.csx"
#load "..\Lib\TextSystem.csx"

using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

// very odd try catch hook that needs to be removed or modtool can't compile it
Replace
(
"gml_Object_obj_tensionbar_Draw_0",
@"@@try_hook@@(2224, 2272)
if (global.tensionselect >= 0)
    shit = 1
@@try_unhook@@()",
""
);

// deltarune function for drawing the text for total messages read
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
    },
    false
);


// changing all deltarune text functions
Replace
(
"gml_GlobalScript_msgnextloc",
"msgnext(str)",
"msgnext(str, localized_string_id)"
);

Replace
(
"gml_GlobalScript_stringsetsubloc",
"return stringset(str);",
"return stringset(str, localized_format_string_id);"
);

Replace
(
"gml_GlobalScript_stringsetloc",
"return stringset(str);",
"return stringset(str, argument1);"
);

Replace
(
"gml_GlobalScript_msgsetloc",
"msgset(msg_index, str)",
"msgset(msg_index, str, localized_string_id)"
);

Replace
(
"gml_GlobalScript_c_msgnextloc",
"c_msgnext(str)",
"c_msgnext(str, localized_string_id)"
);

Replace
(
"gml_GlobalScript_c_msgsetloc",
"c_msgset(msg_index, str)",
"c_msgset(msg_index, str, localized_string_id)"
);

Replace
(
"gml_GlobalScript_msgnextsubloc",
"msgnext(str)",
"msgnext(str, localized_string_id)"
);

Replace
(
@"gml_GlobalScript_msgsetsubloc",
"msgset(msg_index, str)",
"msgset(msg_index, str, localized_format_string_id)"
);

Replace
(
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

// deltarune version of burn_text_id: the suffix depends on the chapter
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

// replacing text functions with burning text id

Replace
(
"gml_GlobalScript_scr_84_get_lang_string_ch1",
"return ds_map_find_value(global.lang_map, argument0);",
"return deltarune_burn_text_id(ds_map_find_value(global.lang_map, argument0), argument0, 1);"
);

Replace
(
"gml_GlobalScript_msgset",
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

Replace
(
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

Replace
(
"gml_GlobalScript_c_msgnext",
"c_cmd(\"msgnext\", argument0, 0, 0, 0)",
"c_cmd(\"msgnext\", argument0, argument1, 0, 0)"
);

Replace
(
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

Replace
(
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
Place
(
"gml_Object_obj_writer_ch1_Create_0",
"specfade = 1",
"global.msg[0] = clean_text_string(global.msg[0])"
);

Replace
(
"gml_GlobalScript_scr_nextmsg_ch1",
"mystring = nstring[msgno]",
"mystring = clean_text_string(nstring[msgno])"
);

// ch2
Place
(
"gml_Object_obj_writer_Create_0",
"miniface_drawn = 0",
"global.msg[0] = clean_text_string(global.msg[0])"
);

Replace
(
"gml_GlobalScript_scr_nextmsg",
"mystring = nstring[msgno]",
"mystring = clean_text_string(nstring[msgno])"
);

// remove for production
UseDebug();

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