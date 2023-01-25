
#Pfad zur Configdatei der PackageSources
$ConfigFileDirectory = "$Env:APPDATA\NMPackageManagement"
$ConfigFileName = 'NMPackageManagement.xml'

<#
.SYNOPSIS
Installiert leise im Hintergrund ein NMPackage auf einem Remotecomputer.

.DESCRIPTION
Long description

.PARAMETER FolderPath
Pfad zum Überordner, in dem Setup und Metadata.xml sind

.PARAMETER ComputerName
Computer auf dem das NMPackage installiert werden soll.

.PARAMETER WriteLog
Schreibt eine Log Datei, die später über Read-NMPackageLog ausgelesen werden kann

.EXAMPLE
Get-NMPackage -FolderPath '7-ZIP' -Version 19.00 | Install-NMPackage -ComputerName "PC1"

.NOTES
General notes
#>
function Install-NMPackage {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        [string]$FolderPath,
        [string[]]$ComputerName = 'localhost',
        [bool]$WriteLog = $true
    )
    
    #Import Metadata
    $Metadata = Import-Clixml -Path $FolderPath\Metadata.xml

    if ($Metadata.hasDependencies -eq 'True') {

        $Dependencies = Import-Clixml -Path $FolderPath\Dependencies.xml

        foreach ($Dependency in $Dependencies.PackageName) {

            if ($WriteLog) {
                Install-NMPackage -FolderPath $Dependency -ComputerName $ComputerName -WriteLog
            }
            else {
                Install-NMPackage -FolderPath $Dependency -ComputerName $ComputerName
            }

            

        }
    }

    $FileName = $Metadata.FileName 
    $SilentArgs = $Metadata.SilentArgs
    $InstallFolder = $Metadata.InstallFolder
    $LocalInstallPath = "$InstallFolder\setup"
    $LocalInstallFile = "$LocalInstallPath\$FileName"
    $FileLocation = $Metadata.FileLocation


    #foreach
    foreach ($PC in $ComputerName) {
        $IsPackageInstalled = Test-NMPackage -FolderPath $FolderPath -ComputerName $PC

        if ($IsPackageInstalled -eq $false) {
            #öffnen neuer PSSession
            Write-Verbose -Message "Öffnen einer PSSession für $PC mit Name $PC"
            $Session = New-PSSession -ComputerName $PC -Name $PC
                                
            #Überprüfung ob C:\Install existiert
            Write-Verbose -Message "Überprüfung ob das Verzeichnis C:\Install auf $PC existiert"
            Invoke-Command -ScriptBlock { if ((Test-Path -Path $Using:InstallFolder) -eq $false) { New-Item -Path $Using:InstallFolder -ItemType Directory } } -Session $Session
                                
            #Kopieren der Dateien
            Write-Verbose -Message "Kopieren der Install Datei auf $PC in Verzeichnis C:\Install"
            Copy-Item -Path "$FileLocation" -Destination C:\Install -ToSession $Session -Recurse
                                
            #Ausführung
            Write-Verbose -Message "Installation von $FileName auf $PC per Job"
            Invoke-Command -Session $Session -ScriptBlock {
                            
                #Installation des Programmes
                Start-Process -FilePath $Using:LocalInstallFile -ArgumentList $Using:SilentArgs -Wait
                Write-Verbose -Message "Löschen der setup Dateien $Using:LocalInstallPath"
                Remove-Item -Path $Using:LocalInstallPath -Force -Recurse
                        
            }
            Remove-PSSession -Session $Session
            $IsPackageInstalled = Test-NMPackage -FolderPath $FolderPath -ComputerName $PC

        }
                
        $prop = @{
            'FunctionName'     = 'Install-NMPackage'
            'ComputerName'     = $PC
            'PackageName'      = $Metadata.PackageName
            'PackageInstalled' = $IsPackageInstalled
        }
                
        $Output = New-Object -TypeName PSObject -Property $prop
                
        Write-Output $Output
        if ($WriteLog) {
            
            $LogFile = "$FileLocation\..\LOG\log.txt"

            #Überprüfen ob Logdatei existiert
            if (((Test-Path -Path $LogFile) -eq $false)) {
                
                New-Item -Path $LogFile
            }

            #Log schreiben
            Add-Content -Path $LogFile -Value ("Install-NMPackage;" + "$PC;" + $Metadata.PackageName + ";$IsPackageInstalled;" + (Get-Date -Format g))

        }
    }

}

