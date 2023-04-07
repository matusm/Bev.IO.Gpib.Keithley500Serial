﻿using Bev.IO.Gpib;
using System;
using System.IO.Ports;
using System.Threading;

namespace Bev.IO.Gpib.Keithley500Serial
{
    public class Keithley500Serial : IGpibHandler
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
            Thread.Sleep(defaultDelay);
        }
        public void Output(string command)
        {
            Send500($"O;{command}");
            Thread.Sleep(defaultDelay);
        }

        // Instruction Manual 3-6
        public string Enter(int address)
        {
            CheckIeeeAddress(address);
            Send500($"EN;{address.ToString("D2")}");
            Thread.Sleep(defaultDelay);
            return Read500();
        }
        public string Enter()
        {
            Send500($"EN");
            Thread.Sleep(defaultDelay);
            return Read500();
        }

        // Instruction Manual 3-13
        public void Trigger(int address)
        {
            CheckIeeeAddress(address);
            Send500($"TR;{address.ToString("D2")}");
            Thread.Sleep(defaultDelay);
        }
        public void Trigger()
        {
            Send500($"TR");
            Thread.Sleep(defaultDelay);
        }

        // Instruction Manual 3-14
        public void Escape() => Send500($"{(char)1}");

        // Instruction Manual 3-3
        public void DeviceClear() => Send500("C");

        // Instruction Manual 3-8
        public void Local(int address)
        {
            CheckIeeeAddress(address);
            Send500($"L;{address.ToString("D2")}");
            Thread.Sleep(defaultDelay);
        }
        public void Local()
        {
            Send500($"L");
            Thread.Sleep(defaultDelay);
        }

        // Instruction Manual 3-10
        public void Remote(int address)
        {
            CheckIeeeAddress(address);
            Send500($"RE;{address.ToString("D2")}");
            Thread.Sleep(defaultDelay);
        }
        public void Remote()
        {
            Send500($"RE");
            Thread.Sleep(defaultDelay);
        }

        private void Send500(string command)
        {
            try
            {
                comPort.WriteLine(command);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">w-timeout<");
                ClosePort();
                Initialize();
            }
        }

        private string Read500()
        {
            try
            {
                char[] buffer = new char[512];
                comPort.Read(buffer, 0, buffer.Length);
                //DebugOut(buffer);
                char[] charsToTrim = { (char)0, '\r', '\n' };
                return new string(buffer).TrimEnd(charsToTrim);
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">r-timeout<");
                ClosePort();
                Initialize();
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
            //TogglePower();
            // send five carriage returns separated by a 0.1 s delay
            // to allow the 500-SERIAL to adjust its baud rate
            Send500("");
            Thread.Sleep(defaultDelay);
            Send500("");
            Thread.Sleep(defaultDelay);
            Send500("");
            Thread.Sleep(defaultDelay);
            Send500("");
            Thread.Sleep(defaultDelay);
            Send500("");
            Thread.Sleep(defaultDelay);
            Send500("I");       // INIT command
            Send500("EC;0");    // turn echo off
            Send500("H;1");     // turn on hardware handshake
            Send500("X;0");     // turn off XON/XOFF handshake
            Send500("TC;2");    // set serial terminator to CR
            Send500("TB;4");    // set bus terminator to CRLF
            Thread.Sleep(defaultDelay); // provide time for the port receive buffer to fill
            Read500();          // clear buffer
            DeviceClear();      // (Device Clear) to clear the GPIB bus
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
    }
}
