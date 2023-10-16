#load "..\Lib\TextSystem.csx"
#load "..\Lib\GMLManip.csx"

using System.Threading.Tasks;

/// <summary>
/// Build the UNDERTALE textbox mod
/// </summary>
/// <returns></returns>
async Task Build ()
{
EnsureDataLoaded();

// used to draw the total text
CreateFunction
(
"counter_draw_text",
@"draw_set_font(fnt_main);
draw_set_color(c_white);
var xpos = argument0
var ypos = argument1
var str = argument2
draw_text(xpos, ypos, str)",
3,
true    
);

await SetupSystem
(
    new []
    {
        "scr_gettext"
    },
    "gml_Object_obj_time_Create_0",
    new Dictionary<string, int[]>()
    {
        { "draw_text", new[] { 2, 3 } },
        { "scr_drawtext_centered", new[] { 2, 3 } }
    },
    new Dictionary<string, int[]>()
    {
        { "string_length", new[] { 0, 1 } }
    },
    new string[]
    {
    },
    true
);

// undertale's version: no suffixes used
CreateFunction
(
"undertale_burn_text_id",
"return burn_text_id(argument0, argument1);",
2,
true
);

// functions regarding text: there is only one and it's scr_gettext
Replace
(
"gml_Script_scr_gettext",
"return text;",
"return undertale_burn_text_id(text, text_id);"
);

// obj_writer entries for UNDERTALE: a few different files and cases it's used
Place
(
"gml_Object_obj_base_writer_Create_0",
"n = 0",
"global.msg[0] = clean_text_string(global.msg[0])"
);

Replace
(
"gml_Object_obj_base_writer_Draw_0",
"originalstring = scr_replace_buttons_pc(mystring[stringno])",
"originalstring = clean_text_string(scr_replace_buttons_pc(mystring[stringno]))"
);

Replace
(
"gml_Object_obj_base_writer_Other_10",
"originalstring = scr_replace_buttons_pc(mystring[stringno])",
"originalstring = clean_text_string(scr_replace_buttons_pc(mystring[stringno]))"
);

Replace
(
"gml_Object_OBJ_NOMSCWRITER_Create_0",
"originalstring = scr_replace_buttons_pc(mystring[0])",
"originalstring = clean_text_string(scr_replace_buttons_pc(mystring[0]));"
);
}

/// <summary>
/// Add debug mode to UNDERTALE
/// </summary>
void UseDebug ()
{
    Replace(
    "gml_Script_SCR_GAMESTART",
    "global.debug = false",
    "global.debug = true"
    );
}
