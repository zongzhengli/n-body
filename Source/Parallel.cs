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
        private static readonly Int32 ThreadCount = 2 * Environment.ProcessorCount;

        /// <summary>
        /// The delegate for the body of a Parallel.For loop. 
        /// </summary>
        /// <param name="i">The index value.</param>
        public delegate void BodyDelegate(Int32 i);

        /// <summary>
        /// The delegate for the block execution code for a thread. 
        /// </summary>
        public delegate void MethodDelegate();

        /// <summary>
        /// Performs a parallelized loop that is analogous to the sequential for 
        /// loop. Parallel.For(start, end, delegate(Int32 i) { ... }) is analogous to 
        /// for (Int32 i = start, i &lt; end; i++) { ... }. 
        /// </summary>
        /// <param name="fromInclusive">The inclusive initial index for the loop.</param>
        /// <param name="toExclusive">The exclusive final index for the loop.</param>
        /// <param name="body">The body of the loop.</param>
        public static void For(Int32 fromInclusive, Int32 toExclusive, BodyDelegate body) {
            Object indexLock = new Object();

            // The step value defines the size of a chunk, which is the number of 
            // iterations (different values of the variable in the loop) each thread 
            // performs in succession before moving on to another chunk. A larger value 
            // reduces locking time to get indices but also reduces load balance between 
            // the threads. Thus this is dynamically determined based on the range of 
            // the loop and the number of threads available. 
            Int32 step = Math.Max(1, (toExclusive - fromInclusive) / (10 * ThreadCount));

            // The value of the variable in the loop. This field is locked so that at 
            // any point values equal or greater to this have not been assigned to any 
            // threads. 
            Int32 index = fromInclusive;

            MethodDelegate method = delegate {
                while (true) {
                    Int32 current;
                    lock (indexLock) {
                        current = index;
                        index += step;
                    }
                    for (Int32 i = current; i < current + step; i++)
                        if (i >= toExclusive)
                            return;
                        else
                            body(i);
                }
            };

            IAsyncResult[] results = new IAsyncResult[ThreadCount];
            for (Int32 i = 0; i < results.Length; i++)
                results[i] = method.BeginInvoke(null, null);
            for (Int32 i = 0; i < results.Length; i++)
                method.EndInvoke(results[i]);
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
            For(0, source.Count, delegate(Int32 i) {
                action(source[i]);
            });
        }

        /// <summary>
        /// Invokes the given parameterless functions in parallel. That is, 
        /// Parallel.Invoke(f1, f2, ..., fn) is analogous to the statements 
        /// f1(); f2(); ..., fn();. 
        /// </summary>
        /// <param name="body">The functions to invoke.</param>
        public static void Invoke(params MethodDelegate[] body) {
            For(0, body.Length, delegate(Int32 i) {
                body[i]();
            });
        }
    }
}