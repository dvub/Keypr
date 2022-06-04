using pwd_mgr_csharp;
using System.Security.Cryptography;
using System.Text;
using Figgle;
using System.Text.Json;

Console.WriteLine(FiggleFonts.Slant.Render("KeyPr") + "by dvub");
Console.WriteLine();
Console.WriteLine("Checking if data file exists...");

Func<int, byte[]> f = GenerateRandomBytes;


Func<Aes, byte[], byte[], string, byte[]> encrypt = Encrypt;

Func<Aes, byte[], string, byte[]> decrypt = Decrypt;
//a small note: encoding/decoding is UTF8 as JSON spec is UTF8
// thanks!

// TODO: 
// encrypt username / email - DONE
// hidden password input - DONE
// make sure password names are unique
// time operations with delegates and callbacks
// make sure that app doesn't crash when user searches pwds
// add notes section to password
// make console look good ig - color, spacing, etc.
// GENERATE RANDOM PASSWORDS!! - DONE
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
        byte[] hashed = GetHash(pass);


        //by using a hash for the master password, the plaintext can also serve as a key for encryption
        if (masterPwd.pwd.SequenceEqual<byte>(hashed))
        {
            string username = Encoding.UTF8.GetString(masterPwd.user);
            Console.WriteLine($"Welcome, {username}!");
            loggedIn = true;
            key = pass;
        } 
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Incorrect Password. Try again:");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
else //if the user doesn't have a data file, then we need to make one and set up a password
{
    Console.WriteLine("No data file was found.");
    Console.WriteLine("Enter username"); //setup a username for welcoming the user later
    string user = Console.ReadLine();
    while (string.IsNullOrEmpty(user))
        user = Console.ReadLine();
    Password master = new Password("master", Encoding.UTF8.GetBytes(user), GetHash(ConsoleHelper.confirmedPwd()));
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

                string name = Console.ReadLine();
                while (string.IsNullOrEmpty(name))
                    name = Console.ReadLine();

                Console.WriteLine("Enter username:");

                string user = Console.ReadLine();
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
                            string base64pass = Convert.ToBase64String(GenerateRandomBytes(16)); //generates a 16-byte-long password, pretty secure!
                            newPass = Encoding.UTF8.GetBytes(base64pass);
                            Console.WriteLine($"Generated a new Password! The password is: {base64pass}");
                            prompting = false;
                            break;
                        case 2:
                            newPass = Encoding.UTF8.GetBytes(ConsoleHelper.confirmedPwd());
                            prompting = false;
                            break;
                    }
                }
                //end of user input, lets encrypt
                byte[] userBytes = Encoding.UTF8.GetBytes(user);

                try 
                {
                    BenchmarkValue<byte[]> encryptPass = Benchmark.MeasureMethod<byte[]>(encrypt, aes, newPass, aes.IV, key);
                    BenchmarkValue<byte[]> encryptUser = Benchmark.MeasureMethod<byte[]>(encrypt, aes, userBytes, aes.IV, key);
                    byte[] total = encryptPass.returnValue;
                    byte[] totalUser = encryptUser.returnValue;

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
                    BenchmarkValue<byte[]> decryptOp = Benchmark.MeasureMethod<byte[]>(decrypt, aes, password.pwd, key);
                    BenchmarkValue<byte[]> decryptUserOp = Benchmark.MeasureMethod<byte[]>(decrypt, aes, password.user, key);

                    byte[] decryptedPwd = decryptOp.returnValue;
                    byte[] decryptedUser = decryptUserOp.returnValue;


                    double time = decryptOp.elapsed.TotalMilliseconds + decryptUserOp.elapsed.TotalMilliseconds;
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



static byte[] GenerateRandomBytes(int length)
{
    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
    {
        byte[] bytes = new byte[length];
        rng.GetBytes(bytes, 0, length);
        return bytes;
    }
}



static byte[] Encrypt(Aes aes, byte[] plain, byte[] iv, string key)
{
    byte[] salt = GenerateRandomBytes(8);


    aes.Key = CreateKey(key, salt);
    aes.GenerateIV();
    byte[] encrypted = aes.EncryptCbc(plain, aes.IV, PaddingMode.PKCS7); //encrypt password from plaintext
    byte[] first = Combine(aes.IV, encrypted); // combine
    byte[] second = Combine(salt, first);
    return second;
}

static byte[] Decrypt(Aes aes, byte[] bytes, string key)
{
    byte[] salt = bytes.Take(8).ToArray(); //get salt with first 8 bytes

    aes.Key = CreateKey(key, salt); //create a key


    byte[] iv = bytes.Skip(8).Take(16).ToArray(); //16 bytes AFTER salt

    byte[] encrypted = bytes.Skip(24).Take(bytes.Length - 24).ToArray(); //the rest of the ciphertext

    byte[] decryptedBytes = aes.DecryptCbc(encrypted, iv, PaddingMode.PKCS7); //decrypt using PKCS7 padding

    return decryptedBytes;
}

//https://www.techiedelight.com/concatenate-byte-arrays-csharp/
static byte[] Combine(byte[] first, byte[] second)
{
    byte[] result = new byte[first.Length + second.Length];
    Buffer.BlockCopy(first, 0, result, 0, first.Length); //copy  the first array into result array
    Buffer.BlockCopy(second, 0, result, first.Length, second.Length); //copy second array into result array, but starting at first.Length
    return result;

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

static byte[] GetHash(string inputString)
{
    using (HashAlgorithm algorithm = SHA256.Create())
    {
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }
}

static byte[] CreateKey(string pwd, byte[] salt)
{

    const int Iterations = 300;
    var keyGenerator = new Rfc2898DeriveBytes(pwd, salt, Iterations);
    return keyGenerator.GetBytes(32);

}
 

//CLASSES
class Password
{
    public string name { get; set; }
    public byte[] user { get; set; }
    public byte[] pwd { get; set; }
    public Password(string _name, byte[] _user, byte[] _pwd)
    {
        name = _name; 
        user = _user;
        pwd = _pwd;
    }
    public Password()
    {

    }
}

