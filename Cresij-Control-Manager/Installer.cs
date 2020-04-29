using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
            // Attach the 'Committed' event.
           
            
        }

        

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            try
            {
                
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {

                    if (registryKey.GetValue("ServerManagerApp") != null)
                    {
                        registryKey.DeleteValue("ServerManagerApp", true);
                    }
                }                
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Registry Keys exception " + ex.Message);
            }           
        }
       
    }
}
