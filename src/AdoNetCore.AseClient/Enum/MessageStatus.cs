namespace AdoNetCore.AseClient.Enum
{
    internal enum MessageStatus : byte
    {
        /// <summary>
        /// The message has no arguments
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TDS_MSG_NONE = 0x00,
        /// <summary>
        /// The message has arguments
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TDS_MSG_HASARGS = 0x01
    }
}
