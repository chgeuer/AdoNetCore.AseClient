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
            throw new NotImplementedException();
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken)
        {
            throw new NotImplementedException();
        }
    }
}
