function append_text_line (argument0, argument1)
{
    var line = argument0;
    var content = argument1;
    if (is_undefined(line))
    {
        line = "";
        show_debug_message("undefined line" + argument1);
    }
    else if (is_real(line))
    {
        line = "";
        show_debug_message("line not set" + argument1);
    }
    var file = file_text_open_append("textstuff/text.txt");
    var add = true;
    show_debug_message("line: " + string(line));
    for (var i = 0; i < global.read_total; i++)
    {
        if (global.read_text[i] == line)
        {
            add = false;
            break;
        }
    }
    if (add && line != "")
    {
        global.read_text[global.read_total] = line;
        global.read_total++;
        file_text_write_string(file, line + "\n");
    }
    file_text_close(file);
}