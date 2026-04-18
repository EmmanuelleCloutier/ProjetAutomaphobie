# Simulacres — UI Package
## Quest 3 / OpenXR / UI Toolkit

---

### Structure des fichiers

```
UXML/
  MenuTitre.uxml      — Écran d'accueil
  MenuPause.uxml      — Pause in-game
  MenuFin.uxml        — Résultats fin de partie

USS/
  SimulacresMenus.uss — Feuille de style partagée (toutes les classes)

Scripts/
  MenuManager.cs      — Contrôleur principal des menus
```

---

### Setup dans Unity (étapes dans l'ordre)

#### 1. Packages requis
- `com.unity.xr.openxr` ≥ 1.9
- `com.unity.xr.interaction.toolkit` ≥ 2.5
- `com.unity.ui` (UI Toolkit, inclus depuis Unity 2021.2)

#### 2. PanelSettings (World Space)
1. `Assets > Create > UI Toolkit > Panel Settings`
2. Renommer : `SimulacresPanelSettings`
3. Paramètres critiques :
   - **Scale Mode** : `Constant Pixel Size`
   - **Scale** : `1`
   - **Target Texture** : laisser vide (render direct sur RenderTexture géré par UIDocument)
   - **Reference Resolution** : 1080 × 1350

#### 3. RenderTexture
1. `Assets > Create > Render Texture`
2. Renommer : `MenuRenderTexture`
3. Size : **1080 × 1350**, Depth : **24**, Format : **ARGB32**
4. Assigner dans `SimulacresPanelSettings > Target Texture`

#### 4. GameObject MenuPanel
1. Créer un `GameObject` vide dans la scène : `MenuPanel`
2. Ajouter **UIDocument** :
   - `Panel Settings` → SimulacresPanelSettings
   - `Source Asset` → MenuTitre.uxml (par défaut ; le script swap au runtime)
3. Ajouter **TrackedDeviceGraphicRaycaster** (requis pour XR ray-cast sur UI Toolkit)
4. Ajouter **MenuManager** (ce script)
5. Ajouter un **MeshRenderer + Quad** enfant pour afficher la RenderTexture dans le monde 3D :
   - Scale du Quad : `(1.08, 1.35, 1)` pour correspondre à 1px = 1mm
   - Material : Unlit/Texture, assigner `MenuRenderTexture`

#### 5. EventSystem
L'EventSystem de la scène doit avoir :
- **XRUIInputModule** (remplace StandaloneInputModule pour XR)
- Assigner les `XRRayInteractor` gauche/droit dans les champs correspondants

#### 6. USS — lier la feuille de style
Dans chaque fichier UXML, ajouter dans le `<ui:UXML>` root :
```xml
<Style src="../USS/SimulacresMenus.uss" />
```
Ou assigner via l'Inspector du UIDocument > `Style Sheets`.

#### 7. Références Inspector sur MenuManager
| Champ | Valeur |
|---|---|
| Ui Document | le UIDocument du GameObject |
| Menu Titre Asset | MenuTitre.uxml |
| Menu Pause Asset | MenuPause.uxml |
| Menu Fin Asset | MenuFin.uxml |
| Panel Distance | 1.8 (mètres) |
| Panel Height | 0.9 (mètres) |
| Xr Camera Transform | XR Origin > Camera Offset > Main Camera |

---

### Utilisation depuis le GameManager

```csharp
// Afficher la pause avec les données actuelles
menuManager.ShowMenuPause(
    wagonActuel: 2,
    wagonTotal: 3,
    anxieteNormalized: 0.62f   // 0.0 → 1.0
);

// Afficher l'écran de fin
menuManager.ShowMenuFin(
    wagons: 3,
    anxieteMax: 0.89f,
    tempsSecondes: 514f,
    mannequinsApproches: 7,
    mannequinsTotal: 11,
    freezeCount: 3,
    clesTrouvees: 3,
    clesTotal: 3
);
```

---

### Notes Quest 3 spécifiques

- **Boutons** : hauteur minimum 88–100px (≈ 9cm dans le monde 3D à scale 0.001).  
  Les `TrackedDeviceGraphicRaycaster` gèrent le ray-cast des manettes automatiquement.
- **Lisibilité** : polices ≥ 20px dans le USS (résolution effective Quest 3 ~22 PPD).
- **Foveated rendering** : éviter du texte fin dans les coins — les éléments critiques  
  (boutons, barres) sont centrés dans la zone de rendu optimale.
- **Time.timeScale = 0** : tous les menus le gèrent. Si des coroutines sont actives  
  pendant la pause, utiliser `WaitForSecondsRealtime` au lieu de `WaitForSeconds`.
- **Font monospace** : importer une police `.ttf` monospace dans `Assets/Fonts/`  
  et créer un `Font Asset` (Window > TextMeshPro > Font Asset Creator si tu utilises TMP,  
  ou assigner directement dans USS via `-unity-font-definition`).
