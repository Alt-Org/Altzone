# 3.0.1

## Stable

### Build 1599 (Dec 05, 2024)

**Bug Fixes**

- Fixed: An issue that caused exceptions when `SceneViewComponents` were destroyed before the `EntityViewUpdater` had a chance to add them properly
- Fixed: An issue with supporting new and legacy InputSystem by introducing the `QuantumUnityInputSystemWithLegacyFallback` script
- Fixed: `DynamicMap.RemoveCollider2D` now properly updates `StaticData.ColliderIndex` of moved colliders

### Build 1595 (Dec 03, 2024)

**Bug Fixes**

- Fixed: A warning shown in Unity about using obsolete API (`EnableTaskProfiler`)
- Fixed: An issue that the `QuantumStats` prefab was using a Quantum logo from the menu package
- Fixed: `QuantumDotnetProjectSettings` - support for `Packages/` paths

### Build 1594 (Nov 29, 2024)

**What's New**

- Added a text viewer for Quantum QTN assets in the Unity inspector
- Added support for the `[OnlyInPrototype]` attribute - applied to a field will not include the field in the state object, only in its prototype, an alternative approach is to apply it to a type in `[OnlyInPrototype("fieldType", "fieldName")]`
- Added support for `ref` parameters in Quantum signal arguments
- Added support for using `input` in DSL `structs` and components
- Added `Frame.AddAsset(AssetObject, AssetGuid)` and `DynamicAssetDB.AddAsset(AssetObject, AssetGuid)` - a way of adding dynamic assets with a GUID known ahead of time, the GUID needs to be of `AssetGuidType.DynamicExplicit` type
- Added support for multithreading in WebGL if enabled in Unity Editor 6+
- The Add-Entity-Component menu on `QuantumEntityPrototype` is now sorted alphabetically and searchable
- Added a demo input unitypackage that can be used to quickly setup Quantum input for most common game genres
- `QuantumEditorSettings` "Enable Task Profiler" build feature - enables task profiler in builds. Behind the scenes, all it does is it adds `QUANTUM_REMOTE_PROFILER` define
- `QuantumTaskProfilerWindow` will now display `QUANTUM_REMOTE_PROFILER not defined` message in source selection dropdown, if the define is not enabled for the current platform
- The demo menu now listens to session shutdown events to be able to react on unexpected errors
- The Quantum graph profiler and the Quantum stats prefabs can now be added to scene using the Quantum menu
- Added a simple navmesh region import option that directly maps Unity navmesh regions to Quantum navmesh regions, this mode is easy to set up, but the maximum number of regions is limited to 30. Toggle the `ImportRegionMode` on the navmesh script. The `ImportRegion` field is marked as obsolete and will automatically be converted to `Advanced` mode for projects that are upgraded
- Added new `structs` `IntVector2` and `IntVector3` to the Quantum math libraries which can also be used inside the DSL
- Added an optional `maxLength` param to `BitStream.SerializeArray()` and `SerializeArrayLength`, it can be used to secure for example command deserialization against unwanted large buffer allocations
- Decoding and encoding can now be done using `DeterministicCommandSerializer.TryDecodeCommand()` and `EncodeCommand()` methods
- Added `RotateAround()` method to the `Transform3D`
- Added `GetIterator()` method to the `RingBuffer<T>` class
- Added `CreateRuntimeDeterministicGuid()` method to the `QuantumUnityDB` class to be able to create deterministic Asset GUIDS used by static assets added at runtime to the DB
- `QuantumUnityDB.AddAsset()` will now generate a deterministic GUID if none was provided
- Added the `Capacity` property to 2D and 3D compound shapes
- Added `RemoveAllColliders3D()` and `RemoveAllColliders2D()` to `DynamicMap` to remove all colliders in one shot
- Added Gizmo toggle for debug draw calls (i.e. Draw.Box)
- Added `TriangleCCW.VertexHashMapper`, a helper class that can be used to serialize triangles with shared vertices to a `ByteStream` more efficiently
- Added `ByteSerializerHashMapper<T>`, a helper class that can be used in combination with a `ByteSerializer` to avoid fully serializing multiple instances of the same value
- Added `DynamicMap.SerializeRuntimeTriangles` and `.DeserializeRuntimeTriangles` virtual methods for custom (de-)serialization of runtime triangles
- Added `DynamicMap.TriangleCcwSerializer` type, an improved triangle serializer for built-in or custom serialization
- Added `DynamicMap.TriangleSerializer` member, an improved triangle serializer for built-in or custom serialization
- Added an API to `DynamicMap` to mass reserve colliders and remove triangles

