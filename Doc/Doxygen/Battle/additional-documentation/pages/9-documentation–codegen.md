# Documenting %Quantum generated code {#page-documentation-codegen}
These are guidelines for writing documentation for **%Quantum generated code**.  

Documentation for **%Quantum generated code** is written in `.dox` files because it can not be written directly in **source code**. Documentation is written using **documentation comments** with the `///` syntax.  
All `.qtn` files created by us should be documented, with a separate `.dox` file for each `.qtn` file.  
The code being documented is that which is found in the **generated code** resulting from what is defined in the `.qtn` file.  
The **generated code** is found in the Quantum namespace.  
When referencing **code symbols**, make sure to reference the correct **generated code**.  
**%Quantum components** should be referenced to as **structs**, as **%Quantum** generates them as such.


---

## Base guidelines {#page-documentation-codegen-base-guidelines}

<br/>

### Unicode characters {#page-documentation-codegen-base-guidelines-unicode}
**Unicode characters** are ok to use inside of `.dox` files.

<br/>

### Documentation comment blocks {#page-documentation-codegen-base-guidelines-documentation-comment-blocks}
[Doxygen🡵] supports multiple **documentation comment** formats. See [[Documenting with Doxygen]](#page-documentation-doxygen-comment-blocks) for more information on these.  
You should only use the `///` format.

<br/>

### Documentation commands {#page-documentation-codegen-base-guidelines-commands}
[Doxygen🡵] supports various **commands** that can be used when documenting **%Quantum generated code**. See [[Documenting with Doxygen]](#page-documentation-doxygen-commands) for more information on these.  
All **commands** are allowed in `.dox` files, so you should choose the most appropriate format.

<br/>

### Linking formats {#page-documentation-codegen-base-guidelines-linking}
[Doxygen🡵] supports various types of **linking formats**. See [[Linking styles and formats]](#page-documentation-doxygen-styles-formats) for more information on these.  
**Markdown links** are preferred. **Custom links** are also preferred when appropriate.  
**Markdown reference link** definitions should be placed at the end of the same **documentation comment block**. They only work within the block they are defined in.

<br/>

### Markdown {#page-documentation-codegen-base-guidelines-markdown}
**Markdown** formatting can me used in `.dox` files. See [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html).  
When writing text, line breaks should be done using two spaces at the end of the line.

<br/>

---

<br/>

## File guidelines {#page-documentation-codegen-documenting-file-guidelines}
**%Quantum generated code** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/quantum-codegen} to not have unnecessary additional files in **Unity**'s @dirref{Altzone,Assets} folder,  
especially because **Unity** would create `.meta` files to keep track of the files for no reason.  
The directory structure inside the above directory matches that found in the project.  
The files are named as follows:  
- Filenames begin with **"qtn"** in all lower case.
- This is then followed by the name of the `.qtn` file being documented as it's named in the project separated by a hyphen.

Example filename:  
`qtn-(ExampleFilename).dox`

<br/>

## Documentation format {#page-documentation-codegen-documenting-format}

### Common Documentation Patterns

Each "fragment" of documentation begins with a `@(type)` command like `@file`, `@struct`, `@enum`, or `@var` followed by name of the file or code symbol specifying to Doxygen what is being documented.

`@(type)` is immediately followed by the **XML** `<summary>`command containing a [{brief/summary}] after which there is an empty line followed by the [{detailed description}].

In `.dox` file
```
/// @(type) ...
/// <summary>
/// Brief Description with some text
/// </summary>
///
/// Detailed Description with some text
```

If the [{brief/summary}] or the [{detailed description}] talks about some concept that has a definition/explanation of some other page or the concept is in some way relevant what is being documented.  
 Then the [{detailed description}] should contain a **Concept link** for more information. And where the concept was mentioned should use the `<b>{Concept mention}</b>` formatting.

In `.dox` file
```
/// @(type) ...
/// <summary>
/// Brief Description with some text and <b>{Concept mention}</b>
/// </summary>
///
/// Detailed Description with some text and <b>{Concept mention}</b>
///
/// See [{Concept link}](#page-link) for more info.
```

<br/>

Sometimes documentation should be grouped together. For example fields and/or methods inside a struct.  
Note that Doxygen already groups fields and/or methods together. So grouping is used to group fields and/or methods to categories.  
However if you mix fields and methods in one group you override Doxygen's default grouping, which may not be a desired result.

This is done using the `@name` command followed by the name of the group and then `@{` on the next line, marking the beginning of the group.  
The group is closed by `@}` and `//  END` comment that repeats the name of the group to make it easier to track where the groups starts and ends.

In `.dox` file
```
/// @name Example Fields
/// @{

...

/// @}
//  END Example Fields
```

The group can have a description written in between `@name` and `@{` separated by empty lines.

```
/// @name Example Fields
///
/// Group Description
///
/// @{

...

/// @}
//  END Example Fields
```

The `@anchor` command is usually added before the `@name` command. The `@anchor` command specifies an **ID** for the group.  
The **ID** is global so to avoid name collisions the **ID** should start with the struct name followed by a hyphen `-` and then the name of the group without any spaces.  
`@anchor (StructName)-(GroupName)`

When a group has an `@anchor` each member of the group should have a "Part of" link in its [{detailed description}] specifying that it is part of a group and linking back to the group itself.

In `.dox` file
```
/// @anchor ExampleStruct-ExampleFields
/// @name Example Fields
/// @{

/// @var ...
/// <summary> ...
///
/// Part of @ref ExampleStruct-ExampleFields "Example Fields"

...

/// @}
//  END Example Fields
```

<br/>

If a `.qtn` file contains an array %Quantum generates a private array that internally stores the data and a public property to access the data.

The public property should contain the main documentation and a link to the internal array.  
The internal array should be put in a group called "Private internal arrays" and have a link to the public property.

In `.qtn` file
```
struct ExampleStruct
{
    ...

    array<ExampleStruct>[42] ExampleArray;

    ...
}
```

In `.dox` file
```

...

/// @var array Quantum::ExampleStruct::ExampleArray
/// <summary>
/// Brief Description
/// Public getter for internal @cref{Quantum.ExampleStruct,_ExampleArray_}
/// </summary>

...

/// @anchor ExampleStruct-PrivateArrays
/// @name Private internal arrays
/// @{

/// @var Byte Quantum::ExampleStruct::_ExampleArray_
/// <summary>Private internal array, access through @cref{Quantum.ExampleStruct,ExampleArray}</summary>

/// @}
//  END Private internal arrays
```

### File structure

All **enums**, **structs** and **components** defined in a `.qtn` file, and the file itself, should be documented.  
**File documentation** should be placed at the top of the file followed by a separator line comment.  
After the separator line all the **enums**, **structs** and **components** are documented in the order they are defined in the `.qtn` file.

`Example.qtn`
```
struct ExampleStruct
{
    FP Value;
    int A;
    int B;
}

enum ExampleEnum
{
    StateA,
    StateB
}

component ExampleComponent
{
    ExampleEnum State;
    array<ExampleStruct>[42] ExampleArray;
}
```

`qtn-Example.dox`
```
/// @file Example.qtn
/// ...

// ---------------------------------------------------------------------------------------------------- //

/// @struct Quantum::ExampleStruct
/// ...

/// @enum Quantum::ExampleEnum
/// ...

/// @struct Quantum::ExampleComponent
/// ...
```


### File documentation

The file should be documented with a `@file` command and a [{brief/summary}].  
In the [{detailed description}] of the file documentation all **enums** and **structs** defined in the file should be listed with bullet points.  

`Example.qtn`
```
struct ExampleStruct
{
    FP Value;
    int A;
    int B;
}

enum ExampleEnum
{
    StateA,
    StateB
}

component ExampleComponent
{
    ExampleEnum State;
    array<ExampleStruct>[42] ExampleArray;
}
```

`qtn-Example.dox`
```
/// @file Example.qtn
/// <summary>
/// Contains ...
/// </summary>
///
/// ## Generated Enums
/// - @cref{Quantum,ExampleEnum}
///   @copybrief Quantum.ExampleEnum
///
/// ## Generated Structs
/// - @cref{Quantum,ExampleStruct}
///   @copybrief Quantum.ExampleStruct
/// - @cref{Quantum,ExampleComponent}
///   @copybrief Quantum.ExampleComponent

// ---------------------------------------------------------------------------------------------------- //

...

```

### Enum documentation

**Enums** should be documented with a `@enum` command with the name of the **enum** formatted with `::` as below followed by a [{brief/summary}]. 
The [{detailed description}] should have a **Generated from** link to the `.qtn` file itself should be listed as shown below.  
Attached below the main enum documentation should be a `// {` comment marking the beginning of the intended **enum variable block**.  
Each **variable** defined in an **enum** should be documented with a `@var` command with the name of the **enum** and the name of the **variable** followed by a [{brief/summary}] text without the `<summary>` command.  
The **enum variable block** should be closed with a `// }` comment.

`Example.qtn`
```
...

enum ExampleEnum
{
    StateA,
    StateB
}

...
```

`qtn-Example.dox`
```
...

/// @enum Quantum::ExampleEnum
/// <summary>
/// Brief Description
/// </summary>
///
/// See [{Concept Link}](#page-link) for more info.
///
/// @bigtext{Generated from @ref Example.qtn}
///
// {
    /// @var Quantum::ExampleEnum Quantum::ExampleEnum::StateA
    /// StateA Description

    /// @var Quantum::ExampleEnum Quantum::ExampleEnum::StateB
    /// StateB Description
// }

...
```

### Struct and Component documentation

Since %Quantum generates **components** as **structs** the following also applies to **components**.

**Structs** should be documented with a `@struct` command with the name of the **struct** formatted with `::` as below followed by a [{brief/summary}]. 
The [{detailed description}] should have a **Generated from** link to the `.qtn` file itself should be listed as shown below.  
Attached below the main struct documentation should be a `// {` comment marking the beginning of the intended **struct variable block**.  
Each **variable** defined in a **struct** should be documented with a `@var` command with the type and the name of the **variable** followed by a [{brief/summary}].  
The **struct variable block** should be closed with a `// }` comment.


`Example.qtn`
```
...

struct ExampleStruct
{
    FP Value;
    int A;
    int B;
}

...

component ExampleComponent
{
    ExampleEnum State;
    array<ExampleStruct>[42] ExampleArray;
}

...
```

`qtn-Example.dox`
```
...

/// @struct Quantum::ExampleStruct
/// <summary>
/// Brief Description with some text and <b>{Concept mention}</b>
/// </summary>
///
/// See [{Concept Link}](#page-link) for more info.
///
/// @bigtext{Generated from @ref Example.qtn}
///
// {
    /// @var FP Quantum::ExampleStruct::Value
    /// <summary>
    /// Brief Description
    /// </summary>

    /// @var int Quantum::ExampleStruct::A
    /// <summary>
    /// Brief Description
    /// </summary>
    ///
    /// Detailed Description

    /// @var int Quantum::ExampleStruct::B
    /// <summary>
    /// Brief Description
    /// </summary>
    ///
    /// See [{Concept link}](#page-link) for more info.

    /// @name Quantum Built-in
    /// @{
    ///
    /// @var Int32 Quantum::ExampleStruct::SIZE
    /// @var Int32 Quantum::ExampleStruct::ALIGNMENT
    ///
    /// @fn Int32 Quantum::ExampleStruct::GetHashCode
    ///
    /// @fn void Quantum::ExampleStruct::Serialize
    ///
    /// @}
// }

...

/// @struct Quantum::ExampleComponent
/// <summary>
/// Brief Description with some text and <b>{Concept mention}</b>
/// </summary>
///
/// See [{Concept Link}](#page-link) for more info.
///
/// @bigtext{Generated from @ref Example.qtn}
///
// {
    /// @anchor ExampleComponent-ExampleFields
    /// @name Example Fields
    /// @{

    /// @var ExampleEnum Quantum::ExampleComponent::State
    /// <summary>
    /// Brief Description
    /// </summary>
    ///
    /// Part of @ref ExampleComponent-ExampleFields "Example Fields"
    ///
    /// See [{Concept Link}](#page-link) for more info.

    /// @var array Quantum::ExampleComponent::ExampleArray
    /// <summary>
    /// Brief description  
    /// Public getter for internal @cref{Quantum.ExampleComponent,_ExampleArray_}
    /// </summary>
    ///
    /// Part of @ref ExampleComponent-ExampleFields "Example Fields"

    /// @}
    //  END Example Fields
    
    /// @anchor ExampleComponent-PrivateArrays
    /// @name Private internal arrays
    /// @{
    
    /// @var Byte Quantum::ExampleComponent::_ExampleArray_
    /// <summary>Private internal array, access through @cref{Quantum.ExampleComponent,ExampleArray}</summary>
    
    /// @}
    //  END Private internal arrays

    /// @name Quantum Built-in
    /// @{
    ///
    /// @var Int32 Quantum::ExampleComponent::SIZE
    /// @var Int32 Quantum::ExampleComponent::ALIGNMENT
    ///
    /// @fn Int32 Quantum::ExampleComponent::GetHashCode
    /// @fn void Quantum::ExampleComponent::ClearPointers
    ///
    /// @fn void Quantum::ExampleComponent::OnRemoved
    /// @fn void Quantum::ExampleComponent::Serialize
    ///
    /// @}
// }

...
```

<br/>

---

[Doxygen🡵]:               https://www.doxygen.nl/index.html
[{brief/summary}]:        #page-documentation-doxygen-terminology-brief-summary
[{detailed description}]: #page-documentation-doxygen-terminology-detailed-description
