using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.InternalsVisibleTo
{
  [ZoneMarker]
  public class ZoneMarker : IRequire<ILanguageCSharpZone>
  {
  }
}