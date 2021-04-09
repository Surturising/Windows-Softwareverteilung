using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace Add_NMPackage
{
    class NMPackageConfigFile
    {
        protected string localInstallFolder;
        protected string sourceName;
        protected string location;

        #region Properties

        public string LocalInstallFolder 
        { 
            get { return localInstallFolder; }
        }

        public string SourceName
        {
            get { return sourceName; }
        }

        public string Location
        {
            get { return location; }
        }


        #endregion


        #region Structs

        public NMPackageConfigFile(string localInstallFolder, string sourceName, string location)
        {
            this.localInstallFolder = localInstallFolder;
            this.sourceName = sourceName;
            this.location = location;
        }

        #endregion


        #region Methoden

        public static List<NMPackageConfigFile> ReadConfig(string filePath) //TODO: Appdata verzeichnis verallgemeinern!!
        {
            //Einlesen des ConfigFiles für PackageSource
            List<NMPackageConfigFile> configFile = new List<NMPackageConfigFile> { };
            char[] seperators = new char[] { '<', '>'};

            string localInstallFolder = "";
            string sourceName = "";
            string location = "";

            FileStream fs = new FileStream(filePath, FileMode.Open);
            StreamReader read = new StreamReader(fs);
            string xmlLine = read.ReadLine();

            while (xmlLine != null)
            {
                if (xmlLine.Contains("LocalInstallFolder"))
                {
                    localInstallFolder = xmlLine.Split(seperators)[2];
                }
                else if(xmlLine.Contains("SourceName"))
                {
                    sourceName = xmlLine.Split(seperators)[2];
                }
                else if (xmlLine.Contains("Location"))
                {
                    location = xmlLine.Split(seperators)[2];
                }

                //Hinzufügen des Elements an die Liste
                if (localInstallFolder != "" && sourceName != "" && location != "")
                {
                    configFile.Add(new NMPackageConfigFile(localInstallFolder, sourceName, location));

                    localInstallFolder = "";
                    sourceName = "";
                    location = "";
                }
                xmlLine = read.ReadLine();
            }
            fs.Close();
            read.Close();

            return configFile;
        }

        public override string ToString()
        {
            return $"LocalInstallFolder {this.localInstallFolder}\tSourceName {this.SourceName}\tLocation {this.Location}";
        }

        #endregion

    }
}
