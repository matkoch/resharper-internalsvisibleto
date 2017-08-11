using System;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
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
    [NotNull, Pure]
    public static TextLookupRanges EvaluateRanges([NotNull] this CSharpCodeCompletionContext context)
    {
      var basicContext = context.BasicContext;
      var selectedRange = basicContext.SelectedRange.TextRange;
      var documentRange = new TextRange(basicContext.CaretDocumentOffset.Offset);
      var caretTreeOffset = basicContext.CaretTreeOffset;
      var tokenNode = basicContext.File.FindTokenAt(caretTreeOffset) as ITokenNode;

      if (tokenNode != null && tokenNode.IsAnyStringLiteral())
        documentRange = tokenNode.GetDocumentRange().TextRange;

      var replaceRange = new TextRange(
        documentRange.StartOffset,
        Math.Max(documentRange.EndOffset, selectedRange.EndOffset));

      return new TextLookupRanges(replaceRange, replaceRange, basicContext.Document);
    }

    [Pure]
    public static bool IsInsideElement([NotNull] this CSharpCodeCompletionContext context, [NotNull] IClrTypeName typeName)
    {
      var nodeAt = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentOffset);
      if (nodeAt?.Parent == null) return false;

      var csharpArgument = (nodeAt.Parent is ICSharpArgument ? nodeAt.Parent : nodeAt.Parent.Parent) as ICSharpArgument;
      var attribute = (csharpArgument != null ? csharpArgument.Parent : nodeAt.Parent) as IAttribute;

      var typeElement = attribute?.TypeReference?.Resolve().DeclaredElement as ITypeElement;
      if (typeElement == null) return false;

      return typeElement.GetClrName().Equals(typeName);
    }
  }
}