using System;
using System.IO;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Caches;
using JetBrains.ProjectModel.Properties.CSharp;

namespace ReSharper.InternalsVisibleTo
{
    [SolutionComponent]
    internal class SnkDataProvider : IProjectFileDataCache
    {
        [NotNull]
        private readonly ProjectFileDataCache projectDataCache;
        [NotNull]
        private readonly ISolution solution;

        private IProject currentProject;

        public SnkDataProvider([NotNull] Lifetime lifetime, [NotNull] ProjectFileDataCache projectDataCache, [NotNull] ISolution solution)
        {
            this.projectDataCache = projectDataCache;
            this.solution = solution;

            projectDataCache.RegisterCache(lifetime, this);
        }

        public object Read(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }

        public void Write(BinaryWriter writer, object data)
        {
            var bytes = (byte[])data;

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public bool CanHandle(IProjectFile projectFile)
        {
            currentProject = projectFile.GetProject();
            if (currentProject == null)
            {
                return false;
            }

            return currentProject.ProjectProperties.BuildSettings is CSharpBuildSettings;
        }

        public object BuildData(XmlDocument document)
        {
            string keyName = ExtractPublicKey(document);
            if (string.IsNullOrWhiteSpace(keyName))
            {
                return null;
            }


        }

        public Action OnDataChanged(IProjectFile projectFile, object oldData, object newData)
        {
            return null;
        }

        public int Version
        {
            get { return 0; }
        }

        private string ExtractPublicKey(XmlDocument document)
        {
            XmlElement documentElement = document.DocumentElement;
            if (documentElement == null || documentElement.Name != "Project")
            {
                return null;
            }

            XmlNode signAssemblyNode = documentElement.SelectSingleNode("//*[local-name()='SignAssembly']");
            if (signAssemblyNode == null ||
                !signAssemblyNode.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            XmlNode keyNode = documentElement.SelectSingleNode("//*[local-name()='AssemblyOriginatorKeyFile']");
            if (keyNode == null)
            {
                return null;
            }

            return keyNode.InnerText;
        }

        [CanBeNull]
        public byte[] GetPublicKey(IProject project)
        {
            return projectDataCache.GetData<byte[]>(this, project, null);
        }
    }
}