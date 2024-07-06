using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Model
{
    public class Country : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyRaised("Name"); }
        }

        private int _sales;
        public int Sales
        {
            get { return _sales; }
            set { _sales = value; OnPropertyRaised("Sales"); }
        }

        private int _downloads;
        public int Downloads
        {
            get { return _downloads; }
            set { _downloads = value; OnPropertyRaised("Downloads"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyRaised(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
