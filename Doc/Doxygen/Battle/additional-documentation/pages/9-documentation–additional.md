# Writing additional documentation {#page-documentation-additional}
These are guidelines for writing **additional documentation**, excluding **documentation pages** and **%Quantum generated code** (see [[Writing documentation pages]](#page-documentation-pages) and [[Documenting Quantum generated code]](#page-documentation-codegen)).

**Additional documentation** is written in `.dox` files because it can not be written directly in **source code**. Documentation is written using **documentation comments** with the `///` syntax.  
The files should be in the correct subdirectories of @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation} to not have unnecessary additional files in **Unity**'s @dirref{Altzone,Assets} folder, especially because **Unity** would create `.meta` files to keep track of the files for no reason.

---

## Base guidelines {#page-documentation-additional-base-guidelines}
These are base guidelines for writing **additional documentation**.  
Refer to the individual sections on this page for specific guidelines on each:  
[Documenting directories](#page-documentation-additional-directories)  
[Documenting namespaces](#page-documentation-additional-namespaces)  
[Documenting scenes](#page-documentation-additional-scenes)  
[Documenting specs](#page-documentation-additional-specs)  
[Documenting prefabs](#page-documentation-additional-prefabs)

<br/>

### Unicode characters {#page-documentation-additional-base-guidelines-unicode}
**Unicode characters** are ok to use inside of **additional documentation** `.dox` files.

<br/>

### Documentation commands {#page-documentation-additional-base-guidelines-commands}
[Doxygen🡵] supports various **commands** that can be used in **additional documentation**. See [[Documenting with Doxygen]](#page-documentation-doxygen-commands) for more information on these.  
All **commands** are allowed in `.dox` files, so you should choose the most appropriate format.

<br/>

### Linking formats {#page-documentation-additional-base-guidelines-linking}
[Doxygen🡵] supports various types of linking formats. See [[Linking styles and formats]](#page-documentation-doxygen-styles-formats) for more information on these.  
**Markdown links** are preferred. **Custom links** are also preferred when appropriate.  
**Markdown reference link** definitions should be placed at the end of the same **documentation comment block**. They only work within the block they are defined in.

<br/>

### Markdown {#page-documentation-additional-base-guidelines-markdown}
**Markdown** formatting can me used in `.dox` files. See [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html).  
When writing text, line breaks should be done using two spaces at the end of the line.

<br/>

---

## Documenting directories {#page-documentation-additional-directories}
The [Base guidelines](#page-documentation-additional-base-guidelines) apply.

<br/>

### File guidelines {#page-documentation-additional-directories-file-guidelines}
**Directory** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/directories}.  
**Directory** documentation files are named as follows:  
- **Filenames** begin with the word **"directory"** in all lower case.
- This is then followed by the name of the **directory** being documented as it's named in the project, including it's **parent directory** structure excluding the **root directory** of the project.
- Only the core **directories** that are part of the **parent directory** structure need to be listed in the **filename**.
- Each **directory name** is separated by a hyphen `-`.
- If the **directories** being documented have **subdirectories** with their own documentation, the **filename** should end with a hyphen `-`.
- The **root directory** documentation file is named `directory-.dox`.

Syntax:  
`directory-(DirectoryName)-.dox`  
`directory-(DirectoryName)-(SubdirectoryName).dox`  

Examples:  
`%directory-.dox`  
`directory-ExampleA.dox`  
`directory-ExampleB-.dox`  
`directory-ExampleB-Subdirectory.dox`  

<br/>

### File contents {#page-documentation-additional-directories-file-contents}
**Directory** documentation consists of documentation for all the **subdirectories** of the **directory** being documented.  
The documentation for the **directory** being documented itself should be in it's **parent directory's** documentation.

<br/>

### Documentation format {#page-documentation-additional-directories-format}
**Directory** documentation consists of the directory name marked with the `@dir` command and a [{brief/summary}].

```
/// @dir DirectoryName
/// <summary>
/// Description
/// </summary>
```

<br/>

---

## Documenting namespaces {#page-documentation-additional-namespaces}
The [Base guidelines](#page-documentation-additional-base-guidelines) apply.  
All **namespaces** should be documented in the existing namespace.dox file, found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/namespaces}.

<br/>

### Documentation format {#page-documentation-additional-namespaces-format}
**Namespace** documentation consists of the namespace definition marked with the `@namespace` command and a [{brief/summary}].  
**Namespace** definitions are formatted with `::` symbols as such:  
`(Namespace)::(Namespace)`  

```
/// @namespace Example::Namespace
/// <summary>
/// Description
/// </summary>
```

<br/>

---

## Documenting scenes {#page-documentation-additional-scenes}
The [Base guidelines](#page-documentation-additional-base-guidelines) apply.

<br/>

### File guidelines {#page-documentation-additional-scenes-file-guidelines}
**Scene** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/scenes}.  
**Scene** documentation files are named as follows:  
- **Filenames** begin with the word **"scene"** in all lower case.
- This is then followed by the **filename** as it is in the project of the **scene** being documented.
- Each word is separated by a hyphen `-`.

Example **filename**:  
`scene-(SceneFileName).dox`

<br/>

### Documentation format {#page-documentation-additional-scenes-format}
**Scene** documentation consists of the scene name marked with the `@file` command and a [{brief/summary}].  
It also includes a listing of all the **GameObjects** in the scene, as well as all **GameObjects** that can be instatiated in the **scene**.  

**GameObjects** are listed under **Markdown headers** defined with `###`. They are listed as bullet points, and marked as bold with the **Markdown** `**` format. Bullet points are separated by an empty line  

```
/// @file (SceneFileName).unity
/// <summary>
/// Main %Battle scene.
/// </summary>
///
/// ### GameObjects in scene:
/// - **GameObject Name**  
///   Description
```

<br/>

---

## Documenting specs {#page-documentation-additional-specs}
The [Base guidelines](#page-documentation-additional-base-guidelines) apply.

<br/>

### File guidelines {#page-documentation-additional-specs-file-guidelines}
**Spec asset** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/specs}.  
**Spec asset** documentation files should be named as such:  
- **Filenames** begin with **"spec-asset"** in all lower case.
- This is then followed by the **filename** as it is in the project of the **spec** being documented.
- Each word is separated by a hyphen `-`.
Documentation is specifically for the `.asset` files, not the source `.cs` files.  

Example **filename**:  
`(ExampleQSpec).dox`

<br/>

### Documentation format {#page-documentation-additional-specs-format}
**Spec asset** documentation consists of the scene name marked with the `@file` command and a [{brief/summary}].  
These are followed by the description shown below, which includes a link to the source `.cs` file.  

```
/// @file ExampleSpec.asset
/// <summary>
/// Description
/// </summary>
///
/// Only contains the data used by the spec.  
/// @bigtext{The spec's structure is defined in @ref ExampleQSpec.cs "ExampleQSpec".}
```

<br/>

---

## Documenting prefabs {#page-documentation-additional-prefabs}
The [Base guidelines](#page-documentation-additional-base-guidelines) apply.

<br/>

### File guidelines {#page-documentation-additional-prefabs-file-guidelines}
**Prefab** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/prefabs}.  
**Prefab** documentation files are named as follows:  
- **Filenames** begin with the word **"prefab"** in all lower case.
- This is then followed by the **filename** of the **prefab** as it is in the project.
- Each word is separated by a hyphen `-`.

Example **filename**:  
`prefab-(PrefabFileName).dox`

<br/>

### Prefab summary format {#page-documentation-additional-prefabs-summary-format}
**Prefab summary** doesn't have a strict format that needs to be used, but it needs to be kept relatively short.

<br/>

### Entity Prototype format {#page-documentation-additional-prefabs-entity-prototype}
**Entity Prototype** documentation is paired with their **prefab's** documentation and is located below it in the same file.  
**Entity Prototype** summary has a link to its **prefab** counterpart. The template below is all that is needed to document **Entity Prototypes**.  

```
/// @file filename.qprototype
/// <summary>
/// EntityPrototype counterpart of @ref prefabfilename.prefab -
/// @copybrief prefabfilename.prefab
/// </summary>
///
/// Contains data about the prefab that %Quantum needs (QComponents used in prefab etc.).
``` 

<br/>

### Prefab Structure format {#page-documentation-additional-prefabs-structure-format}
**Prefabs** need to have their structure documented.

**Structure documentation** follows these rules:
- Structure is listed as a bullet list.
- GameObject names are bolded.
- Components and Children -titles are italicized.
- Component names are regular text style.
- Material components have (Material) after their name.
- If component is a %Quantum Entity Prototype and it has QComponents, those are listed below it on the next level of indentation. They also link to the QComponent's documentation.
- ViewControllers and other components with documentation have a link to their class documentation. Links are on the same level of indentation with other components.
- If another prefab is used as a child GameObject, its prefab is linked in brackets after the GameObject's name.
- If all of the child GameObjects on the same indentation level have same components, Shared components -title is used at the same indentation level as the gameobjects it refers to. Shared components are listed below the title on the next indentation level. If there is only few child GameObjects and/or few components, this rule doesn't need to be strictly followed.
- If all of the child GameObjects on the same indentation level have same children, Shared children -title is used at the same indentation level as the gameobjects it refers to. Shared children are listed below the title on the next indentation level.
- If there is multiple identical child GameObjects with consecutive numbering, they can be listed only once and the number range can be placed inside curly braces.
- Other parts of names can also be replaced with words inside curly braces if needed, but this needs to be used sparingly in order to keep the structure documentation as simple and cohesive as possible.

<br/>

#### Basic template for documenting prefab structures: {#page-documentation-additional-prefabs-structure-format-template}
```
/// ### Structure
/// - **GameObject**
///     - *Components:*
///         - Component1
///         - Component2
///     - *Children:*
///         - **ChildGameObject**
///             - *Components:*
///                 - Component1
///                 - Component2
```
<br/>

#### Example snippets of different rules implemented: {#page-documentation-additional-prefabs-structure-format-examples}
Example of QComponent links:
```
/// - *Components:*
///     - Transform
///     - %Quantum Entity Prototype
///         - @cref{Quantum,BattleSoulWallQComponent}
///         - @cref{Quantum,BattleCollisionTriggerQComponent}
///     - %Quantum Entity View
```
<br/>

Example of ViewController link:
```
/// - *Components:*
///     - Transform
///     - @cref{Battle.View.Projectile,BattleProjectileViewController}
///     - SpriteRenderer
```
<br/>

Example of prefab used as a Child GameObject:
```
/// - *Children:*
///     - **BattleUiTimer** (@ref BattleUiTimer.prefab)
///     - **AnnouncementText**
```
<br/>

Example of shared components and shared children:
```
/// - *Children:*
///     - **ImpactForceHolder**
///     - **HpHolder**
///     - **SpeedHolder**
///     - **CharSizeHolder**
///     - **DefenceHolder**
///     - *Shared components:*
///         - RectTransform
///     - *Shared children:*
///         - **StatName**
///         - **StatValue**
///         - *Shared components:*
///             - RectTransform
///             - CanvasRenderer
///             - TextMeshPro - Text (UI)
```
On the example above, all -Holder children have RectTransform component and they all have StatName and StatValue children. All StatName and StatValue children have the shared components listed below them.

<br/>

Example of GameObjects with consecutive numbering:
```
/// - *Children:*
///     - **BattleLightrayBlue**{ **1** - **10** }
///     - **BattleLightrayRed**{ **1** - **10** }
```

<br/>

Example of other parts of names replaced with curly braces:
```
/// - *Children:*
///     - **CharacterTopBase**
///     - **CharacterBottomBase**
///     - *Shared components:*
///         - Transform
///         - SpriteRenderer
///         - Sprites-Default (Material)
///     - *Shared children:*
///         - **Character**{ **Top** - **Bottom** } **Background**
///         - **Character**{ **Top** - **Bottom** } **Piece**{ **01** - **10** }
///         - *Shared components:*
///             - Transform
///             - SpriteRenderer
///             - Sprites-Default (Material)
///         - *Shared children:*
///             - **Character**{ **Top** - **Bottom** } **PieceWhite**{ **01** - **10** }
///             - *Shared components:*
///                 - Transform
///                 - SpriteRenderer
```
On the example above, Top and Bottom have been placed inside curly braces on shared children, because there is so many shared children that also have more shared children with the same naming style and shared components.  
Note: There is space between the closing curly braces and rest of the names, because doxygen doesn't recognize asterisks after the brace without it. On the original GameObject name there is no space.

<br/>

---

[Doxygen🡵]:               https://www.doxygen.nl/index.html
[{brief/summary}]:        #page-documentation-doxygen-terminology-brief-summary
[{detailed description}]: #page-documentation-doxygen-terminology-detailed-description