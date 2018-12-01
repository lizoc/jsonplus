Includes
========
An _include statement_ consists of the unquoted string `include` or `include?`, followed by whitespace and then:  
- a _single quoted_ string which is interpreted. 
- a _double quoted_ string which is interpreted. 

An include statement can appear in place of an object member.

If the unquoted string `include` (or `include?`) appears at the start of a path expression where an object key would be expected, then it is not interpreted as a path expression or a key.

Instead, the next value must be a _quoted_ string. This value is the _file name_.

Together, the include directive and the resource name substitute for an object member syntactically, and are separated from the following object members or includes by the usual comma (and as usual the comma may be omitted if there's a newline).

If an include directive at the start of a key is followed by anything other than a single quoted string, it is invalid and an error should occur.

There can be any amount of whitespace, including newlines, between the include directiveand the file name.


Substitution
------------
Value concatenation is **NOT** performed on the "argument" to `include`. The argument must be a single quoted string. No substitutions are allowed, and the argument may not be an unquoted string or any other kind of value.


Non-directive
-------------
Unquoted `include` has no special meaning if it is not the start of a key's path expression.

It may appear later in the key:

```
# this is valid
{ foo include : 42 }

# equivalent to
{ "foo include" : 42 }
```

It may appear as an object or array value:

```
{ foo : include }   # value is the string "include"
[ include ]         # array of one string "include"
```

You can quote `"include"` if you want a key that starts with the word `"include"`, only unquoted `include` is special:

```
{ "include" : 42 }
```


Include Semantics: Merging
--------------------------
An _including file_ contains the include directive and a _file name_. 'File' does not refer only to regular files on a filesystem, but assume they are for the moment.

An included file must contain an object, not an array. This is significant because both JSON and Json+ allow arrays as root values in a document.

If an included file contains an array as the root value, it is invalid and an error should occur.

The included file should be parsed, producing a root object. The keys from the root object are conceptually substituted for the include statement in the including file.
- If a key in the included object occurred prior to the include statement in the including object, the included key's value overrides or merges with the earlier value, exactly as with duplicate keys found in a single file.  
- If the including file repeats a key from an earlier-included object, the including file's value would override or merge with the one from the included file.


Include Semantics: Substitution
-------------------------------
Substitutions in included files are looked up at two different paths; first, relative to the root of the included file; second, relative to the root of the including data source.

Recall that substitution happens as a final step, _after_ parsing. It should be done for the entire data tree, not for single files in isolation.

Therefore, if an included file contains substitutions, they must be "fixed up" to be relative to the root.

Say for example that the root is this:

```
{ 
  a : { 
    include "foo.conf" 
  }
}
```

And "foo.conf" might look like this:

```
{ x : 10, y : ${x} }
```

If you parsed "foo.conf" in isolation, then `${x}` would evaluate to 10, the value at the path `x`. If you include "foo.conf" in an object at key `a`, however, then it must be fixed up to be `${a.x}` rather than `${x}`.

Say that the root redefines `a.x`, like this:

```
{
    a : { include "foo.conf" }         
    a : { x : 42 }     
}
```

Then the `${x}` in "foo.conf", which has been fixed up to `${a.x}`, would evaluate to `42` rather than to `10`. Substitution happens _after_ parsing the whole data tree.

However, there are plenty of cases where the included file might intend to refer to the root data tree. For example, to get a value from a system property or from the reference data source. So it's not enough to only look up the "fixed up" path, it's necessary to look up the original path as well.


Include Semantics: Missing Files
--------------------------------
By default, if an included file does not exist, an error occurs.

If however an include directive is optional (indicated using the `include?` directive), then the parsing will not fail if the file cannot be resolved.

The syntax for this can be:

```
include? "foo.conf"
```

Other I/O errors probably should not be ignored but implementations will have to make a judgment which I/O errors reflect an ignorable missing file, and which reflect a problem to bring to the user's attention.


Include Semantics: File Formats and Extensions
----------------------------------------------
Implementations may support including files in other formats. Those formats must be compatible with the JSON type system, or have some documented mapping to JSON's type system.

If an implementation supports multiple formats, then the extension may be omitted from the name of included files:

```
include "foo"
```

If a filename has no extension, the implementation should treat it as a basename and try loading the file with all known extensions.

If the file exists with multiple extensions, they should _all_ be loaded and merged together.

Files in Json+ format should be parsed last. Files in JSON format should be parsed next-to-last.

In short, `include "foo"` might be equivalent to:

```
include "foo.properties"
include "foo.json"
include "foo.conf"
```

This same extension-based behavior is applied to assembly resources and files.

For URLs, a basename without extension is not allowed; only the exact URL specified is used. The format will be chosen based on the Content-Type if available, or by the extension of the path component of the URL if no Content-Type is set. This is true even for file: URLs.


Include Semantics: Locating Resources
-------------------------------------
The Json+ parser may support multiple types of resources by analyzing the file name pattern, such as:

- a URL, if the quoted string is a valid URL with a known protocol
- a resource embedded in a library file
- otherwise, a file "adjacent to" the one being parsed and of the same type as the one being parsed. The meaning of "adjacent to", and the string itself, has to be specified separately for each kind of resource.

Implementations may vary in the kinds of resources they can include.

For plain files on the filesystem:

 - if the included file is an absolute path then it should be kept absolute and loaded as such.  
 - if the included file is a relative path, then it should be located relative to the directory containing the including file. The current working directory of the process parsing a file must NOT be used when interpreting included paths.  
 - if the file is not found, return an empty JSON file.

URLs:

 - for files loaded from a URL, "adjacent to" should be based on parsing the URL's path component, replacing the last path key with the included name.  
 - file: URLs should behave in exactly the same way as a plain filename

Implementations need not support files, assembly resources, or URLs; and they need not support particular URL protocols. However, if they do support them they should do so as described above.

