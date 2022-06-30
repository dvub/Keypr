using System.Text;

namespace pwd_mgr_csharp
{
    // created by me, dvub


    /// <summary>
    /// Static helper class for methods to display and parse data in the console.
    /// </summary>
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
        /// <summary>
        /// Reads a line in the console, preventing empty input.
        /// </summary>
        /// <returns>a string which is not null nor empty.</returns>
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
        public static byte[] PwdGenPrompt()
        {
            string[] passOpts = new string[2] { "Generate Random (Recommended)", "Create one myself" };
            int passInput = 0;
            byte[] passBytes = new byte[] { };
            bool prompting = true;

            // allow the user to randomly generate a password - or create one themself
            while (prompting)
            {
                passInput = ConsoleHelper.PromptOptions("How would you like to create a password?", passOpts);
                switch (passInput)
                {
                    case 1:
                        int len = 0;
                        ConsoleHelper.ColorWrite("Enter password length:", ConsoleHelper.promptColor);
                        string passLength = ConsoleHelper.ReadNotNull();
                        while (true)
                        {
                            try
                            {
                                len = int.Parse(passLength);
                                break;
                            }
                            catch
                            {
                                ConsoleHelper.ColorWrite("Input a valid number:", ConsoleColor.Red);
                                passLength = ConsoleHelper.ReadNotNull();
                            }
                        }

                        bool regen = true;
                        while (regen)
                        {
                            
                            string basePass = CryptoHelper.GeneratePassword(len);
                            passBytes = Encoding.UTF8.GetBytes(basePass);
                            Console.WriteLine();
                            ConsoleHelper.ColorWrite($"Generated a new Password!", ConsoleColor.Green);
                            Console.WriteLine();
                            Console.WriteLine("The password is: ");
                            ConsoleHelper.ColorWrite(basePass, ConsoleColor.Magenta);
                            Console.WriteLine();
                            ConsoleHelper.ColorWrite("Would you like to regenerate the password? (y/n)", ConsoleHelper.promptColor);
                            regen = ConsoleHelper.BoolPrompt();
                        }

                        prompting = false;
                        break;
                    case 2:
                        passBytes = Encoding.UTF8.GetBytes(ConsoleHelper.ConfirmedPwd());
                        prompting = false;
                        break;
                }
            }
            return passBytes;
        }
    }
}
