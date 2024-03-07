A discord chat bot that simulates a turn by turn based video game using c#

Note: A discord bot, a postgresql database (can be a local db) and .net core 8.0 is required to test this



1. make an App.config file in the project. it should have content like this:

```<configuration>
    <appSettings>
        <add key="ConnectionString" value="xxxxx"/>
        <add key="BotToken" value="xxxxx"/></appSettings>
</configuration>
```

2. Set value of key "BotToken"  in the App.config file to your discord bot's bot token
3. Set value of key "ConnectionString"  in the App.config file to your postgre sql database connection string.
4. Before running public static async Task main in Bot.cs for the first time, run the FirstTimeSetupAsync method in the same Bot.cs script. (simply call it in the beginning of the main method asynchourously, meaning put await in front of it). Should be removed after being called for the first time. If done correctly, the project should run as expected
