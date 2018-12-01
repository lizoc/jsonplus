Json+ Syntax
============
Much of this specification is defined with reference to JSON; you can find the JSON spec at http://json.org/.


Unchanged from JSON
-------------------
- files should be valid UTF-8
- an empty file or file consisting solely of whitespace results in an error
- values in quotes `"` are strings
- value types: string, number, boolean, null, object and array


Alternate characters
--------------------
Json+ defines alternative substitutes for special characters and keywords in JSON syntax:

| JSON     | Json+     | Description         |
|----------|-----------|---------------------|
| `"`      | `'`       | Quoted string       |
| `:`      | `=`       | Key-value separator |
| `true`   | `yes`     | True boolean        |
| `false`  | `no`      | False boolean       |

Here's a comparison sample:

```json
{
	"foo": true,
	"bar": "hello world"
}
```

```json+
{
	'foo' = yes
	'bar' = 'hello world'
}
```


Additional types
----------------
Json+ extends JSON types to include timespan:
- `timespan`: represents a period of time
- `ns` for nanosecond
- `us` for microsecond
- `ms` for millisecond
- `s` for second
- `m` for minute
- `h` for hour
- `d` for day


Numbers
-------
Json+ supports all numbers in JSON, as well as `NaN`, `infinity`, `+infinity`, and `-infinity`.

You can append a data size unit to a number, for example:
 - `1kB` creates a number value of `1,000`
 - `1mB` creates a number value of `1,000,000`
 - `1gB` creates a number value of `1,000,000,000`
 - `1tB` creates a number value of `1,000,000,000,000`
 - `1pB` creates a number value of `1,000,000,000,000,000`
 - `1kb` creates a number value of `1,024`
 - `1mb` creates a number value of `1,024^2`
 - `1gb` creates a number value of `1,024^3`
 - `1tb` creates a number value of `1,024^4`
 - `1pb` creates a number value of `1,024^5`

The two-letter unit strings is case sensitive. There must not be any whitespace between the number and the unit.

