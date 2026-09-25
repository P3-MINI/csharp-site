---
title: "Lab12"
weight: 10
---


# Lab 12: Network streams, pipes and memory mapped files

## Network streams

The goal of the task is to create a simple console application that allows two users to exchange messages over a network.

### Starting code

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab12/student/NetworkStreams" >}}

### Task description

#### ChatCommon

The `ChatCommon` project contains shared code used by both the server and the client applications. It includes the `MessageDTO` class, which represents a single message transmitted over the network, as well as the `MessageHandlers` folder with classes responsible for handling messages.


Communication between programs is carried out using the TCP protocol. Each message consists of a 32-bit header, which is an `int` value encoded in big-endian format and specifies the length of the message in bytes. Immediately following the header, the message content is transmitted in JSON format, encoded in UTF-8. The maximum size of a single message must not exceed 10 kB.

As part of this project, the implementations of the following methods must be completed:

 - `ReadMessage` from the `MessageReader` class - in the event of a deserialization error, an `InvalidMessageReceived` exception with an appropriate description should be thrown. If the message length exceeds the allowed limit, a `TooLongMessageException` should be thrown. If the end of the stream is reached, the method should return `null`.

 - `WriteMessage` in the `MessageWriter` class - before sending a message, its length must be validated. If the allowed limit is exceeded, the method should throw a `TooLongMessageException`.

The `Newtonsoft.Json` library should be used for message serialization and deserialization.

#### ChatClient

In the `ChatClient` project, implement an asynchronous `Connect` method that attempts to establish a TCP connection with the server. If a connection is not established within three seconds, the method should return null. Use the provided `progress` object to log the progress of the operation.

#### ChatServer

In the `ChatServer` project, an asynchronous method `ForwardMessagesAsync` must be implemented in the `ChatServer.cs` file. Until cancellation is requested via a `CancellationToken`, this method receives messages from one client, writes them to the standard output, and then forwards them to the other client.

In the same file, the `Run` method must also be implemented. Until cancellation is requested via a `CancellationToken`, it waits for two clients to connect and initiates message exchange between them. After the conversation ends, the method should resume waiting for the next clients. The `HandleClientsAsync` method should be used to handle the connected clients.

{{% hint info %}}
**Useful links:**

- [Microsoft Learn: NetworkStream](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=net-9.0)
- [Microsoft Learn: BinaryPrimitives](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.binary.binaryprimitives?view=net-9.0)
- [Microsoft Learn: TcpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-9.0)
- [Newtonsoft.JSON serialization and deserialization](https://www.newtonsoft.com/json/help/html/SerializingJSON.htm)

{{% /hint %}}

The client application accepts the server’s IP address and port as optional command-line arguments. As an exercise, you can try connecting to a server running on another computer within the local network.

You can check your computer’s IP address using the following commands.

{{< tabs >}}
{{% tab "Linux" %}} 
  ``` bash
  ip a
  ```
{{% /tab %}}
{{% tab "Windows" %}} 
  ```cmd
  ipconfig
  ```
{{% /tab %}}
{{< /tabs >}}

### Example solution

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab12/solution/NetworkStreams" >}}

## Pipes

{{% hint info %}}
**Key-value databases**

Key–value databases are a way of storing data in which each piece of information is saved as a pair: a unique key and its corresponding value.

Instead of using complex tables and relationships, the data is organized in a simple, dictionary-like structure, which enables very fast searching, writing, and reading of data based on the key.

This model is particularly useful in systems that require high performance and easy scalability, for example for handling caches, user sessions, or application configuration.

{{% /hint %}}

### Starting code

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab12/student/Pipes" >}}

### Task description

The goal of the task is to implement a simple key–value database that can respond to requests from other processes running on the same computer. For simplicity, only one client will be handled at a time.

We accept the following query syntax for the server:
 - create or update a key-value pair - `SET <key> <value>`, to which the server responds `OK` on success.
 - retrieve a value - `GET <key>`, to which the server returns the requested value or `NOT_FOUND` if the key does not exist.
 - delete a key-value pair - `DELETE <key>`, to which the server responds `OK` on success or `NOT_FOUND` if the key was not present.
 - for invalid queries the server returns `ERROR <msg>`.

All messages are encoded with UTF-8 and separated by newline characters. Therefore, none of the message values may contain a newline.


#### Client

In the `Client` project implement the following parts:
 - In the `Main` method create a variable `client` of type `NamedPipeClientStream` and connect to the server. If the connection takes more than three seconds - exit the program with an appropriate message.
 - Implement the `GetResponse` method which sends a request to the server and waits for a response. If the connection is interrupted, return `null`.


#### Server

In the `Server` project in class `KvServer` implement the following parts:
 - In the `Start` method create a variable `server` of type `NamedPipeServerStream` and connect to the client. The connection can be interrupted by a `CancellationToken`.
 - Implement the `HandleClientAsync` method to asynchronously read requests from the client and reply to them. Take into account cancellation via `CancellationToken`. Use the `ProcessCommand` method to obtain a response.

{{% hint info %}}
**Useful links:**

- [Microsoft Learn: NamedPipeClientStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeclientstream?view=net-9.0)
- [Microsoft Learn: NamedPipeServerStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream?view=net-9.0)

{{% /hint %}}

### Example solution

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab12/solution/Pipes" >}}

## Memory-mapped files

The goal of the task is to implement a simple library for reading large CSV files. The library should allow processing files that are too large to be loaded entirely into RAM.

### Starting code

> [!NOTE]
> **Student**
> {{< filetree dir="labs/lab12/student/MMF" >}}

### Task description

The `BigCSVReader` project contains an abstract class `BigCsvReader` that is responsible for reading fragments of large CSV files. In its constructor the class creates an auxiliary file with the `.offsets` extension. This is a binary file where consecutive 8-byte values are written, representing the offsets of individual rows in the original CSV file. The library supports UTF-8 encoding only.

The task is to complete the implementations of derived classes `StreamBigCsvReader` and `MmfBigCsvReader`, and then compare their performance using the `BigCSVReader.Benchmark` project.

 - In `StreamBigCsvReader` you should use the standard file reading mechanism using `FileStream`.

 - In `MmfBigCsvReader` you should use file mapping to memory (*Memory-Mapped Files*).

{{% hint info %}}
**Useful links:**

- [Microsoft Learn: MemoryMappedFile](https://learn.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles.memorymappedfile?view=net-9.0)
- [Microsoft Learn: MemoryMappedViewAccessor](https://learn.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles.memorymappedviewaccessor?view=net-9.0)
- [Microsoft Learn: FileStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream?view=net-9.0)
- [Microsoft Learn: StreamReader](https://learn.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=net-9.0)
- [Microsoft Learn: BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/system.io.binaryreader?view=net-9.0)

{{% /hint %}}

### Example solution

> [!TIP]
> **Solution**
> {{< filetree dir="labs/lab12/solution/MMF" >}}
