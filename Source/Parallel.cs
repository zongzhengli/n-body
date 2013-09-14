using System;
using System.Collections.Generic;
using System.Threading;

namespace NBody {

    /// <summary>
    /// Defines methods for easy parallel execution of code. 
    /// </summary>
    public class Parallel {

        /// <summary>
        /// Defines the number of threads to use. A value of 2 * (processors) is used 
        /// to take advantage of hyperthreading if applicable. 
        /// </summary>
        private static readonly int ThreadCount = 2 * Environment.ProcessorCount;

        /// <summary>
        /// Encapsulates a method that takes no parameters and returns no value. 
        /// </summary>
        public delegate void Action();

        /// <summary>
        /// Performs a parallelized loop that is analogous to the sequential for 
        /// loop. Parallel.For(start, end, i => { ... }) is analogous to 
        /// for (int i = start, i &lt; end; i++) { ... }. 
        /// </summary>
        /// <param name="fromInclusive">The inclusive initial index for the loop.</param>
        /// <param name="toExclusive">The exclusive final index for the loop.</param>
        /// <param name="body">The body of the loop.</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> body) {
            Object indexLock = new Object();

            // The step value defines the size of a chunk, which is the number of 
            // iterations (different values of the variable in the loop) each thread 
            // performs in succession before moving on to another chunk. A larger value 
            // reduces locking time to get indices but also reduces load balance between 
            // the threads. Thus this is dynamically determined based on the range of 
            // the loop and the number of threads available. 
            int step = Math.Max(1, (toExclusive - fromInclusive) / (10 * ThreadCount));

            // The value of the variable in the loop. This field is locked so that at 
            // any point values equal or greater to this have not been assigned to any 
            // threads. 
            int index = fromInclusive;

            Action work = () => {
                while (true) {
                    int current;
                    lock (indexLock) {
                        current = index;
                        index += step;
                    }
                    for (int i = current; i < current + step; i++)
                        if (i >= toExclusive)
                            return;
                        else
                            body(i);
                }
            };

            IAsyncResult[] results = new IAsyncResult[ThreadCount];
            for (int i = 0; i < results.Length; i++)
                results[i] = work.BeginInvoke(null, null);
            for (int i = 0; i < results.Length; i++)
                work.EndInvoke(results[i]);
        }

        /// <summary>
        /// Performs a parallelized loop that is analogous to the sequential foreach 
        /// loop. ForEach&lt;T&gt;(collection, member => { ... }) is analogous to 
        /// foreach (T element in collection) { ... }. The type parameter &lt;T&gt; 
        /// can be omitted. 
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection to loop through.</typeparam>
        /// <param name="source">The collection to loop through.</param>
        /// <param name="action">The body of the loop.</param>
        public static void ForEach<T>(IList<T> source, Action<T> action) {
            For(0, source.Count, i => {
                action(source[i]);
            });
        }

        /// <summary>
        /// Invokes the given parameterless functions in parallel. That is, 
        /// Parallel.Invoke(f1, f2, ..., fn) is analogous to the statements 
        /// f1(); f2(); ..., fn();. 
        /// </summary>
        /// <param name="body">The functions to invoke.</param>
        public static void Invoke(params Action[] body) {
            For(0, body.Length, i => {
                body[i]();
            });
        }
    }
}