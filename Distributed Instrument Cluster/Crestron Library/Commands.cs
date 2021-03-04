using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Crestron_Library {
	/// <summary>
	/// Class for getting keyboard and mice emulation commands.
	/// Use "key" from crestron spec sheet as command and translate it to bytes for corresponding "key" command.
	/// Tables imported from spec sheet has inconsistent formatting (keys have value in column 1 and mice have it in column 0).
	/// </summary>
	/// <author>Andre Helland</author>
	public class Commands {
		private readonly Dictionary<string, List<string>> commands = new Dictionary<string, List<string>>();
		public Commands() {
			var keyCommands = getCSV("KeyCommands(edited).csv");

			for (int i=1; i<keyCommands[0].Count ; i++) {
				commands.Add(keyCommands[0][i].ToLower(), new List<string>() { keyCommands[1][i], keyCommands[2][i] });
			}

			//Adds mice commands to command list (without break command).
			var miceCommands = getCSV("MiceCommands(edited).csv");

			for (int i = 1; i < miceCommands[0].Count; i++) {
				commands.Add(miceCommands[1][i].ToLower(),new List<string> {miceCommands[0][i]});
			}
		}

		/// <summary>
		/// Looks for key in key command list and returns byte value for make command.
		/// </summary>
		/// <param name="key">Key the function will find the make byte value for.</param>
		/// <returns>Byte value for make command of given key.</returns>
		public byte getMakeByte(string key) {
			try {
				if (commands.TryGetValue(key.ToLower(), out var temp))
					return Convert.ToByte(temp[0], 16);
			} catch { }
			throw new ArgumentException("Make byte not found for: \"" + key +"\".");
		}

		/// <summary>
		/// Looks for key in command list and returns byte value for break command.
		/// </summary>
		/// <param name="key">Key the function will find the break byte value for.</param>
		/// <returns>Byte value for make command of given key.</returns>
		public byte getBreakByte(string key) {
			try {
				if (commands.TryGetValue(key.ToLower(), out var temp))
					return Convert.ToByte(temp[1], 16);
			} catch { }
			throw new ArgumentException("Break byte not found for: \"" + key + "\".");
			}

		public List<string> getAllCommands() {
			return commands.Select(command => command.Key).ToList();
		}

		/// <summary>
		/// Function reads and pars CSV files into a nested string list.
		/// Function handles CSV formatted with comma delimitation and values enclosed in double quotes.
		/// Function automatically detects CSV with and scales matrix to correct size.
		/// </summary>
		/// <param name="file">CSV file the function will convert into a string matrix</param>
		/// <returns>CSV as a 2d string matrix</returns>
		private static List<List<string>> getCSV(string file) {
			var parser = new TextFieldParser(new StreamReader(file));
			var matrix = new List<List<string>>();

			// Set up parser settings.
			parser.HasFieldsEnclosedInQuotes = true;
			parser.SetDelimiters(",");

			//Generate amount of nested lists necessary
			var field = parser.ReadFields();
			foreach(string s in field) {
				matrix.Add(new List<string>());
			}

			//Add first row to matrix before entering while loop (Dirty fix).
			for (int y = 0; field.Length > y; y++) {
				matrix[y].Add(field[y]);
			}

			//Loop over whole CSV file and add entries to matrix.
			int i = 0;
			while (!parser.EndOfData) {
				field = parser.ReadFields();
				if (field != null)
					for (int y = 0; field.Length > y; y++) {
						matrix[y].Add(field[y]);
					}
				i++;
			}
			parser.Close();
			return matrix;
		}
	}
}