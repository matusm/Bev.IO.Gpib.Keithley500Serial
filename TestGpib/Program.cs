using Bev.IO.Gpib.Keithley500Serial;
using Bev.IO.RemoteInterface;
using System;
using System.Globalization;
using System.Threading;


namespace TestGpib
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            int gpibAddress = 11;
            string port = "COM1";

            Console.WriteLine("initializing ...");
            Keithley500Serial gpib = new Keithley500Serial(port);
            Console.WriteLine("done");

            //gpib.SendGpib(gpibAddress, "SYSTEM:VERSION?");
            //Console.WriteLine($"version: {gpib.ReadGpib(gpibAddress)}");

            //gpib.SendGpib(gpibAddress, "*IDN?");
            //Console.WriteLine($"id: {gpib.ReadGpib(gpibAddress)}");

            ////gpib.SendGpib(gpibAddress, "SENSE:FUNCTION:VOLTAGE:DC");
            //gpib.SendGpib(gpibAddress, "SENSE:VOLTAGE:DC:RANGE:UPPER 20");
            //Console.WriteLine(gpib.ReadGpib(gpibAddress));


            // testing SPEX communication

            // helper function
            string OutputEnter(string command, int delay)
            {
                gpib.Output(gpibAddress, command);
                Thread.Sleep(delay);
                return gpib.Enter(gpibAddress);
            }

            void Debug(string command, int delay)
            {
                string response = OutputEnter(command, delay);
                Console.WriteLine($"'{command}' -> '{response}'");
            }

            //gpib.Remote(gpibAddress);
            //Thread.Sleep(200);

            //Console.WriteLine("initializing ...");
            //gpib = new Keithley500Serial(port);
            //Console.WriteLine("done");


            Console.WriteLine($"Address: {gpibAddress:D2}");


            Debug(" ", 200); // should be *

            char[] buff = { 'O', '0', '0', '0', (char)0 };
            string startMain = new string(buff);
            Debug(startMain, 500); // should be *

            Debug("z", 0); // main version

            Console.WriteLine("===============================================================");


            return;

            Debug(" ", 200); // should be *

            //char[] buff = {'O', '0', '0', '0' , (char)0};
            //string startMain = new string(buff);
            //Debug(startMain, 500); // should be *

            Debug("z", 0); // main version

            Debug("y", 0); // boot version

            Debug("A", 100_000); // motor init

            Debug("C0", 0); // motor read speed

            Debug("H0", 0); // motor read position

            Debug("F0,2000", 0);
            for (int i = 0; i < 20; i++)
            {
                Debug("E", 100);
            }

            Debug("H0", 0); // motor read position


            //    gpib.Remote(gpibAddress);
            //    Thread.Sleep(200);
            //    for (int i = 0; i < 10; i++)
            //    {
            //        gpib.Trigger(gpibAddress);
            //        //gpib.SendGpib(gpibAddress, "FETCH?");
            //        Thread.Sleep(200);
            //        string str = $"{i} - {gpib.Enter(gpibAddress)}";
            //        Console.WriteLine(str);
            //    }
            //    gpib.Local(gpibAddress);
            //    Console.WriteLine();



        }
    }
}
