using System;
using Crestron_Library;

namespace Crestron_Demo_Project {
    /// <summary>
    /// Rough demo project for testing crestron library.
    /// </summary>
    /// <author>Andre Helland</author>
	class Program {
        static void Main(string[] args) {
			SerialPortInterface serialPort = new SerialPortInterface();
            Commands commands = new Commands();
            promptPortChoice(serialPort);
            bool quit = false;
            while(!quit) {
                byte[] command;
                String consoleLine = Console.ReadLine();

                if (consoleLine.ToLower().Equals("quit")) {
                    quit = true;
                    break;
				}

                //List all available crestron commands.
                if (consoleLine.ToLower().Equals("ls")) {
                    Console.WriteLine("Key commands:");
                    foreach (String s in commands.getAllKeyCommands()) { Console.WriteLine(s); }
                    Console.WriteLine("Mice commands:");
                    foreach (String s in commands.getAllMiceCommands()) { Console.WriteLine(s); }
                }
                    try {
					command = commands.getClickBytes(consoleLine);
                    serialPort.sendBytesSafe(command);
                } catch {
                    try {
                        command = new byte[] { commands.getMiceByte(consoleLine) };
                        serialPort.sendBytesSafe(command);
                    } catch {
                        Console.WriteLine("No command found");
                    }
				}
			}
        }

        private static void promptPortChoice(SerialPortInterface serialPort) {
            Console.WriteLine("Available ports:");
            foreach (String s in serialPort.getAvailablePorts()) {
                Console.WriteLine(s);
            }

            String portChoice = Console.ReadLine();
            try {
                serialPort.setSerialPort(portChoice);
            } catch {
                Console.WriteLine("Failed to set port to: \"{0}\"", portChoice);
                promptPortChoice(serialPort);
            }
        }
    }
}
