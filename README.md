# Extending Maximus Connectivity Monitoring solution
### How to create more test objects and monitors to extend featues of the Maximus Connectivity Monitoring SCOM management pack

In this manual, I'm going to explain, step-by-step how more test can be added to the the Maximus Connectivity Monitoring SCOM management pack.
You can find the original MP at : https://github.com/MaxxVolk/Maximus.Connectivity.Monitoring.

## Prerequisites:
You can use different tools to create a SCOM management pack, starting from simple Notepad application, or using special
design tools like Silect MP Author/Studio. However, in this manual, I'm using the most powerful combination of 
Microsoft Visual Studio (you can use any edition) and VSAE (Visual Studio Authoring Extension). You can download both
tools from Microsoft web site.

Visual Studio: https://visualstudio.microsoft.com/

VSAE: http://www.microsoft.com/en-nz/download/details.aspx?id=30169

*Note: This manual is created using Microsoft Visual Studio Community 2019 and VSAE version 1.4.1.0.**

## 1. Create solution.
First, let's prepare a solution containing two projects: SCOM 2012 R2 Management Pack and .Net Framework Class 
Library.

*Note: This example uses managed module to implement Probe Action for actual monitoring probe. However, in your
project you can use PowerShell, or VB/JS scripts, or external commands to implement your mounting probe.*

*Note: The managed module in this example is based on the base classes from the https://github.com/MaxxVolk/Maximus.Base.Library base library.
However, you can either use either PowerShell, or VB/JS scripts, as mentioned earlier, or implement your
own managed module. To do so, you may refer to my other article at: http://maxcoreblog.com/2020/07/23/implementing-scom-managed-modules-part-1/*

Open Visual Studio and select _Create a new project_:

![New Project](.\Screenshots\NewProject.png)

Type "blank" in the search project type bar (1), then select "Blank Solution" (2) and click _Next_ (3).

![New Blank Solution](.\Screenshots\NewProject_BlankSolution.png)

When the new solution is created add two projects. Bring the solution explorer, if it's not on the screen (_View->Solution Explorer or
Ctrl+Alt+L_). Then right click the solution and select _Add -> New Project_ from the context menu.

![Add New Project](.\Screenshots\NewProject_Add.png)

As before search for "Operations Manager 2012 R2 Management Pack" 
(this project type is not available through any filter), then add this project. Then add another project and
search (or select) for Class Library (.Net Framework) project type.

**NB!** Management Pack projects and associated class libraries always should be compiled in _Release_ mode.

Change active build configuration to _Release_. Either simply select _Release_ at the standard toolbar, or bring 
up Configuration Manager: _Build -> Configuration Manger_, then change _Active solution configuration_.

![Configuration Manager](.\Screenshots\BuildConfigurationManager.png)

Now, we need to sign both projects. Right click at the class library project and go to Signing tab. Select 
_Sign the assemble_ checkbox (1), then either browse for an existing snk file, or choose _New..._ to create one (2).

![Signing Class Library](.\Screenshots\SigningClassLibrary.png)

Then sign the management pack project. Right click at the management pack project and select _Properties_ from 
the context menu. Go to _Build_ tab. Select _Create sealed and signed management pack_, then browse for snk file. 
It's recommended to reuse the same key file from the previous step.

![Signing Management Pack](.\Screenshots\SigningManagementPack.png)

The class library project will need references to SCOM Agent base libraries, so we need to add them. SCOM SDK
libraries can be found in any installation of SCOM Agent, SCOM Server, or SCOM Gateway. Some libraries are 
available in SCOM Console deployment, but some might be missing. You will need to locate and add references to the 
following DLLs:

  - `Microsoft.EnterpriseManagement.Core.dll`
  - `Microsoft.EnterpriseManagement.HealthService.dll`
  - `Microsoft.EnterpriseManagement.Modules.DataTypes.dll`
  - `Microsoft.EnterpriseManagement.OperationsManager.dll`
  - `Microsoft.Mom.Modules.DataTypes.dll`


Then we need to include the class library resultant DLL file into management pack. To do this, right click 
_References_ node in the MP project sub-tree, and select _Add Reference_. Select _Projects_ tab, and select 
the class library project.

![Add MP reference](.\Screenshots\MPAddReference.png)

Then, make Visual Studio to include DLL file into compiled MP bundle. Expand References node of the MP project, 
find recently added class library reference, and press F4, or right click ans select _Properties_. Set 
_Package to bundle_ to True.

![Package to bundle](.\Screenshots\PkgToMP.png)

