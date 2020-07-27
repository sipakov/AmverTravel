using NUnit.Framework;

namespace Amver.Api.Tests
{
    [TestFixture]
    public class DbContextSqLiteTest : TestWithSqLite
    {
        [Test]
            public void DatabaseIsAvailableAndCanBeConnectedTo()
            {
                Assert.True(DbContext.Database.CanConnect());
            }
    }
}