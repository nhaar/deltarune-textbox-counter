function unarraify_text_id (argument0)
{
    if (is_undefined(array_length(argument0)))
    {
        return argument0;
    }
    else
    {
        return argument0[0];
    }
}