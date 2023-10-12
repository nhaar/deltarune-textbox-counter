function append_text_line (argument0)
{
    var line = argument0;
    var file = file_text_open_append("textstuff/text.txt");
    var add = true;
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
        show_debug_message("Append succesful");
        global.read_text[global.read_total] = line;
        global.read_total++;
        file_text_write_string(file, line + "\n");
    }
    else
    {
        show_debug_message("Append failed");
    }
    file_text_close(file);
}