Another compulsory reference should be made to link the base Connectivity Monitoring MP. Download Maximus
Connectivity Monitoring management pack from GitHub https://github.com/MaxxVolk/Maximus.Connectivity.Monitoring
and reference it in your MP. Right click at _References_ node in the MP project subtree, then select 
_Add Reference from Management Pack Bundle_ (**don't be confused with Add Reference**). Select `Maximus.Connectivity.Monitoring.mpb`
to reference.

Last thing preparing projects, is to add external references (may not be required if you don't use my base library). 
Download the latest base library from GitHub (https://github.com/MaxxVolk/Maximus.Base.Library). Right click at 
the References node in the class library project subtree and select _Add Reference..._ from the menu. Add reference
to Maximus.Base.Library.ManagedWorkflowBase.dll. For the MP project, right click at _References_, then select 
_Add Reference from Management Pack Bundle_ and add reference to `Maximus.Base.Library.mpb`.

Now, both projects are ready to add new elements.

## 2. Create and decorate test object class.
Right click the MP project and select _Add -> New Item..._, then select Empty Management Pack Fragment element type.

![Add MP class](.\Screenshots\AddClasses.png)

For this example, I'm going to create a test object, which will initialize a random number generator. 
So, its corresponding monitor will be compare output number with a set threshold and alert when outside
of boundaries. I'm also going to add a predefined mode, when module will use preset output, rather than randomly 
generated. To define a new class, type the following XML fragment:

```xml
<TypeDefinitions>
  <EntityTypes>
    <ClassTypes>
      <ClassType ID="Maximus.Connectivity.ExampleExtension.ExampleClass" Abstract="false" Accessibility="Public"
                 Base="MCM!Maximus.Connectivity.Monitoring.Test" Hosted="true">
        <Property ID="MinValue" Type="int" DefaultValue="-10"/>
        <Property ID="MaxValue" Type="int" DefaultValue="10"/>
        <Property ID="FixedValue" Type="int" DefaultValue="0"/>
        <Property ID="ModuleMode" Type="enum" EnumType="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration"/>
      </ClassType>
    </ClassTypes>
    <EnumerationTypes>
      <EnumerationValue ID="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration" Accessibility="Public"/>
      <EnumerationValue ID="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration.FixedValue" Accessibility="Public"
                        Parent="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration"/>
      <EnumerationValue ID="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration.RandomValue" Accessibility="Public"
                        Parent="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration"/>
    </EnumerationTypes>
  </EntityTypes>
</TypeDefinitions>
```

The new test object class has mode switch property `ModuleMode`, and corresponding enumeration definition 
`ModuleMode.Enumeration`. Then it has three properties, one is `FixedValue` for fixed value in Fixed Value Mode, 
and two are `MinValue` and `MaxValue` for Random Value mode.

Another very important fact about the test object class is its `Hosted="true"` attribute. In the base
Maximus Connectivity Monitoring class the base abstract class `Maximus.Connectivity.Monitoring.Test` is also
marked as hosted, but what is important, the base MP defines a relationship, which says that `Maximus.Connectivity.Monitoring.Test`
class and all its children are hosted at the 'Maximus.Connectivity.Monitoring.FullyQualifiedDomainName' class instances.
This has few important consequences:
 
  - An instance of test object class must be always linked to an instance of the destination FQDN class.
  - An instance of test object class have access to host instance properties, i.e. destination DNS name.
  - All test objects hosted at the same destination object are handled by the same action point (i.e. SCOM Agent).

Next step is to properly document (decorate) the test object class. To do this, you need to create a language pack
fragment. It may be located in the same file. In this case, language pack session should be placed after type 
definition one. Let's create a core template for it.

```xml
<LanguagePacks>
  <LanguagePack ID="ENU" IsDefault="true">
    <DisplayStrings>
      <!-- Enumeration: Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration -->
      <DisplayString ElementID="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration.FixedValue">
        <Name>Fixed Value</Name>
        <Description>Module will output a constant value.</Description>
      </DisplayString>
      <DisplayString ElementID="Maximus.Connectivity.ExampleExtension.ExampleClass.ModuleMode.Enumeration.RandomValue">
        <Name>Random Value</Name>
        <Description>Module will output a random value.</Description>
      </DisplayString>
      
      <!-- Class: Maximus.Connectivity.ExampleExtension.ExampleClass -->
      <DisplayString ElementID="Maximus.Connectivity.ExampleExtension.ExampleClass">
        <Name>Example Random Number Generator Probe</Name>
        <Description>This test object is for demonstration purposes only.</Description>
      </DisplayString>
      <DisplayString ElementID="Maximus.Connectivity.ExampleExtension.ExampleClass" SubElementID="FixedValue">
        <Name>Fixed Value</Name>
        <Description>The module will always output this value when Mode is set to Fixed Value.</Description>
      </DisplayString>
    </DisplayStrings>
    <KnowledgeArticles>
      <KnowledgeArticle ElementID="Maximus.Connectivity.ExampleExtension.ExampleClass">
        <MamlContent>
          <section xmlns="http://schemas.microsoft.com/maml/2004/10">
            <title>Description</title>
            <para>
              Text is here.
            </para>
          </section>
        </MamlContent>
      </KnowledgeArticle>
    </KnowledgeArticles>
  </LanguagePack>
</LanguagePacks>
```

In this fragment I create class name and description. Class name will be used in the UI _Add Test..._ context
menu, and class description will be shown in the description area of _New Test Object_ dialog as shown on the
screenshots below. Note, that only `FixedValue` property has proper name and description. This is because
display string elements are missing for other properties. 

![Main MP Context Menu](.\Screenshots\MainMPUI_NewTestMenu.png)

![Main MP New Test Object Dialog](.\Screenshots\MainMP_NewTestDialog.png)

_Learn more..._ and _Documentation_ button at active at the screenshot above, and this is because
there is knowledge article element defined for the test object. As it's shown below, the _Documentation_ window 
shows the exactly same content, I put into the knowledge article element.

![Main MP Document](.\Screenshots\MainMP_Documentation.png)

Similarly, enumeration value names and their descriptions are picked from respective display string elements.

![Main MP Enum display](.\Screenshots\MainMP_Enum.png)

Now it's time to add more display strings for the rest of the object properties and put proper description into the
knowledge article. After this, my test object is ready.

## 3. Create Probe Action managed module in C#.

At this stage, I'm going to implement a Probe Action managed module. It does two main tasks: loads configuration
and produce an output data. Normally, output data is a result of test or probe, but in this demo, it's just a random
number. So, right click at the class library project, and select _Add -> Class..._. Name the new class as 
`ExampleClassPA`. Then let's create class' skeleton. Write the following code:

```csharp
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

    public ExampleClassPA(ModuleHost<PropertyBagDataItem> moduleHost, XmlReader configuration, byte[] previousState) : base(moduleHost, configuration, previousState)
    {
    }

    protected override PropertyBagDataItem[] GetOutputData(DataItemBase[] inputDataItems)
    {
      try
      {

      }
      catch (Exception e)
      {
        // Log error here
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
        // specific class properties -- none
      }
      catch (Exception e)
      {
        ModuleErrorSignalReceiver(ModuleErrorSeverity.FatalError, ModuleErrorCriticality.Stop, e, "Failed to load module configuration.");
        throw new ModuleException("Failed to load module configuration.", e);
      }
    }
  }
}
```

The code above is a minimum frame for any probe action plus it already has some configuration loading
code specific to target destination object, where our test object is hosted. These configuration properties
won't be used in this example, but they are important for any real test, especially the `FullyQualifiedDomainName`
property, which is destination DNS name/IP for any connectivity tests.

Next step is to add code to load properties specific to my test object. Let's do this. First, add more private 
fields to the class definition. It's better to name them same as management pack class properties to avoid any
confusion.

```csharp
    private int MinValue, MaxValue, FixedValue;
    private Guid ModuleMode;
```

and then add the following code to the `LoadConfiguration(XmlDocument cfgDoc)` method:

```csharp
        // specific class properties -- some
        LoadConfigurationElement(cfgDoc, "MinValue", out MinValue, defaultValue: -10, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "MaxValue", out MaxValue, defaultValue: 10, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "FixedValue", out FixedValue, defaultValue: 0, Compulsory: false);
        LoadConfigurationElement(cfgDoc, "ModuleMode", out ModuleMode, defaultValue: "{00000000-0000-0000-0000-000000000000}", Compulsory: false);

        // probe action is persistent, i.e. always loaded into memory.
        // It reloads only is configuration changes. Therefore, we can create Random object just once.
        random = new Random();
```

Note, that all properties are marked as not compulsory. This is made to avoid any problems if test object properties
are set incorrectly. If you find a missing property, it's better to log an error in logs, rather than cause an
exception for missing parameter. For same reason, default values are set to the same values as in the class definition.

There is also another line of code, which creates Random object. It's possible to make it in the `LoadConfiguration(XmlDocument cfgDoc)`
method, because probe action is persistent, i.e. always loaded into memory.

The last thing I need to do before I can actually implement the probe (i.e. write some meaningful code to the 
`PropertyBagDataItem[] GetOutputData(DataItemBase[] inputDataItems)` method) is to find actual enumeration
values. Note, that SCOM agent passes enum values as `Guid`. Let's start _Operations Manager Shell_ and run the
following PowerShell script:

```powershell
$class = Get-SCOMClass -Name Maximus.Connectivity.ExampleExtension.ExampleClass
$enumId = $class.PropertyCollection["ModuleMode"].EnumType.Id
$class.ManagementGroup.EntityTypes.GetChildEnumerations($enumId, [Microsoft.EnterpriseManagement.Common.TraversalDepth]::OneLevel) | ft id, DisplayName
```

It's output will be (believe me or not) exactly the same as below. This is because SCOM ids are not random, see
my blog post on this mater at http://maxcoreblog.com/2019/05/02/are-scom-ids-random/. And this is why I can safely 
use `Guid` constants, rather then querying SCOM for enum Ids each time.

```txt
Id                                   DisplayName
--                                   -----------
1ed90ec7-5063-bfe9-c1bd-99b31bc5984c Random Value
94f9cff7-eb44-dbd9-8cea-712234c667b8 Fixed Value
```

So now, I can finalize my probe action by writing the following:

```csharp
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
```

And it's ready to move to the next stage.

## 4. Wrap managed module in management pack XML.

## 5. Create Monitor Type for Unit Monitor and Data Source for performance data collection.

## 6. Create templates for Monitor and Rule.

## 7. Wrapping up and testing.