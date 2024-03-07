using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;

namespace PasswordGen
{
    internal class Program
    {
        private static Dictionary<ConsoleKey, Action> actions = new Dictionary<ConsoleKey, Action>();
        private static KeyValuePair<ConsoleKey, Action> nullKey = new KeyValuePair<ConsoleKey, Action>((ConsoleKey)27, (Action)null);
        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890~`!@#$%^&*()_-+={[}]|:;'<,>.?/".ToCharArray();
        private static readonly string applicationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "pwGenerator");

        private static EncryptionProfile CurrentProfile { get; set; }

        private TextMeshProUGUI errorMessage;

        private static void PopulateActions(List<Action> actionsToAdd)
        {
            Program.actions.Clear();
            for (int index = 0; index < actionsToAdd.Count; ++index)
            {
                if (index > 8)
                {
                    Exception exception = new Exception("Too many inputs at once!");
                    break;
                }
                Program.actions.Add((ConsoleKey)(index + 49), actionsToAdd[index]);
            }
        }

        private static Action SelectionCheck(
          ConsoleKey input,
          Dictionary<ConsoleKey, Action> collectionToCheck)
        {
            KeyValuePair<ConsoleKey, Action> keyValuePair1 = Program.nullKey;
            foreach (KeyValuePair<ConsoleKey, Action> keyValuePair2 in collectionToCheck)
            {
                if (input == keyValuePair2.Key)
                    keyValuePair1 = keyValuePair2;
            }
            return keyValuePair1.Value;
        }

        private static void GeneratePassword()
        {
            Console.WriteLine("Generate Password Selected!");
            bool flag1 = false;
            string empty = string.Empty;
            string str = string.Empty;
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
            while (!flag1)
            {
                Console.WriteLine("Enter path of password");
                str = Console.ReadLine() + ".pgenpw";
                if (File.Exists(str))
                {
                    Console.WriteLine("That password file already exists, do you want to overwrite it? y/n");
                    //TODO: Prompt whether to overwrite
                }
                else
                    flag1 = true;
            }
            bool flag2 = false;
            int result = 0;
            while (!flag2)
            {
                Console.WriteLine("Enter password size");
                if (int.TryParse(Console.ReadLine(), out result) && result <= 12 && result > 0)
                    flag2 = true;
                if (!flag2)
                    Console.WriteLine("Invalid password size, please try again");
            }
            Console.WriteLine("Generating Password...");
            Password passwordObject = Program.GeneratePasswordObject(result, Program.chars, empty, Program.CurrentProfile);
            Console.WriteLine("password:");
            Console.WriteLine(passwordObject.key);
            using (StreamWriter streamWriter = new StreamWriter((Stream)File.OpenWrite(str)))
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter)streamWriter))
                    jsonSerializer.Serialize((JsonWriter)jsonTextWriter, (object)passwordObject, typeof(Password));
            }
            Console.WriteLine("Password generated!");
        }

        /*TODO: replace
            SimpleAES simpleAes = new SimpleAES(Program.CurrentProfile.encryptionKey, Program.CurrentProfile.encryptionVector);
            with the Program.CurrentProfile.derivedEncryptionKey, need to maintain backwards compatibility for old password
            files. 
        */
        private static void UsePassword()
        {
            Console.WriteLine("Use Password Selected!");
            List<string> stringList = new List<string>();
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
            SimpleAES simpleAes = new SimpleAES(Program.CurrentProfile.encryptionKey, Program.CurrentProfile.encryptionVector);
            bool flag = false;
            while (!flag)
            {
                Console.WriteLine("Please enter password file path");
                string str = Console.ReadLine();
                if (!File.Exists(str))
                {
                    Console.WriteLine("Please enter valid file path!");
                }
                else
                {
                    using (StreamReader reader1 = new StreamReader((Stream)File.OpenRead(str)))
                    {
                        using (JsonTextReader reader2 = new JsonTextReader((TextReader)reader1))
                        {
                            Password password = jsonSerializer.Deserialize<Password>((JsonReader)reader2);
                            Console.WriteLine(simpleAes.DecryptString(password.key));
                        }
                    }
                }
            }
        }

        public static Password GeneratePasswordObject(
          int length,
          char[] charsToUse,
          string pwName,
          EncryptionProfile profToUse)
        {
            Password passwordObject = new Password();
            byte[] numArray = new byte[4 * length];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
                randomNumberGenerator.GetBytes(numArray);
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int index1 = 0; index1 < length; ++index1)
            {
                long index2 = (long)BitConverter.ToUInt32(numArray, index1 * 4) % (long)charsToUse.Length;
                stringBuilder.Append(charsToUse[index2]);
            }
            SimpleAES simpleAes = new SimpleAES(profToUse.derivedEncryptionKey, profToUse.encryptionVector);
            passwordObject.key = simpleAes.EncryptToString(stringBuilder.ToString());
            passwordObject.name = pwName;
            return passwordObject;
        }
    }
}