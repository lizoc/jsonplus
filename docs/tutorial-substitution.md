Substitutions
=============
Substitutions are a way of referring to other parts of the data tree.

The syntax is `${pathexpression}` or `${?pathexpression}` where the `pathexpression` is a path expression as described above. This path expression has the same syntax that you could use for an object key.

The `?` in `${?pathexpression}` must not have whitespace before it; the three characters `${?` must be exactly like that, grouped together.

For substitutions which are not found in the data tree, implementations may try to resolve them by looking at system environment variables or other external sources. (More detail on environment variables in a later section.)

Substitutions are not parsed inside quoted strings. To get a string containing a substitution, you must use value concatenation with the substitution in the unquoted portion:

```
key : ${animal.favorite} is my favorite animal
```

Or you could quote the non-substitution portion:

```
key : ${animal.favorite}" is my favorite animal"
```

Substitutions are resolved by looking up the path in the data tree. The path begins with the root object, i.e. it is "absolute" rather than "relative."

Substitution processing is performed as the last parsing step, so a substitution can look forward in the data tree. If a data tree consists of multiple files, it may even end up retrieving a value from another file.

If a key has been specified more than once, the substitution will always evaluate to its latest-assigned value (that is, it will evaluate to the merged object, or the last non-object value that was set, in the entire document being parsed including all included files).

If a data tree sets a value to `null` then it should not be looked up in the external source. Unfortunately there is no way to "undo" this in a later file; if you have `{ "HOME" : null }` in a root object, then `${HOME}` will never look at the environment variable. There is no equivalent to JavaScript's `delete` operation in other words.

If a substitution does not match any value present in the data tree and is not resolved by an external source, then it is undefined. An undefined substitution with the `${foo}` syntax is invalid and should generate an error.

If a substitution with the `${?foo}` syntax is undefined:

 - if it is the value of an object member then the member should not be created. If the member would have overridden a previously-set value for the same member, then the previous value remains.  
 - if it is an array item, then the item should not be added.  
 - if it is part of a value concatenation with another string then it should become an empty string; if part of a value concatenation with an object or array it should become an empty object or array.  
 - `foo : ${?bar}` would avoid creating member `foo` if `bar` is undefined. `foo : ${?bar}${?baz}` would also avoid creating the member if _both_ `bar` and `baz` are undefined.

Substitutions are only allowed in member values and array items (value concatenations), they are not allowed in keys or nested inside other substitutions (path expressions).

A substitution is replaced with any value type (number, object, string, array, true, false, null). If the substitution is the only part of a value, then the type is preserved. Otherwise, it is value-concatenated to form a string.