<#
.SYNOPSIS
Deinstalliert im Hintergrund ein NMPackage auf einem Remotecomputer

.DESCRIPTION
Long description

.PARAMETER FolderPath
Pfad zum Überordner, in dem Setup und Metadata.xml sind

.PARAMETER ComputerName
Computer auf dem das NMPackage deinstalliert werden soll.

.PARAMETER WriteLog
Schreibt eine Log Datei, die später über Read-NMPackageLog ausgelesen werden kann

.PARAMETER RemoveDependencies
Parameter description

.EXAMPLE
Get-NMPackage -FolderPath '7-ZIP' -Version 19.00 | Uninstall-NMPackage -ComputerName "PC1"

.NOTES
General notes
#>
function Uninstall-NMPackage {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        [string]$FolderPath,
        [string[]]$ComputerName = 'localhost',
        [bool]$WriteLog = $true,
        [switch] $RemoveDependencies
    )
    
    #Import Metadata
    $Metadata = Import-Clixml -Path $FolderPath\Metadata.xml

    if ($RemoveDependencies) {

        $Dependencies = Import-Clixml -Path $FolderPath\Dependencies.xml

        foreach ($Dependency in $Dependencies) {

            Uninstall-NMPackage -FolderPath $Dependency -ComputerName $ComputerName
        }
    }

    $UninstallString = $Metadata.UninstallString 
    $SilentArgsUninstall = $Metadata.SilentArgsUninstall
    $FileLocation = $Metadata.FileLocation

    #foreach
    foreach ($PC in $ComputerName) {

        $IsPackageInstalled = Test-NMPackage -FolderPath $FolderPath -ComputerName $PC

        if ($IsPackageInstalled) {
            
            #öffnen neuer PSSession
            Write-Verbose -Message "Öffnen einer PSSession für $PC mit Name $PC"
            $Session = New-PSSession -ComputerName $PC -Name $PC
            
            #Ausführung
            Write-Verbose -Message "Deinstallation von $FileName auf $PC per Job"
            Invoke-Command -Session $Session -ScriptBlock {
            
                #Deinstallation des Programmes
                Start-Process -FilePath $Using:UninstallString -ArgumentList $Using:SilentArgsUninstall -Wait
            }

            Remove-PSSession -Session $Session
            $IsPackageInstalled = Test-NMPackage -FolderPath $FolderPath -ComputerName $PC
                
        }

        $prop = @{
            'FunctionName'     = 'Uninstall-NMPackage'
            'ComputerName'     = $PC
            'PackageName'      = $Metadata.PackageName
            'PackageInstalled' = $IsPackageInstalled
        }
                    
        $Output = New-Object -TypeName PSObject -Property $prop
        Write-Output $Output

        if ($WriteLog) {
            
            $LogFile = "$FileLocation\..\LOG\log.txt"
    
            #Überprüfen ob Logdatei existiert
            if (((Test-Path -Path $LogFile) -eq $false)) {
                    
                New-Item -Path $LogFile
            }
    
            #Log schreiben
            Add-Content -Path $LogFile -Value ("Uninstall-NMPackage;" + "$PC;" + $Metadata.PackageName + ";$IsPackageInstalled;" + (Get-Date -Format g))
    
        }

    }
}

<#
.SYNOPSIS
Überprüft ob ein NMPackage auf einem Remotecomputer installiert ist. Liefert True oder False zurück

.DESCRIPTION
Long description

.PARAMETER FolderPath
Pfad zum Überordner, in dem Setup und Metadata.xml sind

