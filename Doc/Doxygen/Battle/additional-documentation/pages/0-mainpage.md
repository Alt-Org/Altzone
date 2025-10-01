@mainpage Overview

---

# Namespace Summary {#index-namespace-summary}

|  Namespace                                           || Description                                                                         |
| :------------- | :----------------------------------- | :---------------------------------------------------------------------------------- |
| @cref{Altzone}                                       || @copybrief Altzone                                                                  |
| @crefd{Altzone}| @cref{Altzone,Scripts}               | @copybrief Altzone.Scripts                                                          |
| @crefd{Altzone}| @cref{Altzone.Scripts,BattleUiShared}| @copybrief Altzone.Scripts.BattleUiShared                                           |
|                                                                                                                                           |||
| @cref{Battle}                                        || @copybrief Battle                                                                   |
| @crefd{Battle} | @cref{Battle,QSimulation}            | @copybrief Battle.QSimulation             @ref page-simulation-namespaces "More..." |
| @crefd{Battle} | @cref{Battle,View}                   | @copybrief Battle.View                    @ref page-view-namespaces "More..."       |
|                                                                                                                                           |||
| @cref{Quantum}                                       || @copybrief Quantum                                                                  |

@bigtext{[[Namespace List]](./namespaces.html)}

<br/>

# File Summary {#index-file-summary}

|  Path                                                                                                                                                                                            ||||| Description                                                                                                                                                                                        |
| :--------------- | :---------------------- | :---------------------------------- | :--------------------------------------------- | :--------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| @dirref{Altzone} |                                                                                                                                                                                |||| Project Root.                                                                                                                                                                                      |
| @dirref{Altzone} | @dirref{Altzone,Assets}                                                                                                                                                        |||| Unity Resources Directory.<br/>Where all game resources are stored, including scripts, graphics, audio, etc.                                                                                       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                         ||||||
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,Altzone}                                                                                                                       ||| Main %Altzone directory.<br/>Contains project-wide files which can be used in both %Battle and MenuUi.                                                                                             |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,Altzone}     | @dirref{Altzone/Assets/Altzone,Resources}                                                                        || %Altzone Resource Directory. @ref page-resources "More..."<br/>Contains project-wide resources like prefabs and scriptable objects.                                                                                              |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,Altzone}     | @dirref{Altzone/Assets/Altzone,Resources}      | @dirref{Altzone/Assets/Altzone/Resources,Prefabs/BattleUiShared} | BattleUiShared Prefabs Directory.<br/>Contains prefabs used in both %Battle and MenuUi.                                                                                                            |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,Altzone}     | @dirref{Altzone/Assets/Altzone,Scripts}                                                                          || %Altzone Scripts Directory.<br/>Contains project-wide scripts.                                                                                                                                     |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,Altzone}     | @dirref{Altzone/Assets/Altzone,Scripts}        | @dirref{Altzone/Assets/Altzone/Scripts,BattleUiShared}           | BattleUiShared Scripts Directory.<br/>Contains scripts used in both %Battle and MenuUi.                                                                                                            |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                         ||||||
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser}                                                                                                                   ||| Main %Quantum Directory.<br/>Contains files for %Battle and other %Quantum based development.                                                                                                      |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Resources}                                                                    || Game Resource Directory. @ref page-resources "More..."<br/>Contains %Battle resources like prefabs, configs, spec assets, graphics, audio, etc.                                                    |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Scenes}                                                                       || Game Scene Directory. @ref page-scenes "More..."<br/>Contains %Battle Scenes.                                                                                                                      |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation}                                                                   || Game Simulation Logic Directory. @ref page-simulation-directories "More..."<br/>Contains deterministic %Quantum Simulation logic and state.                                                        |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View}                                                                         || Game View Logic Directory. @ref page-view-directories "More..."<br/>Contains non-deterministic Unity View/Visual logic that is client-side representation of the Simulation.                       |
|                                                                                                                                                                                                                                                                                                                                                                                                                                                         ||||||
| @dirref{Altzone} | @dirref{Altzone,Doc/Doxygen/Battle}                                                                                                                                            |||| %Battle Documentation files. @dirlink{More...:Altzone/Doc/Doxygen/Battle}<br/>Contains [Doxygen🡵] configuration files, additional documentation files and the generated documentation for %Battle. |

@bigtext{[[File List]](./files.html)}

<br/>

---

# Naming {#index-naming}

@bigtext{**Prefix**}
- **%Battle**  
  Marks type as **%Battle** related.
- **%Ui**  
  Marks type as **%UI** related. Used in addition to the **%Battle** prefix.

@bigtext{**Suffix**}
- **%QSystem**  
  Marks type as [Quantum System](#page-simulation-systems).
- **%QComponent**  
  Marks type as [Quantum Component](#page-simulation-components).
- **%QSingleton**  
  Marks type as [Quantum Singleton](#page-simulation-singletons).
- **%QConfig**  
  Marks type as [Quantum Config](#page-resources-configs).
- **%QSpec**  
  Marks type as [Quantum Spec](#page-simulation-specs).
- (Simulation) **%Manager**  
  Marks type as [Manager](#page-simulation-managers).
- (Simulation) **%Controller**  
  Marks type as [Controller](#page-simulation-managers).
- **%ViewController**  
  Marks type as [ViewController](#page-view-controllers).
- (Ui) **%Handler**  
  Marks type as @uihandlerlink when used in addition to the **Ui** prefix.
- (Ui) **%Component**  
  Marks type as @uicomponentlink when used in addition to the **Ui** prefix.


<br/>

---

# Other Pages {#index-other-pages}

- @bigtext{[[Simulation]](#page-simulation)}
- @bigtext{[[View]](#page-view)}
- @bigtext{[[Resources]](#page-resources)}
- @bigtext{[[Scenes]](#page-scenes)}
- @bigtext{[[Documentation]](#page-documentation)}

<br/>

---

[Doxygen🡵]: https://www.doxygen.nl/index.html