﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Instrument_Communicator_Library.Interface {
	/// <summary>
	/// Interface that is used for serializing objects in socket objects
	/// </summary>
	public interface ISerializable {

		/// <summary>
		/// Get the bytes that represents this object
		/// </summary>
		/// <returns></returns>
		public byte[] getBytes();

		/// <summary>
		/// Reconstruct the object from input bytes
		/// </summary>
		/// <param name="arrayBytes">Bytes representing object</param>
		/// <returns></returns>
		public object getObject(byte[] arrayBytes);

		

	}
}