@echo off
set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=1.0.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
        set nuget=src\.nuget\nuget.exe
)

%nuget% restore src\ReSharper.InternalsVisibleTo.sln
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild" src\ReSharper.InternalsVisibleTo.sln /t:Rebuild /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
 
set package_id="ReSharper.InternalsVisibleTo"

%nuget% pack "src\ReSharper.InternalsVisibleTo.nuspec" -NoPackageAnalysis -Version %version% -Properties "Configuration=%config%;ReSharperDep=Wave;ReSharperVer=[4.0];PackageId=%package_id%"