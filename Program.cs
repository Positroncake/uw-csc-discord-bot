using NetCord;
using NetCord.Gateway;
using Microsoft.Extensions.Configuration;

namespace UwCscDiscordBot;

public static class Program
{
    // Place the Discord API Token in appsettings.json
    private static string? Token { get; set; }
    private static GatewayClient Bot { get; set; } = null!;
    // Controls whether cscbot does not log messages FROM ITSELF, or does not log messages FROM ALL BOTS. Set in appsettings.json
    private static bool StrictLoggingPolicy { get; set; }
    private const long OutputChannelId = 1384399717403856946L;
    
    public static async Task Main(string[] args)
    {
        // replace "appsettings.json" with path to config file, or specify it as an argument when running cscbot
        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(args.Length > 0 ? args[0] : "appsettings.json", false, false);
        IConfigurationRoot root = builder.Build();
        Token = root["Discord:Token"];
        StrictLoggingPolicy = root.GetValue<bool>("BotConfig:StrictLoggingPolicy");
        Console.WriteLine(StrictLoggingPolicy.ToString());
        if (string.IsNullOrWhiteSpace(Token))
        {
            Console.WriteLine("Mila has eaten the Discord token.");
            Console.WriteLine("[ TOKEN MISSING ]");
            return;
        }
        
        Bot = new GatewayClient(new BotToken(Token));
        Bot.MessageCreate += MessageReceived;
        await Bot.StartAsync();
        
        // note for users: Ctrl+C also works, but please don't tell Mila that!
        Console.WriteLine("[ RUNNING ] - follow on-screen instructions to quit");
        
        //todo: switch to cryptographically-secure RNG once Mila becomes smart enough to predict the normal Random class's outputs
        var rand = new Random();
        int quitCode = Math.Abs((int)rand.NextInt64() % 1000);
        while (0 != 1) // it's complicated, see notes further down below
        {
            // in case Mila gains access, prevent her from easily disabling the bot by adding a simple challenge
            Console.Write($"Exit? Input {quitCode:000} to confirm exit > ");
            int inputCode = int.Parse(Console.ReadLine() ?? "0");
            if (inputCode == quitCode)
            {
                Console.WriteLine("[ EXITING... ]");

                // resort to primitive methods such as "return", as we are currently unable to set 0 to be equal to 1
                // an application "Application for Exception From Mathematical Laws" has been filed with the government and awaits processing
                // TODO: update next line to be 0 = 1 after application has been approved
                return;
            }
            quitCode = Math.Abs((int)rand.NextInt64() % 1000);
        }
    }

    private static async ValueTask MessageReceived(Message msg)
    {
        bool dontLog = StrictLoggingPolicy switch
        {
            true => msg.Author.IsBot, // under strict rules, if author is any bot, don't log
            false => msg.Author.Id == 1384338486781542430L // under non-strict rules, if author is cscbot, don't log
        };
        if (dontLog) return;
        
        await Bot.Rest.SendMessageAsync(OutputChannelId, $"{msg.Content} from {msg.Author} in {msg.ChannelId}");
        Console.WriteLine($"{msg.Content} from {msg.Author} in {msg.ChannelId} >> {OutputChannelId}");
    }
}