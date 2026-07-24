# CLAUDE.md

Guide pour travailler sur ce dépôt. À lire avant toute modification.

## Commandes

Toujours se placer dans le bon sous-dossier.

- **Build :** `dotnet build`
- **Run :** `dotnet run --project Explorer`
- **Clean :** `dotnet clean`
- **Mises à jour :** `dotnet-outdated`

Il n'y a pas de projet de tests dans la solution actuellement.

## Stack Technique

- **Runtime :** .NET 10 (C# 14)
- **UI :** AvaloniaUI v12 (Multiplateforme : Windows, macOS, Linux)
- **MVVM :** CommunityToolkit.Mvvm
- **DI :** Microsoft.Extensions.DependencyInjection
- **Thèmes :** Semi.Avalonia (locale `fr-FR`) + Avalonia.Themes.Fluent
- **Icônes :** Svg.Controls.Skia.Avalonia (SVG dans `Assets/Icons`)

## Architecture & MVVM Strict

Le projet suit scrupuleusement le pattern MVVM. Aucune logique UI/visuelle ne doit résider dans les ViewModels.

- **Views :** Fichiers `.axaml` et `.axaml.cs` (`UserControl` ou `Window`), dans `Views/` (pages) et `Controls/` (
  composants réutilisables : `NavBar`, `AsideLeft`, `AsideRight`, `DataGrid`, `Chrome`, `Footer`, `PathEditor`,
  `FileIcon`).
- **Composants :** Les petits composants réutilisables doivent se trouver dans le dossier `Controls/Primitives`.
- **ViewModels :** Héritent obligatoirement de `ViewModelBase` (dans `ViewModels/`).
- **Génération MVVM :** Utiliser exclusivement les attributs du toolkit (`[ObservableProperty]`, `[RelayCommand]`) sur
  des classes `partial`.
- **Services :** Logique métier stateless/partagée dans `Services/` (ex. `FileSystemService` pour l'accès disque,
  `NavigationService` pour l'historique de navigation, `FileTypeColorsService`).
- **Models :** POCOs/records dans `Models/` (ex. `FileSystemEntry`, `DriveItem`), plus les messages échangés via le
  messenger (ex. `CurrentPathChangedMessage`).

## Styles & XAML

- **Globaux :** Définis dans `App.axaml` et regroupés dans le dossier `Theme/` (`Theme/Styles` pour les
  couleurs/polices, `Theme/Resources` pour les styles de contrôles comme `DataGrid.axaml`, `Checkbox.axaml`).
- **Locaux :** Les styles spécifiques à un contrôle doivent être déclarés directement dans son fichier `.axaml`.
- **Binding :** Préférer les liaisons fortement typées avec `x:DataType`.

## Règles de Code & Nommage

- **PascalCase :** Classes, méthodes, propriétés publiques.
- **camelCase avec `_` :** Champs privés (ex: `_fileSystemService`).
- **Syntaxe :** Utiliser `var` uniquement lorsque le type est évident à la lecture, sinon préférer le type explicite.

## Convention de commit

Les messages de commit suivent le format `PREFIX: description au participe/infinitif` (ex:
`FEAT: Add PathEditor control for editable path navigation`). Préfixes observés : `FEAT` (nouvelle fonctionnalité),
`REFAC` (refactorisation), `STYLE` (changements visuels/XAML uniquement).

## Règles de génération de code

- **Commentaires** : Aucun commentaire dans le code généré, le code doit être auto-explicatif.

## Règle générale

Tu n'as aucunement le droit de killer le processus de l'application depuis le code, depuis une commande ni de forcer la
fermeture d'une fenêtre. Toute action de fermeture doit être initiée par l'utilisateur via l'UI.