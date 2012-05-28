using Caliburn.Micro.TinyIOC;

namespace Sample
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.Linq;
	using Caliburn.Micro;

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




