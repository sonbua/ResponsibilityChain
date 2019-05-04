using System;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that returns a completed task. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    public class ReturnCompletedTask<TIn> : IHandler<TIn, Task>
    {
        /// <summary>
        /// Returns a completed task.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public virtual Task Handle(TIn input, Func<TIn, Task> next) => Task.FromResult(0);
    }
}