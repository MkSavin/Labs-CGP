using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGP_L1_Savin_M {
    class Processor {

        public string[] ProcessTextCommand(string command) => command switch {
            "help" => new string[] {
                "Список команд:",
                "\t'man_create': создание человека, (экземпляр класса Man)",
                "\t'man_kill': убиение человека",
                "\t'man_talk': заставить человечка говорить (если создан экземпляр класса)",
                "\t'man_go': заставить человечка идти (если создан экземпляр класса)",
                "\t'man_money': баланс человека (если создан экземпляр класса)",
                "\t'man_health': здоровье человека (если создан экземпляр класса)",
                "\t'man_work': заставить человечка работать (если создан экземпляр класса)",
                "\t'man_buy_aspirine': заставить человечка купить аспирин (если создан экземпляр класса)",
                "\t'exit': выход",
            },
            "man_money" => new string[] {
                man == null ? "Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его" :
                    "Баланс человека: " + man.Money(),
            },
            "man_health" => new string[] {
                man == null ? "Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его" : 
                    "Здоровье человека: " + man.Health(),
            },
            _ => new string[] {
                "Ваша команда не определена.",
            }.Concat(ProcessTextCommand("help")).ToArray(),
        };

        private Man man;

        public ProcessorCommand ProcessCommand(string command) {
            switch(command) {
                case "exit":
                    return ProcessorCommand.Exit;

                case "man_create":
                    if (man != null && man.IsAlive()) {
                        Console.WriteLine("Человек уже создан, используйте 'man_kill', чтобы умертвить его");
                        break;
                    }

                    Console.Write("Введите имя: ");
                    man = new Man(Console.ReadLine());
                    Console.WriteLine("Человек создан");
                    break;

                case "man_kill":
                    if (man == null) {
                        Console.WriteLine("Человек еще не создан, используйте 'man_create', чтобы создать его");
                        break;
                    }
                    if (!man.IsAlive()) {
                        Console.WriteLine("Человек уже умертвлен, используйте 'man_create', чтобы воскресить его");
                        break;
                    }

                    man.Kill();
                    Console.WriteLine("Человек умертвлен");
                    break;

                case "man_talk":
                    if (man == null || !man.IsAlive()) {
                        Console.WriteLine("Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его");
                        break;
                    }

                    Console.WriteLine("Человек говорит:");
                    man.Talk();
                    break;

                case "man_go":
                    if (man == null || !man.IsAlive()) {
                        Console.WriteLine("Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его");
                        break;
                    }

                    man.Go();
                    break;

                case "man_work":
                    if (man == null || !man.IsAlive()) {
                        Console.WriteLine("Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его");
                        break;
                    }

                    man.Work();
                    Console.WriteLine("Рабочий день завершен.");
                    ProcessCommand("man_money");
                    ProcessCommand("man_health");
                    break;

                case "man_buy_aspirine":
                    if (man == null || !man.IsAlive()) {
                        Console.WriteLine("Человек еще не создан или умервтлен, используйте 'man_create', чтобы создать/воскресить его");
                        break;
                    }

                    man.BuyAspirine();
                    Console.WriteLine("Аспирин куплен.");
                    ProcessCommand("man_money");
                    ProcessCommand("man_health");
                    break;

                default:
                    Console.WriteLine(string.Join(Environment.NewLine, ProcessTextCommand(command)));
                    break;
            }

            return ProcessorCommand.Nothing;
        }

    }
}
