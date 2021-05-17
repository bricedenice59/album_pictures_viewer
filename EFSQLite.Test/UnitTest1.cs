using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyStreamingApp.Utils.DbContext;
using MyStreamingApp.Utils.Extensions;
using MyStreamingApp.Utils.Models;
using MyStreamingApp.Utils.QueryService;
using NUnit.Framework;

namespace EFSQLite.Test
{
    [TestFixture]
    public class Tests
    {
        private IHost _host;
        private User _userForTesting;
            
        [SetUp]
        public void Setup()
        {
            var host = Host.CreateDefaultBuilder()
                .AddConfiguration()
                .AddDbContext();

            _host = host.Build();
            _host.Start();

            _userForTesting = new User()
            {
                Email = "brice.grenard@gmail.com",
                PasswordHash = "password",
                Username = "brice",
                DatedJoined = DateTime.Now
            };
        }

        [Test]
        public void TestIsTwinMethod()
        {
            bool test1 = "Hello".IsTwin("world");
            bool test2 = "acb".IsTwin("bca");
            bool test3 = "Lookout".IsTwin("Outlook");

            Assert.AreEqual(test1, false);
            Assert.AreEqual(test2, true);
            Assert.AreEqual(test3, true);
        }

        [Test, Order(1)]
        public void TestDatabaseConnection()
        {
            SimpleDbContextFactory contextFactory = _host.Services.GetRequiredService<SimpleDbContextFactory>();
            using SimpleDbContext context = contextFactory.CreateDbContext();
            Assert.AreEqual(context.Database.CanConnect(), true);
        }

        [Test, Order(2)]
        public async Task TestAddUser()
        {
            SimpleDbContextFactory contextFactory = _host.Services.GetRequiredService<SimpleDbContextFactory>();
            using (SimpleDbContext context = contextFactory.CreateDbContext())
            {
                var nonQueryService = new NonQueryDataService<User>(contextFactory);
                _userForTesting = await nonQueryService.Create(_userForTesting);

                bool isUserCreated = context.Users.ToList().FirstOrDefault(x => x.Email == _userForTesting.Email) != null;
                Assert.AreEqual(isUserCreated, true);
            }
        }

        [Test, Order(3)]
        public async Task TestUpdateUser()
        {
            SimpleDbContextFactory contextFactory = _host.Services.GetRequiredService<SimpleDbContextFactory>();
            
            var queryService = new QueryDataService<User>(contextFactory);
            var allUsers = await queryService.GetAll();

            // make sure the object is retrieved from the database !! otherwise update fct will actually insert in db !
            var recordToUpdate = allUsers.ToList().First(x => x.Email == _userForTesting.Email);
            recordToUpdate.Email = "brice.von.nice@gmail.com";

            var nonQueryService = new NonQueryDataService<User>(contextFactory);
            await nonQueryService.Update(recordToUpdate.Id, recordToUpdate);

            bool isUserWithOldEmailFoundInDb = allUsers.ToList().Any(x => x.Email == _userForTesting.Email);
            Assert.AreEqual(isUserWithOldEmailFoundInDb, false);
        }
    }
}