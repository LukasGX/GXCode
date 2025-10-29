# GXCode Programming Language

## Initialization

The entry point of the program is defined with the function:

```gxc
entrypoint() {
}
```

## Namespaces

Programs must have a namespace. This is defined via:

```gxc
#ns abc
```

## Output

Output strings using the `out` command:

```gxc
entrypoint() {
    out "Hello World!";
}
```

## Comments

Two types of comments are supported:

-   Single-line comments start with:

```gxc
// This is a comment
```

-   Multi-line comments are enclosed by triple slashes:

```gxc
///
This is also a comment
///
```

## Variables

Variables can be declared with the following types:

-   `str` for String (text)
-   `int` for Integer (whole numbers)
-   `dec` for Decimal numbers (floating point)
-   `bool` for Boolean (true/false)
-   `rex` for regular expressions
-   Arrays of any type: `<DataType>[]`, e.g. `str[]`
-   Maps (key-value pairs) of types: `<KeyType>{<ValueType>}`, e.g. `str{int}`
-   Maps can have multiple types grouped, e.g. `str{int; bool}`

Example declarations:

```gxc
str name = "LukasGX";
int id = 0;
dec number = 0.5;
bool isLoggedIn = false;
rex number = /\d+/;
str[] friends = ["a", "b"];
str{int} friendsWithId = {
    {"a", 0},
    {"b", 1}
};
str{int; bool} friendsDetailed = {
    {"a", 0, false},
    {"b", 1, true}
};
```

## Control Structures

-   Conditional statements:

```gxc
if (name == "abc") {
    // ...
}
else if (name == "def") {
    // ...
}
else {
    // ...
}
```

-   Switch statements:

```gxc
switch (name) {
    case "abc" {
        // ...
    }
    case "def" {
        // ...
    }
}
```

## Loops

-   Counting loop:

```gxc
iterate(0; 10) {
    str abc = array[i];
}
```

This is equivalent to:

```gxc
for (int i = 0; i <= 10; i++) {
    // ...
}
```

-   Iteration over arrays:

```gxc
str[] names = ["abc", "def"];
iterate(names) {
    // ...
}
```

Equivalent to:

```gxc
for (int i = 0; i < names.length; i++) {
    str element = names[i];
    // ...
}
```

-   While loops are as in C++:

```gxc
while (condition) {
    // ...
}
```

## Classes

```gxc
// Classes are always public, no private class allowed
class Car {
    // public by default
    // private keyword possible
    // public keyword causes error
    str color;

    init(str icolor) {
        color = icolor;
    }

    method ChangeColor(str newColor) {
        color = newColor;
    }

    str GetColor() {
        return color;
    }

}

Car car = new Car("red");
car.ChangeColor("blue");
str color = car.GetColor();
```

## Operators

-   Assignment: `=`
-   Comparison: `==`
-   Negative comparison: `!=`
-   Arithmetic: `+=` `-=` `*=` and `+` `-` `*` `/`
-   Root (nth root): `rt` (example: rt 2 = square root of 2; rt3 50 = cube root of 50)

## Package management

```gxc
using ABC;
```

## Exception Handling

```gxc
try {
    // ...
}
treat {
    // catch all exceptions
}
afterwards {
    // equivalent to finally
}

except "Wrong value"; // throw exception
```
