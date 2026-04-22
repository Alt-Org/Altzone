# Player Character Class 100 - Desensitizer {#page-concepts-player-class-100-desensitizer}

- **Simulation**
    - Has simulation logic  
      @cref{Battle.QSimulation.Player,BattlePlayerClassDesensitizer}
    - Has simulation data components  
      @cref{Quantum,BattlePlayerClassDesensitizerDataQComponent}
    - Has spec  
      @cref{Battle.QSimulation.Player,BattlePlayerClassDesensitizerQSpec}
    - **Projectile**
        - Has simulation logic  
         @cref{Battle.QSimulation.Player,BattlePlayerClassDesensitizerProjectileQSystem}
        - Has simulation data components  
        @cref{Quantum,BattlePlayerClassDesensitizerProjectileQComponent}
- **View**
    - Has view controller  
      @cref{Battle.View.Player,BattlePlayerClassDesensitizerViewController}

- **Prefabs**
    - Has no base prefab  
    - Character prefabs  
      @ref BattlePlayer101Character.prefab  
      @ref BattlePlayer102Character.prefab  
      @ref BattlePlayer103Character.prefab  
      @ref BattlePlayer104Character.prefab  
      @ref BattlePlayer105Character.prefab  
      @ref BattlePlayer106Character.prefab

      Only character 103 "Sotaveteraani" is currently implemented. Implementation for it is done in the Desensitizer character classes test script, and it applies to all characters of that class. When other characters are implemented, the code should be adjusted as needed.