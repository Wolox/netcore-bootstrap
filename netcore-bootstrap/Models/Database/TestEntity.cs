using System;

namespace Models.Database
{
    public class TestEntity
    {
        public bool IsPrime(int candidate) 
        {
            if (candidate < 2) 
            { 
                return false;
            } 
            throw new NotImplementedException("Please create a test first");
        } 
    }
}