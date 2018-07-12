using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Tests.ConnectionProvider;
using Dapper;
using FluentAssertions;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration.Query
{
    [Category("basic")]
#if NET_FRAMEWORK
    [TestFixture(typeof(SapConnectionProvider))]
#endif
    [TestFixture(typeof(CoreFxConnectionProvider))]
    public class MultiTableTests<T> where T : IConnectionProvider
    {
        private DbConnection GetConnection()
        {
            return Activator.CreateInstance<T>().GetConnection(ConnectionStrings.Pooled);
        }

        [TestCaseSource(nameof(SelectMultiple_Dapper_Cases))]
        public void SelectMultiple_Dapper(string query, Action<SqlMapper.GridReader>[] dapperVerifiers, Action<DbDataReader>[] vanillaVerifiers)
        {
            using (var connection = GetConnection())
            {
                using (var multi = connection.QueryMultiple(query))
                {
                    foreach (var verifier in dapperVerifiers)
                    {
                        verifier(multi);
                    }
                }
            }
        }

        [TestCaseSource(nameof(SelectMultiple_Dapper_Cases))]
        public async Task SelectMultiple_Dapper_Async(string query, Action<SqlMapper.GridReader>[] dapperVerifiers, Action<DbDataReader>[] vanillaVerifiers)
        {
            using (var connection = GetConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(query))
                {
                    foreach (var verifier in dapperVerifiers)
                    {
                        verifier(multi);
                    }
                }
            }
        }

        [TestCaseSource(nameof(SelectMultiple_Dapper_Cases))]
        public void SelectMultiple_ExecuteDataReader(string query, Action<SqlMapper.GridReader>[] dapperVerifiers, Action<DbDataReader>[] vanillaVerifiers)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = command.ExecuteReader())
                    {
                        foreach (var verifier in vanillaVerifiers)
                        {
                            verifier(reader);
                            reader.NextResult();
                        }
                    }
                }
            }
        }

        public static IEnumerable<TestCaseData> SelectMultiple_Dapper_Cases()
        {
            {
                yield return new TestCaseData(@"SELECT 1
union
SELECT 2
SELECT 'xxx'
union
SELECT 'yyy'", new Action<SqlMapper.GridReader>[]
                    {
                        gr => gr.Read<int>().ToArray().Should().BeEquivalentTo(new[] {1, 2}),
                        gr => gr.Read<string>().ToArray().Should().BeEquivalentTo("xxx", "yyy")
                    },
                    new Action<DbDataReader>[]
                    {
                        dr => ReadMultipleSimple<int>(dr, 2).Should().BeEquivalentTo(new[] {1, 2}),
                        dr => ReadMultipleSimple<string>(dr, 2).Should().BeEquivalentTo("xxx", "yyy")
                    });
            }
            {
                yield return new TestCaseData(@"select 1 as SomeField, 'x' as OtherField
select 'y' as AnotherField",
                    new Action<SqlMapper.GridReader>[]
                    {
                        gr => gr.Read<ARecord>().ToArray().Should().BeEquivalentTo(new ARecord{SomeField = 1, OtherField = "x"}),
                        gr => gr.Read<BRecord>().ToArray().Should().BeEquivalentTo(new BRecord{AnotherField = "y"})
                    },
                    new Action<DbDataReader>[]
                    {
                        dr =>
                        {
                            Assert.IsTrue(dr.Read());
                            Assert.AreEqual(1, dr[0]);
                            Assert.AreEqual("x", dr[1]);
                            Assert.IsFalse(dr.Read());
                        },
                        dr =>
                        {
                            Assert.IsTrue(dr.Read());
                            Assert.AreEqual("y", dr[0]);
                            Assert.IsFalse(dr.Read());
                        }
                    });
            }
        }

        private class ARecord
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int SomeField { get; set; }
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string OtherField { get; set; }
        }

        private class BRecord
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string AnotherField { get; set; }
        }

        private static TItem[] ReadMultipleSimple<TItem>(DbDataReader reader, int count)
        {
            var items = new List<TItem>();
            for (var i = 0; i < count; i++)
            {
                Assert.IsTrue(reader.Read());
                items.Add((TItem)reader[0]);
            }
            Assert.IsFalse(reader.Read());
            return items.ToArray();
        }
    }
}