**Changes**

- `QuantumGame.ProfilerSampleGenerated` is now obsolete and no longer functional. Use the new callback (`CallbackTaskProfilerReportGenerated`) instead
- The `QuantumEntityViewComponent` does not receive update callbacks anymore when the `GameObject` or `Behavior` is disabled
- Upgraded to Photon Realtime version `5.1.2`, see `changes-realtime.txt` for more details, the updates introduces debug and release dlls and it's best to delete the `Photon\PhotonLibs` folder before upgrading
- Systems in `SystemsConfig` now display system type and name instead of `Element <index>`
- Removing `TRACE` defines from release dLLs
- `QuantumMapDataBaker.UpdateManagedReferenceIds()` method is now public, allowing the creation of new entity prototypes during custom map-baking callbacks
- `QEnums` are now drawn with Odin-style drop-down
- `QuantumUnityLogger` moved to `Quantum.Log` assembly, use partial `QuantumLogInitializer.InitializeUnityLoggerUser` to customize
- Added `QUANTUM_LOGLEVEL_*` conditional attributes to `Log` static methods, which means that logs below the current log level will be compiled out completely
- `QUANTUM_LOGLEVEL_TRACE` is now obsolete. To enabled trace logs, add `QUANTUM_TRACE_*` defines or use `QuantumEditorSettings` editor
- The `QuantumStats` UI got a visual upgrade
- Debug draw (e.g. `Draw.Sphere()` now respects the scene and game view gizmo toggles
- Clicking `QuickMatch` in the sample menu now opens a popup when missing the Photon AppId
- Unity navmesh import now ignores disabled or deactivated `NavMeshLinks` and `OffmeshLinks`
- Improved the Quantum navmesh baking performance, for example FallbackTriangle generation is now optional by setting `NavMeshBakeDataFindClosestTriangle.None`
- The Quantum Hub is now data-driven and can be used by Quantum sample projects
- `QuantumEditorMenuDllToggle` now has key methods and properties exposed as public
- Increased the size of a button on the in-game menu to be more usable on mobile devices
- Reduced the serialization size of triangles in a DynamicMap

**Bug Fixes**

- Fixed: Dotnet generate project and build on Mac
- Fixed: An issue that caused the `ActorId` of clients displayed as `0` on frame snapshots when using sever simulation
- Fixed: An issue that caused the layer import buttons to not be visible
- Fixed: An issue that caused creating BinaryAsset spams log with errors, until the file is saved
- Fixed: An issue that caused the `FrameTimer.IsRunning()` returning `true` when it's invalid
- Fixed: An issue that caused the predicted frames to be used for initial entity view placement and `Activate(Frame)` callbacks when verified bind behavior was selected
- Fixed: An issue that caused an error log flood when deleting an entity view controlled by the EntityViewUpdater even when setting `ManualDisposal`
- Fixed: An issue that spammed error logs when a disconnect was not detected and the online simulation kept running
- Fixed: Performance of `ShapeConfigDrawer`, `QuantumEntityPrototypeEditor` and `QuantumEntityViewEditor` improved
- Fixed: The Asteroids demo now works correctly with HDRP and URP
- Fixed: Ensure that Asteroids renders correctly in builds for all render pipelines
- Fixed: An issue in the instant replay demo with the order of cleanup steps that caused the view to dispose before the `OnReplayStopped` callback
- Fixed: `EntityPrototype had unsorted components` trace message when loading prototypes and maps
- Fixed: `QDictionary` and `QHashSet` no longer throw `OverflowException` in case their respective Key and Value types have a negative hash code and numeric casts are `checked` by default
- Fixed: An issue in the 2.1 migration for upgrading AssetObjects to only consider paths from `QuantumEditorSettings.AssetSearchPaths`
- Fixed: 2D Edge-Edge false-positive collisions and wrong de-penetration
- Fixed: Exceptions in the 2D Physics Engine when using polygons inside compound shapes
- Fixed: An issue in `PhysicsEngine2D.Api.CheckOverlap()` that modified the rotation of its argument if the shape had a rotation offset
- Fixed: An issue in the collision detection of two 3D capsules that resulted in different `Extents` to be used
- Fixed: An issue that could cause incorrect normal vector computation for capsule-box collision 2D
- Fixed: An issue that was accepting negative values for capsule 2D
- Fixed: The drawing of capsules 2D when the extent is greater than the radius
- Fixed: The penetration computation between two capsules 2D using the radius of the two capsules
- Fixed: The penetration between the circle and polygon shapes when the contact points are the vertices of the polygon
- Fixed: An issue in collision detection capsule 3D vs. triangle
- Fixed: An issue in `PhysicsEngine3D.Linecast()` vs shapes that was returning the incorrect UserTag
- Fixed: An issue that could cause `FP` overflow exceptions on 2D and 3D shape casts with larger objects
- Fixed: An issue that could cause `FP` overflow exceptions on raycasts by increasing the range of usable sphere radii and ray lengths
- Fixed: Issue on `DynamicMap.AddCollider2D` that was causing 2D Physics Engine to not be properly reset
- Fixed: DynamicMap.FromStaticMap is now generic
- Fixed: DynamicMap crashing builds when editing a disabled collider
- Fixed: `DynamicMap.RemoveMeshCollider` not updating collider indices correctly
- Fixed: Invalid 3D collider rotations are automatically set to  `Identity` when being added to the `DynamicMap` using `AddCollider3D()`
- Fixed: An issue in the Unity scripts with `QuantumEntityViewUpdater.AddViewComponent()` that caused view components to not get added after toggling them on/off
- Fixed: Preloading Addressables in `QuantumRunnerLocalDebug` correctly attempts to load `AssetObject` instead of `UnityEngine.Object`, only loading the latter will make sure assets are properly preloaded
- Fixed: Unity scripts leak into dotnet simulation project generation
- Fixed: Re-exposed the `DrawShape2DGizmo` and `DrawShape3DGizmo` methods for usage in user code
- Fixed: `LayerMask` inspector overriding property values in multi-select mode
- Fixed: An issue that caused a stack overflow when baking a scene prototype with `EntityRef` fields if Quantum 2 migration is enabled
- Fixed: Incorrect Unity version check for `UnityEngine.AI.NavMesh.GetAreaNames`
- Fixed: A typo of field `DeltaTimeType` inside the `QuantumRunnerLocalDebug` and `QuantumRunnerLocalReplay` scripts
- Fixed: 3D Shape prototypes not displaying Rotation offset field in the Editor
- Fixed: An issue in the demo menu party screen that failed to connect to party codes
- Fixed: An issue in the replay menu that prevented replays to be exported when only `RecordingFlags.Input` were selected
- Fixed: `QuantumMenuConnectionBehaviour.RequestAvailableOnlineRegionsAsync()` properly returns a faulted task on errors
- Fixed: An issue that caused a wrong rotation on the 2D variant of `Draw.Rectangle()`
- Fixed: 2D and 3D Collider Prototypes applying scale twice when using a child Source Collider
- Fixed: An issue that caused changing the scene in the QuantumMenu not to be saved to PlayerPrefs
- Fixed: An issue in the Quantum menu that caused the preferred region to not be previewed correctly in the main menu
- Fixed: An issues that caused Frame Heap errors when late-joining with a snapshot when a CompoundShape buffer had been expanded
- Fixed: Removed debug assertion that expected all inputs to be polled exactly once on the clients, which might not be the case in extreme conditions

# 3.0.0

**Breaking Changes**

- The Quantum SDK is now a unitypackage.
- Renamed PhotonDeterministic.dll to `Quantum.Deterministic.dll` and QuantumCore.dll to `Quantum.Engine.dll`.
- Upgraded network libraries to Photon Realtime 5 (see Realtime changelog separately).
- All Unity-only scripts have been put in `Quantum` namespace and have been given `Quantum` prefix (e.g. `MapData` -> `QuantumMapData`,  `EntityPrototype` -> `QuantumEntityPrototype`).
- The `PhysicsCollider` 2D and 3D components now have a field called `Layer Source` which defines where the layer info should come from. This might require colliders layers to be set again;
- The `AssetBase` class is no longer functional.
- `AssetObject` now derive from `UnityEngine.ScriptableObject`. This means that there is no longer the need for `AssetBase` wrapper and all the partial `AssetBase` extensions need to be moved to `AssetObject` definitions. Tools that migrate asset data without data loss are provided on our website.
- `AssetObjects` need to be created with `AssetObject.Create<T>` or `T.Create`.
- To add a new list of assets (e.g. with Addressables), mark a method with `[QuantumGlobalScriptableObjectLoaderMethod]` attribute.
- `IAssetSerializer` interface changed, it also deals with `RuntimePlayer` and `RuntimeConfig` serialization
- `AssetObjects` referencing other `AssetObjects` directly (not by `AssetRef<T>`) are supported, but will no longer be fully serializable by the default `IAssetSerializer`. This should be a concern for when any non-Unity runner is used or such assets are used in the `DynamicDB`.
- More consistent `DynamicAssetDB` behavior - assets can be added and disposed only during verified frames. Also, `AssetObject.Disposed` is called whenever a dynamic asset is disposed. Previous behavior can be restored with `QuantumGameFlags.EnableLegacyDynamicDBMode` game flag
- Unless `QuantumGameFlags.EnableLegacyDynamicDBMode` is used, all assets in the `DynamicAssetDB` will get disposed upon shutting down simulation.
- Removed "standalone assets" / `QPrefabs`. They have been fully replaced with standalone prototypes. All `_data` files should be removed.
- Removed built-in support for AssetBundles.
- `RuntimeConfig` and `RuntimePlayer` are serialized with Json when send to the server.
- Added the `$type` property to Json serialized `RuntimeConfig` and `RuntimePlayer` to be able to deserialize the file outside of Unity, use `QuantumUnityJsonSerializer.SerializeConfig(Stream stream, IRuntimeConfig config)` to export RuntimePlayer and RuntimeConfig or add `"$type":"Quantum.RuntimeConfig, Quantum.Simulation"` or `"$type":"Quantum.RuntimePlayer, Quantum.Simulation"` to the respective Jsons.
- `MapDataBakeCallbacks` implemented in a custom assembly it has to be made known by adding this `[assembly: QuantumMapBakeAssembly]` to the script.
- Changed the timing around the `GameStarted` callback. It is now only called once per started Quantum session and, when waiting for a snapshot, it will be called after the snapshot has arrived with `isResync` = true.
- Collections used in events are now passed by their appropriate pointer type (e.g. `list<T>` -> `QListPtr<T>`). Was: `Ptr`
- `Frame.Heap` is no longer a `Heap*` but instead a reference to a managed `FrameHeap` instance, which shares the same API and adds allocation tracking capabilities. Direct usages of `frame.Heap->` can be replaced by `frame.Heap.` instead.
- The default backing type for `flags` in DSL is now Int32 (was: Int64)
- Added  the `isResync` parameter to the `GameStarted` callback (which is true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Changed `QuantumNavMesh.ImportSettings.LinkErrorCorrection` type from `bool` to `float` (representing a distance).
- `NavMeshRegionMask.HasValidRegions()` now returns `true` for the "MainArea", use `HasValidNoneMainRegion` instead to only query for non-MainArea regions
- AppSettings.RealtimeAppId was replaced by AppSettings.QuantumAppId, the Unity inspector will automatically swap them when the PhotonServerSettings asset is inspected.
- Changed the replay file format and renamed it to `QuantumReplayFile`, marking the old name obsolete. Asset DB, RuntimeConfig and InputHistory are now saved with a Json friendly wrapper that works around how verbose the Unity Json tool saves byte arrays, reducing the replay file size when saved in Unity a lot,
- The Quantum3 server will automatically block all non-protocol messages and all Photon Realtime player properties. Unblock them using the Photon dashboard and set `BlockNonProtocolMessages` and `BlockPlayerProperties` to `false`.
- Removed `DeterministicGameMode.Spectating` (use `Multiplay`) because all online Quantum simulation are started in spectating mode by default until a player is added.
- Also check out other references 
  - Migration Guide: `https://doc.photonengine.com/quantum/v3/getting-started/migration-guide`
  - What's New: `https://doc.photonengine.com/quantum/v3/getting-started/whats-new`
  - Photon Realtime 5 changelogs (`Assets\Photon\PhotonLibs\changes-library.txt`, `Assets\Photon\PhotonRealtime\Code\changes-realtime.txt`)

**What's New**

- Added input delta compression.
- Increasing minimum Unity version to 2021 LTS.
- New start online protocol to support adding and removing players at runtime.
- Added Quantum webhooks for the Photon Public Cloud.
- Increased maximum player count to 128.
- Added support for up to 512 components (use `#pragma max_components 512` to enable)
- Added predicted commands.
- Added support for capsule shapes.
- Added snapshot interpolation mode for entities.
- Added `QuantumHud` window that displays onboarding information and installs Quantum user scripts,  assets and demos.
- Added a new graphical demo menu that can be installed with the QuantumHub and includes MPPM support for Unity 6.
- Added the Asteroid game sample that can be installed with the QuantumHub.
- Added a new GUI to toggle Quantum gizmos.
- Added async support for connection handling.
- Added support for assets that are neither a Resource nor Addressable. Direct references to such assets are stored in `QuantumUnityDB`.
- `QuantumUnityDB.Get[Global]AssetGuids` - ability to iterate over GUIDs, based on asset types.
- Added support for `netstandard2.1`.
- Added full support for Odin inspector.
- Polished Quantum Unity inspector and added code documentation foldouts.
- Added a Newtonsoft-powered Json deserializer can now read `[SerializeReferences]` (uses the  `Quantum.Json.dll` dependency for non-Unity platforms).
- Added a data-driven way of adding systems (see `SystemsConfig.asset`).
- Added `AllowedLobbyProperties` dashboard variable to restrict lobby property usage for Photon matchmaking.
- Added `Quantum.Log.dll` dependency, which introduces the `Quantum.LogType` and can clash with `UnityEngine.LogType` when migrating.
- Added Quantum debug dll toggle to switch between Debug and Release Quantum dlls in Unity
- Added position and rotation teleport support for Transform2D, Transform3D and QuantumEntityView.
- Increased max navmesh region count to 128.
- Added `Frame.PlayerConnectedCount`.
- Added `FrameThreadSafe.GetGlobal()` extension method.
- Added `EntityViewComponent`s, a quick way to add view logic to the Quantum entity views.
- Added entity view pooling (add `QuantumEntityViewPool` to the `EntityViewUpdater` game object)
- Added dotnet Quantum dll build (see `QuantumDotnetBuildSettings` asset).
- Added a dotnet console replay runner app project that is generated to the non-Unity simulation project.
- Added a tool to quickly convert Unity colliders to Quantum colliders `GameObject/Quantum/Convert Colliders`.
- Added Quantum Unity script templates.
- Added `DynamicMap` core type, a specialization of Map as a Dynamic Asset capable of triggering internal updates when modifying static colliders and of serializing runtime mesh data.

**Changes**

- Unified CodeGen tools into one that is used from the UnityEditor.
- Unified QuantumRunner and SessionContainer into the `SessionRunner` class and moved it to the `QuantumGame` project.
- Component prototype suffix has been changed from `_Prototype` to `Prototype`.
- Component prototype wrapper prefix changed from `EntityComponent` to `QPrototype` (e.g. `EntityComponentTransform2D` -> `QPrototypeTransform2D`).
- `Frame.Assets` is now obsolete. Use `Frame.FindAsset<T>` instead.
- `AssetObjectConfigAttribute` is obsoleted and no longer functional.
- `AssetObject` is no longer restricted to being in the same assembly as Quantum simulation code. If the simulation code needs to access such assets, it can either use a base class or an interface. This allows `AssetObjects` to be extended with any Unity-specific data.
- `AssetRefT` generated types are now obsolete, use `AssetRef<T>` instead.
- `asset T;` and `import asset T;` are no longer needed in .qtn files. Any type derived from `AssetObject` is already a fully functional Quantum asset now and can be used with `AssetRef<T>` fields.
- `AssetObject.Guid` is now deterministic by default, based on Unity's GUID and `fileId`. When needed can be overridden (e.g. for assets imported from Quantum 2.1) - such overrides are stored in `QuantumEditorSettings`. Turning `AssetGuids` deterministic speeds up `QuantumUnityDB` by an order of magnitude.
- `UnityDB` has been obsoleted and `AssetResourceContainer` has been removed. `QuantumUnityDB` replaced both and uses more consistent method naming.
- Prototype assets for new prefabs are created as standalone assets, with `EntityPrototype` suffix and `qprototype` extension. This fully decouples loading prototypes during simulation from their source prefab, improving asset load times and avoiding deadlock issues with Unity's job system.
- All Quantum assets located in asset search paths are now marked with `QuantumAsset` label.
- Moved NavMesh baking to the `Quantum Simulation` project to bake parts deterministically (not the Unity navmesh export).
- Removed the `MovementType.DynamicBody` and `MovementType.CharacterController2D` from the NavmeshAgentConfig. Select `MovementType.Callback` instead and perform movement during the `ISignalOnNavMeshMoveAgent` callback. The removed options were only good for prototyping and led to a lot of questions.
- Changed `QuantumNavMesh.BakeData` and related data structures to only use fixed point vectors.
- Replaced non-deterministic code (Triangle Normal Computation) in `BakeNavMesh()` with fixed point math.
- Moved navmesh link detection and error correction from `BakeNavMesh()` to `ImportFromUnity()`, `StartTriangle` and `EndTriangle` now are set on `BakeData.Links`.
- The MainArea of the navmesh now has its own valid navmesh region (always index 0) and can be toggled on/off and can be properly used for navmesh queries like `LineOfSight()` or `FindClosestTriangle()`.
- Changed `BakeNavMesh` signature to only use relevant data types.
- Replaced the signal `OnPlayerDataSet(PlayerRef player)` with `OnPlayerAdded(PlayerRef player, bool firstTime)` where `firstTime` signals this is the first time that this player was assigned..
- Added `OnPlayerRemoved(PlayerRef player)` signal.
- Added a parameter to the `GameStarted` callback (`isResync`: true, when the callback is invoked after the game has been re-synced for example after a late-join).
- Removed the `ReleaseProfiler` configuration from the code solutions and replacing it with a flag in `StartParameters.GameFlags` called `QuantumGameFlags.EnableTaskProfiler`.
- Removed API that was deprecated in Quantum 2.1.
- All Quantum Unity MonoBehaviours now derive from `QuantumMonoBehaviour`.
- All Quantum Unity ScriptableObjects now derive from `QuantumScriptableObject`.
- The state inspector now shows all relevant configs when selecting the Runner.
- Restructured the Quantum Unity menus.
- `Navigation.Raycast2D()` provides information about the border index that generated the closest hit.
- Reduced the navmesh binary asset size, control the serialization options under `NavMesh.SerializeType`. By default meta data is now recomputed during runtime, set to `NavMeshSerializeType.FullWithMetaData` to disable it.
- Drawing debug shapes for development builds must now be opted in by defining the `QUANTUM_DRAW_SHAPES` define (`QuantumEditorSettings` has a shortcut to toggle it).
- Quantum runtime logging can now be toggled by a `LogLevel` inside the `EditorSettings`, the default is `WARN`.
- The context `LayerInfo` is now exposed in `FrameThreadSafe.Layers` property, similarly to `Frame.Layers`.
- `Quantum.TypeUtils` is now obsolete

## Stable

### Build 1548 (Aug 20, 2024)

- Initial stable release

## RC

### Build 1547 (Aug 15, 2024)

**What's New**

- Added a unity event callback to the `QuantumDebugRunner` start sequence
- DynamicMap can expand existing mesh collider
- Added a `LogLevel` toggle button to the main Quantum Hub screen

**Improvements**

- 2D and 3D Physics engines no longer integrate linear velocities that move the transform position out of the FP usable range
- Instead, a warn will be logged in debug and the velocity will be reset

**Changes**

- Map `TriangleMeshCellSize` is now called `SceneMeshCellSize` and is an FP value instead of Int32

**Removed**

- Obsolete `QuantumEditorSettings.QuantumSolutionPath` property, we have not decided if and how we introduce split solution tooling like in Quantum 2

**Bug Fixes**

- Fixed: Update used count when removing mesh collider
- Fixed: Mark free triangles as serializable
- Fixed: Apply rotations to navmesh links
- Fixed: The contact point from edge shapes collision in 2D. It was not being converted to the world space
- Fixed: Refactor MeshTriangleVerticesCcw to allow for simulation baking
- Fixed: Static box collider rotation offset was calculated incorrectly
- Fixed: Script compilation errors for Unity 2022.1
- Fixed: An issue that prevented the frame differ to work during checksum errors
- Fixed: The collision between capsule 2D and edges when they are parallel

### Build 1543 (Aug 05, 2024)

**Bug Fixes**

- Fixed: `Exception: MemoryIntegrity Check Failed:  
globals.PlayerConnectedCount overlaps previous field globals.PhysicsSettings on type Quantum.globals` error
- Fixed: `The type or namespace name 'IntPtr' could not be found` error when making a il2cpp build

### Build 1542 (Aug 02, 2024)

**What's New**

- Physics `ShapeCastMinIterations` setting to Simulation Config
- Add QoL methods to FrameTimer
- Added XY Toggle to Hub
- AddMeshCollider() include for pos and rot
- 2D and 3D JointPrototype.Materialize overloads that receive anchor references instead of a PrototypeMaterializationContext
- Support to physics QueryOptions and CallbackFlags in the DSL and generated prototypes

**Improvements**

- Physics Shape Cast accuracy when very close to a collider at the cast origin
- `FrameTimer` helper methods and properties, along with improvements to the API doc

**Changes**

- CodeGen - an error is raised if a struct nested in `input` definition has `button` field. `button` fields are only meant to be used directly
- Assemblies added to the `QuantumDotnetProjectSettings.IncludePaths` or marked with `QuantumDotnetInclude` are added as references to the exported project
- `[RangeEx]` attribute supports FP properties
- When removing a component T or destroying an entity that `Has<T>`, `ComponentCount<T>` is now reduced after component-remove signals are invoked instead of before
- Changing the coding conventions of script templates to a more common C# style
- Renamed `PrototypeMaterializationContext.ComponentTypeIndex` to `ComponentTypeId`
- Terrain Collider now detects degenerate triangles, logs a warning and does not bake them into the serialized mesh

**Bug Fixes**

- Fixed: 3D Shape Cast queries with DetectOverlapsAtCastOrigin option not working correctly for 3D mesh hits
- Fixed: AsteroidsWaveSpawnerSystem
- Fixed: Removed halfHeight from Draw2DCapsuleShape when QUANTUM_XY
- Fixed: The LineIntersectsAABB function was setting the Y size of AABB as the same values as X
- Fixed: The size of capsules and circles when using the function Debug.Draw
- Fixed: Issue with component Filters that caused the 'any' filter rule to be disregarded and the 'without' rule be used in its place
- Fixed: Using `entity_ref` in `input` causing "Fixed size input (size: X) is enabled but input size we got was Y" errors
- Fixed: Warnings in `SimulationConfig` in standalone project
- Fixed: Using unions in `input` causing "Fixed size input (size: X) is enabled but input size we got was Y" errors
- Fixed: The penetration correction when an object collides with multiple triangles
- Fixed: `QuantumGameFlags.EnableTaskProfiler` flag having an opposite effect: not passing it in `SessionRunner.Arguments.GameFlags` enabled the profiler, passing it in - disabled
- Fixed: Inconsistent naming of parameters in `GetEventTypeCodeGen`
- Fixed: GC allocs on entering each task profiler section, if task profiler is enabled
- Fixed: Prototype assets of variant prefabs not being updated if base prefab adds/removes component prototypes
- Fixed: Quantum Task Profiler data is saved as .dat,json and cannot be loaded without manually renaming it
- Fixed: Quantum DB Window does not respect the ‘Sync Selection’ option and always syncs selection
- Fixed: Making the Asteriods sample work in `QUANTUM_XY` mode
- Fixed: Added `Preserve` attribute to all Asteroids systems
- Fixed: QuantumEditorSettings now has a toggle for Quantum XY
- Fixed: The collision between capsule 2D and polygon generates normal vectors to the opposite side to the closest edge
- Fixed: An issue that caused `SceneViewComponents` to be initialized twice when located as a child of `EntityViewUpdater` and the `Updater field was set`
- Fixed: Adding the `Preserve` attribute to system templates

### Build 1523 (Jul 10, 2024)

**Breaking Changes**

- FP multiplication is now rounded instead of being truncated, i.e. rounded towards negative infinity

**What's New**

- `FP.FromRoundedFloat_UNSAFE`
- Extension methods `ToRoundedFP`, `ToRoundedFPVector2` and `ToRoundedFPVector3`
- 2D and 3D Compound Shape method to `ReserveCapacity`
- `HostProfiler.CreateMarker` - allows for a lower overhead profiling by creating markers ahead of time

**Changes**

- Upgraded Photon Realtime to version `5.0.10`
- `HostProfiler.Init` - pass an implementation of `IHostProfiler` rather than a set of delegates

**Removed**

- `HostProfiler.Start/EndThread`
- 2D and 3D `CollisionResultInfo.InvertResult` is now internal. The `.Normal` field already takes the inverted flag into account

**Bug Fixes**

- Fixed: An issue with `QuantumEntityView` that caused the error correction to initially glitch in some situations
- Fixed: StateInspector - adding components crashing the editor
- Fixed: DynamicMap.FromStaticMap() does not maintain AllRuntimeTriangles key
- Fixed: Typo in `InstantReplayConfig.LengthSeconds`
- Fixed: 2D and 3D Spring Joint damping ratio not being configurable on the editor
- Fixed: Migration: initial CodeGen not emitting AssetObject stubs correctly
- Fixed: Migration: prefab variants occasionally left unaffected by guid transfer / restoring asset data

### Build 1519 (Jul 02, 2024)

**Breaking Changes**

- FP constant values have been updated to be closer to their target values `PiInv`, `PiTimes2`, `PiOver2`, `PiOver2Inv`, `PiOver4`, `Pi3Over4 `, `Pi4Over3`, `Deg2Rad`, `_0_02`, `_0_03`, `_0_04`, `_0_05`, `_0_10`, `Rad_360`, `Rad_90`, `Rad_45`, `Rad_22_50`, `_1_02`, `_1_05`, `_1_10`, `EN1`, `EN3`, `EN4`, `EN5`, `Epsilon`, `Log2_10`

**Changes**

- Upgraded Photon Realtime to version 5.0.9 (26. June 2024)
- `DebugMesh` is obsolete now, use `QuantumMeshCollider.Global` instead
- `Unlit/Quantum Debug Draw` renamed to `Unlit/Quantum Debug`
- Moved the shader file `QuantumDebugDraw.shader` to `Assets/Photon/Quantum/Runtime/RuntimeAssets`
- The Quantum Hub installation button is no longer disabled when the installation is detected as complete
- Iterating `DynamicAssetDB` is much more performant, but still allocates
- `IResourceManager` extensions now use generics to avoid virtual calls
- `Quantum.Json` and standalone projects now support `ISerializationCallbackReceiver` interface
- Added `Odin.Serialization` and `Odin.Attributes` assembly references to Quantum.Simulation. As all assembly references are optional (unless there are compile errors), a project does not need to have Odin installed
- `Photon.Deterministic.PersistentMap` refactored. It is no longer `IEquatable`, implements visitor pattern and enumeration is much more performant, thought it still allocates a bit
- Assets in `DynamicAssetDB` are now reference-counted. Counters are shared by DB instances that are linked to each other due to copy constructor or `DynamicAssetDB.CopyFrom`. Assets are disposed when their reference count drops to zero, due to `DynamicAssetDB.ReplaceAsset`, `DynamicAssetDB.DisposeAsset`, `DynamicAssetDB.Dispose` (entire db disposal) or `DynamicAssetDB.CopyFrom` (due to releasing the old state). Note that none of this applies if legacy mode is used

**Removed**

- The folder `Assets/Photon/Quantum/Resources/Gizmos` can be deleted as well as the asset `Assets/Photon/Quantum/Resources/QuantumShapes2D.fbx`
- Unused code from `Native`
- `QuantumGameFlags.EnableLegacyDynamicDBMode` - pass in a dynamic DB constructed with legacy mode instead

**Bug Fixes**

- Fixed: The contact point position in the collision check between capsule and polygon 2D
- Fixed: The normal direction of the shape cast when the flag DetectOverlapsAtCastOrigin is enabled
- Fixed: DynamicMap FromStaticMap not binding Entity Views
- Fixed: SetTriangleUnchecked updates collider index
- Fixed: The capsule 2D penetration in the corner of then box when the capsule rotation is frozen
- Fixed: Mesh removal includes AllRuntimeTriangles
- Fixed: An issue that caused the Quantum graph shaders to not work for VR
- Fixed: An issue that caused to close the connection at the end of the Quantum online session, although `ShutdownConnectionOptions.None` was selected
- Fixed: Made the menu config scene info entries work without setting an explicit `Name`
- Fixed: An issue how switching to a `DynamicMap`  affected the scene entities and views of the source map. Now they are left intact and are only removed when switching to a regular map
- Fixed: BitStream missing `ushort` extension method
- Fixed: An issue in the InstantReplayDemo that prevented the replay from successfully restarting/looping etc
- Fixed: A compilation error when scripting define `QUANTUM_REMOTE_PROFILER` is enabled
- Fixed: NavMesh Gizmos throwing null when starting Quantum

### Build 1514 (Jun 18, 2024)

- Initial release

