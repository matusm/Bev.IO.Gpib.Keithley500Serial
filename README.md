Bev.IO.Gpib.Keithley500Serial
=============================

## Overview

A lightweight .NET library to operate the legacy model 500-SERIAL RS-232 to IEEE-488 converter by Keithley. It uses the native SerialPort class only.

The class is an implementation of the `IGpibHandler` interface.

## Usage

Instatiate a `Keithley500Serial` object with the port name as parameter.

There are several methods which can be used:

`DeviceClear()` : clears the GPIB bus.

`Output(int address, string command)` : send a command to the GPIB instrument with the respective address. (one overload)

`Enter(int address)` : read a response from the GPIB instrument with the respective address. (one overload)
 
