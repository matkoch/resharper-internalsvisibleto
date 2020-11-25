using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util.Literals;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.InternalsVisibleTo
{
  public static class RangeExtensions
  {
    [NotNull, Pure]
    public static TextLookupRanges EvaluateRanges([NotNull] this CSharpCodeCompletionContext context)
    {
      var basicContext = context.BasicContext;

      var startOffset = basicContext.CaretDocumentOffset;
      var endOffset = basicContext.SelectedRange.EndOffset;

      if (basicContext.File.FindTokenAt(basicContext.CaretTreeOffset) is ITokenNode tokenNode &&
          tokenNode.IsAnyStringLiteral())
      {
        startOffset = tokenNode.GetDocumentStartOffset();
        endOffset = tokenNode.GetDocumentEndOffset();
      }

      var replaceRange = new DocumentRange(startOffset, endOffset);

      return new TextLookupRanges(replaceRange, replaceRange);
    }

    [Pure]
    public static bool IsInsideElement([NotNull] this CSharpCodeCompletionContext context, [NotNull] IClrTypeName typeName)
    {
      var nodeAt = context.BasicContext.File.FindNodeAt(context.BasicContext.CaretDocumentOffset);
      if (nodeAt?.Parent == null) return false;

      var parentNode = nodeAt.Parent is ICSharpArgument ? nodeAt.Parent : nodeAt.Parent.Parent;
      var attribute = (parentNode is ICSharpArgument csharpArgument ? csharpArgument.Parent : nodeAt.Parent) as IAttribute;

      if (attribute?.TypeReference?.Resolve().DeclaredElement is ITypeElement typeElement)
      {
        return typeElement.GetClrName().Equals(typeName);
      }

      return false;
    }
  }
}