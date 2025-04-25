# Documentation

at the moment this page is used as shared clipboard where people working on documentation can write down documentation templates for speed and consistency

later this page can used for more specific instructions about writing documentation

## External Hyperlink format

Markdow
```
[link text🡵](url)
[link text🡵]: url
```

C# Doc Comment / HTML
```
<see href="url">link text🡵</>
<a href="url">link text🡵</a>
```

@note 
External links should have the "🡵" unicode character at the end

## Quantum System Update method brief/summary format
```
<span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method🡵</a> gets called every frame.</span><br/>
// brief text
@warning
This method should only be called by Quantum.
```

## Quantum System OnInit method brief/summary format
```
<span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System OnInit method🡵</a> gets called when the system is initialized.</span><br/>
// brief text
@warning
This method should only be called by Quantum.
```
