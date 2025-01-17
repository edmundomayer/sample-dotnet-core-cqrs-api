using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using SampleProject.Application.Configuration.Emails;
using SampleProject.Infrastructure;
using Serilog.Core;

namespace SampleProject.IntegrationTests.SeedWork
{
    public class TestBase
    {
        protected string ConnectionString;

        protected EmailsSettings EmailsSettings;

        protected IEmailSender EmailSender;

        protected ExecutionContextMock ExecutionContext;

        [SetUp]
        public async Task BeforeEachTest()
        {
            ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SampleData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            await using var sqlConnection = new SqlConnection(ConnectionString);

            await ClearDatabase(sqlConnection);

            EmailsSettings = new EmailsSettings {FromAddressEmail = "from@mail.com"};

            EmailSender = Substitute.For<IEmailSender>();

            ExecutionContext = new ExecutionContextMock();

            ApplicationStartup.Initialize(
                new ServiceCollection(),
                ConnectionString, 
                new CacheStore(),
                EmailSender,
                EmailsSettings,
                Logger.None,
                ExecutionContext,
                runQuartz:false);
        }

        private static async Task ClearDatabase(IDbConnection connection)
        {
            const string sql = "DELETE FROM app.InternalCommands " +
                               "DELETE FROM app.OutboxMessages " +
                               "DELETE FROM orders.OrderProducts " +
                               "DELETE FROM orders.Orders " +
                               "DELETE FROM payments.Payments " +
                               "DELETE FROM orders.Customers ";

            await connection.ExecuteScalarAsync(sql);
        }
    }
}