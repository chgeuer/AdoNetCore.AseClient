using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Packet
{
    internal class LoginPacket : IPacket
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProcessId { get; set; }
        public string ApplicationName { get; set; }
        public string ServerName { get; set; }
        public string Language { get; set; }
        public string Charset { get; set; }
        public string ClientLibrary { get; set; }

        public CapabilityToken Capability { get; set; }

        public LInt2 LInt2 { get; set; } = LInt2.TDS_INT2_LSB_LO;
        public LInt4 LInt4 { get; set; } = LInt4.TDS_INT4_LSB_LO;
        public LChar LChar { get; set; } = LChar.TDS_CHAR_ASCII;
        public LFlt LFlt { get; set; } = LFlt.TDS_FLT_IEEE_LO;
        public LDt LDt { get; set; } = LDt.TDS_TWO_I4_LSB_LO;
        public LInterfaceSpare LInterfaceSpare { get; set; } = LInterfaceSpare.TDS_LDEFSQL;
        public LType LType { get; set; } = LType.TDS_NONE;

        public LNoShort LNoShort { get; set; } = LNoShort.TDS_NOCVT_SHORT;
        public LFlt4 LFlt4 { get; set; } = LFlt4.TDS_FLT4_IEEE_LO;
        public LDate4 LDate4 { get; set; } = LDate4.TDS_TWO_I2_LSB_LO;
        public LSetLang LSetLang { get; set; } = LSetLang.TDS_NOTIFY;
        public LSetCharset LSetCharset { get; set; } = LSetCharset.TDS_NOTIFY;

        public int PacketSize { get; set; }

        public LoginPacket(string hostname, string username, string password, string processId, string applicationName, string serverName, string language, string charset, string clientLibrary, int packetSize, CapabilityToken capability)
        {
            Capability = capability;
            Hostname = hostname ?? string.Empty;
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
            ProcessId = processId ?? string.Empty;
            ApplicationName = applicationName ?? string.Empty;
            ServerName = serverName ?? string.Empty;
            Language = language ?? string.Empty;
            Charset = charset ?? string.Empty;
            ClientLibrary = clientLibrary ?? string.Empty;
            PacketSize = packetSize;
        }

        // ReSharper disable InconsistentNaming
        private const int TDS_MAXNAME = 30;
        private const int TDS_PROGNLEN = 10;
        private const int TDS_RPLEN = 255;
        private const int TDS_PKTLEN = 6;
        private const int TDS_NETBUF = 4;
        private const int TDS_HA = 6;
        private const int TDS_OLDSECURE = 2;
        // ReSharper restore InconsistentNaming

        public BufferType Type => BufferType.TDS_BUF_LOGIN;
        public BufferStatus Status => BufferStatus.TDS_BUFSTAT_NONE;

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.WriteLine($"Write {Type}");
            //Host Name [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(Hostname, TDS_MAXNAME, env.Encoding);
            //User Name [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(Username, TDS_MAXNAME, env.Encoding);
            //Password [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(Password, TDS_MAXNAME, env.Encoding);
            //Host Process [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(ProcessId, TDS_MAXNAME, env.Encoding);

            //Byte Ordering - int2 [1]
            stream.WriteByte((byte)LInt2);
            //Byte Ordering - int4 [2]
            stream.WriteByte((byte)LInt4);
            //Character Encoding [1]
            stream.WriteByte((byte)LChar);
            //Float Format [1]
            stream.WriteByte((byte)LFlt);
            //Date Format [1]
            stream.WriteByte((byte)LDt);
            //lusedb [1]
            stream.WriteByte((byte)LUseDb.TRUE);
            //ldmpld [1]
            stream.WriteByte((byte)LDmpLd.FALSE);
            //linterfacespare [1]
            stream.WriteByte((byte)LInterfaceSpare);
            //Dialog Type [1]
            stream.WriteByte((byte)LType);

            //lbufsize [4] -- ribo claims this is [1], but that seems to break things
            stream.WriteRepeatedBytes(0, TDS_NETBUF);

            //lspare [3]
            stream.WriteRepeatedBytes(0, 3);
            
            //Application Name [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(ApplicationName, TDS_MAXNAME, env.Encoding);
            //Service Name [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(ServerName, TDS_MAXNAME, env.Encoding);

            //spec's a bit weird when it comes to this bit... following ADO.net driver
            //Remote Passwords [255] + Length [1]
            stream.WriteRemotePasswordString(Password, TDS_RPLEN, env.Encoding);

            //TDS Version [4]
            stream.Write(new byte[] { 5, 0, 0, 0 });
            //Prog Name [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(ClientLibrary, TDS_PROGNLEN, env.Encoding);
            //Prog Version [4] -- doesn't matter what this value is really
            stream.Write(new byte[] { 0x0f, 0x07, 0x00, 0x0d });

            //Convert Shorts [1]
            stream.WriteByte((byte)LNoShort);
            //4-byte Float Format [1]
            stream.WriteByte((byte)LFlt4);
            //4-byte Date Format [1]
            stream.WriteByte((byte)LDate4);

            //Language [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(Language, TDS_MAXNAME, env.Encoding);//llanguage
            
            //Notify when Language Changed [1]
            stream.WriteByte((byte)LSetLang);

            //Old Secure Info [2]
            stream.WriteRepeatedBytes(0, TDS_OLDSECURE);

            //Secure Login Flags [1]
            stream.WriteByte((byte)LSecLogin.TDS_SEC_LOG_NONE);

            //Bulk Copy [1]
            stream.WriteByte(0);

            //HA Login Flags [1]
            stream.WriteByte((byte)LHaLogin.TDS_HA_LOG_REDIRECT);

            //HA Session ID [6]
            stream.WriteRepeatedBytes(0, TDS_HA);

            //Spare [2]
            stream.WriteRepeatedBytes(0, 2);

            //Character Set [30] + Length [1]
            stream.WritePaddedLengthSuffixedString(Charset, TDS_MAXNAME, env.Encoding);
            
            //Notify when Character Set Changed [1]
            stream.WriteByte((byte)LSetCharset);
            
            //Packet Size [6] + Length [1]
            stream.WritePaddedLengthSuffixedString(PacketSize.ToString(), TDS_PKTLEN, env.Encoding);

            //ldummy [4]
            stream.WriteRepeatedBytes(0, 4);

            Capability.Write(stream, env);
        }
    }
}
