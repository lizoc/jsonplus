Concatenation and Object Merging
================================
Json+ has advanced rules to deep merge objects and concat values. This can be very powerful when used correctly.


Duplicate keys and object merging
---------------------------------
It is not clear under the JSON spec how duplicate keys in the same object should be handled. In Json+, duplicate keys that appear later override those that appear earlier, unless both values are objects. If both values are objects, then the objects are merged.

Note: some JSON parsing implementations (such as `NewtonsoftKing` do not throw errors when encountering duplicate keys). Technically, this may contradict our claim that Json+ is a superset of JSON, because we are assuming that JSON does not support duplicate keys.

To merge objects:
- add members present in only one of the two objects to the merged object.  
- for non-object-valued members present in both objects, the member found in the second object must be used.  
- for object-valued members present in both objects, the object values should be recursively merged according to these same rules.

Object merge can be prevented by setting the key to another value first. This is because merging is always done two values at a time; if you set a key to an object, a non-object, then an object, first the non-object falls back to the object (non-object always wins), and then the object falls back to the non-object (no merging, object is the new value). So the two objects never see each other.

These two are equivalent:

```json+
foo { a = 42 }
foo { b = 43 }
```

-vs-

```json+
foo { a = 42, b = 43 }
```

And these two are equivalent:

```json+
foo = { a = 42 }
foo = null
foo = { b = 43 }
```

-vs-

```json+
foo { b = 43 }
```

The intermediate setting of `"foo"` to `null` prevents the object merge.


Value Concatenation 101
-----------------------
Json+ automaticallly concat values placed together. There are three kinds of value concatenation:
- if all the values are literal values (neither objects nor arrays), they are concatenated into a string.
- if all the values are arrays, they are concatenated into one array.
- if all the values are objects, they are merged (as with duplicate keys) into one object.

String value concatenation happens with member keys, member values and array items.


String Concatenation
--------------------
String value concatenation is the trick that makes unquoted strings work; it also supports substitutions (`${foo}` syntax) in strings.

Only literal values participate in string value concatenation. Recall that a literal value is any value other than arrays and objects.

As long as literal values are separated only by non-newline whitespace, the _whitespace between them is preserved_ and the values, along with the whitespace, are concatenated into a string.

String value concatenations never span a newline, or a character that is not part of a literal value.

A string value concatenation may appear in any place that a string may appear, including object keys, object values, and array items.

Whenever a value would appear in JSON, a Json+ parser instead collects multiple values (including the whitespace between them) and concatenates those values into a string.

Whitespace before the first and after the last literal value are always discarded. Only whitespace _between_ literal values must be preserved.

So for example ` foo bar baz ` parses as three unquoted strings, and the three are value-concatenated into one string. The inner whitespace is kept and the leading and trailing whitespace is trimmed. The equivalent string, written in quoted form, would be `"foo bar baz"`.

Value concatenating `foo bar` (two unquoted strings with whitespace) and quoted string `"foo bar"` would result in the same in-memory representation, seven characters.

For purposes of string value concatenation, non-string values are converted to strings as follows (strings shown as quoted strings):

- `true` and `false` become the strings `"true"` and `"false"`.  
- `null` becomes the string `"null"`.  
- quoted and unquoted strings are themselves.  
- numbers should be kept as they were originally written in the file. For example, `0x10` is 16 when by itself, but for purposes of value concatenation, it should be rendered as `0x10` literally.  
- a substitution is replaced with its value which is then converted to a string as above.  
- if an array or object appears in a string value concatenation, an error occurs.

A single value is never converted to a string. That is, it would be wrong to value concatenate `true` by itself; that should be parsed as a boolean-typed value. Only `true foo` (`true` with another simple value on the same line) should be parsed as a value concatenation and converted to a string.


Array and object concatenation
------------------------------
Arrays can be concatenated with arrays, and objects with objects, but an error occurs when an attempt is made to concatenation them together.

For purposes of concatenation, "array" also means "substitution that resolves to an array" and "object" also means "substitution that resolves to an object."

Within an member value or array item, if only non-newline whitespace separates the end of a first array or object or substitution from the start of a second array or object or substitution, the two values are concatenated. Newlines may occur _within_ the array or object, but not _between_ them. Newlines _between_ prevent concatenation.

For objects, "concatenation" means "merging", so the second object overrides the first.

Arrays and objects cannot be member keys, whether concatenation is involved or not.

Here are several ways to define `a` to the same object value:

```
// one object
a : { b : 1, c : 2 }

// two objects that are merged via concatenation rules
a : { b : 1 } { c : 2 }

// two members that are merged
a : { b : 1 }
a : { c : 2 }
```

Here are several ways to define `a` to the same array value:

```
// one array
a : [ 1, 2, 3, 4 ]

// two arrays that are concatenated
a : [ 1, 2 ] [ 3, 4 ]

// a later definition referring to an earlier
// (see "self-referential substitutions" below)
a : [ 1, 2 ]
a : ${a} [ 3, 4 ]
```

A common use of object concatenation is "inheritance":

```
data-center-unspecific = { cluster-size = 6 }
data-center-east = ${data-center-unspecific} { name = "east" }
```

A common use of array concatenation is to add to paths:

```
path = [ /bin ]
path = ${path} [ /usr/bin ]
```

Concatenation with Whitespace and Substitutions
-----------------------------------------------
When concatenating substitutions such as `${foo} ${bar}`, the substitutions may turn out to be strings (which makes the whitespace between them significant) or may turn out to be objects or lists (which makes it irrelevant). Unquoted whitespace must be ignored in between substitutions which resolve to objects or lists. Quoted whitespace will result in an error.


Arrays without commas or newlines
---------------------------------
Json+ allows items in a array to be separated using newlines instead of commas. Whitespace other than newlines can result in some perculiar behaviors due to concatentation:

```
// this is an array with one item, the string "1 2 3 4"
[ 1 2 3 4 ]

// this is an array of four integers
[ 1       2       3       4 ]

// an array of one item, the array [ 1, 2, 3, 4 ]
[ [ 1, 2 ] [ 3, 4 ] ]

// an array of two arrays
[ [ 1, 2 ]       [ 3, 4 ] ]
```

If this gets confusing, just use commas. The concatenation behavior is useful rather than surprising in cases like:

```
[ This is an unquoted string my name is ${name}, Hello ${world} ]
[ ${a} ${b}, ${x} ${y} ]
```

Non-newline whitespace is never an item or member separator.

