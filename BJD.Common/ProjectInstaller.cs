using System.ComponentModel;
using System.Configuration.Install;

namespace Bjd {
    //[RunInstaller(true)]
    public  class ProjectInstaller : Installer {

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;

        public ProjectInstaller() {
            //InitializeComponent();
        }
    }
}