using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC;

namespace Caliburn.Micro.TinyIOC
{
#if WINDOWS_PHONE
  public class TinyBootstrapper : PhoneBootstrapper
#else
  public class TinyBootstrapper<TRootViewModel> : Bootstrapper<TRootViewModel>
#endif
  {
    #region Properties
    protected TinyIoCContainer Container { get; private set; }
    /// <summary>
    /// Should the namespace convention be enforced for type registration. The default is true.
    /// For views, this would require a views namespace to end with Views
    /// For view-models, this would require a view models namespace to end with ViewModels
    /// <remarks>Case is important as views would not match.</remarks>
    /// </summary>
    public bool EnforceNamespaceConvention { get; set; }

#if WINDOWS_PHONE
    /// <summary>
    /// Should the view be treated as loaded when registering the INavigationService.
    /// </summary>
    public bool TreatViewAsLoaded { get; set; }
#endif

    /// <summary>
    /// The base type required for a view model
    /// </summary>
    public Type ViewModelBaseType { get; set; }
    /// <summary>
    /// Method for creating the window manager
    /// </summary>
    public Func<IWindowManager> CreateWindowManager { get; set; }
    /// <summary>
    /// Method for creating the event aggregator
    /// </summary>
    public Func<IEventAggregator> CreateEventAggregator { get; set; }
#if WINDOWS_PHONE
    //  Method for creating the frame adapter
    public Func<FrameAdapter> CreateFrameAdapter { get; set; }
    //  Method for creating the phone application service adapter
    public Func<PhoneApplicationServiceAdapter> CreatePhoneApplicationServiceAdapter { get; set; }
    //  Method for creating the vibrate controller
    public Func<IVibrateController> CreateVibrateController { get; set; }
    //  Method for creating the sound effect player
    public Func<ISoundEffectPlayer> CreateSoundEffectPlayer { get; set; }
#endif
    #endregion

    #region Overrides
    /// <summary>
    /// Do not override this method. This is where the IoC container is configured.
    /// <remarks>
    /// Will throw <see cref="System.ArgumentNullException"/> is either CreateWindowManager
    /// or CreateEventAggregator is null.
    /// </remarks>
    /// </summary>
    protected override void Configure()
    { //  allow base classes to change bootstrapper settings
      ConfigureBootstrapper();

      //  validate settings
      if(CreateWindowManager == null)
// ReSharper disable NotResolvedInText
        throw new ArgumentNullException("CreateWindowManager");
      if(CreateEventAggregator == null)
        throw new ArgumentNullException("CreateEventAggregator");
#if WINDOWS_PHONE
      if (CreateFrameAdapter == null)
        throw new ArgumentNullException("CreateFrameAdapter");
      if (CreatePhoneApplicationServiceAdapter == null)
        throw new ArgumentNullException("CreatePhoneApplicationServiceAdapter");
      if (CreateVibrateController == null)
        throw new ArgumentNullException("CreateVibrateController");
      if (CreateSoundEffectPlayer == null)
        throw new ArgumentNullException("CreateSoundEffectPlayer");
#endif
      // ReSharper restore NotResolvedInText

      //  configure container
      var container = new TinyIoCContainer();
      var assemblies = AssemblySource.Instance.ToArray();

      //  register view models
      container.AutoRegister(assemblies,
                             type =>
                                 //  must be a type with a name that ends with ViewModel
                                 type.Name.EndsWith("ViewModel") &&
                                 //  must be in a namespace ending with ViewModels
                                 (!EnforceNamespaceConvention || (!(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace.EndsWith("ViewModels"))) &&
                                 //  must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this)
                                 (type.GetInterface(ViewModelBaseType.Name, false) != null));

      container.AutoRegister(assemblies,
                             type =>
                                 //  must be a type with a name that ends with ViewModel
                                 type.Name.EndsWith("View") &&
                                 //  must be in a namespace ending with ViewModels
                                 (!EnforceNamespaceConvention || (!(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace.EndsWith("Views"))));



#if WINDOWS_PHONE
      // The constructor of these services must be called
      // to attach to the framework properly.
      var phoneService = CreatePhoneApplicationServiceAdapter();
      var navigationService = CreateFrameAdapter();
      
      container.Register((c,p) => new TinyPhoneContainer(c));

      //  register the singletons
      container.Register<INavigationService>(navigationService).AsSingleton();
      container.Register<IPhoneService>(phoneService).AsSingleton();
      
      //  register the window manager for this container
      container.Register<IWindowManager>((c, p) => CreateWindowManager());
      //  register the event aggregator for this container
      container.Register<IEventAggregator>((c,p) => CreateEventAggregator());

      container.Register<IVibrateController>((c, p) => CreateVibrateController());
      container.Register<ISoundEffectPlayer>((c, p) => CreateSoundEffectPlayer());
      container.Register<StorageCoordinator>();
      container.Register<TaskController>();
#else
      //  register the single window manager for this container
      container.Register<IWindowManager>((c, p) => CreateWindowManager());
      //  register the single event aggregator for this container
      container.Register<IEventAggregator>((c,p) => CreateEventAggregator());
#endif

      //  allow derived classes to add to the container
      ConfigureContainer(container);

      Container = container;
    }    
    /// <summary>
    /// Do not override unless you plan to full replace the logic. This is how the framework
    /// retrieves services from the TinyIoC container.
    /// </summary>
    /// <param name="service">The service to locate.</param>
    /// <param name="key">The key to locate.</param>
    /// <returns>The located service.</returns>
    protected override object GetInstance(System.Type service, string key)
    {
      object instance;
      if (string.IsNullOrWhiteSpace(key))
      {
        if (Container.TryResolve(service, out instance))
          return instance;
      }
      else
      {
        if (Container.TryResolve(service, key, out instance))
          return instance;
      }
      throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
    }
    /// <summary>
    /// Do not override unless you plan to full replace the logic. This is how the framework
    /// retrieves services from the TinyIoC container.
    /// </summary>
    /// <param name="service">The service to locate.</param>
    /// <returns>The located services.</returns>
    protected override System.Collections.Generic.IEnumerable<object> GetAllInstances(System.Type service)
    {
      return Container.ResolveAll(service, true);
    }
    /// <summary>
    /// Do not override unless you plan to full replace the logic. This is how the framework
    /// retrieves services from the Autofac container.
    /// </summary>
    /// <param name="instance">The instance to perform injection on.</param>
    protected override void BuildUp(object instance)
    {
      Container.BuildUp(instance);
    }    
    #endregion
    
    #region Virtuals
    /// <summary>
    /// Override to provide configuration prior to the TinyIOC configuration. You must call the base version BEFORE any 
    /// other statement or the behaviour is undefined.
    /// Current Defaults:
    ///   EnforceNamespaceConvention = true
    ///   ViewModelBaseType = <see cref="System.ComponentModel.INotifyPropertyChanged"/> 
    ///   CreateWindowManager = <see cref="Caliburn.Micro.WindowManager"/> 
    ///   CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator"/>
    /// </summary>
    protected virtual void ConfigureBootstrapper()
    { //  by default, enforce the namespace convention
      EnforceNamespaceConvention = true;
      //  the default view model base type
      ViewModelBaseType = typeof(System.ComponentModel.INotifyPropertyChanged);
      //  default window manager
      CreateWindowManager = () => new WindowManager();
      //  default event aggregator
      CreateEventAggregator = () => new EventAggregator();
    }
    /// <summary>
    /// Override to include your own TinyIOC configuration after the framework has finished its configuration, but 
    /// before the container is created.
    /// </summary>
    /// <param name="container">The TinyIOC container.</param>
    protected virtual void ConfigureContainer(TinyIoCContainer container)
    {
    }    
    #endregion
  }
}
