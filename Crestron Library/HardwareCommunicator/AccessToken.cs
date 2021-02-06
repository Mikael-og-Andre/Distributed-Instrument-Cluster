using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Class that represents a authorization token for the server
/// @Author Mikael Nilssen
/// </summary>

namespace HardwareCommunicator {
    class AccessToken {

        //Intended for use when connecting to remote server. Generate on website and put manually in app settings
        public string connectionHash { get; private set; }

        public AccessToken(string connectionHash) {
            this.connectionHash = connectionHash;
        }

    }
}
