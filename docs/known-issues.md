Known issues & workarounds
==========================
Here are the officially known and documented issues, with possible workarounds.

We will keep updating this page, so come back and check out some time later.


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
