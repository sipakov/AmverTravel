using System;
using System.Collections.Generic;
using System.Linq;
using Amver.Api.Implementations.City;
using Amver.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Amver.Api.Tests.City
{
    [TestFixture]
    public class CityFromStorageGetterTests : TestWithSqLite
    {
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
            DbContext.Countries.AddRange(countries);
            DbContext.SaveChanges();
            DbContext.Cities.AddRange(cities);
        }

        [Test]
        public void TableShouldGetCreated()
        {
            Assert.False(DbContext.Cities.Any());
        }

        [Test]
        public void GetListHaveCorrectEnNamesAndRelations()
        {
            //Arrange
            var targetNamePart = "to";
            //Act
            using (var context = new DbContext(null))
            {
                var targetCityList = new CityFromStorageGetter(context);
   
            }


            Assert.Equals(Guid.Empty, newItem.Id);
        }
    }
}