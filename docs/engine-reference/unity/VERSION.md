# Unity Engine Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity 6000.0.73f1 |
| **Editor Revision** | a166abc3bf0e |
| **Unity Series** | Unity 6.0 |
| **Project Pinned** | 2026-02-13 |
| **Last Docs Verified** | 2026-06-08 |
| **LLM Knowledge Cutoff** | May 2025 |

## Knowledge Gap Warning

The LLM's training data likely covers Unity up to ~2022 LTS (2022.3). The entire
Unity 6 release series introduced significant changes that the model does NOT
know about. Always cross-reference this directory and
`ProjectSettings/ProjectVersion.txt` before suggesting Unity API calls.

## Project Version Source

The authoritative project version is `ProjectSettings/ProjectVersion.txt`:

```text
m_EditorVersion: 6000.0.73f1
m_EditorVersionWithRevision: 6000.0.73f1 (a166abc3bf0e)
```

Do not assume APIs or behavior from later Unity 6 releases unless the project is
explicitly upgraded and this file is updated alongside
`ProjectSettings/ProjectVersion.txt`.

## Post-Cutoff Version Timeline

| Version | Release | Risk Level | Key Theme |
|---------|---------|------------|-----------|
| 6.0 | Oct 2024 | HIGH | Unity 6 rebrand, new rendering features, Entities 1.3, DOTS improvements |

## Major Changes from 2022 LTS to Unity 6

### Breaking Changes
- **Entities/DOTS**: Major API overhaul in Entities 1.0+, complete redesign of ECS patterns
- **Input System**: Legacy Input Manager deprecated, new Input System is default
- **Rendering**: URP/HDRP significant upgrades, SRP Batcher improvements
- **Addressables**: Asset management workflow changes
- **Scripting**: C# 9 support, new API patterns

### New Features (Post-Cutoff)
- **DOTS**: Production-ready Entity Component System (Entities 1.3+)
- **Graphics**: Enhanced URP/HDRP pipelines, GPU Resident Drawer
- **Multiplayer**: Netcode for GameObjects improvements
- **UI Toolkit**: Production-ready for runtime UI
- **Async Asset Loading**: Improved Addressables performance
- **Web**: WebGPU support

### Deprecated Systems
- **Legacy Input Manager**: Use new Input System package
- **Legacy Particle System**: Use Visual Effect Graph
- **UGUI**: Still supported, but UI Toolkit recommended for new projects
- **Old ECS (GameObjectEntity)**: Replaced by modern DOTS/Entities

## Verified Sources

- Project version file: `ProjectSettings/ProjectVersion.txt`
- Official docs: https://docs.unity3d.com/6000.0/Documentation/Manual/index.html
- Unity 6 release: https://unity.com/releases/unity-6
- Migration guide: https://docs.unity3d.com/6000.0/Documentation/Manual/upgrade-guides.html
- Unity 6 support: https://unity.com/releases/unity-6/support
- C# API reference: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/index.html
