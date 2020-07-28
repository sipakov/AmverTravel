using System.Collections.Generic;
using System.Linq;
using Amver.Api.Implementations.City;
using Amver.Api.Interfaces.City;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.EfCli;
using DeepEqual.Syntax;
using EntityFrameworkCore3Mock;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Amver.Api.Tests.City
{
    [TestFixture]
    public class CityFactoryCreatorTests
    {
        [Test]
        public void Resulting_List_Has_Correct_En_Objects()
        {
            //Arrange
            var namePartDto = new NamePartDto
            {
                Part = "to"
            };
            const string targetCulture = "ru";
            const int targetCountryId = 1;
            var expectedModels = new List<Domain.Entities.City>
            {
                new Domain.Entities.City
                {
                    Id = 1,
                    Name = "Tobolsk",
                    ruRu = "Тобольск",
                    CountryId = 1,
                    Country = new Country
                    {
                        Id = 1,
                        Name = "Russia",
                        ruRu = "Россия"
                    },
                },
                new Domain.Entities.City
                {
                    Id = 3,
                    Name = "Tomsk",
                    ruRu = "Томск",
                    CountryId = 1,
                    Country = new Country
                    {
                        Id = 1,
                        Name = "Russia",
                        ruRu = "Россия"
                    },
                },
            };
            var connection = new SqliteConnection("DataSource=:memory:");  
            connection.Open();
            var option = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(connection).Options;
            var dbContextMock = new DbContextMock<ApplicationContext>(option);
            var dbSetMock = dbContextMock.CreateDbSetMock(x => x.Cities, expectedModels);
            var mockCityFromStorageGetter = new Mock<ICityFromStorageGetter>();
            mockCityFromStorageGetter.Setup(x => x.GetListByNamePart(namePartDto.Part, targetCountryId, null))
                .ReturnsAsync(expectedModels);
            //Act
            var storage = new CityFactoryCreator(mockCityFromStorageGetter.Object);
            var result = storage.GetCityListByNamePart(namePartDto, targetCulture).Result
                .ToList();
            //Arrange
            Assert.AreEqual(expectedModels.Count, result.Count);
            expectedModels.ShouldDeepEqual(result);
        }
    }
}