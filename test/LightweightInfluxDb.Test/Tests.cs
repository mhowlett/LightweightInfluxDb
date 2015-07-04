using Xunit;
using LightweightInfluxDb;

namespace LightweightInfluxDb.Test
{
    public class Tests
    {
        [Fact]
        public void CanWrite()
        {
            // TODO
            Assert.True(false);
        }

        [Fact]
        public void CanRead()
        {
            var idb = new InfluxDb(
                "secret",
                "secret",
                "secret",
                "secret");

            var tsk = idb.QuerySingleSeries("show measurements");
            tsk.Wait();
            var r = tsk.Result;
            Assert.True(tsk.Result.Count > 0);

        }
    }
}
