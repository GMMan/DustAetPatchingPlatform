Dust: AET Patching Platform
===========================

The Dust: AET Patching Platform is used to modify and override code in Dust: AET as to improve how it runs and to fix bugs. The platform contains an exception catcher and runtime method replacement functions. Patches are compiled as DLLs that are loaded by the platform when the game runs. Patch files are loaded from the "patches" subfolder in the game folder.

To implement a patch, reference SteamWrapped.dll (DustAetPatchingPlatform project), and implement the `ILoader` interface. The `Priority` property determines loading order. By default it is `1000`, but you may use a lower number for a higher loading priority. Put your patch code in the `Load()` function. If you need to wait until the game fully loads, make a new function and call `Platform.RegisterLoadAfterGameInit()` with it.

This project contains code from Ziad Elmalki's [CLR Injection: Runtime Method Replacer](http://www.codeproject.com/Articles/37549/CLR-Injection-Runtime-Method-Replacer) article.
