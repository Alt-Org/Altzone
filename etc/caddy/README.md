# Caddy dev web server

This folder contains setup for **local WebGL build testing**.

Caddy executable _caddy.exe_ must be found here, it is not included in version control.  
It can be downloaded from official caddy [download](https://caddyserver.com/download) site.  
_caddy.exe_ should be added to `.gitignore` file.

Caddy configuration file _Caddyfile_ must be found here and is configured to handle compressed UNITY WebGL builds.  
Recommended compression method is [Brotli](https://en.wikipedia.org/wiki/Brotli).  
_(Uncompressed build size is 67.8 MB and compressed size is 9.1 MB)_

**WebGL build output** must go to _buildWebGL_ folder under projects root folder for caddy to find and serve it.

### Notes on compression

Uncompressed build size is about 85 Mb vs 9.5 MB with Brotli compression!

_It might be that checking 'Development Build' option disables compression!?_

_If you change compression options you might need to do 'clean build' to get the results on file system._

### Notes on caching

Caching is real PITA during web development phase :-(  
Typically you need be aware how your development tools (server, browser) handle caching and disable caching where appropriate to see changes during development take effect ASAP.

In addition to _normal_ caching web application can cache data in [browser storage](https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API) that must be cleared when necessary as well.

UNITY WebGL build tries to cache everything as much as possible to speed things up and consume less bandwidth.  
If you encounter errors when trying to load WebGL build, try clearing browser storage as well.

UNITY `Project Settings -> Player -> Publish Settings -> Data Caching` can be unchecked to to **disable** _browser local storage_ use. 
