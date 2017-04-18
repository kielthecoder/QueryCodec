using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace QueryCodec
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvFileName = "Test.csv";
            char delim = ',';

            StreamReader reader;
            string text;
            int count;

            if (args.Length > 0)
                csvFileName = args[0];

            try
            {
                reader = new StreamReader(csvFileName);
                text = reader.ReadLine();
                count = 0;

                while (text != null)
                {
                    try
                    {
                        string[] rec = text.Split(delim);
                        RunCommand(rec[0], rec[1], rec[2], rec[3], rec[4]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    count++;
                    text = reader.ReadLine();
                }

                Console.WriteLine("\nProcessed {0} records", count);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        static string SendShellCommand (string cmd, ShellStream stream)
        {
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);
            string text;
            bool ok;

            ok = false;
            while (!ok)
            {
                text = reader.ReadLine();

                if (text != null)
                {
                    // Console.WriteLine("*!* {0}", text);

                    if (text.Contains("OK"))
                        ok = true;
                }
                else
                    Thread.Sleep(100);
            }

            var result = new StringBuilder();

            writer.WriteLine(cmd);
            writer.Flush();

            ok = false;
            while (!ok)
            {
                text = reader.ReadLine();

                if (text != null)
                {
                    // Console.WriteLine("*!* {0}", text);

                    if (text.Contains("OK"))
                        ok = true;
                    else
                    {
                        result.AppendLine(text.Trim());
                    }
                }
                else
                    Thread.Sleep(100);
            }

            return result.ToString();
        }

        static void RunCommand(string host, string user, string password, string command, string createShell)
        {
            Console.WriteLine("--> Host={0}, User={1}, Password={2}, Command={3}, CreateShell={4}", host, user, password, command, createShell);

            PasswordAuthenticationMethod authPw = new PasswordAuthenticationMethod(user, password);

            KeyboardInteractiveAuthenticationMethod authKeyb = new KeyboardInteractiveAuthenticationMethod(user);
            authKeyb.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
            {
                foreach (AuthenticationPrompt prompt in e.Prompts)
                {
                    if (prompt.Request.Contains("Password:"))
                    {
                        prompt.Response = password;
                        Console.WriteLine("<-- Keyboard-Interactive: {0}", password);
                    }
                }
            };

            ConnectionInfo info = new ConnectionInfo(host, user,
                new AuthenticationMethod[]
                {
                    authPw,
                    authKeyb
                });

            using (SshClient client = new SshClient(info))
            {
                client.Connect();
                Console.WriteLine("--- CONNECTED");

                if (createShell == "1")
                {
                    using (var shell = client.CreateShellStream("xterm", 80, 25, 800, 600, 1024))
                    {
                        var result = SendShellCommand(command, shell);
                        Console.WriteLine("<-- Result={0}", result);
                    }
                }
                else
                {
                    // var output = client.RunCommand(command);
                    var cmd = client.CreateCommand(command);
                    cmd.CommandTimeout = TimeSpan.FromSeconds(10.0);

                    // cmd.Execute();
                    // var result = cmd.Result;
                    var result = cmd.EndExecute(cmd.BeginExecute(null, null));

                    // var result = output.Result;
                    Console.WriteLine("<-- Result={0}", result.Trim());
                }

                client.Disconnect();
                Console.WriteLine("--- DISCONNECTED\n");
            }
        }
    }
}
