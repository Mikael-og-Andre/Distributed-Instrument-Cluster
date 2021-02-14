
/// <summary>
/// Enum used for synchronaztion in protocol use between server and client
/// <author>Mikael Nilssen</author>
/// </summary>
namespace Instrument_Communicator_Library {

    /// <summary>
    /// Various different protocol options
    /// 
    /// </summary>
    public enum protocolOption {
        ping,
        message,
        status,
        authorize
    }
}