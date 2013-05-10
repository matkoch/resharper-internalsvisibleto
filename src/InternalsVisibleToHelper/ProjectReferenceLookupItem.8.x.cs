using JetBrains.ProjectModel;

namespace InternalsVisibleTo.ReSharper
{
    public partial class ProjectReferenceLookupItem
    {
        private static byte[] GetPublicKey(ProjectImpl projectImpl)
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