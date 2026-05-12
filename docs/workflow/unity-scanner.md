# Unity Scanner

Last updated: 2026-05-13

## Purpose

`unity-scanner` is an optional local CLI for summarizing Unity YAML assets before an agent reads them.

Use it when we need a compact view of scenes, prefabs, ScriptableObjects, materials, animator controllers, or GUID references without opening the Unity Editor. It is most useful once `Assets/` contains enough serialized Unity files that raw YAML becomes noisy.

## Relationship To Unity CLI Connector

This project already uses `unity-cli-connector` for live Editor checks. Keep the roles separate:

- `unity-cli-connector`: control the currently open Unity Editor, enter Play Mode, read console logs, take screenshots, run small C# inspection scripts, and verify runtime behavior.
- `unity-scanner`: inspect Unity asset files from disk, summarize serialized structure, search asset names/types, and trace GUID/file references without requiring the Editor.

In short: use `unity-scanner` for static asset structure, and use `unity-cli-connector` for live Editor/runtime evidence.

## Recommended Use In BoxStack

Use `unity-scanner` as a supporting analysis tool, not as a required Unity package dependency.

Good first targets:

- `Assets/Scenes/SampleScene.unity`: confirm scene objects and component references.
- `Assets/Resources/Prototype/BoxStackPrototypeConfig.asset`: inspect stage and tuning data without reading full YAML.
- `Assets/Resources/Prototype/Ui/BoxStackRuntimeTheme.tss`: check whether UI Toolkit resources are present in the expected location.
- Script or asset GUID references: trace which scene or asset files point at a script, config, sprite, or theme.

This is especially useful before larger refactors, prefab extraction, UI asset cleanup, or Addressables migration work.

## Install Notes

The upstream Windows install command downloads the latest release binary and updates the user's `PATH`.

Because that changes the local user environment, do not run the install script automatically. Ask for explicit approval before installing it on this machine.

Upstream repository:

- https://github.com/youngwoocho02/unity-scanner

## Example Commands

List a compact asset tree:

```powershell
unity-scanner list -p C:\unity\BoxStack Assets --depth 3 --limit 80
```

Read the active prototype scene:

```powershell
unity-scanner read -p C:\unity\BoxStack Assets\Scenes\SampleScene.unity --depth 3 --limit 80
```

Read the prototype config asset with a field cap:

```powershell
unity-scanner read -p C:\unity\BoxStack Assets\Resources\Prototype\BoxStackPrototypeConfig.asset --field-limit 80
```

Search by asset name:

```powershell
unity-scanner search -p C:\unity\BoxStack Assets --name BoxStackPrototypeConfig
```

Trace references to a script or asset:

```powershell
unity-scanner refs -p C:\unity\BoxStack Assets\Scripts\Prototype\BoxStackPrototypeConfig.cs Assets --detail
```

## Limitations

`unity-scanner` does not replace Unity validation.

It cannot prove runtime-created objects exist, verify Play Mode warnings, inspect live UI Toolkit layout, test input blocking, or confirm WebGL/mobile behavior. For those checks, continue using Unity Editor Play Mode, `unity-cli-connector`, screenshots, and milestone WebGL/device spot checks.

Treat scanner output as a fast static map of serialized Unity files. Confirm behavior in Unity whenever gameplay, UI, physics, rendering, build inclusion, or App-in-Toss runtime behavior matters.
