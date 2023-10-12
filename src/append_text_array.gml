function append_text_array (argument0, argument1)
{
    var array = argument0;
    var content = argument1;
    var length = array_length(argument0);
    show_debug_message("trying to append: " + string(content));
    for (var i = 0; i < length; i++)
    {
        var text_id = array[i];
        if is_undefined(text_id)
        {
            show_debug_message("undefined text id (index " + string(i) + ")");
        }
        else if is_real(text_id)
        {
            show_debug_message("unset text id (index " + string(i) + ")");
        }
        else
        {
            show_debug_message("valid textid (index " + string(i) + "): " + string(text_id));
            append_text_line(array[i])
        }
    }
}