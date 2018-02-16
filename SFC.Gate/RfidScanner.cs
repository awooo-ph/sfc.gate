using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        public static Action<string> ExclusiveCallback { get; set; }
        
        private static StringBuilder _input = new StringBuilder(10);

        private static string GetScannerId(RawKeyboardDevice device)
        {
            var regex = new Regex(@"(?<={)(.*)(?=})");
            if (regex.IsMatch(device.Name))
                return regex.Match(device.Name).Value;
            return device.Name;
        }

        private static Dictionary<Key, Action> _watchKeys = new Dictionary<Key, Action>();

        public static void WatchKey(Key key, Action callback)
        {
            _watchKeys.Add(key, callback);
        }

        private static void RawInputOnKeyPressed(object sender, RawInputEventArgs e)
        {
            if (e.KeyPressState == KeyPressState.Up && _watchKeys.ContainsKey(e.Key))
            {
                _watchKeys[e.Key]?.Invoke();
            }

            if (IsWaitingForScanner)
            {
                Config.Rfid.ScannerId = GetScannerId(e.Device);
                Config.Rfid.Description = e.Device.Description;
                Config.Rfid.ScannerType = e.Device.Type.ToString();
                Config.Rfid.Fullname = e.Device.Name;
                Config.Rfid.Handle = e.Device.Handle.ToInt64();
                Config.Rfid.Save();
                IsWaitingForScanner = false;
                Messenger.Default.Broadcast(Messages.ScannerRegistered);
                _input.Clear();
                e.Handled = true;
                return;
            }

            if (Config.Rfid.Fullname == e.Device.Name)
            {
                Config.Rfid.Handle = e.Device.Handle.ToInt64();
                
                if (e.KeyPressState != KeyPressState.Down) return;
                if (e.Key != Key.Enter)
                {
                    _input.Append((char)e.VirtualKey);
                }
                else
                {
                    if (_input.Length == 0) return;
                    e.Handled = true;
                    
                    if (ExclusiveCallback != null)
                        ExclusiveCallback(_input.ToString());
                    else
                        Messenger.Default.Broadcast(Messages.Scan, _input.ToString());
                    
                    _input.Clear();
                    
                }

                if (Config.Rfid.UseExclusive || RfidScanner.ExclusiveCallback!=null)
                    e.Handled = true;

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
