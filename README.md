# Windows-Softwareverteilung
Ein Powershell Modul mit dem Software in einer Windows-Domäne verteilt werden kann. 
Eine WPF GUI um neue Softwarepakete erstellen zu können.


# Quick-Start

1. Download Release -> NMPackageManagement.zip 
2. Installiere .NET Core 3.1 (https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
2. Installiere NMPackage-Setup.msi

## Importiere Powershell-Modul
- Achtung: Es müssem sowohl die Datei NMPackageManagement.ps1 also auch die NMPackageManagement.psm1 im selben Ordner liegen.

```powershell
Import-Module C:\Temp\NMPackageManagement.psm1
```

## Funktionen des Moduls anzeigen

```PowerShell
Get-Command -Module NMPackageManagement
```
![grafik](https://user-images.githubusercontent.com/72456947/214558281-6bb4c445-76d6-4e9e-9377-d6bc628f02b3.png)

## Eine neue PackageSource einrichten
- Hierfür muss eine Freigabe eingerichtet werden. (z.B.: \\server1\software)
- Die zu verteilenden Softwarepakete werden bei der Paketerstellung auf dieser Freigabe gespeichert und verwaltet

```Powershell
Register-NMPackageSource -UNCPath '\\dc1\Software' -SourceName 'MyPrograms'
```

## Ein neues Softwarepaket bereitstellen
Achtung: Das zu veröffentlichende Softwarepaket sollte bereits auf dem Rechner installiert sein.
Als Beispiel wird hier das Softwarepaket für Notepad++ erstellt

Hierfür Das installierte Tool "Add NM Package" öffnen
- Eine PackageSource wählen (Hier wird das Softwarepaket veröffentlicht)
- Die benötigte Architektur auswählen
-  Über Neuer Ordner kann ein neues Softwarepakt angelegt werden.
![grafik](https://user-images.githubusercontent.com/72456947/214571092-d2e3e2ec-f292-480c-b1d3-0e9d9ae0eeee.png)
- Den neu angelegten Ordner nun auswählen
- Auf Durchsuche Registry klicken
- - Es erscheint eine Liste mit installierter Software
- - - Bei Ordner auswählen '*' auswählen um komplette Softwareliste anzuzeigen
- - Software per Doppelklick nun auswählen
- - Parameter für die leise Installation hinzufügen
- - Parameter für die leise Deinstallation hinzufügen

![grafik](https://user-images.githubusercontent.com/72456947/214571959-617d29b0-5e6d-4921-a14f-9fa3ebd6a223.png)

Das Softwarepaket ist jetzt vorbereitet

## Verfügbare Softwarepakete anzeigen

```Powershell
Get-NMPackage -PackageName 'Notepad++'
```

## Paket auf einem Remote Computer installieren
- Auf dem Remote Computer muss PSRemoting aktiviert sein.

```Powershell
 Get-NMPackage -PackageName 'Notepad++' -Version '8.4.8' | Install-NMPackage -ComputerName asterix
```
- Es erscheint eine Zusammenfassung
![grafik](https://user-images.githubusercontent.com/72456947/214573223-e9a925f3-a826-4345-aac5-f22fd850edee.png)

## Paket auf einem Remote Computer deinstallieren

```Powershell
 Get-NMPackage -PackageName 'Notepad++' -Version '8.4.8' | Uninstall-NMPackage -ComputerName asterix
```

## Überprüfen ob das Softwarepaket auf dem Computer installiert ist

```Powershell
 Get-NMPackage -PackageName 'Notepad++' -Version '8.4.8' | Test-NMPackage -ComputerName asterix
```

## Log auslesen - Wo und wann wurde das Softwarepaket installiert bzw. deinstalliert.

```Powershell
Get-NMPackage -PackageName 'Notepad++' -Version '8.4.8' | Read-NMPackageLog
```

## Einen PC auf Software durchsuchen
- Sucht nach Software die das Wort Notepad im Namen hat.

```Powershell
Search-Package *notepad* -ComputerName asterix
```

