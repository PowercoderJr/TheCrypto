using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecrypto
{
    [Serializable]
    public class Mailbox
    {
        internal string address;
        internal string password;
        internal ObservableCollection<Send_box> send_adresses = new ObservableCollection<Send_box>();

        public Mailbox(string address, string password)
        {
            this.address = address;
            this.password = password;
        }

        public override string ToString() => address;
    }
}
