using SimpleInjector;
using System;

namespace IronBug.Injection
{
    public static class AppInjection
    {
        private static Container _container;

        public static Container Initialize(Action<Container> beforeBuild)
        {
            _container = new Container();
            beforeBuild?.Invoke(_container);

            return _container;
        }

        public static object Resolve(Type type)
        {
            return _container.GetInstance(type);
        }
        public static T Resolve<T>() where T : class
        {
            return _container.GetInstance<T>();
        }
        public static T Inject<T>(out T t) where T : class
        {
            return t = _container.GetInstance<T>();
        }
    }
}