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
**%Quantum generated code** documentation files are `.dox` files found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/quantum-codegen} to not have unnecessary additional files in **Unity**'s @dirref{Altzone,Assets} folder, especially because **Unity** would create `.meta` files to keep track of the files for no reason.  
The directory structure inside the above directory matches that found in the project.  
The files are named as follows:  
- Filenames begin with **"qtn"** in all lower case.
- This is then followed by the name of the `.qtn` file being documented as it's named in the project separated by a hyphen.

Example filename:  
`qtn-(ExampleFilename).dox`

<br/>

## Documentation format {#page-documentation-codegen-documenting-format}
The file should be documented with a `@file` command and a [{brief/summary}].  
In the [{detailed description}] of the file documentation all **enums** and **structs** defined in the file should be listed.  

**Enums** and **structs** should be listed under their own headers as bullet points.  

```
/// @file Filename.qtn
/// <summary>
/// Description
/// </summary>
///
/// ## Generated Enums
/// - @cref{Quantum,EnumName}  
///   @copybrief Quantum.EnumName
///
/// ## Generated Structs
/// - @cref{Quantum,StructName}  
///   @copybrief Quantum.StructName
```

All **enums** and **structs** defined in a `.qtn` file should be documented.  

**Enums** should be documented with a `@enum` command with the name of the **enum** formatted with `::` as below followed by a [{brief/summary}]. 
Below that a link to the `.qtn` file itself should be listed as below.  
Each **variable** defined in an **enum** should be documented with a `@var` command with the name of the **enum** and the name of the **variable** followed by a [{brief/summary}].   

```
/// @enum Quantum::EnumName
/// <summary>
/// Description
/// </summary>
///
/// @bigtext{Generated from @ref Filename.qtn}

/// @var Quantum::EnumName Quantum::EnumName::VariableName
/// <summary>Description</summary>
```

**Structs** should be documented with a `@struct` command with the name of the **struct** formatted with `::` as below followed by a [{brief/summary}]. 
Below that a link to the `.qtn` file itself should be listed as below.  
Each **variable** defined in a **struct** should be documented with a `@var` command with the type and the name of the **variable** followed by a [{brief/summary}]. 


```
/// @struct Quantum::StructName
/// <summary>
/// Description
/// </summary>
///
/// @bigtext{Generated from @ref Filename.qtn}

/// @var Type Quantum::StructName::VariableName
/// <summary>Description</summary>
```

<br/>

---

[Doxygen🡵]:               https://www.doxygen.nl/index.html
[{brief/summary}]:        #page-documentation-doxygen-terminology-brief-summary
[{detailed description}]: #page-documentation-doxygen-terminology-detailed-description