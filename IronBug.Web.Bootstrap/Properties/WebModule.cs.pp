using IronBug.Injection;
using SimpleInjector;

namespace $rootnamespace$.Properties
{
    public class WebModule : IInjectionModule
    {
        public void Load(Container container)
        {
            //container.Register<IYourContext, YourContext>(Lifestyle.Scoped);
        }
    }
}
