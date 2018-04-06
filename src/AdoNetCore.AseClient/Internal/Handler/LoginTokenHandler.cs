using System.Collections.Generic;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class LoginTokenHandler : ITokenHandler
    {
        public bool ReceivedAck { get; private set; }
        public LoginAckToken Token { get; private set; }
        public bool RequiresNegotiation { get; private set; }
        public bool Success { get; private set; }
        
        private static readonly HashSet<TokenType> AllowedTypes = new HashSet<TokenType>
        {
            TokenType.TDS_LOGINACK,
            TokenType.TDS_PARAMFMT,
            TokenType.TDS_PARAMS
        };

        public bool CanHandle(TokenType type)
        {
            return AllowedTypes.Contains(type);
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case LoginAckToken t:
                    ReceivedAck = true;
                    Token = t;

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_FAIL)
                    {
                        throw new AseException("Login failed.");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_NEGOTIATE)
                    {
                        RequiresNegotiation = true;
                        Logger.Instance?.WriteLine($"Login negotiation required");
                    }

                    if (t.Status == LoginAckToken.LoginStatus.TDS_LOG_SUCCEED)
                    {
                        Success = true;
                        Logger.Instance?.WriteLine($"Login success");
                    }
                    break;
                case ParameterFormatToken f:
                    Logger.Instance?.WriteLine($"Received login parameter format token");
                    break;
                case ParametersToken p:
                    Logger.Instance?.WriteLine($"Received login parameters token");
                    break;
                default:
                    return;
            }
        }
    }
}
