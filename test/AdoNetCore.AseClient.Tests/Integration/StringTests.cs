using System;
using System.Collections.Generic;
using System.Data;
using AdoNetCore.AseClient.Internal;
using Dapper;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class StringTests
    {
        private IDbConnection GetConnection()
        {
            Logger.Enable();
            return new AseConnection(ConnectionStrings.Pooled);
        }

        [Test]
        public void CharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(char(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void VarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(varchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void NcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void NvarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(nvarchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void TextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(text, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnicharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unichar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnivarcharEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(univarchar(2), 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [Test]
        public void UnitextEncoding_ShouldWork()
        {
            using (var connection = GetConnection())
            {
                var result = connection.ExecuteScalar<string>("select convert(unitext, 'Àa')");
                Assert.AreEqual("Àa", result);
            }
        }

        [TestCase("select '['+@input+']'", "", "[ ]")]
        [TestCase("select @input", "", " ")]
        [TestCase("select convert(char, @input)", null, null)]
        [TestCase("select '['+convert(char, @input)+']'", null, "[]")]
        [TestCase("select convert(char, '['+@input+']')", null, "[]                            ")]
        public void Select_StringParameter(string sql, object input, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    var p = command.CreateParameter();
                    p.ParameterName = "@input";
                    p.Value = input ?? DBNull.Value;
                    p.DbType = DbType.AnsiString;
                    command.Parameters.Add(p);

                    Assert.AreEqual(expected ?? DBNull.Value, command.ExecuteScalar());
                }
            }
        }

        [TestCase("select ''", " ")]
        [TestCase("select convert(char, '')", "                              ")]
        [TestCase("select convert(char(1), '')", " ")]
        [TestCase("select convert(nchar(1), '')", " ")]
        [TestCase("select convert(unichar(1), '')", " ")]
        [TestCase("select convert(varchar(1), '')", " ")]
        [TestCase("select convert(univarchar(1), '')", " ")]
        [TestCase("select convert(nvarchar(1), '')", " ")]
        [TestCase("select convert(char, null)", null)]
        [TestCase("select convert(char(1), null)", null)]
        [TestCase("select convert(unichar(1), null)", null)]
        [TestCase("select convert(nchar(1), null)", null)]
        [TestCase("select convert(varchar(1), null)", null)]
        [TestCase("select convert(univarchar(1), null)", null)]
        [TestCase("select convert(nvarchar(1), null)", null)]
        public void Select_StringLiteral(string sql, object expected)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"[{result}]");
                    Assert.AreEqual(expected ?? DBNull.Value, result);
                }
            }
        }

        [TestCaseSource(nameof(SelectNCharParam_Cases))]
        public void SelectNCharParam(string _, char input, char expected)
        {
            using (var connection = GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@input", input, DbType.String);
                var result = connection.QuerySingle<char>("select @input", p);
                Assert.AreEqual(expected, result, $"Expected: '{expected}' ({(int)expected}); Result: '{result}' ({(int)result})");
            }
        }

        public static IEnumerable<TestCaseData> SelectNCharParam_Cases()
        {
            yield return new TestCaseData("space", ' ', ' ');
            yield return new TestCaseData("\\x09", '\x09', '\x09');
            yield return new TestCaseData("\\x0A", '\x0A', '\x0A');
            yield return new TestCaseData("\\x0B", '\x0B', '\x0B');
            yield return new TestCaseData("\\x0C", '\x0C', '\x0C');
            yield return new TestCaseData("\\x0D", '\x0D', '\x0D');
            yield return new TestCaseData("\\xA0", '\xA0', '\xA0');
            yield return new TestCaseData("u 2000", '\u2000', '\u2002');
            yield return new TestCaseData("u 2000", '\u2000', '\u2002');
            yield return new TestCaseData("u 2001", '\u2001', '\u2003');
            yield return new TestCaseData("u 2002", '\u2002', '\u2002');
            yield return new TestCaseData("u 2003", '\u2003', '\u2003');
            yield return new TestCaseData("u 2004", '\u2004', '\u2004');
            yield return new TestCaseData("u 2005", '\u2005', '\u2005');
            yield return new TestCaseData("u 2006", '\u2006', '\u2006');
            yield return new TestCaseData("u 2007", '\u2007', '\u2007');
            yield return new TestCaseData("u 2008", '\u2008', '\u2008');
            yield return new TestCaseData("u 2009", '\u2009', '\u2009');
            yield return new TestCaseData("u 200A", '\u200A', '\u200A');
            yield return new TestCaseData("u 3000", '\u3000', '\u3000');
            yield return new TestCaseData("\\0", '\0', ' ');
        }
    }
}
