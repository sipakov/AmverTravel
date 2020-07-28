using System.Collections.Generic;
using System.Linq;
using Amver.Api.Implementations.City;
using Amver.Domain.Entities;
using Amver.EfCli;
using DeepEqual.Syntax;
using NUnit.Framework;

namespace Amver.Api.Tests.City
{
    [TestFixture]
    public class CityFromStorageGetterTests
    {
        private static IContextFactory<ApplicationContext> _contextFactory;
        private static ApplicationContext _applicationContext;

        [SetUp]
        public void SetUp()
        {
            var countries = new List<Domain.Entities.Country>
            {
                new Country
                {
                    Id = 1,
                    Name = "Russia",
                    ruRu = "Россия"
                },
                new Country
                {
                    Id = 2,
                    Name = "USA",
                    ruRu = "США"
                }
            };
            var cities = new List<Domain.Entities.City>
            {
                new Domain.Entities.City
                {
                    Id = 1,
                    Name = "Tobolsk",
                    ruRu = "Тобольск",
                    CountryId = 1
                },
                new Domain.Entities.City
                {
                    Id = 2,
                    Name = "Angarsk",
                    ruRu = "Ангарск",
                    CountryId = 1
                },
                new Domain.Entities.City
                {
                    Id = 3,
                    Name = "Tomsk",
                    ruRu = "Томск",
                    CountryId = 1
                },
                new Domain.Entities.City
                {
                    Id = 4,
                    Name = "Chicago",
                    ruRu = "Чикаго",
                    CountryId = 2
                },
                new Domain.Entities.City
                {
                    Id = 5,
                    Name = "Orlando",
                    ruRu = "Орландо",
                    CountryId = 2
                },
                new Domain.Entities.City
                {
                    Id = 6,
                    Name = "Portland",
                    ruRu = "Портлэнд",
                    CountryId = 1
                },
            };
            _contextFactory = new ConnectionFactory();
            _applicationContext = _contextFactory.CreateContext();
            _applicationContext.Countries.AddRange(countries);
            _applicationContext.SaveChanges();
            _applicationContext.Cities.AddRange(cities);
            _applicationContext.SaveChanges();
        }

        [Test]
        public void Resulting_List_Has_Correct_En_Objects()
        {
            //Arrange
            const string targetNamePart = "to";
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
            //Act
            var storage = new CityFromStorageGetter(_contextFactory);
            var result = storage.GetListByNamePart(targetNamePart, targetCountryId, _applicationContext).Result
                .ToList();
            //Arrange
            Assert.AreEqual(expectedModels.Count, result.Count);
            expectedModels.ShouldDeepEqual(result);
        }
        
        [Test]
        public void Resulting_List_Has_Correct_Ru_Objects()
        {
            //Arrange
            const string targetNamePart = "то";
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
            //Act
            var storage = new CityFromStorageGetter(_contextFactory);
            var result = storage.GetListByNamePartRu(targetNamePart, targetCountryId, _applicationContext).Result
                .ToList();
            //Arrange
            Assert.AreEqual(expectedModels.Count, result.Count);
            expectedModels.ShouldDeepEqual(result);
        }
    }
}