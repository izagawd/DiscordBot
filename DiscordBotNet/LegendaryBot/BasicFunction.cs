using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace DiscordBotNet.LegendaryBot;

public static class BasicFunction
{
    private static ConcurrentDictionary<string, Image<Rgba32>> EntityImages { get; } = new();

    private static MemoryCache UserImages { get; } = new(new MemoryCacheOptions());

    private static MemoryCacheEntryOptions EntryOptions { get; } =new MemoryCacheEntryOptions
    {
        SlidingExpiration = new TimeSpan(0, 30, 0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = EvictionCallback } }
        
    };

    private static async void EvictionCallback(object key, object? value, EvictionReason reason, object? state)
    {
        if (value is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }


    /// <summary>
     /// Gets the image as sixlabors image from the url provided, caches it, and returns it
     /// </summary>
    /// <returns></returns>
    public static async Task<Image<Rgba32>> GetImageFromUrlAsync(string url)
     {
        if (EntityImages.ContainsKey(url)) return EntityImages[url].Clone();
        if (UserImages.TryGetValue(url, out Image<Rgba32>? gottenImage)) return gottenImage!.Clone();
        try{
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = 
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    if (cert != null && !cert.Verify())
                    {
                        if (httpRequestMessage.RequestUri is not null && httpRequestMessage.RequestUri.ToString().Contains(Website.DomainName))
                        {
                            return true;
                        }

                        return false;
                    }

                    if (cert == null) return false;
                    return true;
                };
            using var webClient = new HttpClient(handler);
  
            await using var memoryStream = await webClient.GetStreamAsync(url);
            var characterImage = await Image.LoadAsync<Rgba32>(memoryStream);
    
            //checks if the image is from this bot's domain  so it can permanently cache it
            //instead of temporarily cache it
            if (url.Contains(Website.DomainName))
                EntityImages[url] = characterImage;  
            else
                UserImages.Set(url,characterImage,EntryOptions);
            return characterImage.Clone();
        }
        catch(Exception e)
        {

            var alternateImage =  await GetImageFromUrlAsync($"{Website.DomainName}/battle_images/moves/guilotine.png");
            if (url.Contains(Website.DomainName))
                EntityImages[url] = alternateImage;
            else
                UserImages.Set(url,alternateImage,new MemoryCacheEntryOptions{SlidingExpiration =new TimeSpan(0,30,0) });
            return alternateImage.Clone();
        }
    }


    public static int GetRandomNumberInBetween(int a, int b)
    {
        return new Random().Next(a, b + 1);
    }
    /// <returns>
    /// A random element from the parameters
    /// </returns>
    public static T RandomChoice<T>(params T[] elements)
    {
        
        Random random = new();
        int index = random.Next(elements.Length);
        return elements[index];
    }
    /// <returns>
    /// A random element from the list
    /// </returns>
    public static T RandomChoice<T>(IEnumerable<T> elements)
    {
        return RandomChoice(elements.ToArray());

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