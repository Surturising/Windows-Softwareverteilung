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
-  ![grafik](https://user-images.githubusercontent.com/72456947/214571092-d2e3e2ec-f292-480c-b1d3-0e9d9ae0eeee.png)




