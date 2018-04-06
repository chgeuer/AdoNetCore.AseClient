using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class MessageToken : IToken
    {
        public TokenType Type => TokenType.TDS_MSG;

        public MessageStatus Status { get; set; }
        public MessageIdentifier MessageId { get; set; }

        public void Write(Stream stream, DbEnvironment env)
        {
            stream.WriteByte(3);
            stream.WriteByte((byte)Status);
            stream.WriteUShort((ushort)MessageId);
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken)
        {
            var remainingLength = stream.ReadByte();
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                Status = (MessageStatus) ts.ReadByte();
                MessageId = (MessageIdentifier) ts.ReadUShort();
            }
            Logger.Instance?.WriteLine($"<- {Type}: {Status}, {MessageId}");
        }

        public static MessageToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new MessageToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