.PARAMETER ComputerName
Computer auf dem überprüft wird, ob das NMPackage installiert ist.

.EXAMPLE
Get-NMPackage -FolderPath '7-ZIP' -Version 19.00 | Test-NMPackage -ComputerName "PC1"

.NOTES
General notes
#>
function Test-NMPackage {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        [string]$FolderPath,
        [string]$ComputerName = 'localhost'
    )
        
    #Import Metadata
    $Metadata = Import-Clixml -Path $FolderPath\Metadata.xml
    
    
    $Architecture = $Metadata.Architecture
    $PSChildName = $Metadata.PSChildName
    $Version = $Metadata.Version
    
    #foreach
    foreach ($PC in $ComputerName) {
    
                    
        #Ausführung
        Write-Verbose -Message "Überprüfe auf $PC ob RegistryPath $PSChildname vorhanden ist"
        Invoke-Command -ComputerName $ComputerName -ScriptBlock {
            #Funktion für Registryüberprüfung
            if ($Using:Architecture -eq 'x64') {
                        
                $Program = Test-Path -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$Using:PSCHildName"
                if ($Program -eq $true) {
                    $ProgramInfo = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$Using:PSCHildName"
                    if ($ProgramInfo.Displayversion -eq "$Using:Version") { Write-Output $true } else {
                        Write-Output $false
                    }
                }
                else { Write-Output $false }
    
            }
            if ($Using:Architecture -eq 'x86') {
                                                
                $Program = Test-Path -Path "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\$Using:PSCHildName"
                if ($Program -eq $true) {
                    $ProgramInfo = Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\$Using:PSCHildName"
                    if ($ProgramInfo.Displayversion -eq "$Using:Version") { Write-Output $true } else {
                        Write-Output $false
                    }
                                
                }
                else { Write-Output $false }
            }
        }#scriptblock
    }#foreach
}#function
    
<#
.SYNOPSIS
Durchsucht die Registry eines Remotecomputers, um zu überpüfen, ob das Programm installiert ist.

.DESCRIPTION
Long description

.PARAMETER Name
Name des Programms das gesucht wird

.PARAMETER ComputerName
Computer auf dem das Programm gesucht wird.

.EXAMPLE
Search-NMPackage -Name *zip* -ComputerName "PC1" 

.NOTES
General notes
#>
function Search-Package {
    [CmdletBinding()]
    param (
        [string]$Name = '*',
        [string]$ComputerName = 'localhost'
    )
        
    process {

        Invoke-Command -ComputerName $ComputerName -ScriptBlock {
            
            $64bitProgram = Get-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object -FilterScript { $_.DisplayName -like "*$Using:Name*" } | Select-Object -Property *
            $32bitProgram = Get-ItemProperty -Path HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object -FilterScript { $_.DisplayName -like "*$Using:Name*" } | Select-Object -Property *
          
            Write-Output $64bitProgram
            Write-Output $32bitProgram
        }
    }

}

<#
.SYNOPSIS
Short description 
    Registriert eine neue PackageSource

.DESCRIPTION
Long description

.PARAMETER UNCPath
Parameter description
    Die UNC-Freigabe auf dem die Programme hinzugefügt werden

.PARAMETER SourceName
Parameter description 
    Name der neuen PackageSource

.EXAMPLE
Register-NMPackageSource -UNCPatch '\\server\freigabename' -SourceName 'InternalPrograms'

