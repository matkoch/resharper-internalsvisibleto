using System;
using System.Drawing;
using System.Text;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace InternalsVisibleTo.ReSharper
{
    public partial class ProjectReferenceLookupItem : TextLookupItem
    {
        private const string ellipsis = "…";
        private readonly IProject project;

        public ProjectReferenceLookupItem(IProject project, IconId image, IRangeMarker rangeMarker)
            : base(GetCompleteText(project), image)
        {
            this.project = project;
            VisualReplaceRangeMarker = rangeMarker;
        }

        protected override RichText GetDisplayName()
        {
            RichText displayName = LookupUtil.FormatLookupString(string.Format("\"{0}\"", project.Name));
            
            byte[] publicKey = GetPublicKey(project as ProjectImpl);
            if (publicKey != null)
            {
                RichText publicKeyDisplay = LookupUtil.FormatLookupString("PublicKey=" + ellipsis);
                publicKeyDisplay.SetStyle(TextStyle.FromForeColor(SystemColors.GrayText));
                
                // aligns the "PublicKey=..." text to the right, as if it were a type name
                DisplayTypeName = publicKeyDisplay;
            }

            return displayName;
        }

        private static string GetCompleteText(IProject project)
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
    }
}