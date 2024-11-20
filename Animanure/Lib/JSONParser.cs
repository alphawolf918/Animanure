using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Animanure.Lib;

public static class JsonParser {

    private static readonly JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };

    public static string Serialize<Model>(Model model) => JsonConvert.SerializeObject(model, Formatting.Indented, settings);

    public static Model Deserialize<Model>(object json) => Deserialize<Model>(json!.ToString()!)!;

    public static Model? Deserialize<Model>(string json) => JsonConvert.DeserializeObject<Model>(json, settings);

    public static bool CompareSerializedObjects(object first, object second) => JsonConvert.SerializeObject(first) == JsonConvert.SerializeObject(second);

    public static object GetUpdatedModel(object original, object updated) {
        JObject jOriginal = JObject.Parse(original.ToString()!);
        foreach (JProperty updatedProperty in JObject.Parse(updated!.ToString()!).Properties()) {
            JProperty? originalProperty = jOriginal.Properties().FirstOrDefault(p => p.Name == updatedProperty.Name);
            if (originalProperty != null) {
                originalProperty.Value = updatedProperty.Value;
            } else {
                jOriginal.Add(updatedProperty);
            }
        }
        return jOriginal.ToString();
    }
}