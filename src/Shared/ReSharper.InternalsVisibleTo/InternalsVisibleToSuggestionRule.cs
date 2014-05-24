using System;
using System.Linq;
using JetBrains.DocumentModel;

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.Icons;
using JetBrains.Util;
#if RESHARPER9
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
#endif

namespace InternalsVisibleTo.ReSharper
{
    [Language(typeof(CSharpLanguage))]
    public class InternalsVisibleToSuggestionRule : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
    {
        private readonly IClrTypeName internalsAttributeClrName = new ClrTypeName("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
        private readonly ProjectModelElementPresentationService presentationService;

        public InternalsVisibleToSuggestionRule(ProjectModelElementPresentationService presentationService)
        {
            this.presentationService = presentationService;
        }

        protected override bool IsAvailable(CSharpCodeCompletionContext context)
        {
            CodeCompletionType codeCompletionType = context.BasicContext.CodeCompletionType;
            if (codeCompletionType != CodeCompletionType.BasicCompletion &&
                codeCompletionType != CodeCompletionType.SmartCompletion &&
                codeCompletionType != CodeCompletionType.AutomaticCompletion)
            {
                return false;
            }

            ITreeNode nodeAt = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (nodeAt == null)
                return false;
            var csharpArgument = (nodeAt.Parent is ICSharpArgument ? nodeAt.Parent : nodeAt.Parent.Parent) as ICSharpArgument;
            var attribute = (csharpArgument != null ? csharpArgument.Parent : nodeAt.Parent) as IAttribute;
            if (attribute == null)
                return false;

            var typeElement = attribute.TypeReference.Resolve().DeclaredElement as ITypeElement;
            if (typeElement == null)
                return false;

            return typeElement.GetClrName().Equals(internalsAttributeClrName);
        }

        protected override bool AddLookupItems(CSharpCodeCompletionContext context, GroupedItemsCollector collector)
        {
            IRangeMarker rangeMarker = new TextRange(context.BasicContext.CaretDocumentRange.TextRange.StartOffset).CreateRangeMarker(context.BasicContext.Document);
            ISolution solution = context.BasicContext.CompletionManager.Solution;
            foreach (IProject project in solution.GetAllProjects().Where(p => p.IsProjectFromUserView()))
            {
                IconId iconId = presentationService.GetIcon(project);
                var lookupItem = new ProjectReferenceLookupItem(project, iconId, rangeMarker);
                lookupItem.InitializeRanges(EvaluateRanges(context), context.BasicContext);
                collector.AddAtDefaultPlace(lookupItem);
            }

            return true;
        }

        private static TextLookupRanges EvaluateRanges(CSharpCodeCompletionContext context)
        {
            CodeCompletionContext basicContext = context.BasicContext;
            TextRange selectedRange = basicContext.SelectedRange.TextRange;
            TextRange documentRange = basicContext.CaretDocumentRange.TextRange;
            TreeOffset caretTreeOffset = basicContext.CaretTreeOffset;
            var tokenNode = basicContext.File.FindTokenAt(caretTreeOffset) as ITokenNode;

            if (tokenNode != null && tokenNode.GetTokenType() == CSharpTokenType.STRING_LITERAL)
                documentRange = tokenNode.GetDocumentRange().TextRange;
            var replaceRange = new TextRange(documentRange.StartOffset, Math.Max(documentRange.EndOffset, selectedRange.EndOffset));

            return new TextLookupRanges(replaceRange, replaceRange);
        }
    }
}