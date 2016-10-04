Quick! Check your Properties (and Write Better Software)
===

This repository contains the slide deck, accompanying source code, and errata
for a presentation on random-testing on the CLR (.NET or Mono).

To date, this material has been presented as follows:

+ 08 OCT 2016 at Code Camp NYC [@codecampnyc](https://twitter.com/codecampnyc)

---

### Some notes about the source code

This solution replies on [Paket](http://fsprojects.github.io/Paket/) for
dependency management. And leverages [FAKE](http://fsharp.github.io/FAKE/) for
parts of the build process. So the easiest way to execute the full test suite,
and receive a nice summary, is to execute the following on a command prompt
(_note: this assumes you're working directory is a local copy of this repository_):

  ```
  > runner -ef report
  ```

Note the `-ef report` flag given above. As part of every test run, an HTML
report is generated. By passing the `-ef report` flag, you instruct the runner
to launch the latest report in the system-configures default HTML viewer (usually
a web browser). Meanwhile, issuing the command `> runner show_report` will simply
launch the latest report (if it exists) -- _without_ running any tests.

By default, all tests in all languages will be executed. If you'd like to limit
execution to a single language, use one of the following commands:

  + `> runner fs_tests` .. will only run the F# tests
  + `> runner cs_tests` .. will only run the C# tests
  + `> runner vb_tests` .. will only run the VB tests

Note, that the above commands can be used with or with the `-ef report` flag.
However, remember that a new report is generated everytime anyway.

_Additionally, this code has only been tested on Windows 10 (64-bit). However,
other platforms should work with little or no modification._

---

###### The slide deck and source code are released under the MIT license. Please see the [LICENSE](https://gitlab.com/pblasucci/quickpbt/blob/master/LICENSE.txt) file for further details.
