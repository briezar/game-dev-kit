# GameDevKit

A collection of reusable utilities, systems, and editor tools for Unity, built to accelerate development and reduce boilerplate across projects.

## Features

### UI Manager
- Ready-to-use `UIManager` prefab.
- Type-safe API for opening and managing UI.
- Automatically sync UI prefab paths within the `Resources` folder, no magic strings.

```csharp
UIManager.ShowUI<MyPopup>();
```

### ResetOnExitPlayMode

Save changes made to a `ScriptableObject` during Play Mode and automatically restore its original state when exiting Play Mode.

Useful for testing, balancing, or using runtime `ScriptableObject`s without accidentally persisting changes to assets.

### SourcedAction

An alternative to `System.Action` that tracks event subscribers by source object.

Benefits:
- Unsubscribe all callbacks from a specific source.
- No need to keep delegate references.
- Reduces event subscription leaks.

```csharp
action[this] += OnClicked;
action.UnsubscribeSource(this);
```

### Components & Utilities

Common reusable components and helpers, including:

- MaterialSwapper
- LifeCycleHook
- CameraFacer
- CanvasCameraFinder
- CameraAspectRatioAdapter
- Utility extensions
- Editor utilities

## Installation

### Dependencies

The package will automatically prompt to install required dependencies:

- UniTask
- EditorAttributes
- PrimeTween
- SerializedDictionary

### Unity Package Manager

Add the package using the Git URL:

```
https://github.com/briezar/game-dev-kit.git
```