using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Application.Parts;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Caches;
using JetBrains.Util;
using Microsoft.CodeAnalysis.Interop;

namespace ReSharper.InternalsVisibleTo
{
  [SolutionInstanceComponent(Instantiation.ContainerAsyncPrimaryThread)]
  internal class SnkDataProvider : IProjectFileDataProvider<byte[]>
  {
    [CanBeNull] private VirtualFileSystemPath myCurrentProjectPath;

    [NotNull] public ProjectFileDataCacheImpl ProjectDataCache { get; }

    public SnkDataProvider([NotNull] Lifetime lifetime, [NotNull] ProjectFileDataCacheImpl projectDataCache)
    {
      ProjectDataCache = projectDataCache;

      projectDataCache.RegisterCache(lifetime, this);
    }

    public bool CanHandle(VirtualFileSystemPath projectFileLocation)
    {
      myCurrentProjectPath = projectFileLocation;
      return true;
    }

    public int Version { get; } = 0;

    public byte[] Read(VirtualFileSystemPath projectFileLocation, BinaryReader reader)
    {
      int length = reader.ReadInt32();
      if (length == 0) return EmptyArray<byte>.Instance;

      return reader.ReadBytes(length);
    }

    public void Write(VirtualFileSystemPath projectFileLocation, BinaryWriter writer, byte[] data)
    {
      var bytes = data;
      writer.Write(bytes.Length);
      writer.Write(bytes);
    }

    public byte[] BuildData(VirtualFileSystemPath projectFileLocation, XmlDocument document)
    {
      if (myCurrentProjectPath == null) return EmptyArray<byte>.Instance;

      var keyContainer = ExtractPublicKeyFile(document);
      if (!string.IsNullOrWhiteSpace(keyContainer))
      {
        string keyFilePath = myCurrentProjectPath.Directory.Combine(keyContainer).FullPath;
        return ReadKeysFromPath(keyFilePath);
      }

      return EmptyArray<byte>.Instance;
    }

    public Action OnDataChanged(VirtualFileSystemPath projectFileLocation, byte[] oldData, byte[] newData)
    {
      return null;
    }

    [CanBeNull, Pure]
    private static string ExtractPublicKeyFile([NotNull] XmlDocument document)
    {
      var documentElement = document.DocumentElement;
      if (documentElement == null || documentElement.Name != "Project")
      {
        return null;
      }

      var signAssemblyNode = documentElement.SelectSingleNode("//*[local-name()='SignAssembly']");
      if (signAssemblyNode == null ||
          !signAssemblyNode.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
      {
        return null;
      }

      var keyNode = documentElement.SelectSingleNode("//*[local-name()='AssemblyOriginatorKeyFile']");

      return keyNode?.InnerText;
    }

    [NotNull, MustUseReturnValue]
    private static byte[] ReadKeysFromPath([NotNull] string fullPath)
    {
      try
      {
        var fileContent = File.ReadAllBytes(fullPath);
        var publicKey = IsPublicKeyBlob(fileContent) ? fileContent : GetPublicKey(fileContent);
        return publicKey;
      }
      catch (Exception ex)
      {
        throw new IOException(ex.Message);
      }
    }

    //The definition of a public key blob from StrongName.h

    //typedef struct {
    //    unsigned int SigAlgId;
    //    unsigned int HashAlgId;
    //    ULONG cbPublicKey;
    //    BYTE PublicKey[1]
    //} PublicKeyBlob;

    //__forceinline bool IsValidPublicKeyBlob(const PublicKeyBlob *p, const size_t len)
    //{
    //    return ((VAL32(p->cbPublicKey) + (sizeof(ULONG) * 3)) == len &&         // do the lengths match?
    //            GET_ALG_CLASS(VAL32(p->SigAlgID)) == ALG_CLASS_SIGNATURE &&     // is it a valid signature alg?
    //            GET_ALG_CLASS(VAL32(p->HashAlgID)) == ALG_CLASS_HASH);         // is it a valid hash alg?
    //}

    private static uint GET_ALG_CLASS(uint x)
    {
      return x & (7 << 13);
    }

    internal static unsafe bool IsPublicKeyBlob(byte[] keyFileContents)
    {
      const uint ALG_CLASS_SIGNATURE = 1 << 13;
      const uint ALG_CLASS_HASH = 4 << 13;

      if (keyFileContents.Length < (4 * 3))
      {
        return false;
      }

      fixed(byte* p = keyFileContents)
      {
        return (GET_ALG_CLASS((uint)Marshal.ReadInt32((IntPtr)p)) == ALG_CLASS_SIGNATURE) &&
               (GET_ALG_CLASS((uint)Marshal.ReadInt32((IntPtr)p, 4)) == ALG_CLASS_HASH) &&
               (Marshal.ReadInt32((IntPtr)p, 8) + (4 * 3) == keyFileContents.Length);
      }
    }

    [NotNull, MustUseReturnValue]
    private static byte[] GetPublicKey([NotNull] byte[] keyFileContents)
    {
      var strongName = ClrStrongName.GetInstance();

      IntPtr keyBlob;
      int keyBlobByteCount;

      unsafe
      {
        fixed(byte* p = keyFileContents)
        {
          strongName.StrongNameGetPublicKey(null, (IntPtr)p, keyFileContents.Length, out keyBlob, out keyBlobByteCount);
        }
      }

      var publicKey = new byte[keyBlobByteCount];
      Marshal.Copy(keyBlob, publicKey, 0, keyBlobByteCount);
      strongName.StrongNameFreeBuffer(keyBlob);

      return publicKey;
    }
  }
}
