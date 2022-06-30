namespace pwd_mgr_csharp
{
    //class containing methods to help with displaying and parsing data in the console
    //created by me

    public static class ConsoleHelper 
    {

        public static ConsoleColor promptColor = ConsoleColor.Blue;
        public static bool BoolPrompt()
        {
            string input = ReadNotNull().ToLower();
            bool val = false;
            while (true)
            {
                if (input == "y")
                {
                    val = true;
                    break;
                } 
                else if (input == "n")
                {
                    val = false;
                    break;
                }
                else
                {
                    input = ReadNotNull().ToLower();
                }
            }
            return val;
        }

        public static string ReadNotNull()
        {
            string val = Console.ReadLine();
            while (string.IsNullOrEmpty(val))
                val = Console.ReadLine();

            return val;
        }
        public static int PromptOptions(string prompt, string[] options)
        {
            ColorWrite(prompt, promptColor);
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($" > {i + 1}. {options[i]}");
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

        public static string ConfirmedPwd()
        {
            ColorWrite("Enter a password.", promptColor);
            string pwd = HiddenPassword();

            ColorWrite("Confirm Password: (passwords must match)", promptColor);
            string confirm = HiddenPassword();
            //while loop to confirm the user's password

            while (pwd != confirm)
            {
                ColorWrite("Enter a master password.", promptColor);
                pwd = HiddenPassword();

                ColorWrite("Confirm Password: (passwords must match)", promptColor);
                confirm = HiddenPassword();
            }
            return pwd;
        }
        //https://stackoverflow.com/questions/23433980/c-sharp-console-hide-the-input-from-console-window-while-typing

        public static string HiddenPassword()
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
        public static void ColorWrite(string txt, ConsoleColor color, bool newLine = true, ConsoleColor bg = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = bg;

            if (newLine == true)
            {
                Console.WriteLine(txt);
            } else
            {
                Console.Write(txt);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
