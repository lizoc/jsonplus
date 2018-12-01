Path Expressions
================
Path expressions are used to write out a path through the object graph. They appear in two places; in substitutions, like `${foo.bar}`, and as the keys in objects like `foo.bar = 42`.

Path expressions are syntactically identical to a value concatenation, except that they may not contain substitutions. This means that you can't nest substitutions inside other substitutions, and you can't have substitutions in keys.

When concatenating the path expression, any `.` characters outside quoted strings are understood as path separators, while inside quoted strings `.` has no special meaning. So `foo.bar."hello.world"` would be a path with three keys, looking up key `foo`, key `bar`, then key `hello.world`.

You can use single quote instead of double quote: `foo.bar.'hello.world'` is also valid.

Character escape rules are the same as that for quoted strings.

The main tricky point is that `.` characters in numbers do count as a path separator. When dealing with a number as part of a path expression, it's essential to retain the _original_ string representation of the number as it appeared in the file (rather than converting it back to a string with a generic number-to-string library function).

 - `10.0foo` is a number, followed by the unquoted string `foo`. It should be interpreted as a two-key path with `10` and `0foo` as the keys.  
 - `foo10.0` is an unquoted string with a `.` in it, so this would be a two-key path with `foo10` and `0` as the keys.  
 - `foo"10.0"` is an unquoted then a quoted string which are concatenated, so this is a single-key path.  
 - `1.2.3` is the three-key path with `1`,`2`,`3`

Unlike value concatenations, path expressions are _always_ converted to a string, even if they are just a single value.

If you have an array or item value consisting of the single value `true`, it's a value concatenation and retains its character as a boolean value.

If you have a path expression (in a key or substitution) then it must always be converted to a string, so `true` becomes the string that would be quoted as `"true"`.

If a path key is an empty string, it must always be quoted. That is, `a."".b` is a valid path with three keys, and the middle key is an empty string. But `a..b` is invalid and should generate an error. Following the same rule, a path that starts or ends with a `.` is invalid and should generate an error.


Paths as keys
-------------
If a key is a path expression with multiple keys, it is expanded to create an object for each path key other than the last. The last path key, combined with the value, becomes a member in the most-nested object.

In other words:

```json+
foo.bar = 42

// is equivalent to
foo { bar = 42 }

```

and:

```json+
foo.bar.baz = 42

// is equivalent to
foo { bar { baz = 42 } }
```

and so on. These values are merged in the usual way; which implies that:

```
a.x = 42
a.y = 43

// is equivalent to
a { 
	x = 42
	y = 43
}
```

Because path expressions work like value concatenations, you can have whitespace in keys:

```
a b c = 42

// is equivalent to
"a b c" = 42
```

Because path expressions are always converted to strings, even single values that would normally have another type become strings.

- `true : 42` = `"true" : 42`    
- `3 : 42` = `"3" : 42`    
- `3.14 : 42` = `"3" : { "14" : 42 }`
