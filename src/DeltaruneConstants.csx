using System.Runtime.Serialization;

enum Lang
{
    EN,
    JP
}

enum Chapter
{
    Chapter1,
    Chapter2
}

string GetLangName (Lang lang)
{
    return lang switch
    {
        Lang.EN => "en",
        Lang.JP => "ja",
    };
}

string GetLangFileName (Chapter chapter)
{
    return chapter switch
    {
        Chapter.Chapter1 => "_ch1",
        Chapter.Chapter2 => "",
    };
}

string GetChapterFileName (Chapter chapter)
{
    return chapter switch
    {
        Chapter.Chapter1 => "ch1",
        Chapter.Chapter2 => "ch2",
    };
}