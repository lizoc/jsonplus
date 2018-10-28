Examples
========
Here are some samples to get a feel of how Json+ files look like.

We are going to load the following files in the order they appear.


## mylib/config.jsonp

```
my-lib {
    foo = "I am from mylib/config.jsonp"
    hello = "I am from mylib/config.jsonp"
    bar = "I am from mylib/config.jsonp"
}
```

## myapp/config.jsonp

```
# this won't be affected by anything before it, because it is a unique name
my-app {
    the-answer = 42
}

# Let's override some vars!
my-lib.foo = "I am from myapp/config.jsonp"
my-lib.bar = "I am from myapp/config.jsonp"
```

## golden-hammer-app/config.jsonp

```
# this won't be affected by anything before it, because it is a unique name
golder-hammer-app {
    caret = 24
}

# More overrides
my-lib.foo = "I am from golder-hammer-app/config.jsonp"
my-lib.hello = "I am from golden-hammer-app/config.jsonp"
```

## golden-hammer-app/config2.jsonp

```
golder-hammer-app {
    caret = 18

    # here 'libconfig' will get all the properties of 'my-lib'.
    # then, we customize the 'bar' property.
	my-context {
		libconfig = ${my-lib}
		libconfig {
			bar = "I am from golden-hammer-app/config2.jsonp"
		}
	}
}
```
