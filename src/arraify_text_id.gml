function arraify_text_id (argument0)
{
    var possible_array = argument0;
    if (is_undefined(array_length(argument0)))
    {
        var array_id;
        array_id[0] = argument0;
        return array_id;
    }
    else
    {
        return possible_array;
    }
}