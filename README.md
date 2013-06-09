## InternalsVisibleTo helper for ReSharper ##

This plugin provides IntelliSense (code-completion) for the [`InternalsVisibleTo`](http://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.internalsvisibletoattribute.aspx) attribute, allowing you to select a project within your solution easily:

![](http://i.imgur.com/xoy9Tu7.png)

If the target project is signed with a strong name, the public key is **automatically appended** to the project name:

![](http://i.imgur.com/cjDEZEZ.png)

**Note**: You **must build** your solution for the public key to appear in the code-completion.

For ReSharper 8
-
You can use the new NuGet-based [Extension Manager](http://blogs.jetbrains.com/dotnet/2013/05/resharper-8-eap-nuget-based-extension-manager/), select **ReSharper - Extensions Manager**, and grab for a package called [**ReSharper.InternalsVisibleTo**](https://resharper-plugins.jetbrains.com/packages/ReSharper.InternalsVisibleTo/).

*At the time of writing, ReSharper 8 is in EAP stage, please select "Include Prerelease" in the package selector:*

![](http://i.imgur.com/IGrO9XE.png)

For ReSharper 7:
- 
- Download the latest zip file: [ReSharper.InternalsVisibleTo.v0.1.1.zip](https://github.com/hmemcpy/ReSharper.InternalsVisibleTo/raw/releases/ReSharper.InternalsVisibleTo.v0.1.1.zip)
- Extract everything
- Run the **Install-InternalsVisibleTo.7.1.bat** batch file

## Bugs? Questions? Suggestions?

Please feel free to [report them](../../issues)!
