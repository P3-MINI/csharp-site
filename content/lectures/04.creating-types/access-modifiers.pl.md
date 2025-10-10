---
title: "Modyfikatory dostępu"
weight: 40
---

# Modyfikatory dostępu

* **public** - dostępne zewsząd; domyślny dla **składowych wyliczeń i interfejsów**
* **internal** - dostępne tylko z wewnątrz *assembly* - czyli wewnątrz plików, które tworzą plik `.dll` lub `.exe`; domyślny dla **niezagnieżdżonych typów**
* **private** - dostępne tylko wewnątrz typu; domyślny dla **składowych klas i struktur**
* **protected** - dostępne wewnątrz typów i podklas
* **protected internal** - suma zbiorów **protected** i **internal**
* **private protected** - przecięcie zbiorów **protected** i **internal**
* **file** (C# 11) - dostępne tylko z poziomu tego samego pliku, aplikowalne tylko do deklaracji typów.