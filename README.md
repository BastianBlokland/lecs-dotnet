# L.E.C.S
`L`*ightweight* `E`*ntity* `C`*omponent* `S`*ystem* (Ran out of inspiration 😅)

`Work In Progress`

## Summary
Prototype for a lightweight easy to use ecs framework ([Wiki](https://en.wikipedia.org/wiki/Entity%E2%80%93component%E2%80%93system))
leveraging modern dotnet

## Technologies
* C# 7.3
* Core3.0 (Currently in beta) ([Blog post](https://blogs.msdn.microsoft.com/dotnet/2018/10/04/update-on-net-core-3-0-and-net-framework-4-8/))
([Download](https://dotnet.microsoft.com/download/dotnet-core/3.0))

## Project goals
* Take advantage of core3.0 hardware intrinsics in high-frequency code-paths ([Blog post](https://blogs.msdn.microsoft.com/dotnet/2018/10/10/using-net-hardware-intrinsics-api-to-accelerate-machine-learning-scenarios/))
* Api should not allocate but take in in `Span<T>` to allow caller to choose how to store. ([Blog post](https://msdn.microsoft.com/magazine/mt814808.aspx))
* Should not depend on any dynamic code generation to keep it `aot` friendly
* Should allow reusing as much of the internal memory it allocates as possible
