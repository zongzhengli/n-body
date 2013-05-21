using System;
using System.Collections.Generic;
using System.Threading;

namespace NBody {
    public class Parallel {
        /// <summary>
        /// Defines the number of threads to use. This is given a value of 2 * (processors) to take 
        /// advantage of hyperthreading if applicable. 
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
        /// Performs a parallelized loop that is analogous to the sequential for loop. 
        /// Parallel.For(start, end, delegate(Int32 i) { ... }) is analogous to 
        /// for (Int32 i = start, i &lt; end; i++) { ... }. 
        /// </summary>
        /// <param name="fromInclusive">The inclusive initial index for the loop.</param>
        /// <param name="toExclusive">The exclusive final index for the loop.</param>
        /// <param name="body">The body of the loop.</param>
        public static void For(Int32 fromInclusive, Int32 toExclusive, BodyDelegate body) {
            Object indexLock = new Object();
            Int32 step = Math.Max(1, (toExclusive - fromInclusive) / (10 * ThreadCount));
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
        /// Performs a parallelized loop that is analogous to the sequential foreach loop. 
        /// ForEach&lt;Element&gt;(collection, member => { ... }) is analogous to 
        /// foreach (Element member in collection) { ... }. Usually &lt;Element&gt; can be omitted. 
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
        /// Invokes the given function in parallel. That is, Parallel.Invoke(f1, f2, ...) is analogous
        /// to calling f1, f2, ... in succession. 
        /// </summary>
        /// <param name="body">The functions to invoke.</param>
        public static void Invoke(params MethodDelegate[] body) {
            For(0, body.Length, delegate(Int32 i) {
                body[i]();
            });
        }
    }
}