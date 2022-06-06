using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    //class containing methods to help with displaying and parsing data in the console
    //created by me

    public class ConsoleHelper 
    {
        public static int promptOptions(string prompt, string[] options)
        {
            Console.WriteLine(prompt);
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }
            string? input = Console.ReadLine();
            while (string.IsNullOrEmpty(input))
                input = Console.ReadLine();
            int parsed = 0;
            try
            {
                parsed = int.Parse(input);
            }
            catch
            {

                input = Console.ReadLine();
            }

            return parsed;
        }

        public static string confirmedPwd()
        {
            Console.WriteLine("Enter a password.");
            string pwd = hiddenPassword();

            Console.WriteLine("Confirm Password: (passwords must match)");
            string confirm = hiddenPassword();
            //while loop to confirm the user's password

            while (pwd != confirm)
            {
                Console.WriteLine("Enter a master password.");
                pwd = hiddenPassword();

                Console.WriteLine("Confirm Password: (passwords must match)");
                confirm = hiddenPassword();
            }
            return pwd;
        }
        //https://stackoverflow.com/questions/23433980/c-sharp-console-hide-the-input-from-console-window-while-typing

        public static string hiddenPassword()
        {
            string password = "";
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && !(string.IsNullOrEmpty(password)))
                {
                    Console.Write("\b \b"); //ASCII backspace; removes character and moves the input back
                    password = password.Remove(password.Length - 1, 1);
                }
                else
                {
                    Console.Write("*");
                    password += key.KeyChar;
                }
            }
            Console.WriteLine();
            return password;
            
        }
        public static void ColorWrite(string txt, ConsoleColor color, ConsoleColor bg = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = bg;

            Console.WriteLine(txt);

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
