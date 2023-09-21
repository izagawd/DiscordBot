using System.Diagnostics;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot;

public static class BasicFunction
{
    public static Dictionary<string, Image<Rgba32>> EntityImages = new();
    public static MemoryCache UserImages { get; } = new(new MemoryCacheOptions());
    /// <summary>
    /// Loads all the images of entities
    /// </summary>
    /// <returns>The amount of images loaded</returns>
    public static async Task<int> LoadEntityImagesAsync()
    {
        int count = 0;
        foreach (var type in Bot.AllAssemblyTypes.Where(t => typeof(IHasIconUrl).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
        {
          
            IHasIconUrl icon;
            if (type.IsRelatedToType(typeof(StatusEffect)))
            {
                icon = (IHasIconUrl)Activator.CreateInstance(type, args: new CoachChad())!;
            }
            else
            {
                icon = (IHasIconUrl)Activator.CreateInstance(type)!;
            }
            
            if (icon.IconUrl is not null)
            {

                await GetImageFromUrlAsync(icon.IconUrl, true);
                count += 1;
            }
        }

        return count;
    }
    public static async Task<Image<Rgba32>> GetImageFromUrlAsync(string url)
    {
        return await GetImageFromUrlAsync(url, false);
    }
    private static async Task<Image<Rgba32>> GetImageFromUrlAsync(string url, bool toEntityDictionary)
    {
        if (EntityImages.ContainsKey(url)) return EntityImages[url].Clone();
        if (UserImages.TryGetValue(url, out Image<Rgba32>? gottenImage)) return gottenImage!.Clone();
        try{
            var webClient = new HttpClient();
            var responseMessage = await webClient.GetAsync(url);
            var memoryStream = await responseMessage.Content.ReadAsStreamAsync();
            var characterImage = await Image.LoadAsync<Rgba32>(memoryStream);
            if (toEntityDictionary)
            {
                EntityImages[url] = characterImage;  
            }
            else
            {
  
                UserImages.Set(url,characterImage,new MemoryCacheEntryOptions{SlidingExpiration =new TimeSpan(0,30,0) });
                "via memory cache".Print();
            }
            webClient.Dispose();
            responseMessage.Dispose();
            return characterImage.Clone();
        }
        catch
        {
            var alternateImage =  await GetImageFromUrlAsync("https://legendarygawds.com/move-pictures/guilotine.png", true);
            EntityImages[url] = alternateImage;
            return alternateImage;
        }
    }
    /// <returns>
    /// A random element from the parameters
    /// </returns>
    public static T GetMaxValue<T>(params T[] numbers) 
    {
      
        return numbers.Max()!;
    }

    public static int GetRandomNumberInBetween(int a, int b)
    {
        return new Random().Next(a, b + 1);
    }
    public static T RandomChoice<T>(params T[] elements)
    {
        
        Random random = new();
        int index = random.Next(elements.Count());
        return elements[index];
    }
    /// <returns>
    /// A random element from the list
    /// </returns>
    public static T RandomChoice<T>(IEnumerable<T> elements)
    {

        Random random = new();
        int index = random.Next(elements.Count());
        return elements.ToList()[index];
    }
    /// <summary>
    /// Makes the first letter of each word in a string capital
    /// </summary>
    ///<returns>The computed string</returns>

    public static string FirstLetterCapital(string stringToCapitalize)
    {
        string[] arr = stringToCapitalize.Split(" ");
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] =char.ToUpper(arr[i][0]) + arr[i].Substring(1);
        }
        
        return string.Join(" ", arr);
    }

        
    /// <summary>Spaces out a sentence and makes each word start with a capital letter</summary>
    ///<returns>The computed string</returns>
    public static string Englishify(string stringToEnglishify)
    {
        string[] newArray;
        if (stringToEnglishify.Contains("_"))
        {
            newArray = stringToEnglishify.Split("_");
        
        }
        else if(stringToEnglishify.Contains(" "))
        {
            newArray = stringToEnglishify.Split(" ");

        } else
        {
            string tempString = "";
            for(int i = 0; i < stringToEnglishify.Length; i++)
            {
                tempString += stringToEnglishify[i];
                if (stringToEnglishify.Length > i + 1 &&  char.IsUpper(stringToEnglishify[i + 1]))
                {
                    tempString += ' ';
                }
            }
            newArray = tempString.Split(" ");
        }
        int len = newArray.Length;
        string[] checkers = { "i", "ii", "iii", "iv", "v" };
        for (int i = 0; i < len; i++)
        {
            if (checkers.Contains(newArray[i].ToLower()))
            {
                newArray[i] = newArray[i].ToUpper();
       
            }
            else
            {
                newArray[i] = FirstLetterCapital(newArray[i]);

            }

        }
            
        string newString = string.Join(" ", newArray);

        return newString;



    }


    ///<returns>The amount of time till the next day as a string</returns>
    public static string TimeTillNextDay()
    {
        DateTime now = DateTime.Now;
        int hours = 0, minutes = 0, seconds = 0;
        hours = (24 - now.Hour) - 1;
        minutes = (60 - now.Minute) - 1;
        seconds = (60 - now.Second - 1);
        return ($"{hours}:{minutes}:{seconds}");
    }
    ///<returns>True if the percentage chance procs.
    /// False if it does not proc
    ///</returns>
    ///<param name="number">Percentage chance</param>
    public static bool RandomChance(int number)
    {

        if (number < 0) throw new Exception("Number must be between 0 or 100");
        Random random = new();
        int randomNumberGotten = random.Next(0, 100);
        if (randomNumberGotten <= number) return true;
        return false;

    }
}