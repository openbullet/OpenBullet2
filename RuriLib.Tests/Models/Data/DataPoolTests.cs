using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Models.Data
{
    public class DataPoolTests
    {
        [Fact]
        public void CombinationsDataPool_Constructor_GenerateCombinations()
        {
            DataPool pool = new CombinationsDataPool("AB", 2);
            Assert.Equal(new string[] { "AA", "AB", "BA", "BB" }, pool.DataList.ToArray());
        }

        [Fact]
        public void RangeDataPool_Constructor_GenerateRange()
        {
            DataPool pool = new RangeDataPool(5, 3, 5, true);
            Assert.Equal(new string[] { "05", "10", "15" }, pool.DataList.ToArray());
        }

        [Fact]
        public void InfinityDataPool_Constructor_GenerateMany()
        {
            DataPool pool = new InfiniteDataPool();
            Assert.Equal(100, pool.DataList.Take(100).Count());
        }
    }
}
