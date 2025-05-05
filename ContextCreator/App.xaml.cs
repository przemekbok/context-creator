using System.Windows;

namespace ContextCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize application resources
            InitializeResources();
        }
        
        private void InitializeResources()
        {
            // Load any necessary resources here
        }
    }
}