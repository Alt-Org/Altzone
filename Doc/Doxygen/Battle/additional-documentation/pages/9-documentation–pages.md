# Writing documentation pages {#page-documentation-pages}
[Doxygen🡵] can create **custom pages**. They can be used to list information that is useful to those reading the documentation, such as this page.  
**Subpages** allow for segmenting the information of a page to make it easier to navigate and read.  
See [Doxygen documentation🡵](https://www.doxygen.nl/manual/additional.html#custom_pages).

<br/>

## File guidelines {#page-documentation-pages-file}
**Page files** are `.md` **Markdown** files that are found in @dirref{Altzone/Doc/Doxygen/Battle/additional-documentation/pages}, because they are entirely unrelated to any **source code** of the project and we do not want to have unnecessary additional files in **Unity**'s @dirref{Altzone,Assets} folder, especially because **Unity** would create `.meta` files to keep track of the files for no reason.  
**Page files** should be named with a number and the name of the **page**. The number defines the order of the **pages** as they appear on the documentation webpage. File names should be all lower case and words should be separated by hyphens `-`. If a **page** has a **subpage**, the filename should end with a hyphen `-`.  
`(number)-(page-name).md`  
**Subpages** should be named with the name of the parent page file followed by the name of the **subpage**. File names should be all lower case and words should be separated by hyphens `-`.  
`(number)-(page-name)-(subpage-name).md`  

Documentation text inside these files is written without the use of **documentation comments**.

<br/>

### Unicode characters {#page-documentation-pages-file-unicode}
**Unicode characters** are ok to use inside of **documentation page files**.

<br/>

### Documentation commands {#page-documentation-pages-commands}
[Doxygen🡵] supports various **commands** that can be used in page documentation. See [Documenting with Doxygen](#page-documentation-doxygen-commands) for more information on these.  
All **commands** are allowed in **documentation page files**, so you should choose the most appropriate format.

<br/>

### Linking formats {#page-documentation-pages-linking}
[Doxygen🡵] supports various types of **linking formats**. See [Linking styles and formats](#page-documentation-doxygen-styles-formats) for more information on these.  
**Markdown links** are preferred in **documentation pages**. **Custom links** are also preferred when appropriate.  
**Markdown reference link** definitions should be placed at the end of the file.

<br/>

### Markdown {#page-documentation-pages-markdown}
**Markdown** formatting is preferred in **documentation pages**. See [Doxygen documentation🡵](https://www.doxygen.nl/manual/markdown.html).  
When writing text, line breaks should be done using two spaces at the end of the line.  
**Sections** should be separated by a line break using `<br/>`.  
**Sections** that fit together should be grouped using `---` to create lines on the **page**.

<br/>

---

### Titles

**Mainpage**
- **Mainpage** is a special case with no title or ID. It is defined with the `@mainpage` [Doxygen command🡵](https://www.doxygen.nl/manual/commands.html#cmdmainpage).

**Page titles**
- The first thing on a page should be the **title**, defined with a **Markdown** header followed by the **page ID** as such:  
  `# (Page name) {(#page-ID)}`

**Page IDs**
- **Page IDs** should start with the word **"page"**, followed by the name of the **page**. IDs should be all lower case and words should be separated by hyphens `-`.  
  `#page-(page-name)`

**Subpage IDs**
- **Subpage IDs** start with the ID of the **parent page** they are under, followed by the name of the **subpage**. Follow the same formatting rules as **page IDs**.  
  `#page-(page-name)-(subpage-name)`

<br/>

### Sections
**Documentation pages** should be split into **sections** in a tree format. All **pages** and **sections** must have IDs.  

**Section titles**
- **Sections** are defined with a **Markdown** header followed by the **section ID** as such:  
  `## (Section name) {(#section-ID)}`

**Mainpage section IDs**
- **Mainpage sections** are a special case. The **section IDs** should start with the word index, followed by the name of the **section**.  
  `#index-(section-name)`

**Page section IDs**
- **Page section IDs** start with the ID of the **page** they are on, followed by the name of the **section**. Follow the same formatting rules as **page IDs**.  
  `#page-(page-name)-(section-name)`

**Subpage section IDs**
- **Subpage section IDs** start with the ID of the **subpage** they are on, followed by the name of the **section**. Follow the same formatting rules as **page IDs**.  
  `#page-(page-name)-(subpage-name)-(section-name)`

<br/>

---

[Doxygen🡵]: https://www.doxygen.nl/index.html