.NOTES
General notes
#>#
function Register-NMPackageSource {

    
    

    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$UNCPath,
        [Parameter(Mandatory = $true)]
        [string]$SourceName,
        [string]$LocalInstallFolder = 'C:\install'
    ) 

    Begin {

        #In diesme Pfad wird die Config Datei abgelegt.
        $PackageSources = New-Object System.Collections.Generic.List[System.Object]
    }

    Process {
        #Testen ob Pfad ein UNC Pfad ist.
        if (!($UNCPath.StartsWith('\\') -and $UNCPath.Substring(2).Contains('\') -and (!$UNCPath.EndsWith('\')))) {
            Write-Error -Message "Der Pfad $UNCPath ist kein UNC-Pfad. Bitte geben Sie eine UNC Freigabe als folgendem Schema an: \\Servername\Freigabename"
            break
        }
        #Testen ob der UNC Pfad vorhanden ist.
        if ((Test-Path -Path $UNCPath) -EQ $false) {
            Write-Error -Exception [ItemNotFoundException] -Message "Der Pfad $UNCPath konnte nicht gefunden werden, da er nicht vorhanden ist."
            break
        }
        #Neue XML Config Datei erstellen und unter %Appdata%\Roaming\NMPackageManagement abspeichern (Wenn NMPackageManagement Ordner nicht existiert wird dieser angelegt)
        else {
    
            $prop = @{
                'Location'           = $UNCPath
                'SourceName'         = $SourceName
                'LocalInstallFolder' = "$LocalInstallFolder"
            }
            $NewPackageSource = New-Object -TypeName PSObject -Property $prop
    
            if ((Test-Path $ConfigFileDirectory) -EQ $true) {
                
                #Überprüfung ob Location und SourceName schon in ConfigFile vorkommen
                if ((Test-Path -Path "$ConfigFileDirectory\$ConfigFileName") -EQ $true) {
                    #Importieren der vorhandenen ConfigDatei
                    $ConfigFile = Import-Clixml -Path "$ConfigFileDirectory\$ConfigFileName"

                    
                    foreach ($PackageSource in $ConfigFile) {

                        if ($PackageSource.Location -eq $UNCPath) {
                            
                            Write-Error -Message 'Der Angegebene UNC Pfad existiert beireits als PackageSource' -ErrorAction Stop
                        }
                        elseif ($PackageSource.SourceName -eq $SourceName) {

                            Write-Error -Message 'Der SourceName ist bereits registriert. Bitte anderen Namen wählen.' -ErrorAction Stop
                            
                        }
                        #Hinzufügen der aktuellen PackageSource zur Liste 
                        $PackageSources.Add($PackageSource)

                    }

                    #Hinzufügen der neuen PackageSource zur NMPackageManagement.xml Config Datei
                    $PackageSources.Add($NewPackageSource)

                    #Erstellen und exportieren der neuen NMPackageManagement.xml Config Datei

                    Export-Clixml -InputObject $PackageSources -Path "$ConfigFileDirectory\$ConfigFileName" -Force
                    Write-Output $PackageSources 

                }
                else {
                    #Anlegen der Config Datei, da noch keine vorhanden ist
                    Export-Clixml -InputObject $NewPackageSource -Path "$ConfigFileDirectory\$ConfigFileName"
                    Write-Output $NewPackageSource 
                }
    
            }
            else {
                
                #Erstellen des ConfigFileOrdner und anlegen der Config Datei
                New-Item -Path $ConfigFileDirectory -ItemType Directory
                Export-Clixml -InputObject $NewPackageSource -Path "$ConfigFileDirectory\$ConfigFileName"
                Write-Output $NewPackageSource
    
            } 

        }


    }

}

<#
.SYNOPSIS
Löscht eine NMPackageSource aus der Configdatei.

.DESCRIPTION
Long description

.PARAMETER SourceName
Name der PackageSource

.EXAMPLE
Get-NMPackageSource -SourceName Programs | Unregister-NMPackageSource

.NOTES
General notes
#>
function Unregister-NMPackageSource {

    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        [string]$SourceName
    )

    Begin {
        #Neue Liste für 
        $NewConfigFile = New-Object System.Collections.Generic.List[System.Object]
        $ConfigFile = "$ConfigFileDirectory\$ConfigFileName"
        #Überprüfung ob ConfigFile vorhanden ist.
        if ((Test-Path -Path $ConfigFile) -eq $false) {

            Write-Error -Message "Es wurde keine Konfigurationsdatei unter dem Pfad $ConfigFile gefunden" -ErrorAction Stop
    
        }

    }
    Process {
        $ConfigFile = Import-Clixml $ConfigFile
        $Deleted = $false


        foreach ($PackageSource in $ConfigFile) {

            if ($PackageSource.SourceName -ne $SourceName) {
                
                $NewConfigFile.Add($PackageSource)
            }
            else {
                $Deleted = $true
            }
        }

        if ($Deleted -eq $false) {
            Write-Error -Message "Die PackageSource $SourceName ist nicht registriert und kann deswegen nicht entregistriert werden" -ErrorAction Stop
        }

        try {
            Export-Clixml -Path "$ConfigFileDirectory\$ConfigFileName" -InputObject $NewConfigFile -Force
            if (($NewConfigFile.Count) -eq 0) {
                
                Remove-Item -Path $ConfigFileDirectory\$ConfigFileName
            }
            Write-Output $NewConfigFile
        }
        catch [Exception] {

            Write-Error -Message 'Config Datei konnte nicht erstellt werden' -ErrorAction Stop
            
        }

    }


}

