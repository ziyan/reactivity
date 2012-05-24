﻿using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System;

namespace Reactivity.UI.Collections
{
    public class DispatchedObservableCollection<Titem> : ObservableCollection<Titem>
    {
        DispatchEvent collectionChanged = new DispatchEvent();
        DispatchEvent propertyChanged = new DispatchEvent();

        public DispatchedObservableCollection()
        { }

        public DispatchedObservableCollection(List<Titem> list)
            : base(list)
        { }

        public DispatchedObservableCollection(Dispatcher dispatcher)
        { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //base.OnCollectionChanged(e);
            this.collectionChanged.Fire(this, e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            //base.OnPropertyChanged(e);
            this.propertyChanged.Fire(this, e);            
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this.collectionChanged.Add(value); }
            remove { this.collectionChanged.Remove(value); }
        }

        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged.Add(value); }
            remove { this.propertyChanged.Remove(value); }
        }        
    }
}
