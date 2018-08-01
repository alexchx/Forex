﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Forex
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //protected void SetValue<T>(T newValue, ref T oldValue, [CallerMemberName] string propertyName = "")
        //{
        //    if (Equals(newValue, oldValue))
        //    {
        //        return;
        //    }

        //    oldValue = newValue;

        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
