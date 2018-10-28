Tips and Tricks
===============

1. Use `#` instead of `//`
Less characters to type :)


2. Use `=` instead of `:` when assigning object member values.
Again, less to type.


3. Quote your strings
Other than numbers, there are a number of keywords that the parser will interpret to non-string values, such as timespan, date/time and byte sizes. To avoid 
unintended surprises, just quote your strings whenever in doubt.


4. Choose your quote in the order `single` > `double` > `triple`
Single quotes are easier to type than double quotes, but use double quote if your string has single quote in it but no double quote. Use triple quotes when you 
need multiline strings.


5. Make full use of duplicate keys
Json+ automatically performs a deep merge on duplicate keys that holds object values. You can use this behavior to split your data over multiple files.


6. Don't use commas
Use line breaks in lists. Commas are redundant.


7. Don't use the assignment character when the value is an object. It is redundant.
It is redundant.

For example, use this:
```
fooo {
	bar = 5
}
```

and not this:
```
fooo = {
	bar = 5
}
```


8. Beware of extra whitespace in substitions
Unlike most scripting languages, concating strings is automatic. Therefore, whitespace between concating strings are always included:

```
foo = 'blah'
bar = ${foo} dah
# bar is now 'blah dah'. Notice the space in-between them is added.
car = ${foo} ' haha'
# car is now 'blah  haha'. Notice you get 2 spaces in between.
```

9. Be mindful of units
An integer followed by `ns`, `us`, `ms`, `s`, `m`, `h`, or `d` turns into a `TimeSpan`.

An integer followed by `kb`, `mb`, `gb`, `tb`, or `pb` turns into a 64-bit integer (using the suffix as a data size unit).

So quoting your strings is always good practice to avoid misunderstanding.


10. Units are case sensitive
`kb` means 1024, and `kB` means 1000. All data size units follow this pattern.


11. Json+ supports some special numbers
You can use these special keywords not allowed in JSON: `NaN`, `infinity`, `+infinity` and `-infinity`.

Again, these are case sensitive.
