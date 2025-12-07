---
title: "Serialization"
weight: 60
---

# Serialization

Serialization is the process of converting an object (its current state in memory) into a format that can be easily saved or transmitted. This can be either a binary or text format, such as `JSON` or `XML`.

The main applications of serialization are:

1.  **Persistent Storage:** Allows an object to be saved to disk (in a file) or to a database. After restarting the application, this data can be loaded to recreate the object in the same state it was in at the time of saving.
2.  **Communication:** Enables objects to be transmitted. One application serializes an object and sends it, while another receives the data and deserializes it, thereby recreating the original object.

The reverse process of serialization is deserialization – which is the reconstruction of an object from its saved (serialized) form.

The standard library provides mechanisms for serialization to both `JSON` and `XML` formats through the `JsonSerializer` and `XmlSerializer` classes, available in the `System.Text.Json` and `System.Xml.Serialization` namespaces, respectively. The external `Newtonsoft.Json` NuGet package is also popular for `JSON` serialization. It is simpler to use and frequently employed, though less performant.

Both approaches offer an "automagic" serialization mechanism based on **reflection**. The serialization method takes only the object to be serialized and at runtime reads its fields and properties, then serializes it based on this information. Deserialization takes a string and an object representing the deserialized type, and uses reflection to create and initialize its instance. The serialization and deserialization process can be influenced by decorating classes and their members with attributes.

## System.Text.Json

Serialization is performed using the static `JsonSerializer` class.

### Serialization

```csharp
var options = new JsonSerializerOptions { WriteIndented = true };
var serialized = JsonSerializer.Serialize(forecast, options);
```

```json
{
  "Location": "Warsaw",
  "Temperatures": {
    "2025-12-07T22:16:44.6735555+01:00": 3.463462782637311,
    "2025-12-07T23:16:44.6735555+01:00": -19.053981902530726,
    "2025-12-08T00:16:44.6735555+01:00": -1.900355712285556,
    "2025-12-08T01:16:44.6735555+01:00": 0.549260294866361,
    "2025-12-08T02:16:44.6735555+01:00": 8.865189862795464,
    "2025-12-08T03:16:44.6735555+01:00": -18.058228230029254,
    "2025-12-08T04:16:44.6735555+01:00": 13.068063094521058,
    "2025-12-08T05:16:44.6735555+01:00": 27.008561724678614,
    "2025-12-08T06:16:44.6735555+01:00": 7.273621688485271,
    "2025-12-08T07:16:44.6735555+01:00": 27.049612806241868,
    "2025-12-08T08:16:44.6735555+01:00": 23.570338186867424,
    "2025-12-08T09:16:44.6735555+01:00": -11.79202963667603,
    "2025-12-08T10:16:44.6735555+01:00": 19.010310158995928,
    "2025-12-08T11:16:44.6735555+01:00": 24.2806090207253,
    "2025-12-08T12:16:44.6735555+01:00": 15.272542969051628,
    "2025-12-08T13:16:44.6735555+01:00": -15.570638202617841,
    "2025-12-08T14:16:44.6735555+01:00": -13.395053669977223,
    "2025-12-08T15:16:44.6735555+01:00": -18.17296011210254,
    "2025-12-08T16:16:44.6735555+01:00": 12.321830976357617,
    "2025-12-08T17:16:44.6735555+01:00": -18.447507645769615,
    "2025-12-08T18:16:44.6735555+01:00": -13.383437152501326,
    "2025-12-08T19:16:44.6735555+01:00": 28.467406712047328,
    "2025-12-08T20:16:44.6735555+01:00": -16.137085680790847,
    "2025-12-08T21:16:44.6735555+01:00": -6.799952301806021
  },
  "Summary": [
    "Hot",
    "Windy"
  ]
}
```

### Deserialization

```csharp
Forecast? deserialized = JsonSerializer.Deserialize<Forecast>(serialized);
```

### Attributes

```csharp
public class Person
{
    [JsonPropertyName("FirstName")]
    public string Name { get; set; }
    
    [JsonInclude] // Used to serialize non-public fields and properties
    private DateTime DateOfBirth;
    
    [JsonIgnore]
    public int Age { get; set; }
}
```

## Newtonsoft.Json

Serialization is performed using the static `JsonConvert` class.

### Serialization

```csharp
var serialized = JsonConvert.SerializeObject(forecast, Formatting.Indented);
```

```json
{
  "Location": "Warsaw",
  "Temperatures": {
    "2025-12-07T22:15:35.4733681+01:00": -17.084698423041804,
    "2025-12-07T23:15:35.4733681+01:00": -15.132029763809939,
    "2025-12-08T00:15:35.4733681+01:00": 5.772196161399023,
    "2025-12-08T01:15:35.4733681+01:00": 8.832614803522482,
    "2025-12-08T02:15:35.4733681+01:00": -19.629052158164615,
    "2025-12-08T03:15:35.4733681+01:00": 7.6065850260430565,
    "2025-12-08T04:15:35.4733681+01:00": -19.710601981383896,
    "2025-12-08T05:15:35.4733681+01:00": 13.65822297707701,
    "2025-12-08T06:15:35.4733681+01:00": 7.43438191592119,
    "2025-12-08T07:15:35.4733681+01:00": -18.847585808274825,
    "2025-12-08T08:15:35.4733681+01:00": 19.497623200834,
    "2025-12-08T09:15:35.4733681+01:00": 1.3729643304821089,
    "2025-12-08T10:15:35.4733681+01:00": -4.353138125059385,
    "2025-12-08T11:15:35.4733681+01:00": 14.690275394792117,
    "2025-12-08T12:15:35.4733681+01:00": 14.823212353915117,
    "2025-12-08T13:15:35.4733681+01:00": 1.272628008894607,
    "2025-12-08T14:15:35.4733681+01:00": 29.740090341863343,
    "2025-12-08T15:15:35.4733681+01:00": 13.979668721485552,
    "2025-12-08T16:15:35.4733681+01:00": 23.9485046073197,
    "2025-12-08T17:15:35.4733681+01:00": -14.917400243602472,
    "2025-12-08T18:15:35.4733681+01:00": 22.090026162918193,
    "2025-12-08T19:15:35.4733681+01:00": 3.2913333012331627,
    "2025-12-08T20:15:35.4733681+01:00": 14.130944452865734,
    "2025-12-08T21:15:35.4733681+01:00": -10.767305294541261
  },
  "Summary": [
    "Rain",
    "Cold",
    "Humid"
  ]
}
```

### Deserialization

```csharp
Forecast? deserialized = JsonConvert.DeserializeObject<Forecast>(serialized);
```

### Attributes

```csharp
public class Person
{
    [JsonProperty("FirstName")]
    public string Name { get; set; }

    [JsonProperty] // Fields are not serialized by default in Newtonsoft.Json.
    public DateTime DateOfBirth;

    [JsonIgnore]
    public int Age { get; set; }
}
```

> [!INFO]
> ## Example
> {{< filetree dir="lectures/ipc/Serialization" >}}