<#
.SYNOPSIS
Zeigt alle Registrierten NMPackageSources an

.DESCRIPTION
Long description

.EXAMPLE
Get-NMPackageSource

.NOTES
General notes
#>#
function Get-NMPackageSource {

    [CmdletBinding()]
    param (
        [Parameter()]
        [string]
        $SourceName = '*'
    )

    $ConfigFile = "$ConfigFileDirectory\$ConfigFileName"
    if ((Test-Path -Path $ConfigFile) -eq $true) {

        $Output = Import-Clixml -Path $ConfigFile

        Write-Output $Output | Where-Object SourceName -Like "$SourceName"

    }
    else {
        Write-Error -Message "Es wurde keine Konfigurationsdatei unter dem Pfad $ConfigFile gefunden" -ErrorAction Stop
    }

}

<#
.SYNOPSIS
Verschiebt eine NMPackageSource zu einem neuen Pfad, nimmt deren NMPackages mit und Passt die XML Dateien der Packages an

.DESCRIPTION
Long description

.PARAMETER SourceName
NMPackageSource, die verschoben werden soll

.PARAMETER NewUncPath
Der neue UNC Pfad, wo NMPackages in zukunft sind.

.EXAMPLE
Get-NMPackageSource -SourceName Programs | Move-NMPackageSource -NewUncPath \\server\freigabe

