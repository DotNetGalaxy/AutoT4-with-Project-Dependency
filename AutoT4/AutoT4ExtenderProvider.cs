using System;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace BennorMcCarthy.AutoT4
{
    [Guid(ExtenderGuid)]
    public class AutoT4ExtenderProvider : IExtenderProvider
    {
        public const string ExtenderGuid = "174D1A83-20C0-4783-AD6B-032929BEC4B1";
        public const string Name = "AutoT4ExtenderProvider";

        private readonly DTE _dte;
        private readonly IServiceProvider _serviceProvider;

        public AutoT4ExtenderProvider(DTE dte, IServiceProvider serviceProvider)
        {
            _dte = dte;
            _serviceProvider = serviceProvider;
        }

        public object GetExtender(string extenderCATID, string extenderName, object extendeeObject, IExtenderSite extenderSite, int cookie)
        {
            dynamic extendee = extendeeObject;
            string fullPath = extendee.FullPath;
            var projectItem = _dte.Solution.FindProjectItem(fullPath);
            IVsSolution solution = (IVsSolution) _serviceProvider.GetService(typeof(SVsSolution));
            IVsHierarchy projectHierarchy;
            if (solution.GetProjectOfUniqueName(projectItem.ContainingProject.UniqueName, out projectHierarchy) != 0)
                return null;
            uint itemId;
            if (projectHierarchy.ParseCanonicalName(fullPath, out itemId) != 0)
                return null;

            return new AutoT4Extender(projectItem, extenderSite, cookie);
        }

        public bool CanExtend(string extenderCatid, string extenderName, object extendeeObject)
        {
            var fileProperties = extendeeObject as VSLangProj.FileProperties;
            return extenderName == Name &&
                   fileProperties != null &&
                   ".tt".Equals(fileProperties.Extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}