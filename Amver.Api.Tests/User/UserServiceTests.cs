using System;
using Amver.Domain.Dto;
using Amver.EfCli;
using Amver.Api.Implementations.Services;
using Amver.Api.Implementations.Storages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Amver.Api.Tests.User
{
    [TestFixture]
    public class UserServiceTests
    {
        private static DbContextOptions<ApplicationContext> CreateNewContextOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            builder.UseInMemoryDatabase("Create_writes_to_database")
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        [Test]
        public void Create_writes_to_database()
        {
            //Arrange
            var loggerServiceMoq = Mock.Of<ILogger<UserService>>();
            var loggerStorageMoq = Mock.Of<ILogger<UserStorage>>();
            var appEnvironmentMoq = Mock.Of<IHostingEnvironment>();
            var context = new ApplicationContextFactory(CreateNewContextOptions());

            var newUserDto = new UserDto
            {
                CreatedDate = DateTime.UtcNow, IsBanned = false, Login = "Sip", FirstName = "Stanislav",
                Email = "example@mail.com", GenderId = 1
            };
            //Act
            var service = new UserService(new UserStorage(context, loggerStorageMoq), loggerServiceMoq,
                appEnvironmentMoq, context);
            var createdUserDto = service.CreateAsync(newUserDto).Result;
            //Assert
            Assert.AreEqual(newUserDto, createdUserDto);
        }
    }
}