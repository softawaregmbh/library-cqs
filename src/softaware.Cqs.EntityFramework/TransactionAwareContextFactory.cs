using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace softaware.Cqs.EntityFramework
{
    [Obsolete("This class is not needed and can be removed. The entity framework DbContext will attach itself to open transactions either way, even without this helper class.", error: false)]
    public static class TransactionAwareContextFactory
    {
        private static ConcurrentDictionary<Transaction, DbConnection> connections = new ConcurrentDictionary<Transaction, DbConnection>();

        public static TContext CreateContext<TContext>(
            string connectionString,
            Func<DbContextOptions<TContext>, TContext> dbContextFuncWithOptions,
            Func<DbConnection, TContext> dbContextFuncWithDbConnection)
            where TContext : DbContext
        {
            var currentTransaction = Transaction.Current;
            if (currentTransaction == null)
            {
                var options = new DbContextOptionsBuilder<TContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                return dbContextFuncWithOptions(options);
            }
            else
            {
                var connectionForTransaction = connections.GetOrAdd(currentTransaction, valueFactory: t =>
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();

                    t.TransactionCompleted += (s, e) =>
                    {
                        connection.Close();
                        connections.TryRemove(t, out var _);
                    };

                    return connection;
                });

                return dbContextFuncWithDbConnection(connectionForTransaction);
            }
        }
    }
}