.NOTES
General notes
#>
function Move-NMPackageSource {

    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        [String]
        $SourceName,
        [Parameter(Mandatory = $true)]
        [String]
        $NewUncPath,
        [Parameter()]
        [switch]$RemoveOldResource

    )

    try {
        $OldUNCPath = (Get-NMPackageSource -SourceName $SourceName).Location
        $OldLocalInstallFolder = (Get-NMPackageSource -SourceName $SourceName).LocalInstallFolder

        #Ändern der NMPackageResource in ConfigFile

        if (!($NewUncPath.StartsWith('\\') -and $NewUncPath.Substring(2).Contains('\') -and (!$NewUncPath.EndsWith('\')))) {
            
            Write-Error -Message "Der Pfad $NewUncPath ist kein UNC-Pfad. Bitte geben Sie eine UNC Freigabe als folgendem Schema an: \\Servername\Freigabename"
            break
        }
        Unregister-NMPackageSource -SourceName $SourceName
        Register-NMPackageSource -SourceName $SourceName -UNCPath $NewUncPath -LocalInstallFolder $OldLocalInstallFolder
        
        #Kopieren der Dateien auf neue Freigabe
        Get-ChildItem -Path "$OldUNCPath" | Copy-Item -Destination "$NewUncPath" -Recurse

        #Anpassen der Metadata.xml Dateien
        $Metadatas = Get-ChildItem -Path $NewUncPath -Filter 'Metadata.xml' -Recurse

        foreach ($Metadata in $Metadatas) {

            $XMLPath = $Metadata.FullName
            $XML = Import-Clixml -Path $XMLPath
            $XML.FileLocation = ($XML.FileLocation.Replace("$OldUNCPath", "$NewUncPath"))
            $XML.FolderPath = ($XML.FolderPath.Replace("$OldUNCPath", "$NewUncPath"))
        
            Write-Output -InputObject $XML
            Export-Clixml -InputObject $XML -Path $XMLPath -Force
        }

        #Anpassen der Dependencies.xml Dateien
        $Dependencies = Get-ChildItem -Path $NewUncPath -Filter 'Dependencies.xml' -Recurse

        foreach ($Dependency in $Dependencies) {

            $XMLPath = $Dependency.FullName
            $XML = Import-Clixml -Path $XMLPath

            foreach ($Data in $XML) {

                $Data.PackageName = ($Data.PackageName.Replace("$OldUNCPath", "$NewUncPath"))

            }
        
            Write-Output -InputObject $XML
            Export-Clixml -InputObject $XML -Path $XMLPath -Force
        }
    }
    catch {

        #Löschen der Kopierten Datein im Fehlerfall
        Get-ChildItem $NewUncPath | Remove-Item -Recurse
        Write-Error -Message 'Es ist ein Fehler aufgetreten' -ErrorAction Stop
        break
    }
    
    #Löschen der NMPackages aus alter NMPackageResource
    if ($RemoveOldResource) {

        Get-ChildItem $OldUNCPath | Remove-Item -Recurse
    }


}


<#
.SYNOPSIS
Durchsucht in den Registrierten NMPackageSources nach bereitgestellten NMPackages

.DESCRIPTION
Long description

.PARAMETER PackageName
Name des NMPackages
.PARAMETER Version
Version des NMPackages

.EXAMPLE
Get-NMPackage -PackageName 7-ZIP -Version 19.00

.NOTES
General notes
#>
function Get-NMPackage {

    [CmdletBinding()]
    param (
        [string]$PackageName = '*',
        [string]$Version = '*'
    )

    $Sources = Get-NMPackageSource
    $NMPackages = New-Object System.Collections.Generic.List[System.Object]

    #Füge alle Programme in Liste hinzu
    foreach ($Source in $Sources) {
        $Source = $Source.Location
        $Packages = Get-ChildItem "$Source\$PackageName" -Filter "Metadata.xml" -Recurse

        foreach ($Package in $Packages) {

            if ($Version -eq '*') {
                
                $NMPackages += Import-Clixml -Path $Package.FullName
            }
            #Alles was mit $Version übereinstimmt der Liste hinzufügen
            elseif ((Import-Clixml -Path $Package.FullName).Version -eq $Version) {
                
                $NMPackages += Import-Clixml -Path $Package.FullName

            }
        }
    }

    Write-Output $NMPackages

}

<#
.SYNOPSIS
Liest die Log Datei einer NMPackageSource aus

.DESCRIPTION
Long description

.PARAMETER FolderPath
Pfad zum Überordner, in dem Setup und Metadata.xml sind

.EXAMPLE
Get-NMPackage -PackageName 7-ZIP -Version 19.00 | Read-NMPackageLog

.NOTES
General notes
#>
function Read-NMPackageLog {
    param 
    (
        [Parameter(ValueFromPipelineByPropertyName, Mandatory = $true)]
        $FolderPath
    )
    
    #Liste in der Objekte mit LogFileObjekten kreiert werden
    $LogFileData = New-Object System.Collections.Generic.List[System.Object]
    #Einlesen des Logfiles
    $LogFileContents = Get-Content -Path "$FolderPath\log\log.txt"
    
    foreach ($LogFileContent in $LogFileContents) {

        $LogFileContent = $LogFileContent.Split(';')
        $prop = @{
            'FunctionName'     = $LogFileContent[0]
            'ComputerName'     = $LogFileContent[1]
            'PackageName'      = $LogFileContent[2]
            'PackageInstalled' = $LogFileContent[3]
            'ExecutionTime'    = $LogFileContent[4]
        }

        $LogFileData.Add((New-Object -TypeName PSObject -Property $prop))


    }

    Write-Output $LogFileData
}
