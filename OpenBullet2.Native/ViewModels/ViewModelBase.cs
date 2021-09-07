using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenBullet2.Native.ViewModels
{
    /// <summary>
    /// A basic viewmodel that implements the PropertyChanged event.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>The event that lets the GUI know a property was changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property. If null, the name of the calling property will be used.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Calls OnPropertyChanged on all public properties of this class.
        /// </summary>
        public virtual void UpdateViewModel()
        {
            foreach (var property in GetType().GetProperties())
            {
                OnPropertyChanged(property.Name);
            }
        }
    }
}
