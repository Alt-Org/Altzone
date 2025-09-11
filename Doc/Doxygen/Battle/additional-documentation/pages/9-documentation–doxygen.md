# Documenting with Doxygen {#page-documentation-doxygen}

## Documentation comment blocks {#page-documentation-doxygen-comment-blocks}
Doxygen supports multiple documentation comment formats (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/docblocks.html#specialblock)). You should only use the `///` format.

<br/>

## Commands {#page-documentation-doxygen-commands}
Doxygen specific commands can start with a `\` or `@` symbol (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/commands.html)). Examples on the Doxygen documentation page use `\`, but you should only use the `@` format.  
Doxygen supports XML Commands (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/xmlcmds.html)). These commands are preferred in certain contexts.  
Doxygen supports HTML Commands (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/htmlcmds.html)).  

XML Commands are generally preferred in source code because they are a native C# feature (see [C# documentation🡵](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d3-recommended-tags)) that IDEs such as Visual Studio recognize. Visual Studio will automatically generate documentation comments with XML Commands.  
Documentation comments should use the XML `<summary>` Command instead of the Doxygen specific `@brief` command for the sake of consistency.  

The correct command types to use depend on context.  
See [[Writing documentation pages]](#page-documentation-pages)  
See [[Documenting source code]](#page-documentation-documenting-source-code)

<br/>

---

## Linking styles and formats {#page-documentation-doxygen-styles-formats}
The linking style and format you should use depends on what you are documenting. The style remains consistent in all documentation, but there are many formats you can use for the same kind of link. The correct format to use depends on context.  
See [[Writing documentation pages]](#page-documentation-pages)  
See [[Documenting source code]](#page-documentation-documenting-source-code)

<br/>

### Styles {#page-documentation-doxygen-styles-formats-styles}
Generic documentation links. There is no specific styling that should be used for these.  
Documentation page links. Links that lead to other documentation pages should have square brackets `[]` around them. **"[Example text]"** (This is not a hard rule.)  
External links. Links that lead to external websites should end with a `🡵` symbol. **"Example text🡵"**  
Since unicode characters can not be used inside source code, the <span class="tt">@@u-exlink</span> custom command should be used. See [Circumventing unicode characters](#page-documentation-doxygen-custom-commands-unicode).

<br/>

### Markdown format {#page-documentation-doxygen-styles-formats-markdown}
Markdown style links are preferred when they are available.  
see [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html#md_links)
- Generic links are formatted as such:  
  `[Example text](link_destination)`  
- Documentation page links are formatted as such:  
  `[Example](#page-example)` For internal page links  
  `[[Example]](#page-example)`For external page links  
- External links are formatted as such:  
  `[Example website🡵](exampleurl.com)`  
  - If the same link is used repeatedly, a reference link can be used and is defined as such:  
    `[Example website🡵]: exampleurl.com`  
    And used as such:  
    `[Example website🡵]`  
    From our experience this can have problems when used in contexts other than external links.

Built-in Doxygen pages dont have IDs but can be linked to as such:  
`[Page name](./pagefile.html)`

<br/>

### Doxygen and custom link formats {#page-documentation-doxygen-styles-formats-doxygen-and-custom}
These custom linking formats exist to simplify linking to directories and code symbols and are therefore preferred when they are available.  
- Directory links are formatted as such:  
  <span class="tt">@@dirref{directory/path}</span>  
  <span class="tt">@@dirlink{link text:directory/path}</span>  
  See [Custom reference links](#page-documentation-doxygen-custom-commands-reference-links) for more details.
- Code symbol links are formatted as such:  
  <span class="tt">@@cref{namespace.example.name}</span>  
  See [Custom reference links](#page-documentation-doxygen-custom-commands-reference-links) for more details.

Some destinations have an existing custom link command that can be used. See [Custom documentation links](#page-documentation-doxygen-custom-commands-documentation-links) for more details.  
When other linking formats don't work, the following format can be used:  
`@ref link_destination "link text"`  
See [Doxygen documentation🡵](https://www.doxygen.nl/manual/commands.html#cmdref).

<br/>

### XML format {#page-documentation-doxygen-styles-formats-xml}
These linking formats should be used when the other formats listed above are not available.  
see [Doxygen documentation🡵](https://www.doxygen.nl/manual/xmlcmds.html).  
see [C# documentation🡵](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d314-see).
- Code symbol links are formatted as such:  
  `<see cref="example">`
- External links are formatted as such:  
  `<see href="exampleurl.com">`

<br/>

---

## Markdown {#page-documentation-doxygen-markdown}
Doxygen has Markdown support (see [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html)) which is also used in certain situations.  
See [[Writing documentation pages]](#page-documentation-pages)  
See [[Documenting source code]](#page-documentation-documenting-source-code)

<br/>

---

## Doxygen custom commands {#page-documentation-doxygen-custom-commands}
Doxygen specific [Custom commands🡵] that can be used during documentation are defined in @ref Altzone/Doc/Doxygen/Battle/setup/Doxyfile under the ALIASES section. These are defined by us to add useful shorthands. All added custom commands should be listed on this page.

<br/>

### Reference links {#page-documentation-doxygen-custom-commands-reference-links}
These [Custom commands🡵] can be used to make linking to directories and code symbols easier.  

List of current aliases  

**Directory reference / link aliases**
- **dirref**  
  Note where the "/" is replaced with a ","
  - Basic reference  
    <span class="tt">@@dirref{very/long/example/path}</span>  
    Creates a link to the specified directory.  
    Formatted as such: "very/long/example/path"
  - Short reference  
    <span class="tt">@@dirref{very/long,example/path}</span>   
    Creates a link to the specified directory. Shortens the resulting text.  
    Formatted as such: "example/path"
  - Middle truncated reference  
    <span class="tt">@@dirref{very,long/example,path}</span>  
    Creates a link to the specified directory. Truncates the resulting text.  
    Formatted as such: "very/../path"
- **dirrefr**  
  Note where the "/" is replaced with a ","
  - Relative reference  
    <span class="tt">@@dirrefr{very/long,example/path}</span>  
    Creates a link to the specified directory. Used when referencing another directory in the same parent directory.  
    Formatted as such: "../path"
- **dirlink**
  -  Directory link  
    <span class="tt">@@dirlink{link text:very/long/example/path}</span>  
    Creates a link to the specified directory with arbitrary text.  
    Formatted as such: "link text"

**Code symbol aliases**
- **cref**  
  Note where the "." is replaced with a ","
  - Basic reference 
    <span class="tt">@@cref{namespace.example.name}</span>  
    Creates a link to the specified code symbol.  
    Formatted as such: "namespace.example.name"
  - Short reference  
    <span class="tt">@@cref{namespace.example,name}</span>  
    Creates a link to the specified code symbol. Shortens the resulting text.  
    Formatted as such: "name"
- **crefd**  
  Note where the "." is replaced with a ","
  - Basic reference with dot  
    <span class="tt">@@crefd{namespace.example.name}</span>  
    Creates a link to the specified code symbol. Adds a dot to the end of the resulting text.  
    Formatted as such: "namespace.example.name."
  - Short reference with dot  
    <span class="tt">@@crefd{namespace.example,name}</span>  
    Creates a link to the specified code symbol. Shortens the resulting text. Adds a dot to the end of the resulting text.  
    Formatted as such: "name."

<br/>

### Documentation links {#page-documentation-doxygen-custom-commands-documentation-links}
These [Custom commands🡵] can be used for simplified linking to other sections of documentation.  

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
These [Custom commands🡵] allow for easier formatting of our text.

List of current formatting aliases

| Alias                   | Description        |
| :---------------------- | :----------------- | 
| @@bigtext{example text} | Makes the text big |

<br/>

### Circumventing unicode characters {#page-documentation-doxygen-custom-commands-unicode}
These [Custom commands🡵] are useful in avoiding the use of unicode characters in source code.  

Usage/syntax:  
u stands for unicode.  
(name) is the name of the unicode character as specified by us usually based on what the symbol represents in our documentation.
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

[Custom commands🡵]: https://www.doxygen.nl/manual/custcmd.html