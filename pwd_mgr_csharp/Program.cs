using pwd_mgr_csharp;
using System.Security.Cryptography;
using System.Text;
using Figgle;
using System.Text.Json;
    //a small note: encoding/decoding is UTF8 as JSON spec is UTF8

    // TODO: 
    // measure all operations instead of just some
    // encrypt username / email - DONE
    // hidden password input - DONE
    // make sure password names are unique
    // time operations with delegates and callbacks - DONE
    // make sure that app doesn't crash when user searches pwds
    // add notes section to password
    // ability to edit pwds
    // make console look good - color, spacing, etc. - CURRENT
    // GENERATE RANDOM PASSWORDS!! - DONE
    // user can generate password of n length - DONE
Console.WriteLine(FiggleFonts.Slant.Render("KeyPr"));
Console.WriteLine();
Console.WriteLine("Checking if data file exists...");
string key = "";


// LOGIN TO APPLICATION //
if (File.Exists("data.json")) // if the user already has data file, then the user just has to log in
{
    bool loggedIn = false;
    Console.WriteLine("Found a data file!");
    Console.WriteLine("Please Enter Master Password:");

    List<Password> pwds = readPwds("data.json");
    Password masterPwd = pwds[0];
    while (!loggedIn)
    {
        string pass = ConsoleHelper.hiddenPassword();
        byte[] hashed = CryptoHelper.GetHash(pass);


        //by using a hash for the master password, the plaintext can also serve as a key for encryption
        if (masterPwd.pwd.SequenceEqual(hashed))
        {
            string username = Encoding.UTF8.GetString(masterPwd.user);
            Console.WriteLine($"Welcome, {username}!");
            loggedIn = true;
            key = pass;
        } 
        else
        {
            ConsoleHelper.ColorWrite("Incorrect Password:", ConsoleColor.Red);
        }
    }
}
else //if the user doesn't have a data file, then we need to make one and set up a password
{
    Console.WriteLine("No data file was found.");
    Console.WriteLine("Enter username"); //setup a username for welcoming the user later
    string? user = Console.ReadLine();
    while (string.IsNullOrEmpty(user))
        user = Console.ReadLine();

    Password master = new Password("master", Encoding.UTF8.GetBytes(user), CryptoHelper.GetHash(ConsoleHelper.confirmedPwd()));
    List<Password> pwds = new List<Password>();
    pwds.Add(master);
    writePwds(pwds, "data.json");

    Console.WriteLine("Successfully created a new data file and master password! Restart the application to log in.");
    Environment.Exit(0);
}

// ONCE USER IS LOGGED IN: //
int input = 0;
string[] options = new string[3] { "Add new password", "View Password", "Exit", };

while (input != options.Length)
{
    input = ConsoleHelper.promptOptions("What would you like to do?", options); //prompt the user with options for the application
    switch (input)
    {
        
        case 1: //create a new password
            
            using (Aes aes = Aes.Create())
            {
                List<Password> pwds = readPwds("data.json"); //read file

                Console.WriteLine("Enter name of website/application:"); //get some user input

                string? name = Console.ReadLine();
                while (string.IsNullOrEmpty(name))
                    name = Console.ReadLine();

                Console.WriteLine("Enter username:");

                string? user = Console.ReadLine();
                while (string.IsNullOrEmpty(user))
                    user = Console.ReadLine();


                string[] passOpts = new string[3] { "Generate Random (Recommended)", "Create one myself", "Cancel" };
                int passInput = 0;
                byte[] newPass = new byte[] { };
                bool prompting = true;

                // allow the user to randomly generate a password
                while (prompting)
                {
                    passInput = ConsoleHelper.promptOptions("How would you like to create a password?", passOpts);
                    switch (passInput)
                    {
                        case 1:
                            int len = 0;
                            Console.WriteLine("Enter password length:");
                            string passLength = Console.ReadLine();
                            while (string.IsNullOrEmpty(passLength))
                                passLength = Console.ReadLine();
                            while (true)
                            {
                                try
                                {
                                    len = int.Parse(passLength);
                                    break;
                                } 
                                catch
                                {
                                    Console.WriteLine("Input a valid number:");
                                }
                            }
                            string basePass = CryptoHelper.GeneratePassword(len); //generates a 16-byte-long password, pretty secure!
                            newPass = Encoding.UTF8.GetBytes(basePass);
                            Console.WriteLine($"Generated a new Password! The password is: {basePass}");
                            prompting = false;
                            break;
                        case 2:
                            newPass = Encoding.UTF8.GetBytes(ConsoleHelper.confirmedPwd());
                            prompting = false;
                            break;
                        case 3:
                            prompting = false;
                            break;
                    }
                }
                //end of user input, lets encrypt
                byte[] userBytes = Encoding.UTF8.GetBytes(user);


                if (newPass.Length == 0)
                {
                    break;
                }

                try 
                {
                    Benchmark<byte[]> encryptPass = new Benchmark<byte[]>(CryptoHelper.Encrypt, aes, newPass, aes.IV, key);
                    Benchmark<byte[]> encryptUser = new Benchmark<byte[]>(CryptoHelper.Encrypt, aes, userBytes, aes.IV, key);
                    byte[] total = encryptPass.returnValue;
                    byte[] totalUser = encryptUser.returnValue;

                    float time = (float)(encryptPass.elapsed + encryptUser.elapsed);
                    ConsoleHelper.ColorWrite($"Elapsed {time} ms", ConsoleColor.Yellow);


                    Password password = new Password(name, totalUser, total);
                    pwds.Add(password);
                    writePwds(pwds, "data.json"); //write to file

                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            break;
        case 2: //READ A PASSWORD
            using (Aes aes = Aes.Create())
            {
                List<Password> pwds = readPwds("data.json");

                pwds = readPwds("data.json");
                
                Console.WriteLine("Enter name of password:");
                string? name = Console.ReadLine();
                while (string.IsNullOrEmpty(name))
                    name = Console.ReadLine();
                Password password = new Password();


                password = pwds.Find(x => x.name == name); //search List 

                try
                {
                    Benchmark<byte[]> decryptOp = new Benchmark<byte[]>(CryptoHelper.Decrypt, aes, password.pwd, key);
                    Benchmark<byte[]> decryptUserOp = new Benchmark<byte[]>(CryptoHelper.Decrypt, aes, password.user, key);
                    byte[] decryptedPwd = decryptOp.returnValue;
                    byte[] decryptedUser = decryptUserOp.returnValue;


                    float time = (float)(decryptOp.elapsed + decryptUserOp.elapsed);
                    ConsoleHelper.ColorWrite($"Elapsed {time} ms", ConsoleColor.Yellow);


                    string plainPass = Encoding.UTF8.GetString(decryptedPwd); //decrypt password
                    string plainUser = Encoding.UTF8.GetString(decryptedUser); //decrypt username

                    Console.WriteLine($"User: {plainUser} ");
                    Console.WriteLine($"Password: {plainPass}");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
            break;
        case 3: //exit the app
            Environment.Exit(0);
            break;
    }
}


static List<Password> readPwds(string file)
{
    using (StreamReader r = new StreamReader(file))
    {
        string json = r.ReadToEnd();
        return JsonSerializer.Deserialize<List<Password>>(json);
    }
}
static void writePwds(List<Password> allPwds, string file)
{
    string json = JsonSerializer.Serialize(allPwds, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText(file, json);
}
 
