using Bev.IO.RemoteInterface;
using System;
using System.IO.Ports;
using System.Threading;

namespace Bev.IO.Gpib.Keithley500Serial
{
    public class Keithley500Serial : IRemoteInterface
    {
        public Keithley500Serial(string portName)
        {
            comPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            comPort.Handshake = Handshake.RequestToSend;
            comPort.NewLine = "\r";
            comPort.ReadTimeout = hugeDelay;
            comPort.WriteTimeout = hugeDelay;
            OpenPort();
            TogglePower(); // this is essential!
            Initialize();
        }

        // Instruction Manual 3-9
        public void Output(int address, string command)
        {
            CheckIeeeAddress(address);
            Send500($"OA;{address.ToString("D2")};{command}");
        }
        public void Output(string command) => Send500($"O;{command}");

        // Instruction Manual 3-6
        public string Enter(int address)
        {
            CheckIeeeAddress(address);
            Send500($"EN;{address.ToString("D2")}");
            return Read500();
        }
        public string Enter()
        {
            Send500("EN");
            return Read500();
        }

        // Instruction Manual 3-13
        public void Trigger(int address)
        {
            CheckIeeeAddress(address);
            Send500($"TR;{address.ToString("D2")}");
        }
        public void Trigger() => Send500("TR");

        // Instruction Manual 3-14
        public void Escape() => Send500($"{(char)1}", noDelay);

        // Instruction Manual 3-4
        public void SelectiveDeviceClear(int address) => DeviceClear(address);

        public void DeviceClear(int address)
        {
            CheckIeeeAddress(address);
            Send500($"C;{address.ToString("D2")}");
        }

        // Instruction Manual 3-3
        public void DeviceClear() => Send500("C", noDelay);

        // Instruction Manual 3-8
        public void Local(int address)
        {
            CheckIeeeAddress(address);
            Send500($"L;{address.ToString("D2")}");
        }
        public void Local() => Send500("L");

        // Instruction Manual 3-10
        public void Remote(int address)
        {
            CheckIeeeAddress(address);
            Send500($"RE;{address.ToString("D2")}");
        }
        public void Remote() => Send500("RE");

        // Instruction Manual 3-11
        public string SerialPoll(int address)
        {
            CheckIeeeAddress(address);
            Send500($"SP;{address.ToString("D2")}");
            return Read500();
        }

        // Instruction Manual 3-11
        public string SrqCheck()
        {
            Send500($"SQ");
            return Read500();
        }

        // Instruction Manual 3-10
        public void Resume() => Send500("RS", noDelay);

        // Instruction Manual 3-8
        public void LocalLockout() => Send500("LL", noDelay);

        // Instruction Manual 3-3
        public void Abort() => Send500("A", noDelay);

        // Instruction Manual 3-15
        public void Attention() => Send500("/A", noDelay);

        // Instruction Manual 3-16
        public void MyListenAddress() => Send500("/ML", noDelay);

        // Instruction Manual 3-16
        public void MyTalkAddress() => Send500("/MT", noDelay);

        // Instruction Manual 3-17
        public void UnListen() => Send500("/UL", noDelay);

        // Instruction Manual 3-17
        public void UnTalk() => Send500("/UT", noDelay);

        // Instruction Manual 3-15
        public void Listen(int address)
        {
            CheckIeeeAddress(address);
            Send500($"/L;{address.ToString("D2")}", noDelay);
        }

        // Instruction Manual 3-17
        public void Talk(int address)
        {
            CheckIeeeAddress(address);
            Send500($"/T;{address.ToString("D2")}", noDelay);
        }



        private void Send500(string command) => Send500(command, defaultDelay);

        private void Send500(string command, int delay)
        {
            try
            {
                comPort.WriteLine(command);
                Thread.Sleep(delay);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">write timeout<");
            }
        }

        private string Read500()
        {
            try
            {
                char[] buffer = new char[512];
                comPort.Read(buffer, 0, buffer.Length);
                char[] charsToTrim = { (char)0, '\r', '\n' };
                return new string(buffer).TrimEnd(charsToTrim);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">read timeout<");
                return string.Empty;
            }
        }

        private void TogglePower()
        {
            comPort.DtrEnable = false;
            Thread.Sleep(hugeDelay);
            comPort.DtrEnable = true;
            Thread.Sleep(defaultDelay);
        }

        private void Initialize()
        {
            OpenPort();
            // TogglePower();
            // send five carriage returns separated by a 0.1 s delay (=defaultDelay)
            // to allow the 500-SERIAL to adjust its baud rate
            Send500("");
            Send500("");
            Send500("");
            Send500("");
            Send500("");
            Send500("I", noDelay);      // INIT command
            Send500("EC;0", noDelay);   // turn echo off
            Send500("H;1", noDelay);    // turn on hardware handshake
            Send500("X;0", noDelay);    // turn off XON/XOFF handshake
            Send500("TC;2", noDelay);   // set serial terminator to CR
            Send500("TB;4");            // set bus terminator to CR+LF
            Read500();                  // clear buffer
            DeviceClear();              // (Device Clear) to clear the GPIB bus
        }

        private void OpenPort()
        {
            try
            {
                if (!comPort.IsOpen)
                    comPort.Open();
            }
            catch (Exception)
            { }
        }

        private void ClosePort()
        {
            try
            {
                if (comPort.IsOpen)
                    comPort.Close();
            }
            catch (Exception)
            { }
        }

        private void CheckIeeeAddress(int address)
        {
            if (address < 0) throw new ArgumentException("IEEE 488 address cannot be negative");
            if (address > 31) throw new ArgumentException("IEEE 488 address cannot be larger than 31");
        }

        private readonly SerialPort comPort;
        private const int defaultDelay = 100;   // in ms
        private const int hugeDelay = 5000;     // in ms
        private const int noDelay = 0;          // no delay at all
    }
}
