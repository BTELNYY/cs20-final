﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library
{
    public class Encryption
    {
        public string _privateKey;
        public string _publicKey;
        public UnicodeEncoding _encoder = new UnicodeEncoding();

        public string PublicKeyOfOther;

        public Encryption(string privateKey, string publicKey, UnicodeEncoding encoder, string publicKeyOfOther)
        {
            _privateKey = privateKey;
            _publicKey = publicKey;
            _encoder = encoder;
            PublicKeyOfOther = publicKeyOfOther;
        }

        public Encryption(string publicKeyOfOther) 
        {
            var rsa = new RSACryptoServiceProvider();
            _privateKey = rsa.ToXmlString(true);
            _publicKey = rsa.ToXmlString(false);
            PublicKeyOfOther = publicKeyOfOther;
        }

        public string Decrypt(byte[] array)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_privateKey);
            var decryptedByte = rsa.Decrypt(array, false);
            return _encoder.GetString(decryptedByte);
        }

        public string Encrypt(byte[] data)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_publicKey);
            var encryptedByteArray = rsa.Encrypt(data, false).ToArray();
            var length = encryptedByteArray.Count();
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }
    }
}