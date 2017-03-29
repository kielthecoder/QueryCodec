using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                        RunCommand(rec[0], rec[1], rec[2], rec[3]);
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

        static void RunCommand(string host, string user, string password, string command)
        {
            Console.WriteLine("--> Host={0}, User={1}, Password={2}, Command={3}", host, user, password, command);

            ConnectionInfo info = new ConnectionInfo(host, user,
                new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod(user, password)
                });

            using (SshClient client = new SshClient(info))
            {
                SshCommand output;

                client.Connect();
                output = client.RunCommand(command);
                client.Disconnect();

                Console.WriteLine("<-- {0}", output.Result);
            }
        }
    }
}
