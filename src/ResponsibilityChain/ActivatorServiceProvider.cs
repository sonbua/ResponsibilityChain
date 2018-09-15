using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// Represents a singleton service provider which uses <see cref="Activator"/> to create instance of given service type.
    /// </summary>
    public class ActivatorServiceProvider : IServiceProvider
    {
        static ActivatorServiceProvider()
        {
        }

        private ActivatorServiceProvider()
        {
        }

        /// <summary>
        /// Singleton instance of this service provider.
        /// </summary>
        public static ActivatorServiceProvider Instance { get; } = new ActivatorServiceProvider();

        /// <summary>
        /// Uses <see cref="Activator"/> to create instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType) => Activator.CreateInstance(serviceType);
    }
}