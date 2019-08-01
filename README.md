<h1 align="center">
  <img src="https://raw.githubusercontent.com/lizoc/jsonplus/master/icon.png" height="150" width="150"/>
  <p align="center">Json+</p>
  <p align="center" style="font-size: 0.5em">Just yet another JSON dialect.</p>
</h1>
<p align="center">
    <a href="https://www.nuget.org/packages/Lizoc.JsonPlus"><img src="https://img.shields.io/nuget/v/Lizoc.JsonPlus.svg?style=for-the-badge" alt="NuGet Package"></a>
    <a href="https://www.powershellgallery.com/packages/JsonPlus"><img src="https://img.shields.io/powershellgallery/v/jsonplus.svg?style=for-the-badge" alt="PowerShell Gallery"></a>
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge" alt="MIT License"></a>
</p>

Json+ is **forked from [HOCON](https://github.com/akkadotnet/HOCON)**.

What does this do?
==================
Json+ is a JSON dialect that we think is easier to write by hand. This repo has parser for Json+, written in C#.

Json+ is **initially forked from [HOCON](https://github.com/akkadotnet/HOCON)** in October 2018, and the code base has diverged since. Some primary differences are:

- PowerShell integration through the `ConvertFrom-JsonPlus` cmdlet
- More robust substitution operations with several bug fixes
- More terse syntax


Show me the money
-----------------
A quick compare-and-contrast:

```JSON
{
    "foo": "abc",
    "bar": "abc\ndef",
    "taz": {
        "z": "abcabc"
    }
}
```

If you have written JSON by hand, you probably have experienced these:

- forget a comma delimiter between items in an array
- had an extra comma delimiter at the last item in an array
- tirelessly "quoting" your keys and strings
- got lost with escape characters
- no place to write your comments
- wishing for variables

Json+ to the rescue!

```python
foo = abc
# a comment!
bar = """abc
def"""
taz {
    z = ${foo}${foo}
}
```

Seriously, it is like writing an INI file, but with comments, multi-line, variables and stuff.

What's more! Json+ is a superset of JSON, so it can parse your existing JSON text as well.


Why would I use this?
---------------------
First of all, you **shouldn't** use this if:
- Your JSON is generated for serialization purposes
- You are already happy with your JSON editing experience, probably using some fancy IDE
- Every microsecond counts when you are parsing JSON

We are using Json+ as a project file in our build scripts, as configuration file, and data source for text templates.


Getting started
---------------
The latest package is [available on NuGet](https://www.nuget.org/packages/Lizoc.JsonPlus). Simply reference it in your .NET project file or `packages.config`.

If you are interested in working with Json+ inside PowerShell, the latest module is hosted on PowerShell Gallery:

```PowerShell
Install-Package JsonPlus
```


More details
------------
Look at [more examples](./docs/example.md) here, or read [the whole spec](./docs/spec.md).

The documentation index is [here](./docs/README.md).


Building from source
--------------------
This repo is built on Windows 10.

Just clone this repo, `cd` to the repo directory, and execute things in the following order:

```batch
build configure
build release *
```

For more build options, run `build /?`.
