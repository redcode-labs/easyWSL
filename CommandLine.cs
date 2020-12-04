using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyWSL
{
    class CommandLine
    {
        public static void Show()
        {
            Console.WriteLine("Type in help to show available commands");
            Console.WriteLine(" ");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("easyWSL > ");
                Console.ResetColor();
                string command = Console.ReadLine();

                string command_main = command.Split(new char[] { ' ' }).First();
                string[] arguments = command.Split(new char[] { ' ' }).Skip(1).ToArray();
                if (lCommands.ContainsKey(command_main))
                {
                    Action<string[]> function_to_execute = null;
                    lCommands.TryGetValue(command_main, out function_to_execute);
                    function_to_execute(arguments);
                }
                else
                    Console.WriteLine("Command '" + command_main + "' not found");
            }

        }

        private static Dictionary<string, Action<string[]>> lCommands =
           new Dictionary<string, Action<string[]>>()
           {
                { "install", Commands.Install },
                { "help", Commands.Help },
                { "list", Commands.ShowInstalledDistros },
                { "exit" , Commands.Exit }
           };
    }
}
