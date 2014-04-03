using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Babaluma.Tests
{
    public class TaskRunnerTests
    {

        public TaskRunnerTests()
        {
            ThreadPool.SetMinThreads(100, 100);
        }

        private Random _random = new Random();

        [Fact]
        public void RunsAll_AndOrdered()
        {
            const int count = 20;
            int totalCalls = 0;

            var runner = new ParallelRunner<int,int>((index, item) =>
            {
                Thread.Sleep(item);
                Interlocked.Increment(ref totalCalls);
                return new Tuple<bool, int>(true, index);
            });

            var items = Enumerable.Range(0, count)
                .Select(x => _random.Next(100, 500)).ToArray();
            var results = runner.Run(items,0, count).ToArray();

            for (int i = 0; i < results.Length; i++)
            {
                Assert.Equal(i, results[i]);
            }

            Assert.Equal(count, totalCalls);
        }
    }
}
