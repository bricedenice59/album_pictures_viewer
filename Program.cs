using System;
using System.Linq;
using System.Threading.Tasks;
using EFSqliteDOTNET50.Utils.DbContext;
using EFSqliteDOTNET50.Utils.Extensions;
using EFSqliteDOTNET50.Utils.Models;
using EFSqliteDOTNET50.Utils.QueryService;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkDotNet50
{
    class Program
    {
        private static IHost _host;
        private static User _userForTesting;

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .AddDbContext();
        }

        static void Main(string[] args)
        {
            _host = CreateHostBuilder().Build();
            _host.Start();

            SimpleDbContextFactory contextFactory =
                _host.Services.GetRequiredService<SimpleDbContextFactory>();

            using (SimpleDbContext context = contextFactory.CreateDbContext())
            {
                var lstUsers = context.Users.ToList();
            }

            _userForTesting = new User()
            {
                Email = "brice.grenard@gmail.com",
                PasswordHash = "password",
                Username = "brice",
                DatedJoined = DateTime.Now
            };
            //TestAddUser();
            //TestUpdateUser();
        }

        public static void TestAddUser()
        {
            SimpleDbContextFactory contextFactory =
                _host.Services.GetRequiredService<SimpleDbContextFactory>();
            using (SimpleDbContext context = contextFactory.CreateDbContext())
            {
                var nonQueryService = new NonQueryDataService<User>(contextFactory);
                _userForTesting = nonQueryService.Create(_userForTesting).Result;

                bool isUserCreated = context.Users.ToList().FirstOrDefault(x => x.Email == _userForTesting.Email) != null;
            }
        }


        public static void TestUpdateUser()
        {
            SimpleDbContextFactory contextFactory =
                _host.Services.GetRequiredService<SimpleDbContextFactory>();

            using (SimpleDbContext context = contextFactory.CreateDbContext())
            {
                User userToUpdate = User.Clone(_userForTesting);
                userToUpdate.Email = "brice.von.nice@gmail.com";

                var nonQueryService = new NonQueryDataService<User>(contextFactory);
                nonQueryService.Update(userToUpdate.Id, userToUpdate);

                var queryService = new QueryDataService<User>(contextFactory);
                var allUsers = queryService.GetAll().Result;
                bool isUserWithOldEmailFoundInDb = allUsers.ToList().Any(x => x.Email == _userForTesting.Email);
            }
        }
    }
}
