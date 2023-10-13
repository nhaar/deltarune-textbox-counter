function append_text_line (argument0)
{
    var line = argument0;
    if (!ds_map_exists(global.read_text_map, line))
    {
        show_debug_message("Appended: " + line);
        var file = file_text_open_append("textstuff/text.txt");
        ds_map_add(global.read_text_map, line, 1);
        global.read_total++;
        file_text_write_string(file, line + "\n");
        file_text_close(file);
    }
}