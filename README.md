# EthicalHacker

EthicalHacker is an educational cybersecurity game built with Unity. Players follow scenario stages through dialogue, decisions, questions, skill unlocks, and interactions on a globe. The project includes progress tracking, three save slots, and a desktop content editor.

**Additional local resources are required before opening or running this project.** The globe integration, world map textures, and sphere mesh are included, but some globe package assets must be supplied separately. See [Local resource setup](#local-resource-setup). Unity import, compilation, builds, and gameplay have not been validated for this distribution.

## Requirements

- Unity **6000.3.11f1**, as specified in `ProjectSettings/ProjectVersion.txt`.
- Unity Hub platform modules for the builds you intend to create.
- Python **3.10 or newer** for the resource verification tool; no third-party Python packages are required.
- The separately supplied resources described below.

Unity Package Manager resolves dependencies from `Packages/manifest.json` and `Packages/packages-lock.json`, including uGUI 2.0.0 and Newtonsoft JSON 3.2.2. Keep the lock file when sharing the project.

## Included dependencies

| Component | Version or reference | Included functionality |
| --- | --- | --- |
| Runtime Inspector & Hierarchy | [v1.7.4](https://github.com/yasirkula/UnityRuntimeInspector/tree/v1.7.4) | Runtime scenario editing, including ColorWheel |
| Unity3DGlobe | [Publisher repository](https://github.com/Dandarawy/Unity3D-Globe), revision `e3e6dbb75dfec2c997b552a4a79033c6e8c9f611` | Selected scripts, shaders, and point mesh; additional assets required |
| TextMesh Pro resources | Unity 6000.3.11f1 / uGUI 2.0.0 | Text shaders, settings, and Liberation Sans font resources |

Required third-party notices are included in [Assets/StreamingAssets/ThirdPartyNotices](Assets/StreamingAssets/ThirdPartyNotices/NOTICE.md), so they also accompany builds that include StreamingAssets. Keep these files when distributing the included dependencies. The EmojiOne sample atlas is not included or selected as the default TMP sprite asset.

## Local resource setup

The project retains references to these resources so that an authorized local copy can restore the existing scenes without manual GUID replacement:

| Resource group | Expected source/layout |
| --- | --- |
| Globe package assets | Globe - Data Visualizer **1.0** by Abdullah Aldandarawy, Asset Store product **80008**, imported under `Assets/_ASSETS/Unity3DGlobe` |

The ten separately supplied globe assets include the imported camera script, materials, prefab, animations, and sample-scene resources. The current GitHub globe checkout is **not** an exact replacement for the imported Asset Store package. Availability of that historical store release has not been verified. Obtain a compatible, authorized local resource set from your existing package archive or the project maintainer; do not substitute a newer version silently.

Arrange the supplied files under a separate directory containing the `Assets/` paths listed in `Tools/local-resources.json`. The tool checks file presence and the metadata GUIDs required by the project's references. Differences in file contents, line endings, and importer settings are accepted. The manifest's SHA-256 values record the original baseline only; they are not enforced. These checks do not establish runtime compatibility or permission to redistribute files.

Run from the project root:

```powershell
# Inspect the current installation. Missing resources produce a nonzero exit code.
python Tools/setup_resources.py --check

# Validate all supplied files without changing the project.
python Tools/setup_resources.py --from "C:/LocalResources/EthicalHacker"

# Restore validated missing files, then verify the result.
python Tools/setup_resources.py --from "C:/LocalResources/EthicalHacker" --apply
python Tools/setup_resources.py --check
```

The tool performs no downloads and never overwrites existing files. It checks all required resources before copying missing files, preserves metadata, and can be run repeatedly. A GUID mismatch requires checking the package's metadata and scene references; file presence alone does not guarantee compatibility. Local-only paths are listed in `.gitignore` to prevent accidental inclusion in commits. Restore resources before opening the project in Unity, because importing incomplete assets may modify serialized references.

## Run and play

1. Complete local resource setup.
2. Add this project folder in Unity Hub and open it with Unity 6000.3.11f1. Allow Package Manager to resolve dependencies.
3. Open `Assets/_GAME/Scenes/MainMenu.unity` and enter Play Mode.
4. Choose Play, select an empty save slot, and select a scenario. Existing slots resume saved progress.

Drag with the left mouse button over the globe to rotate it. Use the on-screen buttons to advance dialogue, answer questions, make decisions, and interact with available markers. Progress is stored in Unity `PlayerPrefs` on the current device/browser. Local saves record their content location; moving content may prevent those saves from loading.

The base pack contains eight scenarios: reconnaissance, social engineering, IoT, network scanning, vulnerability analysis, password cracking, dark web threat intelligence, and insider threats. `ExtraContent` contains a cloud security scenario.

## Content and editing

See the [Content Editor Guide (PDF)](Docs/EthicalHacker_Content_Editor_Guide.pdf) for instructions on authoring and publishing content packs.

Content packs live under `Assets/StreamingAssets`. Each pack contains a `manifest.json` listing its scenario JSON files. On desktop, the game discovers local pack folders. The online-content input accepts a directly accessible pack URL or a GitHub repository/tree URL; a repository URL without a branch uses `main`. The loader does not authenticate to private GitHub repositories.

For WebGL, hosted content must be accessible to the browser over HTTPS with appropriate CORS headers. Only BaseContent is loaded automatically in the non-desktop code path; additional packs can be loaded through the online-content input.

Open `Assets/_GAME/Scenes/EditorScene.unity` directly in the Unity Editor to edit an existing local pack. The desktop build also exposes the editor through its menu. Menu visibility depends on platform compilation symbols, so the Editor Play Mode menu does not exactly match the desktop build menu. The editor saves into the content pack directory; keep backups and ensure the folder is writable. The create-new-pack action is currently a stub: create a pack folder and manifest yourself before opening it.

Scenario JSON files are the source of truth. Each pack's manifest lists the scenarios available to the game.

## Builds

Use Unity's Build Profiles window and select the supplied Windows, Linux, macOS, or Web - Desktop - Release profile under `Assets/Settings/Build Profiles`. Install the corresponding platform module in Unity Hub and select a local output folder such as `Builds/Windows` or `Builds/Web`.

Desktop profiles use the global scene list: MainMenu, GameGlobe, and EditorScene. The Web profile overrides it with MainMenu and GameGlobe. WebGL does not provide local pack editing. Serve a WebGL build over HTTP(S); it is not intended to be opened directly through `file://`. Configure your host for Unity's generated build/compression settings.

## Known limitations

- This distribution requires the listed local resources and is not a standalone ready-to-run checkout.
- Static file, JSON, resource-hash, and reference checks do not establish successful Unity import, compilation, platform builds, or gameplay.
- The open-content-folder actions use Windows-specific behavior; Linux and macOS behavior is not validated.
- Content loading failures may only be reported in logs, and the content editor has limited error recovery. Validate and back up edited packs.
- Save compatibility depends on the original content source and scenario identifiers. Browser storage can be cleared by the browser or user.
