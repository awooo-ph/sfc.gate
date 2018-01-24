﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Violations : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Violations()
        {
            Messenger.Default.AddListener<Violation>(Messages.ModelDeleted, v =>
            {
               MainViewModel.ShowMessage("Violation deleted","UNDO",v.Undelete); 
            });
        }
        private static Violations _instance;
        public static Violations Instance => _instance ?? (_instance = new Violations());

        private ListCollectionView _items;

        public ListCollectionView Items
        {
            get
            {
                if (_items != null) return _items;
                _items = new ListCollectionView(Violation.Cache);
                return _items;
            }
        }

        private ICommand _addViolationCommand;

        public ICommand AddViolationCommand => _addViolationCommand ?? (_addViolationCommand = new DelegateCommand(d =>
        {
            NewItem = new Violation();
            ShowNewItem = true;
        }));

        private ICommand _cancelViolationCommand;

        public ICommand CancelViolationCommand =>
            _cancelViolationCommand ?? (_cancelViolationCommand = new DelegateCommand(
                d =>
                {
                    ShowNewItem = false;
                }));

        private ICommand _acceptNewCommand;

        public ICommand AcceptNewCommand => _acceptNewCommand ?? (_acceptNewCommand = new DelegateCommand(d =>
        {
            NewItem?.Save();
            ShowNewItem = false;
        }));

        private bool _ShowNewItem;

        public bool ShowNewItem
        {
            get => _ShowNewItem;
            set
            {
                if(value == _ShowNewItem)
                    return;
                _ShowNewItem = value;
                OnPropertyChanged(nameof(ShowNewItem));
            }
        }

        private Violation _NewItem;

        public Violation NewItem
        {
            get => _NewItem;
            set
            {
                if(value == _NewItem)
                    return;
                _NewItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        
    }
}
