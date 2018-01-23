using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private MainViewModel() { }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private User _CurrentUser;

        public User CurrentUser
        {
            get => _CurrentUser;
            set
            {
                if(value == _CurrentUser)
                    return;
                _CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
        
        

        private SnackbarMessageQueue _messageQueue;
        public SnackbarMessageQueue MessageQueue => _messageQueue ?? (_messageQueue = new SnackbarMessageQueue());

        public static void ShowMessage(string message,string actionContent, Action action, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent, action, promote);
        }

        public static void ShowMessage<T>(string message, string actionContent, Action<T> action, T param, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent,action,param,promote);
        }

      
    }
}
