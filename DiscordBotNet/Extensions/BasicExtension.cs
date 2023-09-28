using System.Linq.Expressions;
using System.Text;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot;

public static class BasicExtension
{

    public static string Multiply(this string theString, uint value)
    {
        var idk = "";
        foreach (var i in Enumerable.Range(1,(int) value))
        {
            idk += theString;
        }

        return idk;
    }
    public static Color ToImageSharpColor(this DiscordColor color)
    {
        return Color.ParseHex(color.ToString());
    }
    
    
    public static string AddNewlines(this string input, int charactersPerLine)
    {
        StringBuilder result = new StringBuilder();
        int charCount = 0;
        foreach (string word in input.Split(' '))
        {
            if (charCount + word.Length + 1 > charactersPerLine)
            {
                result.Append('\n');
                charCount = 0;
            }
            result.Append(word).Append(' ');
            charCount += word.Length + 1;
        }
        return result.ToString().Trim();
    }
    public static DiscordEmbedBuilder WithUser(this DiscordEmbedBuilder embedBuilder, DiscordUser user)
    {
        return embedBuilder.WithAuthor(user.Username, iconUrl: user.AvatarUrl);
    }
    public static string Represent<T>(this IEnumerable<T> it,bool print = true)
    {
        var a =  "{ " + String.Join(", ", it.Select(i => i.ToString())) + " }";
        if (print)
        {
            Console.WriteLine(a);
        }

        return a;
        
    }

    public static bool IsRelatedToType(this Type theType, Type type)
    {
        return theType.IsSubclassOf(type) || theType == type;
    }
    public static int Round(this double theDouble)
    {
        return (int) Math.Round(theDouble);
    }
    public static int Round(this float theFloat)
    {
        return (int) Math.Round(theFloat);
    }
    public static T Print<T>(this T idk)
    {
        Console.WriteLine(idk);
   
        return idk;
    }

    public static U Map<T, U>(this T theObject, Expression<Func<T, U>> expression)
    {
        return expression.Compile().Invoke(theObject);
    }

    public static int IndexOfLastElement<T>(this IEnumerable<T> _enumerable)
    {
        return  _enumerable.Count() - 1;
    }

    

}