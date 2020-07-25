using SimpleInjector;

namespace IronBug.Injection
{
    public interface IInjectionModule
    {
        void Load(Container container);
    }
}