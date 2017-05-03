using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace WpfTreeDemo
{
    public abstract class Changeable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        protected void NotifyOfPropertyChange(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyOfPropertyChange<TProp>(Expression<Func<TProp>> property)
        {
            NotifyOfPropertyChange(property.GetName());
        }

        protected bool UpdateProperty<TProp>(string propertyName, ref TProp propertyValue, TProp value)
        {
            if (EqualityComparer<TProp>.Default.Equals(propertyValue, value))
                return false;
            propertyValue = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        public string GetPropertyName<TProp>(Expression<Func<TProp>> property)
        {
            return property.GetName();
        }
    }
}