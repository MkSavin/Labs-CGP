using System;

namespace CGP_L1_Savin_M {
    class Program {
        static void Main(string[] args) {

            var processor = new Processor();

            Console.WriteLine("© MkSavin @ Максим Савин, ПРИ-117");

            while(true) {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write("> ");
                Console.ForegroundColor = ConsoleColor.Gray;

                if (processor.ProcessCommand(Console.ReadLine()) == ProcessorCommand.Exit) {
                    break;
                }
            }

        }
    }
}
