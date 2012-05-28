using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TinyIoC;

namespace Caliburn.Micro.TinyIOC
{
  internal class TinyPhoneContainer : IPhoneContainer
  {
    #region Fields
    private TinyIoCContainer _container;
    #endregion

    #region Constructor
    public TinyPhoneContainer(TinyIoCContainer container)
    {
      _container = container;
    }
    #endregion

    #region Implementation of IPhoneContainer
    public event Action<object> Activated = _ => { };
    /// <summary>
    /// Registers the service as a singleton stored in the phone state.
    /// </summary>
    /// <param name="service">The service.</param><param name="phoneStateKey">The phone state key.</param><param name="implementation">The implementation.</param>
    public void RegisterWithPhoneService(Type service, string phoneStateKey, Type implementation)
    {
      if (!IsolatedStorageSettings.ApplicationSettings.Contains(phoneStateKey ?? service.FullName))
      {
        IsolatedStorageSettings.ApplicationSettings[phoneStateKey ?? service.FullName] = _container.Resolve(implementation);
      }

      _container.Register((c, p) =>
                            {
                              if (IsolatedStorageSettings.ApplicationSettings.Contains(phoneStateKey ?? service.FullName))
                              {
                                return IsolatedStorageSettings.ApplicationSettings[phoneStateKey ?? service.FullName];
                              }

                              return c.Resolve(implementation);
                            }, phoneStateKey);
    }
    /// <summary>
    /// Registers the service as a singleton stored in the app settings.
    /// </summary>
    /// <param name="service">The service.</param><param name="phoneStateKey">The app settings key.</param><param name="implementation">The implementation.</param>
    public void RegisterWithAppSettings(Type service, string phoneStateKey, Type implementation)
    {
      var pservice = (IPhoneService)GetInstance(typeof(IPhoneService), null);

      if (!pservice.State.ContainsKey(phoneStateKey ?? service.FullName))
      {
        pservice.State[phoneStateKey ?? service.FullName] = _container.Resolve(implementation);
      }

      _container.Register((c, p) =>
                            {
                              var phoneService = c.Resolve<IPhoneService>();
                              if (phoneService.State.ContainsKey(phoneStateKey ?? service.FullName))
                              {
                                return phoneService.State[phoneStateKey ?? service.FullName];
                              }
                              return c.Resolve(implementation);
                            }, phoneStateKey);
    }
    #endregion

    #region Internal Methods
    private object GetInstance(Type service, string key)
    {
      return string.IsNullOrEmpty(key) ? _container.Resolve(service) : _container.Resolve(service, key);
    }
    #endregion
  }
}
