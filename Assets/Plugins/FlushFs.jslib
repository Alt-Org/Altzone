mergeInto(LibraryManager.library, {

  // https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
  // https://docs.unity3d.com/2022.1/Documentation/Manual/webgl-debugging.html
  // https://emscripten.org/docs/api_reference/Filesystem-API.html
  
  HelloWebGl: function () {
    console.log("Hello!");
  },

  FsSyncFs: function () {
    console.log("FS.syncfs");
	FS.syncfs(false, function (err) {
	  if (err) {
		console.log("Error: syncfs failed!"); 
		console.log(err); 
	  }
	 });  
  },

});