There is an unfortunate nightmare with size-in-bytes units, that they may be in powers or two or powers of ten. The approach defined by standards bodies appears to differ from common usage, such that following the standard leads to people being confused. Worse, common usage varies based on whether people are talking about RAM or disk sizes, and various existing operating systems and apps do all kinds of different things. See [http://en.wikipedia.org/wiki/Binary_prefix#Deviation_between_powers_of_1024_and_powers_of_1000](this wiki article) for examples. The notation adopted by Json+ is more of a consideration of convenienence than attempting to follow any standard at all.

Note: any value in zetta/zebi or yotta/yobi will overflow a 64-bit integer, and of course large-enough values in any of the units may overflow. Care must be taken to cath such overflow exceptions when using the parser.


Comments
--------
Anything between `//` or `#` and the next newline is considered a comment and ignored, unless the `//` or `#` is inside a quoted string.


Optional root braces
--------------------
JSON documents must have an array or object at the root. Empty files are invalid documents, as are files containing only a non-array and non-object value such as a string.

In Json+, if the file does not begin with a square bracket or curly brace, it is parsed as if it were enclosed with `{}` curly braces.

```json
{
	"foo": 123
}
```

```json+
foo: 123
```

A Json+ file is invalid if it omits the opening `{` but still has a closing `}`; the curly braces must still be balanced.

```json+
# this won't work
foo: 123
}
```


The assignment symbol
---------------------
In addition to the colon `:`, Json+ allows an alternative assignment character `=`:

```json
{
	"foo": "bar"
}
```

```json+
{
	"foo" = "bar"
}
```

Json+ allows the assignment symbol to be omitted when the value is an object:
```json
{
	"foo": { "bar": 123 }
}
```

```json+
{
	"foo" { "bar": 123 }
}
```


Separators
----------
JSON requires that items in arrays, as well as object members, be separated by a comma `,`, except for the last item.

In Json+, you can replace the comma for a newline character (`\n`, decimal value 10).

The last item in an array or the last member in an object may be followed by a single comma. This extra comma is ignored.

Here are some examples to illustrate:

- `[1,2, 3]` and `[1,2,3]` are the same array.  
- `[1\n2\n3]` and `[1,2,3]` are the same array (where `\n` denotes a newline character).  
- `[1,2,3,]` and `[1,2,3]` are the same array.  
- `[1,2,3,,]` is invalid because it has two trailing commas.  
- `[,1,2,3]` is invalid because it has an initial comma.  
- `[1,,2,3]` is invalid because it has two commas in a row.  
- these same rules apply to members in objects.


Whitespace
----------
While the term "whitespace" is not defined in JSON, it is defined in Json+ as:

 - any Unicode space separator (Zs category), line separator (Zl category), or paragraph separator (Zp category), including nonbreaking spaces (such as 0x00A0, 0x2007, and 0x202F). The BOM (0xFEFF) must also be treated as whitespace.  
 - tab (`\t` 0x0009), newline (`\n` 0x000A), vertical tab (`\v` 0x000B), form feed (`\f` 0x000C), carriage return (`\r` 0x000D), file separator (0x001C), group separator (0x001D), record separator (0x001E), unit separator (0x001F).

While all Unicode separators should be treated as whitespace, here we refer to "newline" as the ASCII newline 0x000A.


Unquoted strings
----------------
Depending on the content, Json+ may allow you to define a string without using quotes:
```json+
foo = bar
```

Obviously, this comes with some caveats. You can only omit the quotes if the string:
- it does not contain "reserved characters": `$`, `"`, `'`, `{`, `}`, `[`, `]`, `:`, `=`, `,`, `+`, `#`, <code>&grave;</code>, `^`, `?`, `!`, `@`, `*`, `&`, `\`, or whitespace.
- it does not contain the two-character string `//` (which starts a comment)  
- its initial characters do not parse as `true`, `false`, `yes`, `no`, `null`, or a number.
- it does not parse as a timespan or data size unit
- it does not contain an character that must be escaped

In all other cases, you must use a quoted string.

Let's see some examples:

`truefoo` parses as the boolean token `true` followed by the unquoted string `foo`. However, `footrue` parses as the unquoted string `footrue`. Similarly, `10.0bar` is the number `10.0` then the unquoted string `bar` but `bar10.0` is the unquoted string `bar10.0`. In practice, this distinction doesn't matter much because of [./tutorial-concat.md](value concatenation).

In general, once an unquoted string begins, it continues until a reserved character or the two-character comment string `//` is encountered. Embedded (non-initial) booleans, nulls, and numbers are not recognized as such, they are part of the string.

An unquoted string may not _begin_ with the digits 0-9 or with a hyphen (`-`, 0x002D) because those are valid characters to begin a JSON number. The initial number character, plus any valid-in-JSON number characters that follow it, must be parsed as a number value. Again, these characters are not special _inside_ an unquoted string; they only trigger number parsing if they appear initially.

Note that quoted JSON strings may not contain control characters (control characters include some whitespace characters, such as newline). This rule is from the JSON spec. However, unquoted strings have no restriction on control characters, other than the ones listed as "reserved characters" above.

Some of the "reserved characters" are reserved because they already have meaning in JSON or Json+, others are essentially reserved keywords to allow future extensions to this spec.


Quoted strings
--------------
A string is quoted between two `"` characters on the same line. All characters between the quotes are interpreted literally, except for the following:
- the character `"` must be represented by `\"`. This is in line with JSON syntax.
- the character `\` must be represented by `\\`.
- the character `'` _may_ be represented by `\'`.
- the character `/` _may_ be represented by `\/'`.
- line feed must be represented by `\n`.
- carriage return must be represented by `\r`.
- tabulate must be represented by `\t`.
- bell must be represented by `\b`.
- vertical tabulate must be represented by `\f`.
- unicode character can be referenced by its 4 digit hexadecimal code: `\u0xABCD` (where ABCD is the hexadecimal code)

The alternative quote character `'` may be used in place of `"`. The rules are similar to using `"`, with this difference:
- the character `"` _may_ be represented by `\"`.
- the character `'` must be represented by `\'`.


Multi-line strings
------------------
Multi-line strings are similar to Python or Scala, using triple quotes. If the three-character sequence `"""` appears, then all unicode characters until a closing `"""` sequence are used unmodified to create a string value. Newlines and whitespace receive no special treatment. The alternative triple single quote `'''` may be used instead.

Escape character rules are also similar to quoted string, except:
- the character `"` _may_ be represented by `\"`.
- the character `'` _may_ be represented by `\'`.
- line feed _may_ be represented by `\n`, or an actual line feed character.
- carriage return _may_ be represented by `\r`, or an actual line feed character.
- tabulate _may_ be represented by `\t`, or an actual tabulate character.

Special care must be taken if the last character in the triple-quoted string is `"`. For example, `foo"` when triple-quoted will result in `"""foo""""`. You need to escape this last character to avoid an error: `"""foo\""""`. Alternatively, use the triple single quote: `'''foo"'''`.
