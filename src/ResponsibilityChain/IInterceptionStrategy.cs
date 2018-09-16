using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInterceptionStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="serviceProvider"></param>
        /// <typeparam name="THandler"></typeparam>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(IHandler<TIn, TOut> handler, IServiceProvider serviceProvider)
            where THandler : IHandler<TIn, TOut>;
    }
}