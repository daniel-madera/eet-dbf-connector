using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EetConnector
{
    public static class PasswordCipher
    {
        public static String Decrypt(String cipher) 
        {
            SimpleAES aes = new SimpleAES();
            return aes.Decrypt(Convert.FromBase64String(cipher));
        }

        public static String Encrypt(String plaintext) 
        {
            SimpleAES aes = new SimpleAES();
            return Convert.ToBase64String(aes.Encrypt(plaintext));
        }
    }
}
