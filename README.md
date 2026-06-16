<p align="center">
  <a href="https://michelmarcotte.fr">
      <img src="Explorer/Assets/Banner.png" alt="Explorer Banner" width="600" />
  </a>
</p>

## À propos du projet

**Explorer** est un explorateur de fichiers de nouvelle génération conçu pour simplifier la gestion de vos fichiers. Avec une interface intuitive et moderne, il offre une expérience utilisateur fluide et agréable.

### Objectifs

- **Interface propre et récente** : Design épuré basé sur le thème Fluent
- **Exploration simplifiée** : Navigation facile dans vos répertoires
- **Affichage structuré** : Grille de données claire avec tri et filtrage
- **Intégration OCR** : Reconnaissance optique de caractères (en développement) pour une gestion intelligente des fichiers

## Stack technologique

- **Framework** : [Avalonia 12.0.3](https://avaloniaui.net/) - Interface utilisateur multi-plateforme basée sur C#/.NET
- **Langage** : C# avec .NET 10.0
- **Thème** : Avalonia Fluent UI + Semi.Avalonia
- **Architecture** : MVVM avec [CommunityToolkit.MVVM](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- **Injection de dépendances** : Microsoft.Extensions.DependencyInjection
- **Composants** : DataGrid pour l'affichage des fichiers

## Fonctionnalités

### Implémentées

- **Affichage des fichiers et dossiers** : Liste complète avec détails (nom, type, taille, date de modification)
- **Tri intelligent** : Les dossiers s'affichent d'abord, puis les fichiers triés alphabétiquement
- **Formatage de la taille** : Conversion automatique en KB, MB, GB, TB
- **Filtrage** : Masquage automatique des fichiers cachés et système
- **Navigation fluide** : Interface réactive basée sur MVVM

### En développement

- **Intégration OCR** : Reconnaissance de texte dans les images pour une meilleure organisation des fichiers
- **Recherche et filtrage avancé** : Améliorations à venir
- **Statistiques** : Analyse de vos fichiers

## État du développement

Le projet est actuellement en développement actif avec les phases suivantes :

1. **Phase 1** : Explorateur de fichiers fonctionnel
2. **Phase 2** : Intégration OCR (en cours)
3. **Phase 3** : Améliorations UX et nouvelles fonctionnalités
