using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.InternalsVisibleTo
{
  [Language(typeof(CSharpLanguage))]
  public class InternalsVisibleToSuggestionProvider : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
  {
    private readonly IClrTypeName internalsAttributeClrName =
      new ClrTypeName("System.Runtime.CompilerServices.InternalsVisibleToAttribute");

    private readonly ProjectModelElementPresentationService presentationService;

    public InternalsVisibleToSuggestionProvider(ProjectModelElementPresentationService presentationService)
    {
      this.presentationService = presentationService;
    }

    protected override bool IsAvailable(CSharpCodeCompletionContext context)
    {
      return context.IsInsideElement(internalsAttributeClrName);
    }

    protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
    {
      var solution = context.BasicContext.CompletionManager.Solution;
      var rangeMarker = context.BasicContext.CaretDocumentOffset.CreateRangeMarker();

      foreach (var project in solution.GetAllProjects())
      {
        if (!project.IsProjectFromUserView()) continue;

        var iconId = presentationService.GetIcon(project);
        var lookupItem = new ProjectReferenceLookupItem(project, iconId, rangeMarker);

        lookupItem.InitializeRanges(context.EvaluateRanges(), context.BasicContext);
        collector.Add(lookupItem);
      }

      return true;
    }
  }
}