Implementation Recommendations
==============================
Implementations of Json+ ideally follow certain conventions and work in a predictable way.


MIME Type
---------
Use "application/jsonplus" for `Content-Type`.


Automatic type conversions
--------------------------
If an application asks for a value with a particular type, the implementation should attempt to convert types as follows:

- number to string: convert the number into a string representation that would be a valid number in JSON.  
- boolean to string: should become the string "true" or "false"  
- string to number: parse the number with the JSON rules  
- string to boolean: the strings "true", "yes", false", "no", should be converted to boolean values. It's tempting to support a long list of other ways to write a boolean, but for interoperability and keeping it simple, it's recommended to stick to these.  
- string to null: the string `"null"` should be converted to a null value if the application specifically asks for a null value.  

The following type conversions should NOT be performed:

 - null to anything: if the application asks for a specific type and finds null instead, that should usually result in an error.  
 - object to anything  
 - array to anything  
 - anything to object  
 - anything to array

Converting objects and arrays to and from strings is tempting, but in practical situations raises thorny issues of quoting and double-escaping.


Substitution fallback to environment variables
----------------------------------------------
Recall that if a substitution is not present (not even set to `null`) within a data tree, implementations may search for it from external sources. One such source could be environment variables.

It's recommended that Json+ keys always use lowercase, because environment variables generally are capitalized. This avoids naming collisions between environment variables and object properties. While on Windows `getenv()` is generally not case-sensitive, the lookup will be case sensitive all the way until the env variable fallback lookup is reached.

See also the notes below on Windows and case sensitivity.

An application can explicitly block looking up a substitution in the environment by setting a value in the data tree, with the same name as the environment variable. You could set `HOME : null` in your root object to avoid expanding `${HOME}` from the environment, for example.

Environment variables are interpreted as follows:
 - env variables set to the empty string are kept as such (set to empty string, rather than undefined)  
 - Unable to read environment variables: treat as not present.



Case Sensitivity of Environment Variables on Windows 
----------------------------------------------------
Json+ lookup environment variable values in a case sensitive manner. However, Linux and Windows differ in their handling of case.

Linux allows one to define multiple environment variables with the same name but with different case; so both "PATH" and "Path" may be defined simultaneously. In this case, Json+ perform a case-sensitive lookup of environment variables in a straight-forward manner.

In contrast, Windows environment variables names may contain a mix of upper and lowercase characters, eg "Path", however Windows does not allow one to define multiple instances of the same name but differing in case. Whilst accessing environment variables in Windows is case insensitive, accessing environment variables in Json+ is case sensitive. So if you know that you Json+ needs "PATH" then you must ensure that the variable is defined as "PATH" rather than some other name such as "Path" or "path". However, Windows does not allow us to change the case of an existing environment variable; we can't simply redefine the variable with an upper case name. The only way to ensure that your environment variables have the desired case is to first undefine all the environment variables that you will depend on, and then redefine them with the required case.

For example, the the ambient environment might have this definition:

``` 
set Path=A;B;C 
``` 

If Json+ needs "PATH", it will fail because it will consider "Path" as a different variable. The start script must take a precautionary approach and enforce the necessary case as follows:

``` 
set OLDPATH=%PATH% 
set PATH= 
set PATH=%OLDPATH%
```

You cannot know what ambient environment variables might exist in the ambient environment when your program is invoked, nor what case those definitions might have. Therefore the only safe thing to do is redefine all the variables you rely on as shown above.


Kabab vs camelCase
------------------
Object keys are encouraged to be `hyphen-separated` rather than `camelCase`.
