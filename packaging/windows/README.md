# Packaging Windows

Ce dossier contient tout ce qu'il faut pour générer un installateur Windows
classique (`ExplorerSetup.exe`) via [Inno Setup](https://jrsoftware.org/isinfo.php).

Le pipeline complet est automatisé par
[`.github/workflows/release-windows.yml`](../../.github/workflows/release-windows.yml),
déclenché à chaque tag `v*.*.*` (ex: `v1.2.0`) ou manuellement (onglet
*Actions* > *Release Windows* > *Run workflow*).

## Ce que fait le workflow

1. `dotnet publish` en `win-x64`, self-contained, single-file (config déjà
   présente dans `Explorer.csproj`).
2. Compile `Explorer.iss` avec Inno Setup (préinstallé sur les runners GitHub
   `windows-latest`) pour produire `ExplorerSetup.exe`.
3. Crée aussi un `.zip` portable (sans installateur) en secours.
4. Sur un tag, publie une Release GitHub avec les deux fichiers.
   Sur un déclenchement manuel, les dépose en artefacts du run.

Aucune clé ni certificat n'est nécessaire : c'est un installateur classique,
pas un paquet MSIX.

## Installation (utilisateur final)

Aucune commande, aucun PowerShell, aucune manipulation technique : uniquement
des clics.

1. Télécharger `ExplorerSetup.exe` depuis la
   [page des releases](../../../../releases).
2. Double-cliquer dessus.
3. Si Windows SmartScreen affiche *"Windows a protégé votre ordinateur"* :
   cliquer sur *Informations complémentaires* puis *Exécuter quand même*
   (normal pour un exécutable non signé — voir "Limite connue" plus bas).
4. Suivre l'assistant d'installation (Suivant, Suivant, Installer).

C'est tout. La désinstallation se fait normalement, depuis *Paramètres >
Applications*, comme n'importe quel autre logiciel.

## Ce que fait l'installateur

- Installe l'application dans `Program Files\Explorer`.
- Ajoute un raccourci dans le menu Démarrer (+ Bureau, optionnel).
- Enregistre une entrée dans **Paramètres > Applications** (et le panneau de
  configuration *Programmes et fonctionnalités*), avec le bon nom, la bonne
  version et la bonne icône.
- La désinstallation supprime tous les fichiers du dossier d'installation.

## Icônes

L'icône affichée dans l'exécutable, les raccourcis, la barre des tâches et la
liste de désinstallation vient de `Explorer/Assets/Logo.ico`, embarquée
directement dans `Explorer.exe` via `<ApplicationIcon>` (`Explorer.csproj`).
Il suffit de remplacer ce fichier `.ico` pour changer le logo partout.

## Limite connue

L'installateur n'étant pas signé (aucun certificat de signature de code),
Windows SmartScreen peut afficher un avertissement *"Windows a protégé votre
ordinateur"* au premier lancement — il suffit de cliquer sur *Informations
complémentaires* puis *Exécuter quand même*. Pour supprimer cet avertissement,
il faudrait un certificat de signature de code payant (DigiCert, SSL.com,
Azure Trusted Signing...), ce qui n'est pas nécessaire pour une installation
basique.

## Compiler localement (optionnel)

```powershell
dotnet publish Explorer/Explorer.csproj -c Release -r win-x64 --self-contained true -o publish
iscc packaging\windows\Explorer.iss /DAppVersion=1.0.0
# -> dist\ExplorerSetup.exe
```
