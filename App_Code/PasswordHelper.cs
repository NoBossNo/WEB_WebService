using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace FSMesService
{
    public class PasswordHelper {

        /// <summary>
        ///  进行DES加密
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串</param>
        /// <param name="sKey">密钥，且必须为8位</param>
        /// <returns>以Base64格式返回的加密字符串</returns>
        public static string Encrypt(string pToEncrypt, string sKey) {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider()) {//加密标准 (DES) 算法的加密服务提供程序
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);//获取或设置数据加密标准 (DES) 算法的机密密钥
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);//获取或设置对称算法的初始化向量 (IV)。 
                MemoryStream ms = new MemoryStream();
                //用目标数据流、要使用的转换和流的模式初始化 CryptoStream 类的新实例。
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write)) {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        /// <summary>
        /// 进行DES解密
        /// </summary>
        /// <param name="pToDecrypt">要解密的以Base64</param>
        /// <param name="sKey">密钥，且必须为8位</param>
        /// <returns>已解密的字符串</returns>
        public static string Decrypt(string pToDecrypt, string sKey) {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider()) {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write)) {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        #region PMS加密函数，key=18
        public static string Encrypt_PMS(string pToEncrypt, string sKey)
        {
            #region PMS源代码，作为参考
            /*
                function Encrypt(Src: string; Key: Word = CS_ENCRYPT_KEY): string;
                var
                    s_str:string[255];
                    str:Array[0..255] of Byte absolute s_str;
                    i:integer;
                    ikey :integer;
                    iRandom:integer;
                begin
                    iRandom := 8;//Random(10);
                    ikey := Key + iRandom;//strtoint(inttostr(key)+inttostr(length(inttostr(key))));
                    s_str:=Src;
                    for i:=1 to Ord(s_str[0]) do
                        str[i]:=str[i] XOR ikey;
                    result := IntToStr(iRandom) + s_str;
                end;
                 * */
            #endregion
            int iRandom = 8;
            int ikey = int.Parse(sKey) + iRandom;
            string s_str = pToEncrypt;
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(s_str);
            byte[] shi = System.BitConverter.GetBytes(ikey);

            int j = byteArray.Length;
            for (int i = 0; i <= j - 1; i++)
            {
                byteArray[i] = (byte)(byteArray[i] ^ shi[0]);
            }
            return iRandom.ToString() + System.Text.Encoding.Default.GetString(byteArray);
        }
        #endregion

        #region PMS解密函数，key=18
        public static string Decrypt_PMS(string pToDecrypt, string sKey)
        {
            #region PMS源代码，作为参考
            /*
            function Decrypt(Src: string; Key: Word = CS_ENCRYPT_KEY): string;
            var
                s_str:string[255];
                str:Array[0..255] of Byte absolute s_str;
                i:integer;
                ikey :integer;
                iRandom:integer;
            begin
                Try
                iRandom := StrToInt(Copy(Src,1,1));
                Except
                Exit;
                END;
                ikey :=Key+iRandom;
                s_str:=Copy(Src,2,Length(Src));
                for i:=1 to Ord(s_str[0]) do
                    str[i]:=str[i] XOR ikey;
                result := s_str;
            end;
             * */
            #endregion
            int iRandom;
            try
            {
                iRandom = int.Parse(pToDecrypt.Substring(0, 1));
            }
            catch (Exception ex)
            {
                return "";
            }
            int ikey;
            ikey = int.Parse(sKey) + iRandom;
            string s_str = pToDecrypt.Substring(1);
            int j = s_str.Length; ;

            byte[] byteArray = System.Text.Encoding.Default.GetBytes(s_str);
            byte[] shi = System.BitConverter.GetBytes(ikey);

            j = byteArray.Length;
            for (int i = 0; i <= j - 1; i++)
            {
                byteArray[i] = (byte)(byteArray[i] ^ shi[0]);
            }
            return System.Text.Encoding.Default.GetString(byteArray); 
        }
        #endregion
    }

   

}
