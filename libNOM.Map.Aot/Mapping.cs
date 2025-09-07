using System.Text.Json.Nodes;

namespace libNOM.Map.Aot;

    public class Mapping
    {
        public static void Initialize(string json)
        {
            var node = JsonNode.Parse(json);
            var mapping = node?["Mapping"]?.AsArray();
            if (mapping == null) return;
    
            // 将 JsonArray 转换为 KeyValuePair 集合
            var mappingData = mapping
                .Where(item => item != null)
                .Select(item => new KeyValuePair<string, string>(
                    item["Key"]?.ToString() ?? string.Empty,
                    item["Value"]?.ToString() ?? string.Empty
                ))
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key))
                .ToList();
    
            // 查找 "UserSettingsData" 的索引
            var splitIndex = mappingData.FindIndex(kvp => kvp.Value.Equals("UserSettingsData"));
    
            if (splitIndex >= 0)
            {
                // 找到分割点：前半部分存入 CommonKey，后半部分存入 AccountKey
                for (int i = 0; i < splitIndex; i++)
                {
                    var kvp = mappingData[i];
                    CommonKey[kvp.Key] = kvp.Value;
                }
        
                for (int i = splitIndex; i < mappingData.Count; i++)
                {
                    var kvp = mappingData[i];
                    AccountKey[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                throw new Exception("Mapping data does not contain 'UserSettingsData' key.");
            }
        }

        public static JsonNode Obfuscate(JsonNode? node, bool useAccount = false)
        {
            var keyMap = useAccount ? AccountKey : CommonKey;
            // 创建反向映射用于混淆
            var reverseMap = keyMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            return ReplacePlainKeys(node, reverseMap);
        }

        public static JsonNode Deobfuscate(JsonNode? token, bool useAccount = false)
        {
            if (useAccount)
            {
                return ReplaceObfuscatedKeys(token, AccountKey);
            }
            else
            {
                return ReplaceObfuscatedKeys(token, CommonKey);
            }
        }

        private static JsonNode ReplaceObfuscatedKeys(JsonNode token, Dictionary<string, string> keyMap)
        {
            if (token is JsonObject obj)
            {
                var newObj = new JsonObject();
                foreach (var prop in obj)
                {
                    string newKey = keyMap.ContainsKey(prop.Key) ? keyMap[prop.Key] : prop.Key;
                    newObj[newKey] = ReplaceObfuscatedKeys(prop.Value, keyMap);
                }
                return newObj;
            }
            else if (token is JsonArray arr)
            {
                var newArr = new JsonArray();
                foreach (var item in arr)
                {
                    newArr.Add(ReplaceObfuscatedKeys(item, keyMap));
                }
                return newArr;
            }
            else
            {
                return token.DeepClone();
            }
        }

        private static JsonNode ReplacePlainKeys(JsonNode token, Dictionary<string, string> reverseMap)
        {
            if (token is JsonObject obj)
            {
                var newObj = new JsonObject();
                foreach (var prop in obj)
                {
                    string newKey = reverseMap.ContainsKey(prop.Key) ? reverseMap[prop.Key] : prop.Key;
                    newObj[newKey] = ReplacePlainKeys(prop.Value, reverseMap);
                }
                return newObj;
            }
            else if (token is JsonArray arr)
            {
                var newArr = new JsonArray();
                foreach (var item in arr)
                {
                    newArr.Add(ReplacePlainKeys(item, reverseMap));
                }
                return newArr;
            }
            else
            {
                return token.DeepClone();
            }
        }

        public static Dictionary<int, int> SlotTrack = new Dictionary<int, int>
        {
            {0,3},
            {1,4},
            {2,5},
            {3,6},
            {4,7},
            {5,8},
            {6,17},
            {7,18},
            {8,19},
            {9,20},
            {10,21},
            {11,22}
        };

        //真他妈的恶心
        public static Dictionary<string, string> CommonKey { get; set; } = new();
        public static Dictionary<string, string> AccountKey { get; set; } = new();
    }