Self-Referential Substitutions
------------------------------
The big picture:

 - substitutions normally "look forward" and use the final value for their path expression  
 - when this would create a cycle, when possible the cycle must be broken by looking backward only (thus removing one of the substitutions that's a link in the cycle)

The idea is to allow a new value for a member to be based on the older value:

```
    path : "a:b:c"     
    path : ${path}":d"
```

A _self-referential member_ is one which:

 - has a substitution, or value concatenation containing a substitution, as its value  
 - where this member value refers to the member being defined, either directly or by referring to one or more other substitutions which eventually point back to the member being defined

Examples of self-referential members:

 - `a : ${a}`  
 - `a : ${a}bc`  
 - `path : ${path} [ /usr/bin ]`

Note that an object or array with a substitution inside it is _not_ considered self-referential for this purpose. The self-referential rules do _not_ apply to:

 - `a : { b : ${a} }`  
 - `a : [${a}]`

These cases are unbreakable cycles that generate an error. If "looking backward" were allowed for these, something like `a={ x : 42, y : ${a.x} }` would look backward for a nonexistent `a` while resolving `${a.x}`.

A possible implementation is:

 - substitutions are resolved by looking up paths in a document. Cycles only arise when the lookup document is an ancestor node of the substitution node.  
 - while resolving a potentially self-referential member (any substitution or value concatenation that contains a substitution), remove that member and all members which override it from the lookup document.

The simplest form of this implementation will report a circular reference as missing; in `a : ${a}` you would remove `a : ${a}` while resolving `${a}`, leaving an empty document to look up `${a}` in. You can give a more helpful error message if, rather than simply removing the member, you leave a marker value describing the cycle. Then generate an error if you return to that marker value during resolution.

Cycles should be treated the same as a missing value when resolving an optional substitution (i.e. the `${?foo}` syntax). If `${?foo}` refers to itself then it's as if it referred to a nonexistent value.


The `+=` member separator
-------------------------
Members may have `+=` as a separator rather than `:` or `=`. A member with `+=` transforms into a self-referential array concatenation, like this:

```
a += b
// becomes
a = ${?a} [b]

```

`+=` appends an item to a previous array. If the previous value was not an array, an error will result just as it would in the long form `a = ${?a} [b]`. Note that the previous value is optional (`${?a}` not `${a}`), which allows `a += b` to be the first mention of `a` in the file (it is not necessary to have `a = []` first).


Examples of Self-Referential Substitutions
------------------------------------------
In isolation (with no merges involved), a self-referential member is an error because the substitution cannot be resolved:

```
foo : ${foo} // an error
```

When `foo : ${foo}` is merged with an earlier value for `foo`, however, the substitution can be resolved to that earlier value. When merging two objects, the self-reference in the overriding member refers to the overridden member. For instance:

```
foo : { a : 1 }
foo : ${foo}
```

Then `${foo}` resolves to `{ a : 1 }`, the value of the overridden member.

It would be an error if these two members were reversed, so:

```
foo : ${foo}
foo : { a : 1 }
```

Here the `${foo}` self-reference comes before `foo` has a value, so it is undefined, exactly as if the substitution referenced a path not found in the document.

Because `foo : ${foo}` conceptually looks to previous definitions of `foo` for a value, the error should be treated as "undefined" rather than "intractable cycle"; as a result, the optional substitution syntax `${?foo}` does not create a cycle:

```
foo : ${?foo} // this member just disappears silently
```

If a substitution is hidden by a value that could not be merged with it (by a non-object value) then it is never evaluated and no error will be reported. So for example:

```
foo : ${does-not-exist}
foo : 42
```

In this case, no matter what `${does-not-exist}` resolves to, we know `foo` is `42`, so `${does-not-exist}` is never evaluated and there is no error. The same is true for cycles like `foo : ${foo}, foo : 42`, where the initial self-reference must simply be ignored.

A self-reference resolves to the value "below" even if it's part of a path expression. So for example:

```
foo : { a : { c : 1 } }
foo : ${foo.a}
foo : { a : 2 }
```

Here, `${foo.a}` would refer to `{ c : 1 }` rather than `2` and so the final merge would be `{ a : 2, c : 1 }`.

Recall that for a member to be self-referential, it must have a substitution or value concatenation as its value. If a member has an object or array value, for example, then it is not self-referential even if there is a reference to the member itself inside that object or array.

Implementations must be careful to allow objects to refer to paths within themselves, for example:

```
bar : { 
    foo : 42,
    baz : ${bar.foo}
}
```

Here, if an implementation resolved all substitutions in `bar` as part of resolving the substitution `${bar.foo}`, there would be a cycle. The implementation must only resolve the `foo` member in `bar`, rather than recursing the entire `bar` object.

Because there is no inherent cycle here, the substitution must "look forward" (including looking at the member currently being defined). To make this clearer, `bar.baz` would be `43` in:

```
bar : { 
    foo : 42,
    baz : ${bar.foo}
}
bar : { foo : 43 }
```

Mutually-referring objects should also work, and are not self-referential (so they look forward):

```
// bar.a should end up as 4
bar : { a : ${foo.d}, b : 1 }
bar.b = 3

// foo.c should end up as 3
foo : { c : ${bar.b}, d : 2 }
foo.d = 4
```

Another tricky case is an optional self-reference in a value concatenation, in this example `a` should be `foo` not `foofoo` because the self reference has to "look back" to an undefined `a`:

```
    a = ${?a}foo
```

In general, in resolving a substitution the implementation must:

 - lazy-evaluate the substitution target so there's no "circularity by side effect"  
 - "look forward" and use the final value for the path specified in the substitution  
 - if a cycle results, the implementation must "look back" in the merge stack to try to resolve the cycle  
 - if neither lazy evaluation nor "looking only backward" resolves a cycle, the substitution is missing which is an error unless the `${?foo}` optional-substitution syntax was used.

For example, this is not possible to resolve:

```
bar : ${foo}
foo : ${bar}
```

A multi-step loop like this should also be detected as invalid:

```
a : ${b}
b : ${c}
c : ${a}
```

Some cases have undefined behavior because the behavior depends on the order in which two members are resolved, and that order is not defined. For example:

```
a : 1
b : 2
a : ${b}
b : ${a}
```
Implementations are allowed to handle this by setting both `a` and `b` to 1, setting both to `2`, or generating an error. Ideally this situation would generate an error, but that may be difficult to implement. Making the behavior defined would require always working with ordered maps rather than unordered maps, which is too constraining. Implementations only have to track order for duplicate instances of the same member (i.e. merges).

Implementations must set both `a` and `b` to the same value in this case, however. In practice this means that all substitutions must be memoized (resolved once, with the result retained). Memoization should be keyed by the substitution "instance" (the specific occurrence of the `${}` expression) rather than by the path inside the `${}` expression, because substitutions may be resolved differently depending on their position in the file.

