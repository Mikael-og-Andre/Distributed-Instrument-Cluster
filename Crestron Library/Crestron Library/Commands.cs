using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Crestron_Library {
	/// <summary>
	/// Class for getting keyboard and mice emulation commands.
	/// Use "key" from crestron spec sheet as command and translate it to bytes for corresponding "key" command.
	/// Use "command" for mice commands, tables imported from spec sheet has inconsistent formatting (keys have value in column 1 and mice have it in column 0).
	/// Making some functions seemingly janky but is because the imported data is inconsistent.
	/// See function "commandIndexInList()"
	/// </summary>
	/// <author>Andre Helland</author>
	public class Commands {
		private List<List<String>> keyCommands;
		private List<List<String>> miceCommands;
		public Commands() {
			keyCommands = getCSV("KeyCommands(edited).csv");
			miceCommands = getCSV("MiceCommands(edited).csv");
		}

		/// <summary>
		/// Finds byte value for given command.
		/// </summary>
		/// <param name="command">Command the function will find byte value of.</param>
		/// <returns>Byte value for command.</returns>
		public byte getMiceByte(String command) {
			int index = commandIndexInList(command);
			return Convert.ToByte(miceCommands[0][index], 16);
		}

		/// <summary>
		/// Finds byte values for make and break command and combine them to produce a click command.
		/// </summary>
		/// <param name="key">Key the function will find the click byte value for.</param>
		/// <returns>Byte value for click command of given key.</returns>
		public byte[] getClickBytes(String key) {
			int index = keyIndexInList(key);
			byte[] clickBytes = new byte[2];
			clickBytes[0] = Convert.ToByte(keyCommands[1][index], 16);
			clickBytes[1] = Convert.ToByte(keyCommands[2][index], 16);
			return clickBytes;
		}

		/// <summary>
		/// Looks for key in key command list and returns byte value for make command.
		/// </summary>
		/// <param name="key">Key the function will find the make byte value for.</param>
		/// <returns>Byte value for make command of given key.</returns>
		public byte getMakeByte(String key) {
			int index = keyIndexInList(key);
			return Convert.ToByte(keyCommands[1][index], 16);
		}

		/// <summary>
		/// Looks for key in key command list and returns byte value for break command.
		/// </summary>
		/// <param name="key">Key the function will find the break byte value for.</param>
		/// <returns>Byte value for make command of given key.</returns>
		public byte getBreakByte(String key) {
			int index = keyIndexInList(key);
			return Convert.ToByte(keyCommands[2][index], 16);
		}

		/// <summary>
		/// Searches linearly key commands to find a match and returns index of that command/key.
		/// </summary>
		/// <param name="key">Key, function will try to find index of.</param>
		/// <returns>Index of given key.</returns>
		private int keyIndexInList(String key) {
			int index = -1;

			//TODO: improve search using hashmap mby?
			for(int i = 1; keyCommands[0].Count > i; i++) {
				if(keyCommands[0][i].ToLower().Equals(key.ToLower())) {
					index = i;
					break;
				}
			}

			//Key was not found throw exception.
			if (index == -1) {
				throw new ArgumentException("Key \"" + key + "\" was not found.");
			}

			return index;
		}

		/// <summary>
		/// Almost identical to the function "keyIndexInList()" but contains modifications for searching in mice command list.
		/// Byproduct of inconsistent formatting of tables from crestron cable documentation.
		/// </summary>
		/// <param name="command">Command, function will try to find index of.</param>
		/// <returns>Index of given command.</returns>
		private int commandIndexInList(String command) {
			int index = -1;

			//TODO: improve search using hashmap mby?
			for (int i = 1; keyCommands[1].Count > i; i++) {
				if (miceCommands[1][i].ToLower().Equals(command.ToLower())) {
					index = i;
					break;
				}
			}

			//Key was not found throw exception.
			if (index == -1) {
				throw new ArgumentException("Command \"" + command + "\" was not found.");
			}

			return index;
		}

		/// <summary>
		/// Lists all available key commands.
		/// </summary>
		/// <returns>String list of all key commands.</returns>
		public List<string> getAllKeyCommands() {
			return keyCommands[0].GetRange(1, keyCommands[0].Count - 1);
		}

		/// <summary>
		/// Lists all available mice commands.
		/// </summary>
		/// <returns>String list of all mice commands.</returns>
		public List<string> getAllMiceCommands() {
			return miceCommands[1].GetRange(1, miceCommands[0].Count - 1);
		}

		/// <summary>
		/// Function reads and pars CSV files into a nested string list.
		/// Function handles CSV formatted with comma delimitation and values enclosed in double quotes.
		/// Function automatically detects CSV with and scales matrix to correct size.
		/// TODO: handle other formats or throw exception. Add unit test.
		/// </summary>
		/// <param name="file">CSV file the function will convert into a string matrix</param>
		/// <returns>CSV as a 2d string matrix</returns>
		private List<List<String>> getCSV(String file) {
			TextFieldParser parser = new TextFieldParser(new StreamReader(file));
			List<List<String>> matrix = new List<List<String>>();

			// Set up parser settings.
			parser.HasFieldsEnclosedInQuotes = true;
			parser.SetDelimiters(",");

			//Generate amount of nested lists necessary
			string[] field;
			field = parser.ReadFields();
			foreach(String s in field) {
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
