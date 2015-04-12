using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace BennorMcCarthy.AutoT4
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidAutoT4PkgString)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    public sealed class AutoT4Package : Package
    {
        private DTE _dte;
        private BuildEvents _buildEvents;
        private ObjectExtenders _objectExtenders;
        private AutoT4ExtenderProvider _extenderProvider;
        private readonly List<int> _extenderProviderCookies = new List<int>();
        private HashSet<string> successfullProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public AutoT4Package()
        {
            Debug.WriteLine("Entering constructor for: {0}", this.GetType().FullName);
        }

        protected override void Initialize()
        {
            Debug.WriteLine("Entering Initialize() of: {0}", this.GetType().FullName);

            base.Initialize();

            _dte = (DTE)GetService(typeof(DTE));
            if (_dte == null)
                return;

            var window = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);


            RegisterEvents();

            RegisterExtenderProvider(VSConstants.CATID.CSharpFileProperties_string);
            RegisterExtenderProvider(VSConstants.CATID.VBFileProperties_string);
        }

        private void RegisterEvents()
        {
            _buildEvents = _dte.Events.BuildEvents;
            _buildEvents.OnBuildBegin += OnBuildBegin;
            _buildEvents.OnBuildDone += OnBuildDone;
            _buildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
        }

        private void RegisterExtenderProvider(string catId)
        {
            const string name = AutoT4ExtenderProvider.Name;

            _objectExtenders = _objectExtenders ?? GetService(typeof(ObjectExtenders)) as ObjectExtenders;
            if (_objectExtenders == null)
                return;

            _extenderProvider = _extenderProvider ?? new AutoT4ExtenderProvider(_dte, this);
            _extenderProviderCookies.Add(_objectExtenders.RegisterExtenderProvider(catId, name, _extenderProvider));
        }

        private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            successfullProjects.Clear();
            RunTemplates(Scope, BuildEvent.BeforeBuild);
        }

        private void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            RunTemplates(Scope, BuildEvent.AfterBuild);
        }

        private void OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            if (Success)
                successfullProjects.Add(Project);
        }

        private void RunTemplates(vsBuildScope scope, BuildEvent buildEvent)
        {
            _dte.GetProjectsWithinBuildScope(vsBuildScope.vsBuildScopeSolution)
                .FindT4ProjectItems()
                .ThatShouldRunOn(buildEvent, successfullProjects)
                .ToList()
                .ForEach(item => item.RunTemplate());
        }

        protected override int QueryClose(out bool canClose)
        {
            int result = base.QueryClose(out canClose);
            if (!canClose)
                return result;

            if (_buildEvents != null)
            {
                _buildEvents.OnBuildBegin -= OnBuildBegin;
                _buildEvents.OnBuildDone -= OnBuildDone;
                _buildEvents.OnBuildProjConfigDone -= OnBuildProjConfigDone;
                _buildEvents = null;
            }
            return result;
        }
    }
}
