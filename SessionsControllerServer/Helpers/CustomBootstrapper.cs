using Nancy;
using Nancy.TinyIoc;
using SessionsControllerServer.ViewModels;

namespace SessionsControllerServer.Helpers
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<MainViewModel>().AsSingleton();
        }

        protected override TinyIoCContainer GetApplicationContainer()
        {
            return TinyIoCContainer.Current;
        }
    }
}
