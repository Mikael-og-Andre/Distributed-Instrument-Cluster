using System;

namespace Instrument_Communicator_Library.Information_Classes {

    /// <summary>
    /// Used when sending videoData
    /// </summary>
    [Serializable]
    public class VideoObject {
        private string name;

        public VideoObject(string name) {
            this.name = name;
        }

        public string GetName() {
            return name;
        }

        public VideoObject Deserialize() {
            throw new NotImplementedException();
        }

    }
}