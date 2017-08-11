using System;
using System.Reflection;
using System.Security.Cryptography;
using JetBrains.DocumentModel;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.InternalsVisibleTo
{
  [Language(typeof(CSharpLanguage))]
  public class AssemblyKeyNameSuggestionProvider : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
  {
    private readonly IClrTypeName assemblyKeyNameAttributeClrName =
      new ClrTypeName("System.Reflection.AssemblyKeyNameAttribute");

    protected override bool IsAvailable(CSharpCodeCompletionContext context)
    {
      return context.IsInsideElement(assemblyKeyNameAttributeClrName);
    }

    protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
    {
      var rangeMarker = context.BasicContext.CaretDocumentOffset.CreateRangeMarker();

      foreach (var kc in KeyUtilities.EnumerateKeyContainers("Microsoft Strong Cryptographic Provider"))
      {
        var cspParameters = new CspParameters { KeyContainerName = kc, Flags = CspProviderFlags.UseMachineKeyStore };
        using (var prov = new RSACryptoServiceProvider(cspParameters))
        {
          if (!prov.CspKeyContainerInfo.Exportable) continue;

          var kp = new StrongNameKeyPair(prov.ExportCspBlob(true));
          if (kp.PublicKey.Length != 160) continue;

          var lookupItem = new SimpleTextLookupItem($"\"{kc}\"", rangeMarker);
          lookupItem.InitializeRanges(context.EvaluateRanges(), context.BasicContext);
          collector.Add(lookupItem);
        }
      }

      return true;
    }
  }
}