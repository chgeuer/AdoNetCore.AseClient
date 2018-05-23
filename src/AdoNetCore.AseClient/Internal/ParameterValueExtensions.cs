using System;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ParameterValueExtensions
    {
        internal static object AsSendableValue(this object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            switch (value)
            {
                case string s:
                    return s.AsSendable();
                case char c:
                    return c.AsSendable();
                default:
                    return value;
            }
        }

        private static string AsSendable(this string value)
        {
            return value.HandleTerminator().HandleEmpty();
        }

        private static string HandleTerminator(this string value)
        {
            var iTerminator = value.IndexOf('\0');
            if (iTerminator < 0)
            {
                return value;
            }

            return value.Substring(0, iTerminator);
        }

        private static string HandleEmpty(this string value)
        {
            return string.Equals(value, string.Empty)
                ? " "
                : value;
        }

        private static char AsSendable(this char value)
        {
            return value == '\0'
                ? ' '
                : value;
        }
    }
}
