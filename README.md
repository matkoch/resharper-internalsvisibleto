## InternalsVisibleTo helper for ReSharper ##

This plugin provides IntelliSense (code-completion) for the [`InternalsVisibleTo`](http://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.internalsvisibletoattribute.aspx) attribute, allowing you to select a project within your solution easily:

![](http://i.imgur.com/xoy9Tu7.png)

If the target project is signed with a strong name, the public key is **automatically appended** to the project name:

![](http://i.imgur.com/cjDEZEZ.png)

**Note**: You **must build** your solution for the public key to appear in the code-completion.

For ReSharper 8
-
To install, simply get [**ReSharper.InternalsVisibleTo**](https://resharper-plugins.jetbrains.com/packages/ReSharper.InternalsVisibleTo/) package from ReSharper's [Extensions Gallery](http://resharper-plugins.jetbrains.com/).

## Bugs? Questions? Suggestions?

Please feel free to [report them](../../issues)!
