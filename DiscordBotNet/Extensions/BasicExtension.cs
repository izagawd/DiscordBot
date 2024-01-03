using System.Linq.Expressions;
using DSharpPlus.Entities;

namespace DiscordBotNet.Extensions;

public static class BasicExtension
{
    public static string Join(this IEnumerable<string> enumerable, string seperator)
    {
        return string.Join(seperator, enumerable);
    }
   
    public static Color ToImageSharpColor(this DiscordColor color)
    {
        return Color.ParseHex(color.ToString());
    }

    public static DiscordEmbedBuilder WithUser(this DiscordEmbedBuilder embedBuilder, DiscordUser user)
    {
        return embedBuilder.WithAuthor(user.Username, iconUrl: user.AvatarUrl);
    }
    public static string Represent<T>(this IEnumerable<T> it,bool print = false)
    {
        var a =  "{ " + String.Join(", ", it.Select(i => i?.ToString())) + " }";
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

    public static TExpressionResult Map<TObject, TExpressionResult>(this TObject theObject, Expression<Func<TObject, TExpressionResult>> expression)
    {
        return expression.Compile().Invoke(theObject);
    }



}