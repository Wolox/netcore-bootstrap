using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace NetCoreBootstrap.Tests.Faker.Strategies
{
    public class ModelList<T> where T : class
    {
        private Faker<T> Faker;

        public ModelList(Faker<T> faker)
        {
            this.Faker = faker;
        }
        
        public IEnumerable<object[]> GetModelList(int amount) =>  Faker
                                                        .Generate(amount)
                                                        .Select(x => new object[] { x })
                                                        .ToList();
    }
}
