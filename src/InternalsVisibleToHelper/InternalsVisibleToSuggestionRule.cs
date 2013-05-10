using System;
using System.Linq;
using System.Text;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Rules;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.Lookup.Impl;
using JetBrains.ReSharper.Features.Shared.UnitTesting;
using JetBrains.ReSharper.Features.SolBuilderDuo.Engine.MsbuildExe.Components;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;

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
            foreach (IProject project in solution.GetTopLevelProjects().Except(new[] { solution.MiscFilesProject, solution.SolutionProject }))
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
            TextRange textRange1 = basicContext.SelectedRange.TextRange;
            TextRange textRange2 = basicContext.CaretDocumentRange.TextRange;
            TreeOffset caretTreeOffset = basicContext.CaretTreeOffset;
            ITokenNode tokenNode = basicContext.File.FindTokenAt(caretTreeOffset) as ITokenNode;
            if (tokenNode != null && tokenNode.GetTokenType() == CSharpTokenType.STRING_LITERAL)
                textRange2 = TreeNodeExtensions.GetDocumentRange((ITreeNode)tokenNode).TextRange;
            TextRange textRange3 = new TextRange(textRange2.StartOffset, Math.Max(textRange2.EndOffset, textRange1.EndOffset));
            return new TextLookupRanges(textRange3, false, textRange3);
        }

    }

    public class ProjectReferenceLookupItem : TextLookupItem
    {
        private const string ellipsis = "…";
        private readonly IProject project;

        public ProjectReferenceLookupItem(IProject project, IconId image, IRangeMarker rangeMarker)
            : base(string.Format("\"{0}\"", project.Name), image)
        {
            this.project = project;
            VisualReplaceRangeMarker = rangeMarker;
        }

        protected override RichText GetDisplayName()
        {
            RichText displayName = LookupUtil.FormatLookupString(string.Format("\"{0}", project.Name));
            LookupUtil.AddEmphasize(displayName, new TextRange(0, displayName.Length));

            byte[] publicKey = GetPublicKey(project as ProjectImpl);
            if (publicKey != null)
            {
                string publicKeyString = publicKey.ToHexString();
                Assertion.AssertNotNull(publicKeyString, "publicKeyString != null");
                Assertion.Assert(publicKeyString.Length > 8, "publicKeyString.Length > 8");
                
                displayName.Append("\t");
                LookupUtil.AddInformationText(displayName, "PublicKey=" + publicKeyString.Substring(0, 8) + ellipsis);
            }
            displayName.Append("\"");

            return displayName;
        }

        public override string Text
        {
            get { return GetCompleteText(); }
            set { base.Text = value; }
        }

        private string GetCompleteText()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}", project.Name);

            byte[] publicKey = GetPublicKey(project as ProjectImpl);
            if (publicKey != null)
            {
                string publicKeyString = publicKey.ToHexString();
                sb.AppendFormat(", PublicKey={0}", publicKeyString);
            }
            sb.Append("\"");

            return sb.ToString();
        }

        private byte[] GetPublicKey(ProjectImpl projectImpl)
        {
            if (projectImpl == null)
            {
                return null;
            }
            
            if (projectImpl.OutputAssemblyInfo != null)
            {
                return projectImpl.OutputAssemblyInfo.AssemblyNameInfo.GetPublicKey();
            }

            return null;
        }
    }
}