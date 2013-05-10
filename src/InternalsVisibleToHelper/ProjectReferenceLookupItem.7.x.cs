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

            if (projectImpl.OutputAssemblyFileCookie != null)
            {
                return projectImpl.OutputAssemblyFileCookie.Assembly.AssemblyName.GetPublicKey();
            }

            return null;
        }
         
    }
}