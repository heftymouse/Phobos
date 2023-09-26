using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Twin.Core.Services;
using Twin.Services;
using static System.Formats.Asn1.AsnWriter;

namespace Twin.Helpers
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
            serviceScopes.TryGetValue(context, out IServiceScope scope);
            scope.ServiceProvider.GetRequiredService<XamlRootService>().Root = xamlRoot;
        }

        public static void DisposeServices(this ITabContext context)
        {
            serviceScopes.TryGetValue(context, out IServiceScope scope);
            scope?.Dispose();
        }
    }
}
