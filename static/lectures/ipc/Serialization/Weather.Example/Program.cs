#define SYSTEM_JSON

#if SYSTEM_JSON
using System.Globalization;
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif
using Weather.Common;

Forecast forecast = Forecast.Generate("Warsaw");

#if SYSTEM_JSON
var options = new JsonSerializerOptions { WriteIndented = true };
var serialized = JsonSerializer.Serialize(forecast, options);
Console.WriteLine(serialized);
Forecast? deserialized = JsonSerializer.Deserialize<Forecast>(serialized);
Console.WriteLine(deserialized);
#else
var serialized = JsonConvert.SerializeObject(forecast, Formatting.Indented);
Console.WriteLine(serialized);
Forecast? deserialized = JsonConvert.DeserializeObject<Forecast>(serialized);
Console.WriteLine(deserialized);
#endif