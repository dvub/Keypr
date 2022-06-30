using pwd_mgr_csharp;
using System.Security.Cryptography;
using System.Text;
using Figgle;
using System.Linq;
using System.Diagnostics;
//a small note: encoding/decoding is UTF8 as JSON spec is UTF8

//EF Core:
// * PMC: Add-Migration [migration name]
// * Update-Database

//color coding:
// green = good response
// magenta = information ?
// red = bad
// Blue = information prompt


// TODO: 
// XML DOCS - begun
// json config - not yet implemented
// sqlite - done
// boolean prompt - DONE
// cancel a new password creation- 
// all user input to lower case -
// make sure password names are unique
// make sure that app doesn't crash when user searches pwds - DONE
// add notes section to password - DONE
// ability to edit pwds - DONE
// make console look good - color, spacing, etc. - DONE for now
// regenerate passwords - DONE
// implement class exceptions

Console.WriteLine(FiggleFonts.Slant.Render("KeyPr"));
Console.WriteLine();
using var db = new PwdsContext(); // constructor with 0 args

ConsoleHelper.ColorWrite($"Database path: {db.DbPath}", ConsoleColor.Yellow);
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
string[] options = new string[4] { "Add new password", "View Password", "Edit Passowrd", "Exit", };
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
                while (name == ".MASTER")
                {
                    ConsoleHelper.ColorWrite("Reserved password name is not allowed. Try again:", ConsoleColor.Red);
                    name = ConsoleHelper.ReadNotNull();
                }

                ConsoleHelper.ColorWrite("Enter username:", ConsoleHelper.promptColor);
                string user = ConsoleHelper.ReadNotNull();

                ConsoleHelper.ColorWrite("Add Notes:", ConsoleHelper.promptColor);
                string? notes = Console.ReadLine();

                //end of user input, lets encrypt
                byte[] passBytes = ConsoleHelper.PwdGenPrompt();
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
                    List<Benchmark<byte[]>> operations = new List<Benchmark<byte[]>>() { encryptNotes, encryptPass, encryptUser };
                    double time = 0;
                    operations.ForEach(x =>
                    {
                        time += x.elapsed;
                    });
                    ConsoleHelper.ColorWrite($"Elapsed {time} ms. (encryption)", ConsoleColor.Yellow);

                    byte[] _pass = encryptPass.returnValue;
                    byte[] _user = encryptUser.returnValue;
                    byte[] _notes = encryptNotes.returnValue;
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
                    string findName = ConsoleHelper.ReadNotNull();

                    password = db.Passwords.FirstOrDefault(x => x.Name == findName);

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
                    List<Benchmark<byte[]>> operations = new List<Benchmark<byte[]>>() { decrypt, decryptNotes, decryptUser };
                    double time = 0;
                    operations.ForEach(x =>
                    {
                        time += x.elapsed;
                    });
                    ConsoleHelper.ColorWrite($"Elapsed {time} ms. (decryption)", ConsoleColor.Yellow);


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
                    Console.Write($" > Notes: ");
                    ConsoleHelper.ColorWrite(plainNotes, ConsoleColor.Magenta);
                    Console.WriteLine();
 

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            break;
        case 3: // EDIT A PASSWORD
            Password? editPwd = null;
            //search List 
            while (editPwd == null)
            {
                ConsoleHelper.ColorWrite("Enter name of password:", ConsoleHelper.promptColor);
                string findName = ConsoleHelper.ReadNotNull();

                editPwd = db.Passwords.FirstOrDefault(x => x.Name == findName);

            }
            ConsoleHelper.ColorWrite("Leave any field blank if you do not wish to edit it.", ConsoleColor.White);
            ConsoleHelper.ColorWrite("Edit Name:", ConsoleHelper.promptColor);
            string? newName = Console.ReadLine();

            ConsoleHelper.ColorWrite("Edit User:", ConsoleHelper.promptColor);
            string? newUser = Console.ReadLine();

            ConsoleHelper.ColorWrite("Would you like to edit password? (y/n)", ConsoleHelper.promptColor);
            bool isEditingPwd = ConsoleHelper.BoolPrompt();
            byte[]? newPassBytes = null;
            if (isEditingPwd == true)
                newPassBytes = ConsoleHelper.PwdGenPrompt();

            ConsoleHelper.ColorWrite("Edit Notes:", ConsoleHelper.promptColor);
            string? newNotes = Console.ReadLine();

            Stopwatch sw = Stopwatch.StartNew();

            using (Aes aes = Aes.Create())
            {
                byte[] newUserBytes = CryptoHelper.Encrypt(aes, Encoding.UTF8.GetBytes(newUser), aes.IV, key);
                byte[] encryptPassBytes = CryptoHelper.Encrypt(aes, newPassBytes, aes.IV, key);
                byte[] newNotesBytes = CryptoHelper.Encrypt(aes, Encoding.UTF8.GetBytes(newNotes), aes.IV, key);


                if (!string.IsNullOrWhiteSpace(newName))
                    editPwd.Name = newName;
                
                if (!string.IsNullOrWhiteSpace(newUser))
                    editPwd.Username = newUserBytes;

                if (newPassBytes != null)
                    editPwd.Pass = encryptPassBytes;

                if (!string.IsNullOrEmpty(newNotes))
                    editPwd.Notes = newNotesBytes;

            }

            db.SaveChanges();
            sw.Stop();
            ConsoleHelper.ColorWrite($"Elapsed {sw.Elapsed.TotalMilliseconds} ms. (edit)", ConsoleColor.Yellow);

            break;
        case 4: //exit the app
            Environment.Exit(0);
            break;
    }
}
