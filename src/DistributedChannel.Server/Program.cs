using System;

namespace DistributedChannel.Server
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Press esc key to stop");

            int i = 0;

            void PeriodicallyClearScreen()
            {
                i++;
                if (i > 15)
                {
                    Console.Clear();
                    Console.WriteLine("Press esc key to stop");
                    i = 0;
                }
            }

            //Write the host messages to the console
            void OnHostMessage(string input)
            {
                PeriodicallyClearScreen();
                Console.WriteLine(input);
            }

            var bll = new ServerChannel(1, OnHostMessage);
            bll.Open(); //Server runs in a dedicated thread separate from mains thread

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.Clear();
                Console.WriteLine("Press esc key to stop");
            }

            Console.WriteLine("Attempting clean exit");
            bll.Close();

            Console.WriteLine("Exiting console Main.");
        }
    }
}