using pwd_mgr_csharp;
using System.Security.Cryptography;
using System.Text;
using Figgle;
using System.Linq;
using System.Diagnostics;

//EF Core:
// * PMC: Add-Migration [migration name]
// * Update-Database


//a small note: encoding/decoding is UTF8 as JSON spec is UTF8

//color coding:
// green = good response
// magenta = information ?
// red = bad
// Blue = information prompt



// TODO: 
// ***** REWRITE WITH SQLITE ***** - READ PASSWORDS
// measure all operations instead of just some
// boolean prompt - DONE
// cancel a new password creation
// all user input to lower case -
// make sure password names are unique
// make sure that app doesn't crash when user searches pwds - DONE
// add notes section to password - CURRENT
// ability to edit pwds 
// make console look good - color, spacing, etc. - DONE for now
// regenerate passwords - DONE
// implement class exceptions


Console.WriteLine(FiggleFonts.Slant.Render("KeyPr"));
Console.WriteLine();
Stopwatch sw = Stopwatch.StartNew();
using var db = new PwdsContext(); // constructor with 0 args

Console.WriteLine($"Database path: {db.DbPath}");
// initial query to get the master password - if one exists
Password? master = db.Passwords.FirstOrDefault(x => x.Name == ".MASTER");


if (master == null)
{
    Console.WriteLine("No master password exists.");
    Console.WriteLine("Enter username"); //setup a username for welcoming the user later
    string user = ConsoleHelper.ReadNotNull();
    byte[] userBytes = Encoding.UTF8.GetBytes(user);
    db.Add(new Password { Name = ".MASTER", Pass = CryptoHelper.GetHash(ConsoleHelper.ConfirmedPwd()), Username = userBytes });
    db.SaveChanges();

    Console.WriteLine("Successfully created a new master password! Restart the application to log in.");
    Environment.Exit(0);
}

// LOGIN TO APPLICATION //
string key = "";
bool loggedIn = false;

Console.WriteLine();
ConsoleHelper.ColorWrite("Please Enter Master Password:", ConsoleHelper.promptColor);

int attempts = 0; //counting the attempts of logging in lol
while (!loggedIn)
{
    string pass = ConsoleHelper.HiddenPassword();
    byte[] hashed = CryptoHelper.GetHash(pass);


    //by using a hash for the master password, the plaintext can also serve as a key for encryption
    if (master.Pass.SequenceEqual(hashed))
    {
        string username = Encoding.UTF8.GetString(master.Username);
        Console.WriteLine();
        ConsoleHelper.ColorWrite($"Welcome, {username}!", ConsoleColor.Green);
        Console.WriteLine();
        loggedIn = true;
        key = pass;
    } 
    else
    {
        attempts++;
        ConsoleHelper.ColorWrite($"Incorrect Password. Try again: ({attempts})", ConsoleColor.Red);
        // Thread.Sleep(1000); maybe add a delay to prevent brute force..?
    }
}

// ONCE USER IS LOGGED IN: //
int input = 0;
string[] options = new string[3] { "Add new password", "View Password", "Exit", };
while (input != options.Length)
{

    input = ConsoleHelper.PromptOptions("What would you like to do?", options); //prompt the user with options for the application
    switch (input)
    {
        
        case 1: //create a new password
            
            using (Aes aes = Aes.Create())
            {

                ConsoleHelper.ColorWrite("Enter name of website/application:", ConsoleHelper.promptColor); //get some user input
                string name = ConsoleHelper.ReadNotNull();

                ConsoleHelper.ColorWrite("Enter username:", ConsoleHelper.promptColor);
                string user = ConsoleHelper.ReadNotNull();

                ConsoleHelper.ColorWrite("Add Notes:", ConsoleHelper.promptColor);
                string? notes = Console.ReadLine();

                string[] passOpts = new string[3] { "Generate Random (Recommended)", "Create one myself", "Cancel" };
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
                        case 3:
                            prompting = false;
                            break;
                    }
                }
                //end of user input, lets encrypt
                byte[] userBytes = Encoding.UTF8.GetBytes(user);
                byte[] notesBytes = Encoding.UTF8.GetBytes(notes);

                if (passBytes.Length == 0)
                {
                    break;
                }
                try 
                {
                    // benchmarking and getting data
                    Benchmark<byte[]> encryptPass = new Benchmark<byte[]>(CryptoHelper.Encrypt, aes, passBytes, aes.IV, key);
                    Benchmark<byte[]> encryptUser = new Benchmark<byte[]>(CryptoHelper.Encrypt, aes, userBytes, aes.IV, key);
                    Benchmark<byte[]> encryptNotes = new Benchmark<byte[]>(CryptoHelper.Encrypt, aes, notesBytes, aes.IV, key);

                    List < Benchmark<byte[]>> operations = new List<Benchmark<byte[]>>();

                    byte[] _pass = encryptPass.returnValue;
                    byte[] _user = encryptUser.returnValue;
                    byte[] _notes = encryptNotes.returnValue;


                    
                    encryptPass.DisplayBench();
                    encryptUser.DisplayBench();
                    encryptNotes.DisplayBench();
                    Console.WriteLine();

                    db.Add(new Password { Name = name, Username = _user, Pass = _pass, Notes = _notes });
                    db.SaveChanges();


                } catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }


            break;
        case 2: //READ A PASSWORD
            using (Aes aes = Aes.Create())
            {
                Password? password = null;
                //search List 
                while (password == null)
                {
                    ConsoleHelper.ColorWrite("Enter name of password:", ConsoleHelper.promptColor);
                    string name = ConsoleHelper.ReadNotNull();
                    password = db.Passwords.FirstOrDefault(x => x.Name == name);

                }
                //decrypt
                try
                {

                    Benchmark<byte[]> decrypt = new Benchmark<byte[]>(CryptoHelper.Decrypt, aes, password.Pass, key);
                    Benchmark<byte[]> decryptUser = new Benchmark<byte[]>(CryptoHelper.Decrypt, aes, password.Username, key);
                    Benchmark<byte[]> ?decryptNotes = new Benchmark<byte[]>(CryptoHelper.Decrypt, aes, password.Notes, key);
                    byte[] _password = decrypt.returnValue;
                    byte[] _user = decryptUser.returnValue;
                    byte[]? _notes = decryptNotes.returnValue;
                    Console.WriteLine();
                    decrypt.DisplayBench();
                    decryptUser.DisplayBench();
                    string plainPass = Encoding.UTF8.GetString(_password); //decrypted password
                    string plainUser = Encoding.UTF8.GetString(_user); //decrypted username
                    string plainNotes = Encoding.UTF8.GetString(_notes);
                    //display
                    Console.WriteLine();
                    ConsoleHelper.ColorWrite("Password Information:", ConsoleColor.Green);
                    Console.Write($" > User: ");
                    ConsoleHelper.ColorWrite(plainUser, ConsoleColor.Magenta);
                    Console.Write($" > Password: "); 
                    ConsoleHelper.ColorWrite(plainPass, ConsoleColor.Magenta);
                    Console.Write($" > Password: ");
                    ConsoleHelper.ColorWrite(plainNotes, ConsoleColor.Magenta);
                    Console.WriteLine();
 

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
