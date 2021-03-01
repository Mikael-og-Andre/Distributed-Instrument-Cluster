using System;
using System.Collections.Generic;
using System.Text;

namespace Instrument_Communicator_Library.Interface {
	public interface ISerializeableObject {

		public byte[] getBytes();

		public object getObject(byte[] arrayBytes);

		

	}
}
