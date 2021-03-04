using System;
using System.Collections.Generic;
using System.Text;

namespace Instrument_Communicator_Library.Interface {
	/// <summary>
	/// Interface that is used for serializing objects in socket objects
	/// </summary>
	public interface ISerializeableObject {

		public byte[] getBytes();

		public object getObject(byte[] arrayBytes);

		

	}
}
