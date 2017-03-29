using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryCodec
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvFileName = "test.csv";
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
    }
}
