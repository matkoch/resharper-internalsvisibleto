using System;
using System.Drawing;
using System.Text;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;
using JetBrains.Util.Dotnet.TargetFrameworkIds;

namespace ReSharper.InternalsVisibleTo
{
  public class ProjectReferenceLookupItem : TextLookupItem
  {
    private const string ellipsis = "…";
    private readonly IProject project;

    public ProjectReferenceLookupItem(IProject project, IconId image, IRangeMarker rangeMarker)
      : base(GetCompleteText(project), image)
    {
      this.project = project;
      VisualReplaceRangeMarker = rangeMarker;
    }

    protected override RichText GetDisplayName()
    {
      var projectDisplayName = GetProjectDisplayName(project);
      var displayName = LookupUtil.FormatLookupString($"\"{projectDisplayName}\"");
      if (!projectDisplayName.Equals(project.Name, StringComparison.OrdinalIgnoreCase))
      {
        LookupUtil.AddInformationText(displayName, project.Name);
      }

      var publicKeyString = GetPublicKeyString(project as ProjectImpl);
      if (!string.IsNullOrWhiteSpace(publicKeyString))
      {
        var publicKeyDisplay = LookupUtil.FormatLookupString("PublicKey=" + ellipsis);
        publicKeyDisplay.SetStyle(TextStyle.FromForeColor(SystemColors.GrayText));

        // aligns the "PublicKey=..." text to the right, as if it were a type name
        DisplayTypeName = publicKeyDisplay;
      }

      return displayName;
    }

    [NotNull, MustUseReturnValue]
    private static string GetCompleteText([NotNull] IProject project)
    {
      var sb = new StringBuilder();
      sb.AppendFormat("\"{0}", GetProjectDisplayName(project));

      string publicKeyString = GetPublicKeyString(project);
      if (!string.IsNullOrWhiteSpace(publicKeyString))
      {
        sb.AppendFormat(", PublicKey={0}", publicKeyString);
      }
      sb.Append("\"");

      return sb.ToString();
    }

    [NotNull, Pure]
    private static string GetProjectDisplayName([NotNull] IProject project)
    {
      return project.GetOutputAssemblyName(TargetFrameworkId.Default);
    }

    [CanBeNull, MustUseReturnValue]
    private static string GetPublicKeyString([CanBeNull] IProject project)
    {
      if (project == null) return null;

      var solution = project.GetSolution();
      var snkProvider = solution.GetComponent<SnkDataProvider>();

      byte[] data = snkProvider.ProjectDataCache.GetData(snkProvider, project.ProjectFileLocation, null);
      return data?.Length > 0
        ? (SnkDataProvider.IsPublicKeyBlob(data) ? StringUtil.ToHexString(data) : null)
        : null;
    }
  }
}