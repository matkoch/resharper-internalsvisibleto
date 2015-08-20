using System;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util.Literals;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharper.InternalsVisibleTo
{
    public static class RangeExtensions
    {
        public static TextLookupRanges EvaluateRanges(this CSharpCodeCompletionContext context)
        {
            CodeCompletionContext basicContext = context.BasicContext;
            TextRange selectedRange = basicContext.SelectedRange.TextRange;
            TextRange documentRange = basicContext.CaretDocumentRange.TextRange;
            TreeOffset caretTreeOffset = basicContext.CaretTreeOffset;
            var tokenNode = basicContext.File.FindTokenAt(caretTreeOffset) as ITokenNode;

            if (tokenNode != null && tokenNode.IsAnyStringLiteral())
                documentRange = tokenNode.GetDocumentRange().TextRange;

            var replaceRange = new TextRange(documentRange.StartOffset, Math.Max(documentRange.EndOffset, selectedRange.EndOffset));

            return new TextLookupRanges(replaceRange, replaceRange);
        }

        public static bool IsInsideElement(this CSharpCodeCompletionContext context, IClrTypeName typeName)
        {
            ITreeNode nodeAt = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentRange);
            if (nodeAt?.Parent == null)
                return false;

            var csharpArgument = (nodeAt.Parent is ICSharpArgument ? nodeAt.Parent : nodeAt.Parent.Parent) as ICSharpArgument;
            var attribute = (csharpArgument != null ? csharpArgument.Parent : nodeAt.Parent) as IAttribute;

            var typeElement = attribute?.TypeReference?.Resolve().DeclaredElement as ITypeElement;
            if (typeElement == null)
                return false;

            return typeElement.GetClrName().Equals(typeName);
        }
    }
}