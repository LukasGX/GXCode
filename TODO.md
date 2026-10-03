# ToDo

## Language features

- pipes (|> instead of .)
- extend iterate (names as name; names as index, name; users where users.age >= 18 as user)
- pattern maching (switch + match {})
- null safety
- string interpolation ("Hello" + name; ..."Hello {name}")
- ranges (iterate (1..10 as i); int[] numbers = [1..100]; iterate(maybe 0..100 step 5 as i))
- obvious blocks (block abc {}; run abc) maybe parallel
- easy event system
    - event UserJoined(...) {}
    - emit UserJoined(...)
    - on UserJoined {}
- reactive variables
    - watch score {}
    - whenever temp > 30 {}
- destructuring
    - str[] person = ["Lukas", "Grambs"];
    - [name, surname] = person;
- retry
- timeout (in ms)
- maybe assert
- debug blocks
- trace blocks

## Other

- Flexible Block Opening / Brace Placement
