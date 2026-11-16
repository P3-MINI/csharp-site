---
title: "Project"
weight: 25
---

# Project

As part of the course credit, students can implement any project in C#. The number of points depends on quality and effort invested.

> [!IMPORTANT]
> **Rules** (a summary of the [regulations]({{< ref "/regulations" >}})):
> - Projects are optional and there is no limit on points for them.
> - Each project must be solved independently.
> - Every project has a mandatory defense. Failing the defense results in a -10 point penalty applied to all projects.

> [!TIP]
> - Agree on the topic and scope of your project with your lab instructor before you start. The point values below are approximate - the final number depends on quality and effort.
> - Keep the project in a git repository from the very beginning and commit regularly.
> - Prepare for the defense: you should be able to explain any part of your code and the design decisions behind it.

Below are example projects:

## 1. Raytracer (Raytracing in One Weekend)

**Description:** Implement a CPU raytracer based on the first book of *"Ray Tracing in One Weekend"* by Peter Shirley (available for free at <https://raytracing.github.io/books/RayTracingInOneWeekend.html>) in C#: rays, spheres and hittable objects, a configurable camera (with depth of field / defocus blur), diffuse (Lambertian), metal, and dielectric (glass) materials, and rendering the final scene with many spheres to an image file (e.g., PPM or PNG).

**Important:** Minimize memory allocations during rendering: make conscious decisions about which types should be value types (`struct` / `readonly struct`) and which should be reference types (`class`), avoid unnecessary allocations in the inner ray-intersection loop, and verify allocations with a profiler or BenchmarkDotNet.

**Welcome additions:** Parallelizing the rendering loop (`Parallel.For` over scanlines or tiles with thread-local random number generators), saving to PNG/JPEG (e.g., with ImageSharp or SkiaSharp), a live preview window, bounding volume hierarchy (BVH), or further chapters from the second book (*The Next Week*).

**Points:** approx. 4

## 2. CityBike - LINQ Data Analysis

**Description:** Download real trip datasets (e.g., from <https://citibikenyc.com/system-data> or any other public dataset/database), parse and load the data into strongly-typed C# records, and perform at least 5 non-trivial LINQ queries answering interesting analytical questions (e.g., for Citi Bike: busiest stations and routes, peak hours on weekdays vs. weekends, trip duration distributions, member vs. casual rider behavior, or bike type usage). Present the results clearly in the console (e.g., with Spectre.Console tables/charts) or export them.

**Important:** Handle large CSV files efficiently using streaming (`StreamReader` / `CsvHelper` or `IAsyncEnumerable<T>`) instead of loading huge files blindly into memory as raw strings; handle missing or malformed fields gracefully; use clean LINQ queries (grouping, joining, aggregations) rather than manual imperative loops.

**Welcome additions:** Combining rental data with historical weather data (e.g., temperature, precipitation) to analyze weather impact on ridership, comparing multiple months/years, interactive CLI filters, or benchmarking LINQ vs. PLINQ.

**Points:** approx. 4 (depending on query complexity)

## 3. Backup Program

**Description:** Implement a continuous directory backup and synchronization tool inspired by the classic OPS (Operating Systems) backup project, using C# threads, tasks (`Task`), or asynchronous programming (`async`/`await`) instead of OS processes (`fork`). The program manages one or more backup pairs (source directory -> target directory): it performs an initial copy, monitors the source directory for changes (file/subdirectory creation, modification, deletion, and renaming) using `FileSystemWatcher`, and mirrors them to the target. It also supports restoring a backup back to the source, listing active backup jobs, and shutting down cleanly on Ctrl+C (`Console.CancelKeyPress` / `CancellationToken`).

**Important:** Do not block threads with busy-waiting or `Thread.Sleep` polling; handle `FileSystemWatcher` quirks properly (buffer overflows, duplicate/rapid events with debouncing, locked files); handle subdirectories and symbolic links safely without infinite recursion; protect shared state (active backup jobs, file operations) against race conditions.

**Welcome additions:** Incremental/hash-based synchronization (copying only changed files), versioned snapshots or compression (ZIP/TAR) of backups, structured logging, a rich console UI (e.g., Spectre.Console) showing active watchers and transfer progress, or unit/integration tests for edge cases.

**Points:** approx. 4

## 4. Photo Organization Program

**Description:** A program that monitors a specified folder (recursively, including inside `.zip` and `.tar` archives) for new photos and automatically organizes them by date and geographic location. It reads EXIF metadata (capture timestamp and GPS coordinates, e.g., using `MetadataExtractor`), performs reverse geocoding via an external API (such as OpenStreetMap Nominatim) to resolve coordinates into country and city names, and sorts/copies the photos into `"year/month/day/"` and `"country/city/"` directory structures.

**Important:** Respect external API rate limits (Nominatim requires a custom `User-Agent` header and at most 1 request per second) and cache coordinate-to-location lookups locally so nearby photos do not trigger redundant requests; use a single shared `HttpClient` with `async`/`await`; handle photos without EXIF dates or GPS coordinates gracefully (e.g., falling back to file modification time or an `"Unknown"` folder) and avoid duplicate processing or filename collisions.

**Welcome additions:** Deduplication by file hash (SHA-256), a dry-run mode previewing planned moves without modifying disk, creating hard links/symlinks instead of duplicating files across both structures, support for RAW image formats, or a configuration file.

**Points:** approx. 4

## 5. Network Game

**Description:** Create a multiplayer network game of any genre with a server and client(s) communicating over the network (TCP, UDP, or WebSockets/SignalR). Examples range from a turn-based console game (chess, battleship, card game) to a real-time 2D arena game (like an *agar.io* or *Snake* clone). Design a clear application protocol covering message framing, serialization, game state synchronization, lobby/matchmaking, and player disconnect handling.

**Important:** Asynchronous networking (`async`/`await`, no thread blocked per client); clean separation between game logic/simulation and networking/rendering; server-authoritative state validation so clients cannot trivially cheat; handle partial TCP reads, unexpected client disconnects, and timeouts gracefully without crashing the server.

**Welcome additions:** Use of a 2D/3D graphics or physics library (e.g., Raylib-cs, MonoGame, Silk.NET, Avalonia), client-side prediction/interpolation for real-time games, spectator mode, in-game chat, or automated tests for the protocol and game rules.

**Points:** 4-12 (e.g., 4 pts for a turn-based console game, 8 pts for a real-time 2D *agar.io* clone, up to 12 pts for more advanced real-time 2D/3D games)

## 6. Game in Godot or Unity

**Description:** Create a complete, playable game in Godot (4.x with .NET/C#) or Unity using C# as the scripting language. The game should have a clear gameplay loop, menus/UI (start screen, pause, game over/win), audio/visual feedback, and at least one non-trivial mechanic (e.g., procedural level generation, enemy AI/pathfinding, physics-based puzzles, inventory/crafting system, or save/load persistence).

**Important:** The project is graded primarily on **C# code quality and architecture**, not just visual assets: keep game logic decoupled from engine nodes/`MonoBehaviour` where possible, use C# events/signals cleanly, avoid per-frame allocations (`Update`/`_Process`) that trigger GC spikes, and organize scripts cleanly without giant "god classes".

**Welcome additions:** Unit tests for pure C# game logic (outside the engine loop), custom editor tools/inspectors, shader effects, procedural generation, or local/online multiplayer.

**Points:** 4-8 (depending on complexity and C# code quality)

## 7. Raytracer + P/Invoke

**Description:** Implement a high-performance interoperability layer between a native C++ ray tracing engine ("Ray Tracing in One Weekend", including a progressive renderer in `camera.h`) and a managed C# application using the provided starter code and pre-built Avalonia-based `Windowing` library:

1. **Native Export Layer (C++, `RayTracing/native/export.cpp`) [1 pt]:** Create a C-compatible ABI with opaque handles (`CreateScene`, `CreateMaterial`, etc.) and matching `Destroy...` functions; implement `RenderScene` accepting a `CameraConfig` struct by value, instantiating a local `camera` object populated with the config data, and calling its `render` method with the scene world, the RGBA output buffer pointer, and a `RenderCallback` function pointer (`typedef void (*RenderCallback)(int samples, uint8_t* buffer);`); provide `SavePng` using `stb_image_write` to save the RGBA buffer to a file.
2. **Low-Level Bindings (C#, `RayTracing/NativeMethods.cs`) [1 pt]:** Bind the exported C functions using P/Invoke (preferably `[LibraryImport]`), implement `SceneSafeHandle` and `MaterialSafeHandle` (`SafeHandle` subclasses automatically calling the C++ `Destroy...` functions), define the `RenderCallback` delegate matching the C++ signature, and handle filename string marshalling for `SavePng`.
3. **High-Level Safe API (C#, `RayTracing/*.cs`) [1 pt]:** Build an idiomatic object-oriented C# wrapper implementing `IDisposable` to manage native handle lifetimes without ever exposing `IntPtr`/`nint` or raw pointers in the public API.
4. **Demo & Integration (`RayTracingDemo/Program.cs`) [1 pt]:** Procedurally generate the iconic scene from the cover of the first book, spawn the preview window via `Windowing.Viewer.Show`, update the window's status text (samples/time) and image buffer in real time from the callback, and save the final image as `output.png` using `SavePng`.

**Important:** Do not expose C++ class layouts, `IntPtr`/`nint`, or raw pointers in the public C# API; ensure every native resource is deterministically released via `SafeHandle` and `IDisposable`; keep the `RenderCallback` delegate alive for the duration of `RenderScene` so the GC does not collect it while native code holds the function pointer. The project uses CMake integrated with MSBuild (`dotnet build -c Release` and `dotnet run -c Release --project RayTracingDemo/RayTracingDemo.csproj`).

> [!NOTE]
> **Starting code:**
> {{< filetree dir="projects/project01/student" >}}

**Points:** 4 (1 point per part)

## 8. Interpreter for a Small Language (Crafting Interpreters)

**Description:** Implement the tree-walk interpreter of the Lox language from part II of the book "Crafting Interpreters" by Robert Nystrom (available for free at <https://craftinginterpreters.com>): a scanner, a recursive descent parser, an abstract syntax tree (AST), an evaluator, and a REPL. The interpreter runs Lox programs from files and reports errors with line numbers.

**Important:** Do not translate the Java code from the book line by line - use C# idioms: the AST as a hierarchy of records, evaluation with `switch` expressions and pattern matching instead of the Visitor pattern (or both, for comparison), nullable reference types enabled without warnings.

**Welcome additions:** Native functions written in C# and registered with attributes and reflection, a test suite that runs sample Lox programs and compares their output with the expected one, the bytecode virtual machine from part III of the book.

**Points:** approx. 6 (chapters 4-10, up to functions and closures), approx. 8 (the whole part II, including classes and inheritance)

## 9. Mini Unit-Testing Framework

**Description:** Write a simple unit-testing framework (in the spirit of NUnit or xUnit) and a console test runner. The framework provides attributes (`[Test]`, `[TestCase(...)]` with arguments, `[SetUp]`/`[TearDown]`, `[Ignore("reason")]`) and an `Assert` class (`Equal`, `True`, `Throws<TException>`, ...) whose failure messages contain the checked expression (`[CallerArgumentExpression]`). The runner takes the path to a compiled test assembly, loads it into a separate `AssemblyLoadContext`, discovers tests with reflection, creates a new instance of the test class for every test, runs both synchronous and asynchronous (`async Task`) tests, and prints a summary: passed/failed/ignored tests, durations, failure messages with stack traces.

**Important:** A failing or hanging test must not crash or block the runner (unwrap `TargetInvocationException`, use timeouts). Mind type identity: the framework assembly must be shared between the runner and the loaded tests - otherwise `TestAttribute` seen by the runner and the one used by the tests may be two different types.

**Welcome additions:** Running test classes in parallel, filtering tests by name or category from the command line, a report in the JUnit XML format, a watch mode (rerunning the tests after recompilation and unloading the previous context, created with `isCollectible: true`).

**Points:** approx. 6

## 10. Concurrent Web Crawler

**Description:** A command-line tool that, starting from a given URL, crawls the pages of one site (up to a given depth or number of pages), extracts links (e.g., with AngleSharp or HtmlAgilityPack), and produces a report: broken links (4xx/5xx responses), redirects, the slowest pages, and the graph of links (e.g., in the Graphviz DOT format). Organize the work as a pipeline of stages (downloading, parsing, collecting results) connected by bounded `Channel<T>` queues, with a configurable number of concurrent requests and cancellation with Ctrl+C (the partial report is still saved).

**Important:** Asynchronous I/O with a single shared `HttpClient`, no blocking calls (`.Result`, `.Wait()`); every URL is visited at most once, even with many concurrent workers; respect `robots.txt` and limit the request rate per host. Test on a site you control (e.g., a local server), not on third-party servers.

**Welcome additions:** Retries with exponential backoff, live progress statistics, saving an offline copy of the site, generating `sitemap.xml`, exposing the results as `IAsyncEnumerable<T>`.

**Points:** approx. 4-6 (depending on the report and the pipeline)

## 11. N-Body Simulation with SIMD

**Description:** Simulate the gravitational interaction of many bodies (from thousands to hundreds of thousands), e.g., a collision of two galaxies. Start with a simple scalar O(n^2) version and optimize it step by step: data layout (array of structures vs. structure of arrays), vectorization with `System.Numerics` (`Vector<T>`, `Vector3`) or `System.Runtime.Intrinsics` (`Vector128<T>`, `Vector256<T>`), and parallelization (`Parallel.For` with range partitioning). Visualize the simulation in real time (e.g., with Raylib-cs) or render frames to image files.

**Important:** Measure every optimization with BenchmarkDotNet and present the results (speed-up over the scalar version, scaling with the number of cores). Check correctness by comparing the results with the scalar version (within a tolerance) and by monitoring the total energy of the system. No allocations in the main simulation loop.

**Welcome additions:** The Barnes-Hut algorithm (O(n log n)), a GPU version (e.g., OpenGL compute shaders through Silk.NET), an interactive camera and simulation parameters.

**Points:** approx. 6 (up to 8 with Barnes-Hut and real-time visualization)

## 12. Disk Usage Analyzer in Avalonia

**Description:** A desktop application (Avalonia UI, MVVM pattern) that shows what takes up space in a selected directory: a tree of directories and files sorted by size (with the percentage of the parent directory) and a treemap - rectangles with areas proportional to sizes, drawn by a custom control. Scanning runs in the background: the UI stays responsive, shows the progress (number of files, bytes scanned), and the scan can be cancelled. Selecting an item in the tree highlights it in the treemap and vice versa.

**Important:** Separation of the view and the logic: view models do not reference controls, and the view talks to them only through data binding and commands (e.g., `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm); the UI is updated only on the UI thread (`IProgress<T>`, `Dispatcher.UIThread`); inaccessible directories and symbolic links are handled (no infinite loops, nothing counted twice).

**Welcome additions:** Finding duplicate files (grouping by size, then hashing), statistics by file type (LINQ), parallel scanning, deleting files with confirmation, unit tests of the view models.

**Points:** approx. 6

## 13. LAN Chat with File Transfer

**Description:** A peer-to-peer chat for the local network, without a central server. Peers discover each other via UDP broadcast or multicast (periodic announcements with the user name and the TCP port; peers that stop announcing disappear from the list). Messages and files are sent over TCP using your own framing protocol (message length + type + payload serialized, e.g., with `System.Text.Json`). Required features: messages to a single peer and to all peers, sending files with progress reporting and integrity verification (SHA-256), several transfers at the same time, cancelling a transfer, and a console (e.g., Spectre.Console) or graphical interface.

**Important:** Asynchronous networking (`async`/`await`, no thread blocked per connection); TCP is a byte stream - handle partial reads and several messages arriving in one read; large files are streamed, never loaded into memory as a whole; a peer that disconnects does not crash the application.

**Welcome additions:** Resuming interrupted transfers, encryption (e.g., `SslStream`), message history stored locally, a GUI in Avalonia.

**Points:** approx. 6

## 14. Redis-Compatible Key-Value Server

**Description:** An in-memory key-value database server that speaks a subset of the Redis protocol (RESP), so the standard `redis-cli` client can connect to it. Required commands: `PING`, `ECHO`, `SET` (with `EX`/`PX` expiration), `GET`, `DEL`, `EXISTS`, `INCR`, `EXPIRE`, `TTL`, `KEYS` with a glob pattern, and the list commands `LPUSH`, `RPUSH`, `LPOP`, `LRANGE`. The server handles many clients concurrently (asynchronous TCP), supports pipelining (many commands sent at once), removes expired keys both lazily and periodically, and persists data in an append-only file replayed at startup.

**Important:** Thread safety of the data store is the core of this project - choose a synchronization strategy (a single lock, `ReaderWriterLockSlim`, lock striping, `ConcurrentDictionary`, or a single thread executing commands from a `Channel<T>`, as Redis itself does), justify it, and test it under load (e.g., `redis-benchmark -t set,get,incr`). Operations such as `INCR` must be atomic. The parser must handle commands split across several reads and several commands in one read.

**Welcome additions:** `PUBLISH`/`SUBSCRIBE`, transactions (`MULTI`/`EXEC`), the blocking `BLPOP` command with a timeout, parsing with `System.IO.Pipelines`, compacting the append-only file in the background.

**Points:** approx. 8

## 15. Safe .NET Wrapper for SQLite

**Description:** A library wrapping the C API of SQLite (the native `sqlite3` library) without using existing bindings (Microsoft.Data.Sqlite, SQLitePCLRaw). The low-level layer declares the native functions (`sqlite3_open_v2`, `sqlite3_prepare_v2`, `sqlite3_bind_*`, `sqlite3_step`, `sqlite3_column_*`, `sqlite3_finalize`, `sqlite3_close_v2`, `sqlite3_errmsg`, ...) with `[LibraryImport]` and represents connections and prepared statements as `SafeHandle` subclasses. The high-level layer is an idiomatic C# API: connections and statements implementing `IDisposable`, parameter binding, transactions, exceptions carrying the SQLite error message, reading rows (e.g., mapped to records). Demonstrate the library in a small application (e.g., a task manager, or importing a CSV file and querying it).

**Important:** The public API does not expose pointers or `nint`; strings are passed as UTF-8; `NULL` and BLOB values (`ReadOnlySpan<byte>`) are handled correctly; every native resource is released, also when an exception is thrown. On Linux, SQLite is usually available as the `libsqlite3` system library; on Windows, the precompiled `sqlite3.dll` can be downloaded from <https://sqlite.org/download.html>.

**Welcome additions:** User-defined SQL functions written in C# (`sqlite3_create_function_v2` with `[UnmanagedCallersOnly]` callbacks), mapping rows to objects with reflection or a source generator, a benchmark against Microsoft.Data.Sqlite. After agreeing with the instructor, you can wrap a different C library instead (e.g., zlib with a `Stream`-based API).

**Points:** approx. 6

## 16. Source Generator for Boilerplate Code

**Description:** An incremental source generator (`IIncrementalGenerator`) that generates at least two of the following for types marked with attributes: (a) for enums: `ToStringFast()`, `TryParse`, `GetValues()` and `IsDefined` implemented with `switch` instead of reflection (including `[Flags]` enums); (b) the `INotifyPropertyChanged` implementation for partial properties marked with `[Notify]`, including notifications about dependent properties; (c) mapping methods between two types (e.g., an entity and a DTO), with a diagnostic for properties that cannot be mapped. Include a demo project, unit tests of the generator (running it with `CSharpGeneratorDriver` and checking the generated code and diagnostics, e.g., as snapshot tests with Verify.SourceGenerators), and a benchmark (BenchmarkDotNet) comparing the generated code with a reflection-based equivalent.

**Important:** The generator reports meaningful diagnostics (e.g., when the type is not `partial`) instead of generating code that does not compile. The pipeline is really incremental: the models passed between its stages are equatable (no `ISymbol` or `SyntaxNode` stored in them, collections compared by value). The generated code compiles without warnings with nullable reference types enabled.

**Welcome additions:** An analyzer with a code fix (e.g., "add the `partial` modifier"), a test checking that unchanged input is served from the generator cache, support for nested and generic types, publishing the generator as a NuGet package.

**Points:** approx. 6

## 17. Command-Line Tool Compiled with Native AOT

**Description:** A useful command-line tool published as a single native executable with Native AOT (`<PublishAot>true</PublishAot>`), e.g., a web server log analyzer (top client addresses, status codes, requests per minute), a `du`/`tree`-like disk usage tool, a CSV-to-JSON converter, or a simple JSON query tool in the style of `jq`. Required: subcommands, options and `--help` (with the System.CommandLine library or your own parser), reading from files or from the standard input (so the tool works in shell pipelines), meaningful exit codes, errors written to the standard error stream, JSON handled with source generation (`JsonSerializerContext`).

**Important:** Publishing must not produce any trimming or AOT warnings (IL2xxx/IL3xxx) - the code cannot rely on unrestricted reflection. Compare the startup time, executable size, and memory usage with the regular (JIT) build and present the results at the defense. Native AOT requires a platform C/C++ toolchain (see the prerequisites in the .NET documentation).

**Welcome additions:** Streaming processing of large files with few allocations (`Span<T>`, `ArrayPool<T>`, `System.IO.Pipelines`), building part of the functionality as a native shared library (`<NativeLib>Shared</NativeLib>`) with functions exported through `[UnmanagedCallersOnly(EntryPoint = "...")]` and calling it from C or Python.

**Points:** approx. 4
