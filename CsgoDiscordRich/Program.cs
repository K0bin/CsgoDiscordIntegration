using System;
using System.Threading;

namespace CsgoDiscordRich
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var handlers = new Discord.EventHandlers();
            var discord = new Discord(Secret.DiscordApplicationId, ref handlers);
            var presence = new Discord.RichPresence()
            {
                 Instance = 1,
                 Details = "hi",
                 State = "Test"
            };
            discord.UpdatePresence(in presence);
            Console.ReadKey();
        }
    }
}
