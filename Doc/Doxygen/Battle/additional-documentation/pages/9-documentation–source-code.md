# Documenting Source code {#page-documentation-documenting-source-code}
All aspects of **source code**, such as **files**, **methods** and **variables**, should be documented with clear and concise information.  
Documentation should be written in **documentation comment blocks** using the `///` format.  
Different sections of the documentation should be separated by an empty line.  

<br/>

## Base guidelines {#page-documentation-source-code-base-guidelines}
These are base guidelines for all **source code** documentation.  
For additional guidelines refer to:  
[Documenting Quantum systems](#page-documentation-source-code-quantum-systems) if you are documenting a **%Quantum system**.  
[Documenting Unity/View](#page-documentation-source-code-unity-view) if you are documenting **Unity/View** code.

<br/>

### Unicode characters {#page-documentation-source-code-base-guidelines-unicode}
**Unicode characters** can not be used inside of **source code**, even in **documentation comments**. The use of them can be circumvented with **custom commands**. 
See [[Circumventing unicode characters]](#page-documentation-doxygen-custom-commands-unicode)

<br/>

### Doxygen specific and non-Doxygen specific {#page-documentation-source-code-base-guidelines-doxygen-specific}
Some parts of documentation in **source code** are only read by [Doxygen🡵], while others are recognized by the IDE or Unity. 
This places some limitations on what formats can be used in certain contexts.  

**Non-Doxygen specific** areas of code documentation:
- **Class**, **method** and **variable** documentation:
  - **XML** `<summary>` Command
  - **XML** `<param>` Command
  - **XML** `<returns>` Command

**Doxygen specific** areas of code documentation:
- All other areas, such as:
  - **File documentation** (A `<summary>` section inside a **File documentation** block is also **Doxygen specific**)
  - Other areas of **class**, **method** and **variable** documentation

### Documentation commands {#page-documentation-source-code-base-guidelines-commands}
[Doxygen🡵] supports various **commands** that can be used in **source code** documentation. See [[Documenting with Doxygen]](#page-documentation-doxygen-commands) 
for more information on these.  

Always use these **XML Commands** when documenting **files**, **classes**, **methods** and **variable**:
- `<summary>`
- `<param>`
- `<returns>`

In a [Non-Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) part of documentation you should only use **XML Commands**. 
When using **XML Commands** you should consider compatibility with both **Doxygen** and other tools.  
With careful consideration other **kinds of commands** can sometimes be used, 
as long as they don't cause issues when any tool reads them and don't negatively affect the readibility of the documentation too much.  
The most important thing is to test compatibility and readability.  

Other **kinds of commands** can be used if they are in a [Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) part of documentation, 
such as the **file documentation**.  
You should still consider consistency. For example even though **file documentation** is **Doxygen specific**, `<summary>` should still be used.  

The `<br/>` **command** should be used for line breaks.  

<br/>

### Linking formats {#page-documentation-source-code-base-guidelines-linking}
[Doxygen🡵] supports various types of **linking formats**. See [[Linking styles and formats]](#page-documentation-doxygen-styles-formats) for more information on these.  

In a [Non-Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) part of documentation you should use the **XML link** format. 
When using **XML links** you should consider compatibility with both **Doxygen** and other tools.  
With careful consideration **other kinds of links** can sometimes be used, 
as long as they don't cause issues when any tool reads them and don't negatively affect the readibility of the documentation too much.  
The most important thing is to test compatibility and readability.  

In a [Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) part of documentation you can use **other link formats**, 
such as **Markdown**, **Doxygen** and **custom links**.  
You should still consider consistency.

<br/>

### Markdown {#page-documentation-source-code-base-guidelines-markdown}
**Markdown** can be used in [Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) contexts when documenting **source code**, 
but you should always test to see if it functions as desired.  
With careful consideration **Markdown** can be used in [Non-Doxygen specific](#page-documentation-source-code-base-guidelines-doxygen-specific) contexts, 
as long as it doesn't cause issues when any tool reads it and doesn't negatively affect the readibility of the documentation too much. 
The most important thing is to test compatibility and readability.

<br/>

### Documenting C# Files {#page-documentation-source-code-base-guidelines-files}
**File documentation** should have a reference to the **file**, a [{brief/summary}] and possible longer information as its own section.

```
/// @file BattlePlayerMovementController.cs
/// <summary>
/// Handles player input, movement and rotations.
/// </summary>
///
/// Gets player's Quantum.Input and updates player's position and rotation depending on player's actions.
/// Handles moving, rotating and teleporting players and all their hitboxes.
```

<br/>

### Documenting Classes {#page-documentation-source-code-base-guidelines-classes}
**Classes** should be documented with a [{brief/summary}].

```
/// <summary>
/// Handles player input, movement and rotations.
/// </summary>
public static unsafe class BattlePlayerMovementController
```

<br/>

### Documenting Methods {#page-documentation-source-code-base-guidelines-methods}
When documenting any **method**, the different sections of the comment such as the [{brief/summary}], **parameters** and **return value** should be separated by an empty line.

```
/// <summary>
/// Clamps the grid position of the player to the playfield of their team.
/// </summary>
///
/// <param name="playerData">Pointer to the player's data component.</param>
/// <param name="gridPosition">The grid position of the player.</param>
/// <param name="clampedPosition">The resulting clamped position of the player.</param>
///
/// <returns>True if the position changed from clamping, false if it remained the same.</returns>
```

<br/>

### Documenting public getters {#page-documentation-source-code-base-guidelines-public-getters}
The format used for documenting **public getters** for **private variables**. Using both [{brief/summary}] and **value** tags 
so that in [Doxygen🡵] it's clear that it's a **getter** for another **variable**, and in code it's clear what it's **value** is.

```
/// <summary>Public getter for #_rectTransform.</summary>
/// <value>Reference to the %Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</value>
public RectTransform RectTransformComponent => _rectTransform;
```

<br/>

### Documenting variables {#page-documentation-source-code-base-guidelines-variables}
**Variables** should be documented with a [{brief/summary}].

```
/// <summary>Reference to the %Battle Ui element's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RectTransform.html">RectTransform@u-exlink</a> component.</summary>
private RectTransform _rectTransform;
```

<br/>

---

## Documenting Quantum Systems {#page-documentation-source-code-quantum-systems}
The [base guidelines](#page-documentation-source-code-base-guidelines) apply to **%Quantum system** documentation. 
Below are additional guidelines for specifically formatting **%Quantum system** documentation.  
In the examples parameter documentation has been ommitted.

<br/>

### Quantum System Class [{brief/summary}] format {#page-documentation-source-code-quantum-systems-system-classes}
The format used for documenting **System** and **SystemSignalsOnly** classes. Must contain the header with the correct system name and link to **%Quantum documentation**.

**System**

```
/// <summary>
/// <span class="brief-h">%Diamond <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
/// Handles spawning diamonds, managing their lifetime and destroying them.
/// </summary>
public unsafe class BattleDiamondQSystem : SystemMainThreadFilter<BattleDiamondQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnDiamondHitPlayer
```

**SystemSignalsOnly**

```
/// <summary>
/// <span class="brief-h">%Goal <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
/// Triggers the end of the game when it receives signal
/// </summary>
public unsafe class BattleGoalQSystem : SystemSignalsOnly, ISignalBattleOnProjectileHitGoal
```

<br/>

### Quantum System OnInit method [{brief/summary}] format {#page-documentation-source-code-quantum-systems-oninit-methods}
The format used for documenting **%Quantum OnInit** methods. Must contain the **header** with the link to **%Quantum documentation**. Must also contain the appropriate **warning message**.

```
/// <summary>
/// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System OnInit method</a> gets called when the system is initialized.</span><br/>
/// Initializes the arena, player system, and sets the game session as initialized.
/// @warning
/// This method should only be called by Quantum.
/// </summary>
public override void OnInit(Frame f)
```

<br/>

### Quantum System Update method [{brief/summary}] format {#page-documentation-source-code-quantum-systems-update-methods}
The format used for documenting **%Quantum Update** methods. Must contain the header with the link to **%Quantum documentation**. Must also contain the appropriate **warning message**.

```
/// <summary>
/// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method</a> gets called every frame.</span><br/>
/// Controls state transitions of the game session per frame. Manages countdowns and progression to 'Playing'.
/// @warning
/// This method should only be called by Quantum.
/// </summary>
public override void Update(Frame f)
```

<br/>

### Quantum System Signal method [{brief/summary}] format {#page-documentation-source-code-quantum-systems-signal-methods}
The format used for documenting **%Quantum Signal** methods. Must contain the header with the link to **%Quantum documentation** and a reference to the signal the method responds to. 
Must also contain the appropriate **warning message**.

```
/// <summary>
/// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Signal method@u-exlink</a>
/// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitPlayerShield">ISignalBattleOnProjectileHitPlayerShield</see> is sent.</span><br/>
/// Handles behavior when the projectile hits a player shield.<br/>
/// Applies bounce logic based on surface normal of the shield hitbox.
/// @warning
/// This method should only be called via Quantum signal.
/// </summary>
public void BattleOnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
```

<br/>

---

## Documenting Unity/View {#page-documentation-source-code-unity-view}
The [base guidelines](#page-documentation-source-code-base-guidelines) apply to **Unity/View** documentation. 
Below are additional guidelines for specifically formatting **Unity/View** documentation.

<br/>

### Documenting **SerializeFields** {#page-documentation-source-code-unity-view-serializefields}
The format used for grouping **SerializeFields**. Example provided also has **SerializeField** [{brief/summary}] documentation.
- Anchor name is in format:  
  `(ClassName)-(SerializeFields)`
- Make sure that the **header** is copied entirely, and the example comments starting with two slashes `//` are removed.
- Place all **SerializeField variables** inside the group.
- **SerializeField variable** documentation should be on a single line, use [{brief/summary}] tags and have `[SerializeField]` prefix.

```
/// @anchor BattleUiJoystickHandler-SerializeFields // Anchor name
/// @name SerializeField variables // The group name
/// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor. // Group description
/// @{ // Start of the grouped SerializeFields

/// <summary>[SerializeField] Reference to BattleUiController.</summary> // SerializeField summary documentation
/// @ref BattleUiMovableJoystickElement-SerializeFields // Anchor reference
[SerializeField] private BattleUiController _uiController; // SerializeField variable

/// @} // End of the grouped SerializeFields
```

<br/>

### UI Handler class method [{brief/summary}] format {#page-documentation-source-code-unity-view-ui-handlers}
```
<span class="brief-h">handler name @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
// brief text
```

---

[Doxygen🡵]:         https://www.doxygen.nl/index.html
[{brief/summary}]:  #page-documentation-doxygen-terminology-brief-summary