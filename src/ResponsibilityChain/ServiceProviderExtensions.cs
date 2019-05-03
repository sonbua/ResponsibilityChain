using System;

namespace ResponsibilityChain
{
    internal static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService) serviceProvider.GetService(typeof(TService));
        }
    }
}