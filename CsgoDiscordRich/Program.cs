using CSGSI;
using System;
using System.Threading;

namespace CsgoDiscordRich
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            short ? port = null;
            if (args != null && args.Length > 0 && short.TryParse(args[0], out short _port))
            {
                port = _port;
            }
            var integration = new CsgoDiscordIntegration(port);
            integration.Start();
        }
    }
}
