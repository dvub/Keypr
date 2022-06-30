using System.Diagnostics;
using System.Reflection;

namespace pwd_mgr_csharp
{
    // class to represent a benchmark of a given executed method
    // takes a delegate /func, as well as the delegate's corresponding params
    // returns a Benchmark which contains the return value of type T,
    // as well as a double which measures the time in milliseconds of execution
    public class Benchmark<T>
    {
        //only use getter = field becomes readonly
        public T returnValue { get; }
        public double elapsed { get; } // most of the time double will be cast to a float but wtv
        public MethodInfo methodInfo { get; }


        // original source:
        // https://stackoverflow.com/questions/22834120/func-with-unknown-number-of-parameters
        public Benchmark(Delegate f, params object[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            T result = (T)f.Method.Invoke(null, args);
            //invoke is significantly faster than dynamic
            //var result = f.DynamicInvoke(args); //SLOW AF!
            sw.Stop();
            this.methodInfo = f.Method;
            this.returnValue = result;
            this.elapsed = sw.Elapsed.TotalMilliseconds;
        }
        public void DisplayBench()
        {
            ConsoleHelper.ColorWrite($"Elapsed {this.elapsed} ms - ({this.methodInfo.Name})", ConsoleColor.Yellow);
        }
    }
}
