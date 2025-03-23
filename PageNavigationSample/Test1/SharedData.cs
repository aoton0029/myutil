using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageNavigationSample.Test1
{
    class SharedData : ObservableObject
    {
        private string _username;
        private int _id;

        public string UserName { get => _username; set => SetProperty(ref _username, value); }
        public int Id { get => _id; set => SetProperty(ref _id, value); }
    }
}
