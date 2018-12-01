Examples
========
Here is an example to demonstrate how Json+ feels like.

Let's see how the Json+ parser sees the values as it goes down the blocks below.

```
mylib {
    foo = "foo from first block"
    bar = "bar from first block"
    baz = "baz from first block"
}
```

Now the data tree is pretty straight forward. There is an object called "mylib", with properties "foo", "bar", and "baz" set to string values.

More stuff for the parser below:

```
myapp {
    the-answer = 42
}

mylib.foo = "foo overriden in second block"
mylib.bar = "bar overriden in second block"
```

Now there is a new object called "myapp", with one property called "the-answer". The object "mylib" has changed the values of its properties "foo" and "bar".

The parser continues on its adventure:

```
hammer {
    caret = 24
}

mylib.foo = "foo overriden in third block"
mylib.baz = "baz overriden in third block"
```

Another new object is created, called "hammer", with one property called "caret". The object "mylib" encounters more changes to its property "foo", and its property "baz" gets changed too.

Now the parser analyze the final section:

```
hammer {
    caret = 18

	context {
		config = ${mylib}
		config {
			bar = "bar overriden by the hammer"
		}
	}
}
```

The object "hammer" has the following changes:
- the value of its property "caret" is changed from 24 to 18.
- a new property called "context" is created, and its value is an object, with a property called "config.
- the value of this "config" property is exactly the same as that of the "mylib" at the root: it has 3 properties called "foo" ("foo overriden in third block"), "bar" ("bar overriden in second block"), and "baz" ("baz overriden in third block").
- the value of `hammer` > `context` > `config` > `bar` is further overriden to "bar overriden by the hammer".


Final data structure
--------------------
Let's see the final structure in JSON:

```json
{  
   "mylib": {  
      "foo": "foo overriden in third block",
      "bar": "bar overriden in second block",
      "baz": "baz overriden in third block"
   },
   "myapp": {  
      "the-answer": 42
   },
   "hammer": {  
      "caret": 18,
      "context": {  
         "config": {  
            "foo": "foo overriden in third block",
            "bar": "bar overriden by the hammer",
            "baz": "baz overriden in third block"
         }
      }
   }
}
```
