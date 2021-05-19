using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Extensions;
using PhotoApp.Db.Models;
using PhotoApp.Db.QueryService;


namespace EntityFrameworkDotNet50
{
    class Program
    {
        private static IHost _host;
        private static UserDto _userForTesting;

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

            AppDbContextFactory contextFac =
                _host.Services.GetRequiredService<AppDbContextFactory>();

            using (AppDbContext context = contextFac.CreateDbContext())
            {
                var lstUsers = context.Users.ToList();
            }

            _userForTesting = new UserDto()
            {
                Email = "brice.grenard@gmail.com",
                PasswordHash = "password",
                Username = "brice",
                DatedJoined = DateTime.Now
            };
            TestAddUser();
            TestUpdateUser();
        }

        public static void TestAddUser()
        {
            AppDbContextFactory contextFac =
                _host.Services.GetRequiredService<AppDbContextFactory>();
            using (AppDbContext context = contextFac.CreateDbContext())
            {
                var nonQueryService = new NonQueryDataService<UserDto>(contextFac);
                _userForTesting = nonQueryService.Create(_userForTesting).Result;

                bool isUserCreated = context.Users.ToList().FirstOrDefault(x => x.Email == _userForTesting.Email) != null;
            }
        }


        public static void TestUpdateUser()
        {
            AppDbContextFactory contextFac =
                _host.Services.GetRequiredService<AppDbContextFactory>();

            using (AppDbContext context = contextFac.CreateDbContext())
            {
                UserDto userToUpdate = UserDto.Clone(_userForTesting);
                userToUpdate.Email = "brice.von.nice@gmail.com";

                var nonQueryService = new NonQueryDataService<UserDto>(contextFac);
                nonQueryService.Update(userToUpdate.Id, userToUpdate);

                var queryService = new QueryDataService<UserDto>(contextFac);
                var allUsers = queryService.GetAll().Result;
                bool isUserWithOldEmailFoundInDb = allUsers.ToList().Any(x => x.Email == _userForTesting.Email);
            }
        }
    }
}
