using System;
using System.Media;
using System.Runtime.Versioning;

namespace CyberSecurityChatbot
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayGreeting();
            ShowLogo();

            Console.Write("Enter your name: ");
            string? name = Console.ReadLine();
            Console.WriteLine($"Hello {name}, welcome to the Cybersecurity Awareness Bot!");

            while (true)
            {
                Console.Write("\nAsk me about cybersecurity (or type 'exit'): ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("I didn’t quite understand that. Could you rephrase?");
                    continue;
                }

                input = input.ToLower();

                if (input == "exit") break;

                RespondToInput(input);
            }
        }

        [SupportedOSPlatform("windows")]
        static void PlayGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("greeting.wav");
                player.PlaySync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing greeting: " + ex.Message);
            }
        }

        static void ShowLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            Console.WriteLine("   CYBERSECURITY AWARENESS BOT");
            Console.WriteLine("");
            Console.ResetColor();
        }

        static void RespondToInput(string input)
        {
            if (input.Contains("password"))
                Console.WriteLine("Tip: Use long, unique passwords with multi-factor authentication.");
            else if (input.Contains("phishing"))
                Console.WriteLine("Tip: Don’t click suspicious links or attachments in emails.");
            else if (input.Contains("browsing"))
                Console.WriteLine("Tip: Always check for HTTPS and avoid unsecured sites.");
            else
                Console.WriteLine("I can answer about passwords, phishing, or safe browsing.");
        }
    }
}
