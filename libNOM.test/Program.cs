using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using libNOM.io;
using libNOM.io.Enums;
using libNOM.io.Settings;
using libNOM.Map.Aot;

namespace libNOM.test;

class Program
{
    static void Main(string[] args)
    {
        var path = @"C:\Users\kaito\AppData\Roaming\HelloGames\NMS\st_76561198975176038";
        var settings = new PlatformSettings { LoadingStrategy = LoadingStrategyEnum.Current };

        var collection = new PlatformCollection();
        var platforms = collection.AnalyzePath(path,settings);

        var platform = platforms.First();

        var account = platform.GetAccountContainer();
        var save = platform.GetSaveContainer(6);

        platform.Load(save);

        var rawData = save.GetJsonObject();
        
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "libNOM.test.mapping.json"; // 根据实际命名空间调整
        
        string mappingJson;
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new FileNotFoundException($"找不到嵌入资源: {resourceName}");
            
            using var reader = new StreamReader(stream);
            mappingJson = reader.ReadToEnd();
        }
        
        Mapping.Initialize(mappingJson);

        var jN = JsonNode.Parse(rawData.ToString());
        
        var deobfuscated = Mapping.Deobfuscate(jN!);
        
        Console.WriteLine(deobfuscated.ToString());
    }
}