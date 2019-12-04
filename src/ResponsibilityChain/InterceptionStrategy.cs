using EnsureThat;

namespace ResponsibilityChain
{
    /// <summary>
    /// </summary>
    public static class InterceptionStrategy
    {
        static InterceptionStrategy()
        {
        }

        /// <summary>
        /// <para>The strategy used to intercept any child handler within a composite handler.</para>
        /// <para><see cref="NoopInterceptionStrategy"/> is the default strategy, which does nothing. It just returns
        /// the original handler to the caller.</para>
        /// </summary>
        public static IInterceptionStrategy Default { get; private set; } = new NoopInterceptionStrategy();

        /// <summary>
        /// Sets the default strategy.
        /// </summary>
        /// <param name="strategy">The interception strategy.</param>
        public static void SetDefault(IInterceptionStrategy strategy)
        {
            EnsureArg.IsNotNull(strategy, nameof(strategy));

            Default = strategy;
        }
    }
}