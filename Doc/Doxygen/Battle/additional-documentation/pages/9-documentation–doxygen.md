# Documenting with Doxygen {#page-documentation-doxygen}

## Terminology {#page-documentation-doxygen-terminology}

<br/>

### Brief / Summary {#page-documentation-doxygen-terminology-brief-summary}
[Doxygen🡵] refers to this part of documentation as a **brief**, but in **C#** it is called a **summary**.  
It should contain a **brief** description of whatever is being documented and appears automatically in documentation as the short description wherever the thing being documented is listed.  
The **brief** will appear at the beginning of the individual **documentation page** of the thing it is defined for.
It can also be linked to manually to have it appear where we want, which is used often.

<br/>

### Detailed description {#page-documentation-doxygen-terminology-detailed-description}
This is a [Doxygen🡵] term. The **detailed description** is defined as any text we write outside of the **brief**.  
It will appear lower down on the individual **documentation page** of the thing it is defined for. The **brief** is repeated before the **detailed description** text.  

<br/>

## Documentation comment blocks {#page-documentation-doxygen-comment-blocks}
[Doxygen🡵] supports multiple **documentation comment** formats (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/docblocks.html#specialblock)). You should only use the `///` format.

<br/>

## Unicode characters {#page-documentation-doxygen-unicode}
The use of **unicode characters** depends on context.  
See [[Documenting source code]](#page-documentation-source-code-base-guidelines-unicode)  
See [[Documenting Quantum generated code]](#page-documentation-codegen-base-guidelines-unicode)  
See [[Writing documentation pages]](#page-documentation-pages-file-unicode)  
See [[Writing additional documentation]](#page-documentation-additional-base-guidelines-unicode)  

In contexts where **unicode characters** are not allowed, they can be [circumvented](#page-documentation-doxygen-custom-commands-unicode).

<br/>

## Commands {#page-documentation-doxygen-commands}
[Doxygen🡵] supports multiple kinds of commands (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/commands.html)).  
**Doxygen specific commands** can start with a `\` or `@` symbol. Examples on the **Doxygen** documentation page use `\`, but you should only use the `@` format.  
**Doxygen** supports **XML Commands** (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/xmlcmds.html)). These commands are preferred in certain contexts.  
**Doxygen** supports **HTML Commands** (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/htmlcmds.html)).  

**XML Commands** are generally preferred in **source code** because they are a native **C#** feature 
(see [C# documentation🡵](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d3-recommended-tags)) 
that IDEs such as Visual Studio recognize. Visual Studio will automatically show **documentation comments** with **XML Commands**.  
**Documentation comments** should in general use the **XML** `<summary>` Command instead of the **Doxygen specific** `@brief` command for the sake of consistency.  

**The correct command types to use depend on context.**  
See [[Documenting source code]](#page-documentation-source-code-base-guidelines-commands)  
See [[Documenting Quantum generated code]](#page-documentation-codegen-base-guidelines-commands)  
See [[Writing documentation pages]](#page-documentation-pages-commands)  
See [[Writing additional documentation]](#page-documentation-additional-base-guidelines-commands)

<br/>

---

## Linking styles and formats {#page-documentation-doxygen-styles-formats}
[Doxygen🡵] supports multiple kinds of **linking formats**. We have our own guidelines for the **styling** of **links**.  
The **linking style** and **format** you should use depends on what you are documenting. The **style** remains consistent in all documentation, 
but there are many **formats** you can use for the same kind of link.  

**The correct format to use depends on context.**  
See [[Documenting source code]](#page-documentation-source-code-base-guidelines-linking)  
See [[Documenting Quantum generated code]](#page-documentation-codegen-base-guidelines-linking)  
See [[Writing documentation pages]](#page-documentation-pages-linking)  
See [[Writing additional documentation]](#page-documentation-additional-base-guidelines-linking)

<br/>

### Styles {#page-documentation-doxygen-styles-formats-styles}
- **Generic documentation links**  
  There is no specific **styling** that should be used for these.
- **Documentation page links**  
  Links that lead to other **documentation pages** should have square brackets `[]` around them.  \"<span class="fake-link">[Example text]</span>\" (This is not a hard rule.)
- **Concept links**  
  Links that link to **concept** explanations should have curly brackets `{}` around them. \"<span class="fake-link">{Example text}</span>\"  
  These can be used optionally when referring to potentially unfamiliar **concepts** or **terms** to those reading the documentation.
- **External links**  
  Links that lead to **external websites** should end with a `🡵` symbol. \"<span class="fake-link">Example text🡵</span>\"  
  Since **unicode characters** can not be used inside **source code**, the <span class="tt">@@u-exlink</span> **custom command** should be used. 
  See [Circumventing unicode characters](#page-documentation-doxygen-custom-commands-unicode).
- **Code symbol links**  
  **Code symbol** paths are constructed by separating each part, such as the **namespace** and **class** name with a `.` \"<span class="fake-link">{namespace.class.method}</span>\"


<br/>

### Markdown format {#page-documentation-doxygen-styles-formats-markdown}
**Markdown style links** are preferred when they are available.  
see [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html#md_links)
- **Generic links** are formatted as such:  
  `[Example text](link_destination)`  
- **Documentation page links** are formatted as such:  
  `[Example](#page-example)` For internal page links  
  `[[Example]](#page-example)` For external page links  
- **External links** are formatted as such:  
  `[Example website🡵](exampleurl.com)`  
  - If the same link is used repeatedly, a **reference link** can be used and is defined as such:  
    `[Example website🡵]: exampleurl.com`  
    And used as such:  
    `[Example website🡵]`  
    **Reference links** may not always work, so you should test to see.

**Built-in Doxygen pages** dont have IDs but can be linked to as such:  
`[Page name](./pagefile.html)`

<br/>

### Doxygen and custom link formats {#page-documentation-doxygen-styles-formats-doxygen-and-custom}
These **custom linking formats** exist to simplify linking to **directories** and **code symbols** and are therefore preferred when they are available.  
- **Directory links** are formatted as such:  
  <span class="tt">@@dirref{directory/path}</span>  
  <span class="tt">@@dirlink{link text:directory/path}</span>  
  See [Custom reference links](#page-documentation-doxygen-custom-commands-reference-links) for more details.
- **Code symbol links** are formatted as such:  
  <span class="tt">@@cref{namespace.example.name}</span>  
  See [Custom reference links](#page-documentation-doxygen-custom-commands-reference-links) for more details.

Some destinations have an existing **custom link command** that can be used. See [Custom documentation links](#page-documentation-doxygen-custom-commands-documentation-links) 
for more details.  
When **other linking formats** don't work, the following format can be used:  
`@ref link_destination "link text"`  
See [Doxygen documentation🡵](https://www.doxygen.nl/manual/commands.html#cmdref).

<br/>

### XML format {#page-documentation-doxygen-styles-formats-xml}
These **linking formats** should be used when the **other formats** listed above are not available.  
see [Doxygen documentation🡵](https://www.doxygen.nl/manual/xmlcmds.html).  
see [C# documentation🡵](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d314-see).
- **Code symbol links** are formatted as such:  
  `<see cref="example">example text</see>`
- **External links** are formatted as such:  
  `<see href="exampleurl.com">example text@e-exlink</see>`

<br/>

---

## Markdown {#page-documentation-doxygen-markdown}
[Doxygen🡵] has **Markdown** support (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html)) which is also used in certain situations.  
See [[Documenting source code]](#page-documentation-source-code-base-guidelines-markdown)  
See [[Documenting Quantum generated code]](#page-documentation-codegen-base-guidelines-markdown)  
See [[Writing documentation pages]](#page-documentation-pages-markdown)  
See [[Writing additional documentation]](#page-documentation-additional-base-guidelines-markdown)

<br/>

---

## Doxygen custom commands {#page-documentation-doxygen-custom-commands}
[Doxygen🡵] **specific** [Custom commands🡵] that can be used during documentation are defined in @ref Altzone/Doc/Doxygen/Battle/setup/Doxyfile under the **ALIASES** section. 
These are defined by us to add useful shorthands. All added **custom commands** should be listed on this page.

<br/>

### Reference links {#page-documentation-doxygen-custom-commands-reference-links}
These [Custom commands🡵] can be used to make **linking** to **directories** and **code symbols** easier.  

List of current aliases  

**Directory reference / link commands**
- **dirref**  
  - Basic reference  
    <span class="tt">@@dirref{very/long/example/path}</span>  
    Creates a link to the specified **Directory**.  
    Formatted as such: \"<span class="fake-link">very/long/example/path</span>\"
  - Short reference  
    <span class="tt">@@dirref{very/long,example/path}</span>   
    Creates a link to the specified **Directory**. **Shortens** the resulting text by only showing the part **after** the comma `,`.  
    Formatted as such: \"<span class="fake-link">example/path</span>\"
  - Middle truncated reference  
    <span class="tt">@@dirref{very,long/example,path}</span>  
    Creates a link to the specified **Directory**. **Truncates** the resulting text by replacing the part **between** the two commas `,` with `/../`.  
    Formatted as such: \"<span class="fake-link">very/../path</span>\"
- **dirrefr**  
  - Relative reference  
    <span class="tt">@@dirrefr{very/long,example/path}</span>  
    Creates a link to the specified **Directory**. Used when referencing another **Directory** in the same parent **Directory**. Replaces the part **before** the comma `,` with `../`.  
    Formatted as such: \"<span class="fake-link">../example/path</span>\"
- **dirlink**
  -  **Directory** link  
    <span class="tt">@@dirlink{link text:very/long/example/path}</span>  
    Creates a link to the specified **Directory** with arbitrary text.  
    Formatted as such: \"<span class="fake-link">link text</span>\"

**Code symbol reference / link commands**
- **cref**  
  - Basic reference  
    <span class="tt">@@cref{namespace.example.name}</span>  
    Creates a link to the specified **Code symbol**.  
    Formatted as such: \"<span class="fake-link">namespace.example.name</span>\"
  - Short reference  
    <span class="tt">@@cref{namespace.example,name}</span>  
    Creates a link to the specified **Code symbol**. **Shortens** the resulting text by only showing the part **after** the comma `,`.  
    Formatted as such: \"<span class="fake-link">name</span>\"
- **crefd**  
  - Basic reference with dot  
    <span class="tt">@@crefd{namespace.example.name}</span>  
    Creates a link to the specified **Code symbol**. **Adds a dot** `.` to the **end** of the resulting text.  
    Formatted as such: \"<span class="fake-link">namespace.example.name.</span>\"
  - Short reference with dot  
    <span class="tt">@@crefd{namespace.example,name}</span>  
    Creates a link to the specified **Code symbol**. **Shortens** the resulting text by only showing the part **after** the comma `,`. **Adds a dot** '.' to the **end** of the resulting text.  
    Formatted as such: \"<span class="fake-link">name.</span>\"
- **clink**
  -  **Code symbol** link  
    <span class="tt">@@clink{link text:namespace.example}</span>  
    Creates a link to the specified **Code symbol** with arbitrary text.  
    Formatted as such: \"<span class="fake-link">link text</span>\"

<br/>

### Documentation links {#page-documentation-doxygen-custom-commands-documentation-links}
These [Custom commands🡵] can be used for simplified **linking** to **other sections of documentation**.  

List of current documentation link aliases  

| Alias             | Text         | documentation index       |
| :---------------- | :----------- | :------------------------ |
| @@systemslink     | Systems      | \#page-simulation-systems |
| @@uihandlerlink   | UI Handler   | \#page-view-uihandler     |
| @@uihandlerslink  | UI Handlers  | \#page-view-uihandler     |
| @@uicomponentlink | UI Component | \#page-view-uicomponent   |

They are defined as such:
```
(name)="[(Visible text)]{(documentation section index)}" \

example:
uihandlerlink="[UI Handler](#page-view-uihandler)" \
```

<br/>

### Extra formatting {#page-documentation-doxygen-custom-commands-extra}
These [Custom commands🡵] allow for easier **formatting** of our text.

List of current formatting aliases

| Alias                   | Description        |
| :---------------------- | :----------------- |
| @@bigtext{example text} | Makes the text big |

<br/>

### Circumventing unicode characters {#page-documentation-doxygen-custom-commands-unicode}
These [Custom commands🡵] are useful in avoiding the use of **unicode characters** in **source code**.  

Usage/syntax:  
**u** stands for **unicode**.  
**(name)** is the name of the **unicode character** as specified by us usually based on what the symbol represents in our documentation.
```
@u-(name)
```

List of current unicode aliases

| Alias      | Character | Description          |
| :--------- | :-------- | :------------------- |
| @@u-exlink | 🡵        | external link symbol |

They are defined as such:
```
u-(name)="(unicode character)"

example:
u-exlink="🡵"
```

---

[Doxygen🡵]:         https://www.doxygen.nl/index.html
[Custom commands🡵]: https://www.doxygen.nl/manual/custcmd.html