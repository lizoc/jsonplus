Known issues & workarounds
==========================
Here are the officially known and documented issues, with possible workarounds.

We will keep updating this page, so come back and check out some time later.


Substitution operation not supported inside array
-------------------------------------------------
### Affected versions
- 2.0

### Synopsis
Substitution expressions inside an array causes a casting error:
```
foo = bar
daz = [${foo}]
```

### Workarounds
- Will be fixed by version 2.1


Unintended modification of value
--------------------------------
### Affected versions
- 2.0

### Synopsis
```
foo = dummy
subfoo = ${foo}sub1
subbar = ${foo}sub2
subbar = world
# subfoo is now equal to sub1
```

### Workarounds
- Will be fixed by version 2.1


Setting value by dot notation fails with objects that were overrided once
-------------------------------------------------------------------------
### Affected versions
- 2.0

### Synopsis
```
foo { x = 1 }
foo { y = 2 }
foo.z = 32
# foo.z is not set
```

### Workarounds
- Will be fixed by version 2.1


Error referencing properties with the same name
-----------------------------------------------
### Affected versions
- 2.0

### Synopsis
```
foo {
    x = 32
}
bar {
    # Unable to perform substitution because the path "foo" cannot be resolved.
    foo = ${foo}
    # Object reference not set to an instance of an object.
    foo = ${foo.x}
}
```

### Workarounds
- Will be fixed by version 2.1


Include at root may cause errors
--------------------------------
### Affected versions
- 1.10

### Synopsis
At the root level, any statements before the first include results in an error:
```
foo = bar
include "myres"
```

but this does not:
```
include "myres"
foo = bar
```

This bug does not affect non-root level includes:
```
a {
	foo = bar
	include "dar"
}
```

Similarly, you can have multiple includes, but any statement between the includes results in an error:
```
include "myres"
x = 123
include "myres2"
foo = bar
```

but this does not:
```
include "myres"
include "myres2"
foo = bar
```

### Workarounds
- Will be fixed by version 1.11


Support for empty objects
-------------------------
### Affected versions
- 1.2 and below

### Synopsis
This throws an error:
```
foo {}
```

### Workarounds
- Will be fixed by version 1.3


Array of objects
----------------
### Affected versions
- 1.2 and below

### Synopsis
This throws an error:
```
foo = [
	{ a = 1 }
]
```

### Workarounds
- Will be fixed by version 1.3


Substitutions containing dot paths
----------------------------------
### Affects versions
- 1.2 and below.

### Synopsis
This throws an error:
```
'foo.bar' = 5
baz = ${ 'foo.bar' }
```

### Workarounds
- Will be fixed by version 1.3
