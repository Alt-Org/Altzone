# Player Character Class 100 - Desensitizer {#page-concepts-player-class-100}

- **Simulation**
    - Has test simulation logic  
      @cref{Battle.QSimulation.Player,BattlePlayerClass100Test}
    - Has simulation data components  
      @cref{Quantum,BattlePlayerClass100DataQComponent}
    - Has spec  
      @cref{Battle.QSimulation.Player,BattlePlayerClass100QSpec}
    - **Projectile**
        - Has simulation logic  
         @cref{Battle.QSimulation.Player,BattlePlayerClass100ProjectileQSystem}
        - Has simulation data components  
        @cref{Quantum,BattlePlayerClass100ProjectileQComponent}
- **View**
    - Has test view controller  
      @cref{Battle.View.Player,BattlePlayerClass100ViewControllerTest}

- **Prefabs**
    - Has no base prefab  
    - Character prefabs  
      @ref BattlePlayer101Character.prefab  
      @ref BattlePlayer102Character.prefab  
      @ref BattlePlayer103Character.prefab  
      @ref BattlePlayer104Character.prefab  
      @ref BattlePlayer105Character.prefab  
      @ref BattlePlayer106Character.prefab

      Only character 103 "Sotaveteraani" is currently implemented. Implementation for it is done in the 100 character classes test script, and it applies to all characters of that class. When other characters are implemented, the code should be adjusted as needed.