using Caliburn.Micro;
using Caliburn.Micro.TinyIOC;

namespace Sample
{
//	using Caliburn.Micro;

  public class AppBootstrapper : TinyBootstrapper<ShellViewModel>
	{
    protected override void ConfigureBootstrapper()
    {
      base.ConfigureBootstrapper();

      EnforceNamespaceConvention = false;
      ViewModelBaseType = typeof(PropertyChangedBase);
    }
	}
}

