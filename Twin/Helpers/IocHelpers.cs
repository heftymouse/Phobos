using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Twin.Core.Services;
using Twin.Services;

namespace Twin.Helpers
{
    internal static class IocHelpers
    {
        public static T InitViewModel<T>(Getter<XamlRoot> root)
        {
            List<object> paramList = new();
            Type t = typeof(T);

            // not validating if multiple ctors have the attribute because that's done for us later
            ConstructorInfo ctor = t.GetConstructors().First(ctor => ctor.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute)));
            foreach (var param in ctor.GetParameters())
            {
                if (param.ParameterType == typeof(IDialogService))
                {
                    paramList.Add(new DialogService(root));
                }
            }

            return ActivatorUtilities.CreateInstance<T>((App.Current as App).Services, paramList.ToArray());
        }
    }
}
