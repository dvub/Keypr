using System.Diagnostics;

namespace pwd_mgr_csharp
{
    //Class to represent a benchmark of a given executed method
    //Takes a delegate /func, as well as the delegate's corresponding params
    //Returns a Benchmark which contains the return value of type T,
    //As well as a double which measures the time in milliseconds of execution
    public class Benchmark<T>
    {
        //only use getter = field becomes readonly
        public T returnValue { get; }
        public double elapsed { get; } // most of the time double will be cast to a float but wtv

        // original source:
        // https://stackoverflow.com/questions/22834120/func-with-unknown-number-of-parameters
        public Benchmark(Delegate f, params object[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            T result = (T)f.Method.Invoke(null, args);
            //invoke is significantly faster than dynamic
            //var result = f.DynamicInvoke(args); //SLOW AF!
            sw.Stop();
            this.returnValue = result;
            this.elapsed = sw.Elapsed.TotalMilliseconds;
        }
    }
}
