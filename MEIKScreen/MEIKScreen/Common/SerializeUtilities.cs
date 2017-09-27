using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

/** 使用序列化，反序列化
MyData md = new MyData();            
//序列化
String str = SerializeUtilities.Serialize<MyData>(md);            
//反序列化
MyData mdD = SerializeUtilities.Desrialize<MyData>(str) as MyData;
**/
namespace MEIKScreen.Common
{
    /**
     * 这段代码在某网站源码的基础上改了一下
     * 在VS2008 SP1中测试通过
     * */
    public class SerializeUtilities
    {
        ///// <summary>
        ///// 序列化 对象到字符串
        ///// </summary>
        ///// <param name="obj">泛型对象</param>
        ///// <returns>序列化后的字符串</returns>
        //public static string Serialize<T>(T obj)
        //{            
        //    IFormatter formatter = new BinaryFormatter();
        //    MemoryStream stream = new MemoryStream();
        //    formatter.Serialize(stream, obj);
        //    stream.Position = 0;
        //    byte[] buffer = new byte[stream.Length];
        //    stream.Read(buffer, 0, buffer.Length);
        //    stream.Flush();
        //    stream.Close();
        //    return Convert.ToBase64String(buffer);            
        //}

        /// <summary>
        /// 序列化对象到文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">泛型对象</param>
        /// <param name="filePath">文件全路径</param>
        public static void Serialize<T>(T obj,string filePath)
        {            
            IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using (var stream = System.IO.File.OpenWrite(filePath))
            {
                stream.Position = 0;
                formatter.Serialize(stream, obj);
                stream.Close();
            }                            
        }

        ///// <summary>
        ///// 反序列化 字符串到对象
        ///// </summary>
        ///// <param name="obj">泛型对象</param>
        ///// <param name="str">要转换为对象的字符串</param>
        ///// <returns>反序列化出来的对象</returns>
        //public static T Desrialize<T>(string str)
        //{
            
        //    IFormatter formatter = new BinaryFormatter();
        //    byte[] buffer = Convert.FromBase64String(str);
        //    MemoryStream stream = new MemoryStream(buffer);
        //    T obj = (T)formatter.Deserialize(stream);
        //    stream.Flush();
        //    stream.Close();            
        //    return obj;
        //}

        /// <summary>
        /// 反序列化文件到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T Desrialize<T>(string filePath)
        {           
            IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                T obj = (T)formatter.Deserialize(stream);
                stream.Close();
                return obj;
            }                                               
            
        }
    }
}
