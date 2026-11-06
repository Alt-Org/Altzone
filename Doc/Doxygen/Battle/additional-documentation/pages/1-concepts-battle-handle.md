# Battle Handle {#page-concepts-battle-handle}

The **%Battle Handle Pattern** is a way to handle a collection of data. At this moment, it is mainly used to handle **Entity** types through [{Entity Type Handles}](#page-concepts-battle-handle-entity-type-handle).  
Regardless of their type, **Handles** are not part of **%Quantum's** structure, can be created as needed,
and <span style = color:var(--warning-color-text);>**should not be stored across frames as it would break %Quantum's structure**</span>.

As stated before, **Handles** are collections of data that make sense to group together for ease of use, usually consisting of references to the actual data.  
In code, a **Handle** is simply a **struct** that contains data/references and can implement properties and methods for accessing and managing that data.  
One type of method that should be implemented is one or more **static Create methods** for creating **Handle instances**.  
Obviously, the **Create** methods take in arguments used for creating the instance,
which in many cases are **Identifiers** that are used to **fetch data** for the collection during the creation process.

C#
```cs
struct Battle(name)Handle
{
  public static Battle(name)Handle Create(/*arguments and/or identifiers*/)

  // Fields/Properties for the data/references
  ...

  // Implementation for the properties and methods
  ...
}
```

<br/>

## Entity Type Handle {#page-concepts-battle-handle-entity-type-handle}

An **Entity Type Handle** is a [{Battle Handle}](#page-concepts-battle-handle)
for a collection of data that is composed of an **%Quantum EntityRef** and pointers to **%Quantum Components** that the **Entity** uses.

Each **Entity Type Handle** usually has a [{Create method}](#page-concepts-battle-handle)
that takes an **EntityRef** as an [{Identifier}](#page-concepts-battle-handle) used to fetch the **Components**.

An **Entity Type Handle** can have a [{Handle Link Component}](#page-concepts-battle-handle-entity-type-handle-link-component),
allowing the use of any [{Linked/Associated}](#page-concepts-battle-handle-entity-type-handle-link-component) **Entities** as an argument.  
Each **Entity Type Handle** that has a [{Handle Link Component}](#page-concepts-battle-handle-entity-type-handle-link-component) should implement a **static CreateLink** method
for creating and adding it's respective [{Handle Link Component}](#page-concepts-battle-handle-entity-type-handle-link-component) to a given **Entity** that should be
[{Linked/Associated}](#page-concepts-battle-handle-entity-type-handle-link-component).

**Entity Type Handles** can also be extended to contain other data/references and
implement other properties and methods overlapping with the more general [{Battle Handle}](#page-concepts-battle-handle).

C#
```cs
struct Battle(entity type)Handle
{
  public static Battle(entity type)Handle Create(EntityRef entityRef)

  // Fields for EntityRef, Component pointers and other data/references
  ...

  // Implemented if the Entity Type Handle has a {Handle Link Component}
  public static void CreateLink(Frame f, EntityRef entityRef, EntityRef linkEntityRef)

  // Implementation for other properties and methods
  ...
}

```

### Handle Link Component {#page-concepts-battle-handle-entity-type-handle-link-component}

Each **Entity Type** that has a [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle) can also have a **Link Component** associated with it.  
The **Link Component** can be attached to any **Entity** associated with it's respective **Entity Type**.  
**Link Components** contain [{Identifiers}](#page-concepts-battle-handle)
that are used during the respective [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle) **Creation Process**.  
In practice, this forms a **link/association** between some **Entity** and a specific **Entity Type**, which allows for the **Creation** of an [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle)
for an **Entity Type** from any **Entity** that is associated with it.

<br/>

## Managed Entity Handle {#page-concepts-battle-handle-managed-entity-handle}

The @cref{Battle.QSimulation.Game,BattleManagedEntityHandle} is a type of [{Entity Type Handle}](#page-concepts-battle-handle-type-handle)
used to handle [{Managed entities}](#page-concepts-entity-management-managed-entities).  
The @cref{Battle.QSimulation.Game,BattleManagedEntityHandle} also contains the @cref{Battle.QSimulation.Game.BattleManagedEntityHandle,IComposite} interface
that is implemented by [{Managed Composite Type Handles}](#page-concepts-battle-handle-managed-composite-type-handle).

<br/>

## Managed Composite Entity Type Handle {#page-concepts-battle-handle-managed-composite-type-handle}

A **Managed Composite Entity Type Handles** is an [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle)
for a [{Managed entity}](#page-concepts-entity-management-managed-entities) of a more **specific type**.  
A **Managed Composite Entity Type Handles** contains a [{Managed Entity Handle}](#page-concepts-battle-handle-managed-entity-handle)
instead of directly containing a **%Quantum EntityRef**, forming the **Composite**, and implements the @cref{Battle.QSimulation.Game,BattleManagedEntityHandle.IComposite} interface.

If a [{Managed Entity}](#page-concepts-entity-management-managed-entities) is [{Registered}](#page-concepts-entity-management-registered-entities),
it's corresponding **Managed Composite Entity Type Handle** should protect the **Transform Component**,
giving an error when there is a **possible attempt to modify the transform** when the [{Managed Entity}](#page-concepts-entity-management-managed-entities)
is [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate).

**Managed Composite Entity Type Handles** can also be extended to contain other data/references and
implement other properties and methods overlapping with the more general [{Battle Handle}](#page-concepts-battle-handle).

C#
```cs
struct Battle(entity type)Handle : BattleManagedEntityHandle.IComposite
{
  public static Battle(entity type)Handle Create(EntityRef entityRef)

  // Fields for ManagedEntityHandle, Component pointers and other data/references
  ...

  // Should be implemented if the Entity Type Handle has a {Handle Link Component}
  public static void CreateLink(Frame f, EntityRef entityRef, EntityRef linkEntityRef)

  //{ Transform Component access should be implemented with protection

  // pointer for mutable access with protection
  public Transform2D* TransformPtr
  {
    get
    {
        // assert that the {Managed Entity} is InPlay
        return _transformInternalPtr;
    }
  }

  // access to a copy, ensuring immutability
  public Transform2D TransformCopy => *_transformInternalPtr;

  // private internal storage for the transform pointer
  private Transform2D* _transformInternalPtr { get; set; }

  //} Transform Component access should be implemented with protection

  void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)

  // Implementation for other properties and methods
  ...
}

```

<br/>

---

<br/>

## Handle List {#page-concepts-battle-handle-list}

 - @cref{Battle.QSimulation.Game,BattleManagedEntityHandle}:
   [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle),
   ([Special Case](#page-concepts-battle-handle-managed-entity-handle))  
   Handles [{Managed entities}](#page-concepts-entity-management-managed-entities).
 - @cref{Battle.QSimulation.Game,BattlePlayerHandle}:
   [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle),
   ([Special Case](#page-concepts-player-simulation-management-playerhandle))  
   Handles players.
 - @cref{Battle.QSimulation.Game,BattlePlayerShieldHandle}:
   [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle)  
   Handles player shields.
 - @cref{Battle.QSimulation.Game,BattleProjectileHandle}:
   [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle)  
   Handles the projectile.