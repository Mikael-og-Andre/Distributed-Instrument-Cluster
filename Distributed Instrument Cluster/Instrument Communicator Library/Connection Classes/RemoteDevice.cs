using System;
using System.Collections.Generic;
using System.Text;
using Instrument_Communicator_Library.Information_Classes;


namespace Instrument_Communicator_Library {

	/// <summary>
	/// Represents a video and Keyboard/mouse connection to a remote device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDevice {

        private string name;    //Name of the device
        private int id;         //Id Of the connection

        private CrestronConnection crestronConnection;  //Contains information about the socket and connection to the crestron of the device
        private VideoConnection videoConnection;    //Contains information about the socket and connection to the video device

        public bool hasCrestron { get; private set; }
        public bool hasVideo { get; private set; }

        public RemoteDevice(string name) {
            this.name = name;
            this.crestronConnection = null;
            this.videoConnection = null;
            this.hasCrestron = false;
            this.hasVideo = false;
        }

        /// <summary>
        /// Sets the crestron connection
        /// </summary>
        /// <param name="con">Crestron connection Object</param>
        public void setCrestronConnection(CrestronConnection con) {
            this.crestronConnection = con;
            this.hasCrestron = true;
        }

        /// <summary>
        /// Get the crestronConnection, or throw null refrence exception if null
        /// </summary>
        /// <returns>CrestronConnection object</returns>
        public CrestronConnection getCrestronConnection() {
            if (this.hasCrestron) {
                return this.crestronConnection;
            } else {
                throw new NullReferenceException();
            }
        }

        /// <summary>
        /// Set the VideoConnection object
        /// </summary>
        /// <param name="con"></param>
        public void setVideoConnection(VideoConnection con) {
            this.videoConnection = con;
            this.hasVideo = true;
        }

        /// <summary>
        /// Get videoConnection Object, or throw null refrence exception if null
        /// </summary>
        /// <returns>Video connection object</returns>
        public VideoConnection getVideoConnection() {
            if (this.hasVideo) {
                return this.videoConnection;
            } else {
                throw new NullReferenceException();
            }
        }

    }
}
