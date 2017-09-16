QUICK! Check your Properties
===

This repository contains the slide deck, accompanying source code, and errata
for a presentation on property-based testing, using FsCheck with .NET Core

To date, this material has NOT been presented.

---

### Overview

The code has been arranged as three independent projects, each inside its own folder:

+ `quickpbt_fs/` ... contains all of the example materials in F#
+ `quickpbt_vb/` ... contains all of the example materials in Visual Basic
+ `quickpbt_cs/` ... contains all of the example materials in C#

Note, however, each project is merely a transliteration of the others. 
Additionally, all three projects have been designed for use in conjunction with the `dotnet test` command-line interface.

For browsing the F# source, Visual Studio Code (with the Omnisharp and Ionide plugins) is recommended.

For browsing the Visual Basic source, Visual Studio 2017 (any edition, for Windows) is recommended.

For browsing the C# source, either Visual Studio Code (with the Omnisharp plugin) or Visual Studio 2017 (any edition, for Windows) is recommended.

_At the time of last update, none of the projects are known to load successfully in Jet Brain's Rider or Visual Studio for Mac._

### Pre-Requistes

+ .NET Core SDK (version 2.0 or higher)


### Running the Tests

Navigate to the desired project folder:
```
~ > cd quickpbt/quickbot_fs/
```

Restore any missing packages:
```
~/quickpbt/quickbot_fs> dotnet restore
```

Execute the full test suite:
```
~/quickpbt/quickbot_fs> dotnet test
```

Execute only tests related to a single category ("Teaser", for example):
```
~/quickpbt/quickbot_fs> dotnet test --filter=teaser
```

Note that the tests around "Observations" and/or "Data Generation" require a change to the default verbosity for the `dotnet` CLI:
```
~/quickpbt/quickbot_fs> dotnet test --filter=observations --verbosity normal
```

---

###### The slide deck and source code are released under the MIT license. Please see the [LICENSE](https://github.com/pblasucci/quickpbt/blob/master/LICENSE.txt) file for further details.
