using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AvaloniaCommunityToolkitSample.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected static T GetService<T>() where T : class
       => Ioc.Default.GetRequiredService<T>();
    }
}
