using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.CompilerServices;
using Phobos.Services;

namespace Phobos.Helpers
{
    internal static class IocHelpers
    {
        private static ConditionalWeakTable<ITabContext, IServiceScope> serviceScopes = new();

        public static IServiceProvider GetServicesForTab(ITabContext context)
        {
            IServiceScope scope = (App.Current as App).Services.CreateScope();
            serviceScopes.Add(context, scope);
            return scope.ServiceProvider;
        }

        public static void SetRootForTab(this ITabContext context, XamlRoot xamlRoot)
        {
            var scope = serviceScopes.GetValue(context, (key) => (App.Current as App).Services.CreateScope());
            scope.ServiceProvider.GetRequiredService<XamlRootService>().Root = xamlRoot;
        }

        public static void DisposeServices(this ITabContext context)
        {
            serviceScopes.TryGetValue(context, out IServiceScope? scope);
            scope?.Dispose();
        }
    }
}
