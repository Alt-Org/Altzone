@mainpage Overview

---

# Namespace Summary {#index-namespace-summary}

|  Namespace                                || Description                                                             |
| :------------- | :------------------------ | :---------------------------------------------------------------------- |
| @cref{Battle}                             || @copybrief Battle                                                       |
| @crefd{Battle} | @cref{Battle,QSimulation} | @copybrief Battle.QSimulation @ref page-simulation-namespaces "More..." |
| @crefd{Battle} | @cref{Battle,View}        | @copybrief Battle.View        @ref page-view-namespaces "More..."       |
|                                                                                                                    |||
| @cref{Quantum}                            || @copybrief Quantum                                                      |

@bigtext{[[Namespace List]](./namespaces.html)}

<br/>

# File Summary {#index-file-summary}

|  Path                                                                                                                          |||| Description                                                                                                                                                                                        |
| :--------------- | :---------------------- | :---------------------------------- | :--------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| @dirref{Altzone} |                                                                                                              ||| Project Root.                                                                                                                                                                                      |
| @dirref{Altzone} | @dirref{Altzone,Assets}                                                                                      ||| Unity Resources Directory.<br/>Where all game resources are stored, including scripts, graphics, audio, etc.                                                                                       |
|                                                                                                                                                                                                                                                                                                                                    |||||
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser}                                                 || Main %Quantum Directory.<br/>Contains files for %Battle and other %Quantum based development.                                                                                                      |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Resources}  | Game Resource Directory. @ref page-resources "More..."<br/>Contains %Battle resources like prefabs, configs, spec assets, graphics, audio, etc.                                                                                  |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Scenes}     | Game Scene Directory. @ref page-scenes "More..."<br/>Contains %Battle Scenes.                                                                                                                                                 |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | Game Simulation Logic Directory. @ref page-simulation-directories "More..."<br/>Contains deterministic %Quantum Simulation logic and state.                                                        |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View}       | Game View Logic Directory. @ref page-view-directories "More..."<br/>Contains non-deterministic Unity View/Visual logic that is client-side representation of the Simulation.                       |
|                                                                                                                                                                                                                                                                                                                                    |||||
| @dirref{Altzone} | @dirref{Altzone,Doc/Doxygen/Battle}                                                                          ||| %Battle Documentation files. @dirlink{More...:Altzone/Doc/Doxygen/Battle}<br/>Contains [Doxygen🡵] configuration files, additional documentation files and the generated documentation for %Battle. |

@bigtext{[[File List]](./files.html)}

<br/>

---

# Naming {#index-naming}

@bigtext{**Prefix**}
- **%Battle**  
  Marks type as %Battle related.
- **%Ui**  
  Marks type as %UI related. Used in addition to the %Battle prefix.

@bigtext{**Suffix**}
- **%QSystem**  
  Marks type as [Quantum System🡵].
- **%QComponent**  
  Marks type as [Quantum Component🡵].
- **%QSingleton**  
  Marks type as [Quantum Singleton🡵].
- **%QConfig**  
  Marks type as %Quantum Config.
- **%QSpec**  
  Marks type as [Quantum Spec🡵].
- **%ViewController**  
  Marks type as ViewController.
- **%Handler**  
  Marks type as @uihandlerlink.
- **%Component**  
  Marks type as @uicomponentlink.


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

[Quantum System🡵]:    https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems
[Quantum Component🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Quantum Singleton🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Quantum Spec🡵]:      https://doc.photonengine.com/quantum/current/manual/assets/assets-simulation
[Doxygen🡵]:           https://www.doxygen.nl/index.html