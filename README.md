# Yoakke Language and Compiler

## Status warning: Early stage

I'm still at very early stages of the compiler. I'm in university and this project basically came from one of my uni projects. I hope it will mature as my time allows, but please look at this as something that's very far away from being usable.

Yoakke aims to be what C could have been with todays design principles and ideas. It tries to give you very homogenous and few building blocks to build up high level abstractions. Turns out, my idea of this is somewhat similar to Zig, but more on that later.

## A feel for the language

Here are some samples for how the language looks. These already work and compile with the current compiler.

### Summing from a to b

```rs
const sum = proc(a: i32, b: i32) -> i32 {
	var s = 0;
	while a < b {
		s += a;
		a += 1;
	}
	s
};
```

### Recursive Fibonacci

```rs
const fib = proc(n: i32) -> i32 {
	if n < 2 { 1 } else { fib(n - 1) + fib(n - 2) }
};
```

### Structs as modules

```rs
const Math = struct {
    const abs = proc(x: i32) -> i32 {
        if x > 0 { x } else { -x }
    };
};
const main = proc() -> i32 {
    Math.abs(-15)
};
```

### Returning types from a procedure

```rs
const get_type = proc(b: bool) -> type {
	if b { i32 } else { u32 }
};
```

### A generic 2D vector

```rs
const Vector2 = proc(T: type) -> type {
	struct {
		x: T;
		y: T;
	}
};
```

### Identity procedure with dependent types

```rs
const identity = proc(T: type, x: T) -> T { x };
```

## Core ideas

Here are some of the ideas and principles that drove me while designing the core language.

### Systems-level, static typing

Small or no runtime at all. Type safety should be guaranteed statically, no implicit conversions.

### A very small set of features

There should be a very-very small set of features that should be powerful enough to cover most use cases but still be sanely usable for standard programming tasks.

### Types are values (at least at compile time)

This idea is derived from Jonathan Blows' demo titled [First-Class (-Ish?) Types](https://www.youtube.com/watch?v=iVN3LLf4wMg). Later I found out that Zig took a similar path here.

Types should be regular values at compile time and this value semantics should be able to support basic generics.

### What feels like it should work, it should just work

I really dislike when I feel like the language can express something but later it turns out it actually has a limitation on it not to allow that use case. So I would like to reduce this friction by trying to make everything work, that can be made work without too big tradeoffs.