using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EnvDTE;

namespace BennorMcCarthy.AutoT4
{
    [CLSCompliant(false)]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class AutoT4ProjectItemSettings : ProjectItemSettings
    {
        private const BuildEvent DefaultRunOnBuildSetting = BuildEvent.AfterBuild;

        public AutoT4ProjectItemSettings(ProjectItem item)
            : base(item, "AutoT4") { }

        [DefaultValue("")]
        [DisplayName("Depends on project")]
        [Category("AutoT4")]
        [Description("Run only if specified project is built succesfully")]
        public string DependsOnProject
        {
            get { return Get("", null); }
            set { Set(value != null ? value.Trim() : null); }
        }

        [DefaultValue(DefaultRunOnBuildSetting)]
        [DisplayName("Run on build")]
        [Category("AutoT4")]
        [Description("Whether to run this template at build time or not.")]
        public BuildEvent RunOnBuild
        {
            get { return Get(DefaultRunOnBuildSetting, CoerceOldRunOnBuildValue); }
            set { Set(value); }
        }

        /// <summary>
        /// Converts the old <see cref="bool"/> <see cref="RunOnBuild"/> property to <see cref="BuildEvent"/>
        /// </summary>
        private BuildEvent CoerceOldRunOnBuildValue(string value)
        {
            var newRunOnBuildValue = DefaultRunOnBuildSetting;
            bool previousRunOnBuild;
            if (bool.TryParse(value, out previousRunOnBuild))
                newRunOnBuildValue = previousRunOnBuild ? BuildEvent.AfterBuild : BuildEvent.DoNotRun;

            //coercion was needed, therefore the new value needs to be assigned so that it gets migrated in the settings
            RunOnBuild = newRunOnBuildValue;

            return newRunOnBuildValue;
        }
    }

    public enum BuildEvent
    {
        DoNotRun,
        BeforeBuild,
        AfterBuild
    }
}
