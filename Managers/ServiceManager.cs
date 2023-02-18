using Microsoft.Extensions.DependencyInjection;

namespace Moobot.Managers
{
    public static class ServiceManager
    {
        public static IServiceProvider Provider { get; private set; }

        public static void SetProvider(ServiceCollection collection) => Provider = collection.BuildServiceProvider();

        public static T GetService<T>() where T : new()
        {
            if (Provider == null) throw new ArgumentNullException(nameof(Provider));
            return Provider.GetRequiredService<T>();
        }
    }
}