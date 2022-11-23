using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace angular_server.Extensions
{
    public static class ObjectJsonExtensions
    {



        public static void Save(this object value, string path)
        {
            var json = JsonConvert.SerializeObject(value);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            File.WriteAllText(path,json);
        }
        
        public static T Load<T>(this T value, string path)
        {
            
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                try
                {
                    var readObject = JsonConvert.DeserializeObject<T>(json);


                    return readObject;
                }
                catch (Exception)
                {
                    Console.WriteLine();
                }
            }
            return default;
        }
        
    }
}