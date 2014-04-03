using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babaluma
{
      public class AsyncParallelRunner<TItem, TResult>
    {
        private Func<int, TItem, Task<Tuple<bool, TResult>>> _processor;

        public AsyncParallelRunner(Func<int, TItem, Task<Tuple<bool, TResult>>> processor)
        {
            _processor = processor;
        }

        public async Task<IEnumerable<TResult>> RunAsync(TItem[] items, int from, int count)
        {
            int currentIndex = 0;
            int Surplus = count / 4;
            var results = new ConcurrentDictionary<int, TResult>();
            int totalParallelRuns = 0;

            while (results.Count < (count + from) && currentIndex < items.Length)
            {
                totalParallelRuns++;

                await Task.WhenAll(
                    Enumerable.Range(currentIndex,  currentIndex + count + Surplus)
                    .Select(async (i) =>
                    {
                        if (i >= items.Length)
                            return;

                        var result = await _processor(i, items[i]);
                        if(result.Item1)
                            results.AddOrUpdate(i, result.Item2, (j, k) => result.Item2);
                    }));

                currentIndex += count + Surplus;
            }

            Trace.TraceInformation("Had to run {0} parallel runs and get {1} items for count {2} and from {3}",
                totalParallelRuns,
                currentIndex + count + Surplus,
                count,
                from);

            return results.Select(x => new { p = x.Value, order = x.Key })
               .OrderBy(y => y.order)
               .Select(z => z.p)
               .Skip(from)
               .Take(count);
        }
    }
}
