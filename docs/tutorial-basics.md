JsonPlus Basics
===============
Read this first!


What is Json+
-------------
Json+ is a superset of JSON. 

Practically, this means that a Json+ parser can parse JSON, but a JSON parser cannot reliably parse Json+. This is because Json+ has extended the JSON grammer, hopefully so that it is easier and quicker to write, and less painful to read too.

The highlight features of Json+ are:
- less keystrokes to write
- reduce repetitions by reusing values
- support multi-line string value
- support comments

Although Json+ is flexible to write, it is designed to retain the deterministic nature of JSON. That means the syntax is strict and unambiguous, and the parser will throw an error when it encounters invalid code.

For performance reasons, Json+ is designed to require minimal lookahead. However, the added features necessarily means the Json+ parser needs to work harder than JSON parsers, and that sometimes translates to slower parsing. For that reason, we recommend using Json+ for writing short tree structure data that is less than 1MB. If performance is a concerning factor, pre-process Json+ to regular JSON, and then use a regular JSON parser.


Definitions
-----------
Here are some terms we throw around. You need to know what they mean to follow:
- a _value_ can be a _literal value_, an _array_, or an _object_.
- an _array_ contain a one-dimensional ordered list of _values_.
- a _literal value_ is a string, boolean, null, number (including NaN and infinity), and timespan (including infinite timespan).
- an object is a collection of _members_.
- a _member_ is a pair of _key_ and _value_. For example, the expression `"foo": "bar"` describes a key called "foo", and its value is "bar". The entire pair of key ("foo") and its value ("bar") is called a _member_ of an object (or just _member_). A _key_ can only be a string.

