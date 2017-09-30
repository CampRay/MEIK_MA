using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MEIKReport.Common
{
    /// <summary>
    /// 加密,解密工具类
    /// </summary>
    public class SecurityTools
    {
        /// <summary>
        /// 使用TripleDES加密方式加密数据到文件
        /// </summary>
        /// <param name="data">指定要加密的数据</param>
        /// <param name="fileName">指定加密后的数据保存到的文件</param>
        /// <param name="encryptionPrivateKey">指定加密密钥（最好16位字符）</param>        
        public static void EncryptTextToFile(String data, String fileName, string encryptionPrivateKey=null)
        {
            try
            {
                TripleDESCryptoServiceProvider tDESalg = new TripleDESCryptoServiceProvider();
                if (String.IsNullOrEmpty(encryptionPrivateKey))
                    encryptionPrivateKey = "CampRay@20140416";
                
                tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
                tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8)); 

                // Create or open the specified file.
                FileStream fStream = File.Open(fileName, FileMode.Create);

                // Create a CryptoStream using the FileStream 
                // and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(fStream,
                    new TripleDESCryptoServiceProvider().CreateEncryptor(tDESalg.Key, tDESalg.IV),
                    CryptoStreamMode.Write);

                // Create a StreamWriter using the CryptoStream.
                StreamWriter sWriter = new StreamWriter(cStream);

                // Write the data to the stream 
                // to encrypt it.
                //sWriter.WriteLine(data);
                sWriter.Write(data);
                // Close the streams and
                // close the file.
                sWriter.Close();
                cStream.Close();
                fStream.Close();
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("A file access error occurred: {0}", e.Message);
            }

        }

        /// <summary>
        /// 使用TripleDES解密方式解密数据到文件
        /// </summary>
        /// <param name="fileName">要解密的文件</param>
        /// <param name="encryptionPrivateKey">加密私钥</param>        
        /// <returns>文件解密后的内容</returns>
        public static string DecryptTextFromFile(String fileName, string encryptionPrivateKey=null)
        {
            try
            {
                TripleDESCryptoServiceProvider tDESalg = new TripleDESCryptoServiceProvider();
                if (String.IsNullOrEmpty(encryptionPrivateKey))
                    encryptionPrivateKey = "CampRay@20140416";

                tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
                tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8)); 
                // Create or open the specified file. 
                FileStream fStream = File.Open(fileName, FileMode.Open);

                // Create a CryptoStream using the FileStream 
                // and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(fStream,
                    new TripleDESCryptoServiceProvider().CreateDecryptor(tDESalg.Key, tDESalg.IV),
                    CryptoStreamMode.Read);

                // Create a StreamReader using the CryptoStream.
                StreamReader sReader = new StreamReader(cStream);

                // Read the data from the stream 
                // to decrypt it.
                //string val = sReader.ReadLine();
                string val = sReader.ReadToEnd();
                // Close the streams and
                // close the file.
                sReader.Close();
                cStream.Close();
                fStream.Close();

                // Return the string. 
                return val;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("A file access error occurred: {0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Encrypt text
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Encrypted text</returns>
        public static string EncryptText(string plainText, string encryptionPrivateKey = null)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (String.IsNullOrEmpty(encryptionPrivateKey))
                encryptionPrivateKey = "CampRay@20140416";

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8));            

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateEncryptor(tDESalg.Key, tDESalg.IV), CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = new UnicodeEncoding().GetBytes(plainText);
                    cs.Write(toEncrypt, 0, toEncrypt.Length);
                    cs.FlushFinalBlock();
                }
                return Convert.ToBase64String(ms.ToArray());                
            }
            
        }

        /// <summary>
        /// Decrypt text
        /// </summary>
        /// <param name="cipherText">Text to decrypt</param>
        /// <param name="encryptionPrivateKey">Encryption private key</param>
        /// <returns>Decrypted text</returns>
        public static string DecryptText(string cipherText, string encryptionPrivateKey = null)
        {
            if (String.IsNullOrEmpty(cipherText))
                return cipherText;

            if (String.IsNullOrEmpty(encryptionPrivateKey))
                encryptionPrivateKey = "CampRay@20140416";

            var tDESalg = new TripleDESCryptoServiceProvider();
            tDESalg.Key = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(0, 16));
            tDESalg.IV = new ASCIIEncoding().GetBytes(encryptionPrivateKey.Substring(8, 8));

            byte[] buffer = Convert.FromBase64String(cipherText);
            
            using (var ms = new MemoryStream(buffer))
            {
                using (var cs = new CryptoStream(ms, new TripleDESCryptoServiceProvider().CreateDecryptor(tDESalg.Key, tDESalg.IV), CryptoStreamMode.Read))
                {
                    var sr = new StreamReader(cs, new UnicodeEncoding());
                    return sr.ReadLine();
                }
            }
        }
        
    }
}
