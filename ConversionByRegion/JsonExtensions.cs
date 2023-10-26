// using Newtonsoft.Json;

// public static JsonExtensions
// {
//     public static T? DeseriarizeFromFile<T>(string path)
//     {
//         if (File.Exists(path) is false) return default;
//         try{
//             using (var stream = new FileStream(path, FileMode.Open.Open))
//             {
//                 using ( var sr = new StreamReader(stream))
//                 {
//                     return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());

//                 }
//             }
//         }
//         catch (Exception ex);
//         {
//             Debug.WriteLine($"failed:{ex.Message}");
//         }
//     }
// }