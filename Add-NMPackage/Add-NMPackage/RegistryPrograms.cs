using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using System.Windows;

namespace Add_NMPackage
{
    class RegistryPrograms
    {
        protected string displayName;
        protected string displayVersion;
        protected string uninstallString;
        protected string psChildName;

        #region Properties

        public string DisplayName
        {
            get { return displayName; }
        }

        public string DisplayVersion
        {
            get { return displayVersion; }
        }

        public string UninstallString
        {
            get { return uninstallString; }
        }
        
        public string PSChildName
        {
            get { return psChildName; }
        }
        #endregion


        #region Structs

        public RegistryPrograms(string displayname, string displayversion, string uninstallstring, string psChildName)
        {
            this.displayName = displayname;
            this.displayVersion = displayversion;
            this.uninstallString = uninstallstring;
            this.psChildName = psChildName;
        }

        #endregion


        #region Methoden

        
        public static List<RegistryPrograms> SearchRegistryPrograms(string architektur, string name)
        {
            //Einlesen des ConfigFiles für PackageSource
            List<RegistryPrograms> programList = new List<RegistryPrograms> { };

            string displayName = "";
            string displayVersion = "";
            string uninstallString = "";
            string psChildName = "";

            PowerShell programs = PowerShell.Create();
            programs.AddCommand("Get-ItemProperty");

            if (architektur == "x64")
            {
                programs.AddParameter("Path", "HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*");
            }
            else if (architektur == "x86")
            {
                programs.AddParameter("Path", "HKLM:\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*");
            }
            else
            {
                MessageBox.Show("Programmarchitektur wählen");
            }
            //Aufteilen des Strings

            if (name == "*")
            {
                foreach (PSObject prog in programs.Invoke())
                {

                    foreach (string property in prog.ToString().Split(';'))
                    {
                        if (property.Contains("DisplayName"))
                        {
                            displayName = property.Split('=')[1];
                        }
                        else if (property.Contains("DisplayVersion"))
                        {
                            displayVersion = property.Split('=')[1];
                        }
                        else if (property.Contains("UninstallString"))
                        {
                            uninstallString = property.Split('=')[1];
                        }
                        else if (property.Contains("PSChildName"))
                        {
                            psChildName = property.Split('=')[1];
                        }
                    }

                    programList.Add(new RegistryPrograms(displayName, displayVersion, uninstallString, psChildName));

                    displayName = "";
                    displayVersion = "";
                    uninstallString = "";
                    psChildName = "";
                }
            }
            else
            {

                foreach (PSObject prog in programs.Invoke())
                {

                    if (prog.ToString().Contains(name))
                    {
                        foreach (string property in prog.ToString().Split(';'))
                        {
                            if (property.Contains("DisplayName"))
                            {
                                displayName = property.Split('=')[1];
                            }
                            else if (property.Contains("DisplayVersion"))
                            {
                                displayVersion = property.Split('=')[1];
                            }
                            else if (property.Contains("UninstallString"))
                            {
                                uninstallString = property.Split('=')[1];
                            }
                            else if (property.Contains("PSChildName"))
                            {
                                psChildName = property.Split('=')[1];
                            }
                        }

                    }


                    programList.Add(new RegistryPrograms(displayName, displayVersion, uninstallString, psChildName));

                    displayName = "";
                    displayVersion = "";
                    uninstallString = "";
                    psChildName = "";
                }
            }
   

            return programList;
        }

        #endregion
    }
}
