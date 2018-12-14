using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecrypto
{
    [Serializable]
    public class Send_box
    {
        public string adress = "";
        public string key_RSA_keys_public = "";
        public string key_RSA_DS_public = "";
        
        public string My_key_RSA_keys_private = "";
        public string My_key_RSA_keys_public = "";
        public string My_key_RSA_DS_private = "";
        public string My_key_RSA_DS_public = "";

        public string My_key_Rijndael = "";
    }
}
