@mainpage Overview

---

# Namespace Summary {#index-namespace-summary}

|  Namespace                     || Description                                                                        |
| :------------- | :------------------------ | :---------------------------------------------------------------------- |
| @cref{Battle}                             || @copybrief Battle                                                       |
| @crefd{Battle} | @cref{Battle,QSimulation} | @copybrief Battle.QSimulation @ref page-simulation-namespaces "More..." |
| @crefd{Battle} | @cref{Battle,View}        | @copybrief Battle.View                                                  |
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
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | Game Simulation Logic Directory. @ref page-simulation-directories "More..."<br/>Contains deterministic %Quantum Simulation logic and state.                                                        |
| @dirref{Altzone} | @dirref{Altzone,Assets} | @dirref{Altzone/Assets,QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View}       | Game View Logic Directory. @ref page-view "More..."<br/>Contains non-deterministic Unity View/Visual logic that is client-side representation of the Simulation.                                   |
|                                                                                                                                                                                                                                                                                                                                    |||||
| @dirref{Altzone} | @dirref{Altzone,Doc/Doxygen/Battle}                                                                          ||| %Battle Documentation files. @dirlink{More...:Altzone/Doc/Doxygen/Battle}<br/>Contains [Doxygen🡵] configuration files, additional documentation files and the generated documentation for %Battle. |

@bigtext{[[File List]](./files.html)}

<br/>

---

# Naming {#index-naming}

@bigtext{**Prefix**}
- **%Battle**  
  Marks type as %Battle related.

@bigtext{**Suffix**}
- **%QSystem**  
  Marks type as [Quantum System🡵].
- **%QComponent**  
  Marks type as [Quantum Component🡵].
- **%QSingleton**  
  Marks type as [Quantum Singleton🡵].

<br/>

---

# Other Pages {#index-other-pages}

- @bigtext{[[Simulation]](#page-simulation)}
- @bigtext{[[View]](#page-view)}

<br/>

---

[Quantum System🡵]:    https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems
[Quantum Component🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Quantum Singleton🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Doxygen🡵]:           https://www.doxygen.nl/index.html