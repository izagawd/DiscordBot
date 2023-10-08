using System.Globalization;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using DiscordBotNet.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DiscordBotNet;

public static class Website 
{
    public static readonly string DomainName = "https://localhost";
    public static async Task<string> RenderImageTagAsync(Image image)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        await using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);  // Save the image to a stream
        byte[] imageBytes = stream.ToArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        var imageSrc = $"data:image/png;base64,{base64Image}";
        return imageSrc;
    }
    public static string GetDiscordUserName(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        return 
            claim?.Value!;
    }

    public static ulong GetDiscordUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i =>
            i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return ulong.Parse(claim!.Value);
    }
    public static string GetDiscordUserAvatarUrl(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(i =>
            i.Type == "urn:discord:avatar:url");
        return claim?.Value!;
    }
    public static void ConfigureServices(IServiceCollection services)
    {

        services.AddRazorPages();
        services.AddDbContext<PostgreSqlContext>();
        services.AddSession(i =>
            {
                i.IdleTimeout = TimeSpan.MaxValue;
            }
  
        );

        services.AddAuthentication(options =>
            {

                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddDiscord(options =>
            {

                options.ClientId = "340054610989416460";
                options.ClientSecret = "IV9wq7ljxtCasXJrMHs_osm-ObL6vhQm";
                options.SaveTokens = true;
                options.CallbackPath = "/signin-discord";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        user.GetString("avatar").StartsWith("a_") ? "gif" : "png"));
            })
            .AddCookie(options =>
            {

            }).AddCertificate(i => i.Validate());

    }
    public static async Task Start(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services);

        var app = builder.Build();
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCertificateForwarding();
        app.UseRouting();
        app.UseAuthentication();
                    
        app.UseAuthorization();
        app.UseSession();
        app.MapRazorPages();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        await app.RunAsync(DomainName);

    }
}