# Documentation

at the moment this page is used as shared clipboard where people working on documentation can write down documentation templates for speed and consistency

later this page can used for more specific instructions about writing documentation

## External Hyperlink format

Markdown
```
[link text🡵](url)
[link text🡵]: url
```

C# Doc Comment / HTML
```
<see href="url">link text@u-exlink</>
<a href="url">link text@u-exlink</a>
```

@note 
External links should have the "🡵" unicode character or @@u-exlink at the end

@important
In source code @@u-exlink should be used instead of "🡵"

## Quantum System Class brief/summary format

System
```
<span class="brief-h">system name <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
// brief text
```

SystemSignalsOnly
```
<span class="brief-h">system name <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
// brief text
```

## Quantum System Update method brief/summary format
```
<span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
// brief text
@warning
This method should only be called by Quantum.
```

## Quantum System OnInit method brief/summary format
```
<span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System OnInit method@u-exlink</a> gets called when the system is initialized.</span><br/>
// brief text
@warning
This method should only be called by Quantum.
```

## Quantum System Signal method brief/summary format
```
<span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a> that gets called when <see cref="signal interface">signal interface</see> is sent.</span><br/>
// brief text
@warning
This method should only be called via Quantum signal.
```