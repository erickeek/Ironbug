using IronBug.Injection;
using SimpleInjector;
using System.Collections.Generic;

namespace $rootnamespace$
{
    public static class InjectionConfig
    {
        public static void ConfigureContainer()
        {
            var container = new Container();
            //container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            foreach (var module in Modules())
            {
                module.Load(container);
            }

            //container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            //container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.Verify();

            //DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            //GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }

        private static IEnumerable<IInjectionModule> Modules()
        {
            yield break;
        }
    }
}