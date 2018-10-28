What is Json+
=============
Just yet another JSON dialect. We want an easier way to write JSON by hand.


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

```JSON
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
