using Maximus.Library.ManagedModuleBase;

using Microsoft.EnterpriseManagement.HealthService;
using Microsoft.EnterpriseManagement.Mom.Modules.DataItems;

using System;
using System.Xml;

namespace Maximus.Connectivity.ExampleExtension.Modules
{
  [MonitoringModule(ModuleType.ReadAction)]
  [ModuleOutput(true)]
  class ExampleClassPA : ModuleBaseSimpleAction<PropertyBagDataItem>
  {
    private string TestDisplayName, FullyQualifiedDomainName;
    private int TargetIndex;
    private int MinValue, MaxValue, FixedValue;
    private Guid ModuleMode;

    public ExampleClassPA(ModuleHost<PropertyBagDataItem> moduleHost, XmlReader configuration, byte[] previousState) : base(moduleHost, configuration, previousState)
    {
    }

    private Random random;
    private readonly Guid RandomMode = new Guid("1ed90ec7-5063-bfe9-c1bd-99b31bc5984c");
    private readonly Guid FixedMode = new Guid("94f9cff7-eb44-dbd9-8cea-712234c667b8");
    protected override PropertyBagDataItem[] GetOutputData(DataItemBase[] inputDataItems)
    {
      try
      {
        if (ModuleMode == FixedMode)
          return new PropertyBagDataItem[] { CreatePropertyBag("Value", FixedValue) };
        if (ModuleMode == RandomMode)
          return new PropertyBagDataItem[] { CreatePropertyBag("Value", random.Next(MinValue, MaxValue)) };
        return null;
      }
      catch (Exception e)
      {
        // Log error here
        return null;
      }
    }

    protected override void ModuleErrorSignalReceiver(ModuleErrorSeverity severity, ModuleErrorCriticality criticality, Exception e, string message, params object[] extaInfo)
    {
      // log parent class errors here
    }

    protected override void LoadConfiguration(XmlDocument cfgDoc)
    {
      try
      {
        // base class properties
        LoadConfigurationElement(cfgDoc, "TestDisplayName", out TestDisplayName, "<no test name provided>", false); // for logging purposes only
        // parent class property
        LoadConfigurationElement(cfgDoc, "FullyQualifiedDomainName", out FullyQualifiedDomainName);
        LoadConfigurationElement(cfgDoc, "TargetIndex", out TargetIndex);
        // specific class properties -- some
        LoadConfigurationElement(cfgDoc, "MinValue", out MinValue, defaultValue: -10, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "MaxValue", out MaxValue, defaultValue: 10, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "FixedValue", out FixedValue, defaultValue: 0, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "ModuleMode", out ModuleMode, defaultValue: "{00000000-0000-0000-0000-000000000000}", Compulsory: false);

        // probe action is persistent, i.e. always loaded into memory.
        // It reloads only is configuration changes. Therefore, we can create Random object just once.
        random = new Random();
      }
      catch (Exception e)
      {
        ModuleErrorSignalReceiver(ModuleErrorSeverity.FatalError, ModuleErrorCriticality.Stop, e, "Failed to load module configuration.");
        throw new ModuleException("Failed to load module configuration.", e);
      }
    }
  }
}
