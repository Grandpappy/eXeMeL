using Microsoft.Extensions.DependencyInjection;

namespace eXeMeL.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private readonly ServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainViewModel>();
            _serviceProvider = services.BuildServiceProvider();
        }

        public MainViewModel Main
        {
            get
            {
                return _serviceProvider.GetRequiredService<MainViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
