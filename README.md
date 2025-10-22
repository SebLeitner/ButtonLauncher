# Button Launcher

Ein kleines WPF-Tool, das häufig genutzte Aufgaben über konfigurierbare Buttons ausführbar macht.

## Projektstruktur

- `ButtonLauncher.sln` – Visual-Studio-Lösung
- `ButtonLauncherApp/` – WPF-Anwendung (.NET 7)
  - `ButtonLauncherApp.csproj` – Projektdatei
  - `App.xaml` / `App.xaml.cs` – Anwendungseinstieg
  - `MainWindow.xaml` / `MainWindow.xaml.cs` – Benutzeroberfläche mit dynamisch erzeugten Buttons
  - `Models/` – Klassen zur Abbildung der JSON-Konfiguration
  - `Services/` – Laden der Konfiguration, Logging sowie Aktionsausführung
  - `buttons.json` – Beispielkonfiguration, wird beim Start automatisch geladen

## Nutzung

1. Projekt in Visual Studio 2022 (oder neuer) öffnen.
2. `buttons.json` anpassen und im Projektstamm belassen. Änderungen werden zur Laufzeit erkannt und nach einem Klick auf „Neu laden“ oder automatisch nach Speichern neu eingelesen.
3. Anwendung starten (`F5`).
4. Auf einen Button klicken, um die konfigurierte Aktion auszuführen.

Unterstützte Aktionen:

- Explorer mit einem Pfad öffnen
- EXE- oder BAT-Datei ausführen (optional mit Administratorrechten)
- PowerShell-Skript starten (`-ExecutionPolicy Bypass`)
- Text in die Zwischenablage kopieren
- URL in Firefox (oder ersatzweise im Standardbrowser) öffnen

Fehler werden in `logs/button-launcher.log` protokolliert.
