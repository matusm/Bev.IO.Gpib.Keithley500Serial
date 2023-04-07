using Bev.IO.Gpib.Keithley500Serial;
using Bev.IO.Gpib;
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

            int gpibAddress = 3;
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

            gpib.Remote(gpibAddress);
            Thread.Sleep(200);
            gpib.Output(gpibAddress, " ");
            Thread.Sleep(200);
            Console.WriteLine(gpib.Enter(gpibAddress));

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
