using System;
using Xunit;
using Models.Database;

namespace Models
{
    public class TestEntity_IsPrimeShould
    {
        private readonly TestEntity _testEntity;

        public TestEntity_IsPrimeShould()
        {
            _testEntity = new TestEntity();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void ReturnFalseGivenValuesLessThan2(int value)
        {
            var result = _testEntity.IsPrime(value);
            
            Assert.False(result, $"{value} should not be prime");
        }
    }
}
