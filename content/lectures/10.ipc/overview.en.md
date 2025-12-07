---
title: "Overview"
weight: 10
---

# Overview

**Inter-Process Communication (IPC)** is a set of mechanisms that enable processes to exchange data and synchronize their actions. In the .NET environment, various IPC methods are available, allowing for the creation of applications composed of multiple cooperating processes.

Among others:

- **Pipes**: Enable stream-based communication between processes on the same machine. We distinguish between Named Pipes and Anonymous Pipes, intended for communication between parent-child processes.
- **Network**: Utilizes network protocols for communication between processes, regardless of whether they are on the same machine, within a local network, or globally.
- **Files**: Processes communicate by writing and reading data from shared files on disk. This is a simple method but may require additional synchronization.
- **Shared Memory**: Allows processes direct access to a common block of memory, which is the fastest form of IPC. However, it requires advanced management of synchronization for memory access.