using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RawInputProcessor;
using SFC.Gate.Configurations;

namespace SFC.Gate
{
    static class RfidScanner
    {
        private static RawInput _rawInput;

        public static void Hook(Visual visual)
        {
 if (_rawInput == null)
            {
                _rawInput = new RawPresentationInput(visual, RawInputCaptureMode.ForegroundAndBackground);
                _rawInput.KeyPressed += RawInputOnKeyPressed;
            }
            
        }

        private static StringBuilder _input = new StringBuilder(10);

        private static void RawInputOnKeyPressed(object sender, RawInputEventArgs e)
        {
            if(IsWaitingForScanner)
            {
                Config.Rfid.Scanner = e.Device.Name;
                Config.Rfid.Save();
                IsWaitingForScanner = false;
                Messenger.Default.Broadcast(Messages.ScannerRegistered);
            }
            
            if(e.Device.Name == Config.Rfid.Scanner)
            {
                if (e.KeyPressState != KeyPressState.Down) return;
                if (e.Key != Key.Enter)
                {
                    _input.Append((char) e.VirtualKey);
                }
                else
                {
                    Messenger.Default.Broadcast(Messages.Scan,_input.ToString());
                    _input.Clear();
                }
              //  TrapKey = true;
            }
            
        }

        public static bool IsWaitingForScanner { get; set; }
        public static void RegisterScanner()
        {
            IsWaitingForScanner = true;
        }

        public static void CancelRegistration()
        {
            IsWaitingForScanner = false;
        }

        public static void UnHook()
        {
            //if(hookPtr != IntPtr.Zero)
            //{
            //    UnhookWindowsHookEx(hookPtr);
            //    hookPtr = IntPtr.Zero;
            //}
            _rawInput.KeyPressed -= RawInputOnKeyPressed;
            _rawInput.Dispose();
        }
        
    }
}
