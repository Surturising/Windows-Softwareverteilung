using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.DirectoryServices;
using Microsoft.VisualBasic;
using System.Xml.Schema;
using System.Management.Automation;

namespace Add_NMPackage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Dateiart { exe, msi, msu };

        public MainWindow()
        {




            InitializeComponent();

        }

        List<RegistryPrograms> registryPrograms;

        private void combo_PackageSource_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NMPackageManagement\\NMPackageManagement.xml";
            List<NMPackageConfigFile> packageSources = NMPackageConfigFile.ReadConfig(appdata);

            //Leeren der Combobox

            combo_PackageSource.Items.Clear();

            //Hinzufügen der PackageSources zur Combobox

            foreach (NMPackageConfigFile packageSource in packageSources)
            {
                combo_PackageSource.Items.Add(packageSource.SourceName);
            }

        }

        //Einblenden der Textbox um neuen Ordner anzulegen
        private void comboItem_NeuerOrdner_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtBox_NeuerOrdnerName.Visibility = Visibility.Visible;
            btn_NeuerOrdner_anlegen.Visibility = Visibility.Visible;
        }

        //Ordner der ausgewählten PackageSource in combobox hinzufügen.
        private void Programmordner_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtBox_NeuerOrdnerName.Visibility = Visibility.Hidden;
            btn_NeuerOrdner_anlegen.Visibility = Visibility.Hidden;
            txtBox_NeuerOrdnerName.Text = "neuer Ordner Name";

            //Hinzufügen der Ordner von PackageSource

            if (txt_PackageSource.Text != "")
            {
                DirectoryInfo packageFolders = new DirectoryInfo(txt_PackageSource.Text);

                foreach (DirectoryInfo packageFolder in packageFolders.GetDirectories())
                {
                    if (!(combo_Programmordner.Items.Contains(packageFolder.Name)))
                    {
                        combo_Programmordner.Items.Add(packageFolder.Name);
                    }


                }
            }

        }

        //Erstellen eines Neuen Ordners für neues Package
        private void btn_NeuerOrdner_anlegen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(txt_PackageSource.Text + "\\" + txtBox_NeuerOrdnerName.Text);
            }
            catch (Exception)
            {

                throw;
            }

            MessageBox.Show($"Der Ordner {txtBox_NeuerOrdnerName.Text} wurde angelegt");
        }

        //Anzeige des PackageSourcepfades in der nebenanliegenden Textbox
        private void combo_PackageSource_MouseLeave(object sender, MouseEventArgs e)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NMPackageManagement\\NMPackageManagement.xml";
            List<NMPackageConfigFile> packageSources = NMPackageConfigFile.ReadConfig(appdata);

            foreach (NMPackageConfigFile packageSource in packageSources)
            {
                if (packageSource.SourceName.Equals(combo_PackageSource.Text))
                {
                    txt_PackageSource.Text = packageSource.Location;
                }
            }
        }

        //Textbox Clearn.
        private void txtBox_NeuerOrdnerName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtBox_NeuerOrdnerName.Text.Equals("neuer Ordner Name"))
            {
                txtBox_NeuerOrdnerName.Text = "";
            }
        }

        //Aktualisierung der Combobox mit den Programmordnern
        private void combo_Programmordner_MouseEnter(object sender, MouseEventArgs e)
        {
            // Überprüfung ob ComboboxItem Neuer Ordner angezeigt werden darf.
            if (txt_PackageSource.Text != "")
            {
                comboItem_NeuerOrdner.Visibility = Visibility.Visible;
            }
            else
            {
                comboItem_NeuerOrdner.Visibility = Visibility.Hidden;
            }
        }

        //Beim Klicken leeren der Textbox
        private void tbox_Programmpfad_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (tbox_Programmpfad.Text.Equals("Pfad zur Programmdatei") || tbox_Programmpfad.Text.Equals("Pfad zum Programmordner"))
            {
                tbox_Programmpfad.Text = "";
            }
        }

        //Setzen des Textes der Textbox
        private void comboItem_NeuProgrammDatei_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbox_Programmpfad.Text = "Pfad zur Programmdatei";
        }

        //Setzen des Textes der Textbox
        private void comboItem_NeuProgrammordner_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbox_Programmpfad.Text = "Pfad zum Programmordner";
            combo_Item_exeMSI.Content = "exe, msi auswählen";
        }

        //Programmpfad OpenFileDialog OpenBrowserDialog
        private void btn_Programmpfad_Click(object sender, RoutedEventArgs e)
        {
            if (comboItem_NeuProgrammDatei.IsSelected)
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "All files (*.*) | *.*";
                openFile.ShowDialog();

                if (openFile.FileName != "")
                {
                    tbox_Programmpfad.Text = openFile.FileName;

                    if (openFile.FileName.Contains(".msi"))
                    {

                        combo_Item_exeMSI.Content = new DirectoryInfo(openFile.FileName).Name;
                        t_silentArgs.Text = "/quiet /passive";
                    }
                    else if (openFile.FileName.Contains(".exe"))
                    {
                        combo_Item_exeMSI.Content = new DirectoryInfo(openFile.FileName).Name;
                        t_silentArgs.Text = "Silent install Parameter eintragen";
                    }

                }
            }
            else if (comboItem_NeuProgrammordner.IsSelected)
            {
                MessageBox.Show("Noch nicht vorhanden Pfad bitte per Hand eingeben :(");

            }
        }

        private void combo_Programmdateipfad_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (comboItem_NeuProgrammordner.IsSelected)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(tbox_Programmpfad.Text);
                if (directoryInfo.Exists)
                {
                    //exe Dateie
                    FileInfo[] exeInfos = directoryInfo.GetFiles("*.exe");

                    //msi Datei
                    FileInfo[] msiInfos = directoryInfo.GetFiles("*.msi");

                    foreach (FileInfo exeInfo in exeInfos)
                    {

                        if (!combo_Programmdateipfad.Items.Contains(exeInfo.Name))
                        {
                            combo_Programmdateipfad.Items.Add(exeInfo.Name);
                        }
                    }

                    foreach (FileInfo msiInfo in msiInfos)
                    {

                        if (!combo_Programmdateipfad.Items.Contains(msiInfo.Name))
                        {
                            combo_Programmdateipfad.Items.Add(msiInfo.Name);
                        }
                    }
                }
            }
        }

        //Cleard Textfeld und übernimmt gleich von UninstallStrings
        private void t_silentArgs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txt_uninstallSilentArgs.Text != "Silent Uninstallargs")
            {
                t_silentArgs.Text = txt_uninstallSilentArgs.Text;
            }
            else
            {
                t_silentArgs.Text = "";
            }
        }

        //Cleard Textfeld und übernimmt gleich von Silent install Parameter
        private void txt_uninstallSilentArgs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (t_silentArgs.Text != "Silent install Parameter eintragen")
            {
                txt_uninstallSilentArgs.Text = t_silentArgs.Text;
            }
            else
            {
                txt_uninstallSilentArgs.Text = "";
            }
        }

        //Sucht in der Registry nach Uninstallstring und Verison
        private void btn_sucheProg_Click(object sender, RoutedEventArgs e)
        {
            //Überprüfen ob Architektur und Programmfolder ausgewählt wurde

            
            try
            {
                if (combo_Programmordner.Text != "Softwareordner wählen" && combo_Programmordner.Text != "Neuer Ordner")
                {
                    //Hinzufügen der Listenelemente (Programme aus der Registry)

                    List<RegistryPrograms> registryPrograms = RegistryPrograms.SearchRegistryPrograms(Combo_Architektur.Text, combo_Programmordner.Text);

                    list_RegistryProgs.Items.Clear();

                    foreach (RegistryPrograms program in registryPrograms)
                    {
                        if (!list_RegistryProgs.Items.Contains(program.DisplayName))
                        {
                            list_RegistryProgs.Items.Add(program.DisplayName);
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Softwareordner nicht ausgewählt.\nBitte auswählen.");
                }

            }
            catch (Exception)
            {

                MessageBox.Show("Unerwarteter Fehler!");
            }



             
        }

        //Anzeigen der Displayversion und des Uninstallstrings in Abhängigkeit des Listenelements. TODO//////////////////////////////TODO!!!!!!!!!!!!!

       


        private void list_RegistryProgs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            
            List<RegistryPrograms> registryPrograms = RegistryPrograms.SearchRegistryPrograms(Combo_Architektur.Text, combo_Programmordner.Text);

            foreach (RegistryPrograms program in registryPrograms)
            {

                if (list_RegistryProgs.SelectedItems.Contains(program.DisplayName))
                {
                    //Überarbeiten der Silent Uninstallargs 

                    if (program.UninstallString.Contains("MsiExec.exe"))
                    {
                        txt_uninstall.Text = "MsiExec.exe";
                        txt_uninstallSilentArgs.Text = program.UninstallString.Split('/')[1].Replace("I{", "/X {") + " /quiet";
                    }
                    else
                    {
                        txt_uninstall.Text = program.UninstallString;
                    }
                                        

                    txt_version.Text = program.DisplayVersion;
                    txt_psChildName.Text = program.PSChildName;
                }

            }
            
        }


        //Generiert / aktualisiert eine die Dependencies Liste
        private void check_dependencies_Checked(object sender, RoutedEventArgs e)
        {
            if (check_dependencies.IsChecked == true)
            {

                if (txt_PackageSource.Text != "")
                {
                    DirectoryInfo packageFolders = new DirectoryInfo(txt_PackageSource.Text);

                    foreach (DirectoryInfo packageFolder in packageFolders.GetDirectories())
                    {
                        CheckBox checkBox = new CheckBox();
                        checkBox.Content = packageFolder.Name;
                        checkBox.Checked += check_dropdown_dependencies;
                        checkBox.Unchecked += uncheck_dropdown_dependencies;
                        wrapp_Dependencies.Children.Add(checkBox);
                    }
                }
                else
                {
                    MessageBox.Show("Bitte Packagesource auswählen!");
                    check_dependencies.IsChecked = false;
                }

            }
        }

        //Dropdown Menue fuer Dependencies mit allen Optionen der Gecheckten Software
        private void check_dropdown_dependencies(object sender, RoutedEventArgs e)
        {


            //Erstellen der Combobox für Dependcy auswahl
            ComboBox box = new ComboBox();
            box.Name = "combobox_dependencies";
            wrapp_combo_Dependencies.Children.Add(box);

            //Fügt Programname in Combobox hinzu
            ComboBoxItem item = new ComboBoxItem();
            item.Content = (sender as CheckBox).Content;
            item.IsSelected = true;
            box.Items.Add(item);

            //Befüllen der Combobox

            DirectoryInfo Programs = new DirectoryInfo($"{txt_PackageSource.Text}\\{(sender as CheckBox).Content}");

            foreach (DirectoryInfo Program in Programs.GetDirectories("*", SearchOption.AllDirectories))
            {

                if (Program.FullName.Contains("LOG") == false && Program.FullName.Contains("Setup") == false && Program.FullName.EndsWith("x64") == false && Program.FullName.EndsWith("x86") == false)
                {
                    item = new ComboBoxItem();
                    item.Content = Program.FullName;
                    box.Items.Add(item);
                }


                
            }

            Programs.GetDirectories();
        }

        private void uncheck_dropdown_dependencies(object sender, RoutedEventArgs e)
        {
            wrapp_combo_Dependencies.Children.Clear();
        }

        //Leeren der Dependencies Liste bei unchecked
        private void check_dependencies_Unchecked(object sender, RoutedEventArgs e)
        {
            wrapp_Dependencies.Children.Clear();
        }

        //Schließt das Fenster ohne etwas zu speichern
        private void btn_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            window_NMPakcageMain.Close();
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            //leeren der Stack_info (infoboxen)
            listv_info.Items.Clear();

            //Info Item für Infobox
            ListViewItem info;
            #region Überprüfung
            //Überprüfen ob alle relevanten Fenster etc ausgefüllt sind.
            //Textbox für neue infos

            bool ueberpruefung = true;

            if (txt_PackageSource.Text == "")
            {
                info = new ListViewItem();
                info.Content = "Packagsource wurde nicht gesetzt";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (Combo_Architektur.Text == "Programmarchitektur")
            {
                info = new ListViewItem();
                info.Content = "Programmarchitektur muss ausgeählt werden!";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (combo_Programmordner.Text == "Softwareordner wählen" || combo_Programmordner.Text == "Neuer Ordner")
            {
                info = new ListViewItem();
                info.Content = "Softwareordner wurde nicht gewählt";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (tbox_Programmpfad.Text == "Pfad zur Prgrammdatei" || tbox_Programmpfad.Text == "Pfad zum Programmordner")
            {
                info = info = new ListViewItem();
                info.Content = "Pfad zur Programmdatei / Programmordner nicht gesetzt!";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (combo_Programmdateipfad.Text == "exe, msi auswählen")
            {
                info = info = new ListViewItem();
                info.Content = "exe, msi auswählen.";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (t_silentArgs.Text == "Silent install Parameter eintragen")
            {
                info = info = new ListViewItem();
                info.Content = "Keine Silent install Argumente eingetragen";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (txt_version.Text == "Versionsnummer")
            {
                info = info = new ListViewItem();
                info.Content = "Keine Versionsnummer eingetragen";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (txt_uninstall.Text == "Uninstallstring")
            {
                info = new ListViewItem();
                info.Content = "Kein Uninstallstring eingetragen";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }
            if (txt_uninstallSilentArgs.Text == "Silent Uninstallargs")
            {
                info = new ListViewItem();
                info.Content = "Keine Silent Uninstall Argumente angegeben";

                listv_info.Items.Add(info);
                ueberpruefung = false;
            }

            #endregion

            //Wenn Überprüfung OK, dann Ordner anlegen, Dateien kopieren, XML Metadaten anlegen.
            if (ueberpruefung)
            {

                #region Ordnererstellung

                string[] folderpaths = new string[]
{
                    txt_PackageSource.Text + "\\" + combo_Programmordner.Text,
                    txt_PackageSource.Text + "\\" + combo_Programmordner.Text + "\\" + Combo_Architektur.Text,
                    txt_PackageSource.Text + "\\" + combo_Programmordner.Text + "\\" + Combo_Architektur.Text + "\\" + txt_version.Text,
                    txt_PackageSource.Text + "\\" + combo_Programmordner.Text + "\\" + Combo_Architektur.Text + "\\" + txt_version.Text + "\\LOG",
                    txt_PackageSource.Text + "\\" + combo_Programmordner.Text + "\\" + Combo_Architektur.Text + "\\" + txt_version.Text + "\\Setup"
};

                bool isfolderCreated = true;
                try
                {
                   
                    //Überprüfen / Erstellen der Ordner

                    TestFolderPath(folderpaths);
                    
                }
                catch (Exception)
                {

                    MessageBox.Show("Fehler beim anlegen der Ordner");
                    isfolderCreated = false;
                }
                #endregion

                #region Kopiere Dateien / Ordner
                bool isFileCopied = true;
                if (isfolderCreated)
                {
                    
                    try
                    {
                        
                        //Kopieren der Datein

                        if (combo_Programmart.Text == "Neue Programmdatei")
                        {
                            File.Copy(tbox_Programmpfad.Text, $"{folderpaths[4]}\\{combo_Programmdateipfad.Text}");
                        }
                        else if (combo_Programmart.Text == "Neuer Programmordner")
                        {
                            DirectoryCopy(tbox_Programmpfad.Text, folderpaths[4], true);
                        }
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Fehler beim kopieren der Datein");
                        isFileCopied = false;
                    }
                }
                #endregion

                #region Generiere Metadata XML
                bool isMetadataGenerated = true;
                if (isfolderCreated)
                {
                    try
                    {
                        #region Read LocalinstallFolder
                        //ReadConfig für localInstallFolder
                        string localInstallFolder = "";
                        string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NMPackageManagement\\NMPackageManagement.xml";
                        List<NMPackageConfigFile> packageSources = NMPackageConfigFile.ReadConfig(appdata);

                        foreach (NMPackageConfigFile packageSource in packageSources)
                        {
                            if (packageSource.SourceName.Equals(combo_PackageSource.Text))
                            {
                                localInstallFolder = packageSource.LocalInstallFolder;
                            }
                        }
                        #endregion

                        


                        //Generiere Metadaten.
                        string pathDependencies = $"{txt_PackageSource.Text}\\{combo_Programmordner.Text}\\{Combo_Architektur.Text}\\{txt_version.Text}\\Dependencies.xml";
                        string pathMetadata = $"{txt_PackageSource.Text}\\{combo_Programmordner.Text}\\{Combo_Architektur.Text}\\{txt_version.Text}\\Metadata.xml";
                        string packageName = combo_Programmordner.Text;
                        string fileName = combo_Programmdateipfad.Text;
                        string fileLocation = $"{ txt_PackageSource.Text }\\{ combo_Programmordner.Text}\\{ Combo_Architektur.Text}\\{ txt_version.Text}\\Setup";
                        string folderPath = $"{ txt_PackageSource.Text }\\{ combo_Programmordner.Text}\\{ Combo_Architektur.Text}\\{ txt_version.Text}";
                        string installFolder = localInstallFolder;
                        string architecture = Combo_Architektur.Text;
                        string silentArgs = t_silentArgs.Text;
                        string version = txt_version.Text;
                        string uninstallString = txt_uninstall.Text;
                        string silentArgsUninstall = txt_uninstallSilentArgs.Text;
                        string psChildName = txt_psChildName.Text;
                        #region Überprüfung Checkboxen
                        bool hasDependencies = false;
                        bool needsRestart = false;

                        if (check_dependencies.IsChecked == true)
                        {
                            hasDependencies = true;
                        }

                        if (check_restart.IsChecked == true)
                        {
                            needsRestart = true;
                        }
                        #endregion

                        GenerateMetadataXML(pathMetadata, packageName, fileName, fileLocation,folderPath, installFolder, architecture, silentArgs, version, uninstallString, silentArgsUninstall, psChildName, needsRestart, hasDependencies);

                        if (hasDependencies)
                        {
                            GenerateDependenciesXML(pathDependencies, wrapp_combo_Dependencies.Children); 
                        }


                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Fehler beim generieren der Metadaten / Dependencies XML");
                        isMetadataGenerated = false;
                    }
                }
                #endregion

                #region Alles erstellt, kopiert...
                //Überprüfung ob alles erstellt und Kopiert wurde.
                if (isfolderCreated && isFileCopied && isMetadataGenerated)
                {
                    MessageBox.Show($"Package für {combo_Programmdateipfad.Text} wurde erstellt");
                }
                else
                {
                    MessageBox.Show($"Package wurde nicht richtig hinzugefügt!\n Bitte überprüfen Sie den Pfad: {txt_PackageSource.Text + "\\" + combo_Programmdateipfad.Text}", "Alert");
                }
                #endregion

            }
        }

        #region Hilfsfunktionen

        private static void TestFolderPath(params string[] paths)
        {

            foreach (string path in paths)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                if (!directoryInfo.Exists)
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        private static void GenerateMetadataXML(string path, string packageName, string fileName, string fileLocation, string folderPath, string installFolder, string architecture, string silentArgs, string version, string unistallString, string silentArgsUninstall, string psChildName, bool needsRestart, bool hasDependencies)
        {

            #region Grundaufbau XML

            string xmlContent = @"<Objs Version=""1.1.0.1"" xmlns=""http://schemas.microsoft.com/powershell/2004/04"">" + "\n" +
    @"    <Obj RefId=""0"">" + "\n" +
    @"      <TN RefId=""0"">" + "\n" +
    @"        <T>Metadata</T>" + "\n" +
    @"        <T>System.Object</T>" + "\n" +
    @"      </TN>" + "\n" +
    @"      <ToString>Metadata</ToString>" + "\n" +
    @"      <Props>" + "\n" +
    "         <S N=" + '"' + "PackageName" + '"' + $">{packageName}</S>" + "\n" +
    "         <S N=" + '"' + "FileName" + '"' + $">{fileName}</S>" + "\n" +
    "         <S N=" + '"' + "FileLocation" + '"' + $">{fileLocation}</S>" + "\n" +
    "         <S N=" + '"' + "FolderPath" + '"' + $">{folderPath}</S>" + "\n" +
    "         <S N=" + '"' + "InstallFolder" + '"' + $">{installFolder}</S>" + "\n" +
    "         <S N=" + '"' + "Architecture" + '"' + $">{architecture}</S>" + "\n" +
    "         <S N=" + '"' + "SilentArgs" + '"' + $">{silentArgs}</S>" + "\n" +
    "         <S N=" + '"' + "Version" + '"' + $">{version}</S>" + "\n" +
    "         <S N=" + '"' + "UninstallString" + '"' + $">{unistallString}</S>" + "\n" +
    "         <S N=" + '"' + "SilentArgsUninstall" + '"' + $">{silentArgsUninstall}</S>" + "\n" +
    "         <S N=" + '"' + "PSChildName" + '"' + $">{psChildName}</S>" + "\n" +
    "         <S N=" + '"' + "NeedsRestart" + '"' + $">{needsRestart}</S>" + "\n" +
    "         <S N=" + '"' + "HasDependencies" + '"' + $">{hasDependencies}</S>" + "\n" +
    "       </Props>" + "\n" +
    "     </Obj>" + "\n" +
    "   </Objs>";

            #endregion


            #region Ausführung
            
            FileInfo metadata = new FileInfo(path);
            if (!metadata.Exists)
            {
                System.IO.File.WriteAllText(path, xmlContent);

            }

            

            #endregion




        }
        private static void GenerateDependenciesXML(string path, UIElementCollection dependencies)
        {
            int counter = 0;
            #region Grundaufbau XML

            string xmlContent = @"<Objs Version=""1.1.0.1"" xmlns=""http://schemas.microsoft.com/powershell/2004/04"">" + "\n" +
    @"    <Obj RefId=""0"">" + "\n" +
    @"      <TN RefId=""0"">" + "\n" +
    @"        <T>System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</T>" + "\n" +
    @"      <T>System.Object</T>" + "\n" +
    @"    </TN>" + "\n" +
    @"    <LST>" + "\n";
            

            foreach (object checkbox in dependencies)
            {
                if (checkbox is ComboBox)
                {
                        counter++;
                        xmlContent += @"      <Obj RefId=" + '"' + counter + @""">" + "\n" +
                            @"        <TN RefId=" + '"' + counter + @""">" + "\n" +
                            @"          <T>Deserialized.System.Management.Automation.PSCustomObject</T>" + "\n" +
                            @"          <T>Deserialized.System.Object</T>" + "\n" +
                            @"        </TN>" + "\n" +
                            @"        <MS>" + "\n" +
                            @"          <S N=""PackageName"">" + $"{(checkbox as ComboBox).Text}" + "</S>" + "\n" +
                            @"        </MS>" + "\n" +
                            @"      </Obj>" + "\n";
                    

                }
            }

            //Schluss XML

            xmlContent += @"    </LST>" + "\n" +
                @"  </Obj>" + "\n" +
                @"</Objs>";
            #endregion

            #region Ausführung
            FileInfo metadata = new FileInfo(path);
            if (!metadata.Exists)
            {
                System.IO.File.WriteAllText(path, xmlContent);

            }



            #endregion
        }

#endregion


    }
}
