using System.Diagnostics;

namespace pwd_mgr_csharp
{
    public class Benchmark
    {
        public Benchmark()
        {

        }
        // SOLUTION FOUND HERE:
        // https://stackoverflow.com/questions/22834120/func-with-unknown-number-of-parameters
        public static BenchmarkValue<T> MeasureMethod<T>(Delegate f, params object[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var result = f.Method.Invoke(null, args); //invoke is significantly faster than dynamic
            //var result = f.DynamicInvoke(args);

            sw.Stop();

            T val = (T)Convert.ChangeType(result, typeof(T));
            var bmv = new BenchmarkValue<T>(val, sw.Elapsed);

            return bmv;
        }
    }
    public class BenchmarkValue<T>
    {
        public T returnValue { get; }

        public TimeSpan elapsed { get; }

        public BenchmarkValue(T returnValue, TimeSpan elapsed)
        {
            this.returnValue = returnValue;
            this.elapsed = elapsed;
        }
    }
}
