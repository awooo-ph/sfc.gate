using System;
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
    class VisitorsViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private VisitorsViewModel()
        {
            Messenger.Default.AddListener<string>(Messages.Scan, code =>
            {
                if (Guard.RedirectScan != null) return;
                if (!MainViewModel.Instance.HasLoggedIn) return;
                if (MainViewModel.Instance.Screen == MainViewModel.VISITORS || Configurations.Config.Rfid.GlobalScan)
                if (!IsAddingVisitor && Visit.Cache.Any(x => !x.HasLeft && x.Rfid.ToLower() == code?.ToLower()))
                {
                    MainViewModel.Instance.Screen = MainViewModel.VISITORS;
                    
                    ReturnRfid = code;
                    ShowReturnDialog = true;
                    IsReturningCard = true;
                    return;
                }

                if(MainViewModel.Instance.Screen != MainViewModel.VISITORS)
                    return;

                if (IsAddingVisitor && !IsReturningCard)
                    ScanAddVisitor(code);
                else if (IsReturningCard && !IsAddingVisitor)
                    ScanReturnCard(code);
            });
        }

        private void ScanReturnCard(string code)
        {
            ReturnRfid = code;
        }

        private void ScanAddVisitor(string code)
        {
            if (Visit.Cache.Any(x => !x.HasLeft && x.Rfid.ToLower() == code?.ToLower())) return;
            NewRfid = code;
        }

        private bool _IsAddingVisitor;

        public bool IsAddingVisitor
        {
            get => _IsAddingVisitor;
            set
            {
                if(value == _IsAddingVisitor)
                    return;
                _IsAddingVisitor = value;
                OnPropertyChanged(nameof(IsAddingVisitor));
                Guard.Instance.IgnoreScans = !value;
            }
        }

        private bool _IsReturningCard;

        public bool IsReturningCard
        {
            get => _IsReturningCard;
            set
            {
                if(value == _IsReturningCard)
                    return;
                _IsReturningCard = value;
                OnPropertyChanged(nameof(IsReturningCard));
                Guard.Instance.IgnoreScans = !value;
            }
        }

        private Visit _ReturnVisitor;

        public Visit ReturnVisit
        {
            get => _ReturnVisitor;
            set
            {
                if(value == _ReturnVisitor)
                    return;
                _ReturnVisitor = value;
                OnPropertyChanged(nameof(ReturnVisit));
            }
        }
        
        private static VisitorsViewModel _instance;
        public static VisitorsViewModel Instance => _instance ?? (_instance = new VisitorsViewModel());

        private ListCollectionView _visitors;
        public ListCollectionView Visitors
        {
            get
            {
                if (_visitors != null) return _visitors;
                _visitors = new ListCollectionView(Visitor.Cache);
                _visitors.Filter = FilterVisitors;
                Visitor.Cache.CollectionChanged += (sender, args) =>
                {
                    _visitors.Filter = FilterVisitors;
                };
                return _visitors;
            }
        }

        private string _SearchKeyword;

        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if(value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                Visitors.Filter = FilterVisitors;
            }
        }
        
        private bool FilterVisitors(object o)
        {
            var v = o as Visitor;
            if (v == null) return false;
            
            if(FilterCurrentVisitors)
                if (Visit.Cache.FirstOrDefault(x => x.VisitorId == v.Id && !x.HasLeft) == null) return false;
            if(FilterVisitorsToday)
                if (Visit.Cache.FirstOrDefault(x => x.TimeIn.Date == DateTime.Now.Date) == null) return false;
            
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                return v.Name.ToLower().Contains(SearchKeyword.ToLower()) ||
                       v.Number.ToLower().Contains(SearchKeyword.ToLower()) ||
                       v.Address.ToLower().Contains(SearchKeyword.ToLower());
            
            return true;
        }

        private bool _FilterCurrentVisitors;

        public bool FilterCurrentVisitors
        {
            get => _FilterCurrentVisitors;
            set
            {
                if(value == _FilterCurrentVisitors)
                    return;
                _FilterCurrentVisitors = value;
                OnPropertyChanged(nameof(FilterCurrentVisitors));
                Visitors.Filter = FilterVisitors;
            }
        }

        private bool _FilterVisitorsToday;

        public bool FilterVisitorsToday
        {
            get => _FilterVisitorsToday;
            set
            {
                if(value == _FilterVisitorsToday)
                    return;
                _FilterVisitorsToday = value;
                OnPropertyChanged(nameof(FilterVisitorsToday));
                Visitors.Filter = FilterVisitors;
            }
        }
        

        private bool _ShowNewDialog;

        public bool ShowNewDialog
        {
            get => _ShowNewDialog;
            set
            {
                if(value == _ShowNewDialog)
                    return;
                _ShowNewDialog = value;
                OnPropertyChanged(nameof(ShowNewDialog));
               
                    IsAddingVisitor = value;
            }
        }

        private ICommand _addVisitorCommand;

        public ICommand AddVisitorCommand => _addVisitorCommand ?? (_addVisitorCommand = new DelegateCommand(d =>
        {
            NewName = null;
            NewAddress = null;
            NewNumber = "";
            NewRfid = "";
            NewPurpose = "";
            ShowNewDialog = true;
            IsAddingVisitor = true;
        }));

        private ICommand _newCancelCommand;

        public ICommand NewCancelCommand => _newCancelCommand ?? (_newCancelCommand = new DelegateCommand(d =>
        {
            ShowNewDialog = false;
            NewName = "";
            NewAddress = "";
            NewNumber = "";
            NewRfid = "";
            NewPurpose = "";
        }));

        private ICommand _timeOutCommand;

        public ICommand TimeOutCommand => _timeOutCommand ?? (_timeOutCommand = new DelegateCommand<Visit>(visit =>
        {
            visit.Update(nameof(Visit.TimeOut),DateTime.Now);
            Log.Add("VISITOR LEFT", $"{visit.Visitor.Name} has left.");
        }));

        private ICommand _newAcceptCommand;

        public ICommand NewAcceptCommand => _newAcceptCommand ?? (_newAcceptCommand = new DelegateCommand(d =>
        {
            var visitor = Visitor.Cache.FirstOrDefault(x => x.Name.ToLower() == NewName.ToLower());
            if(visitor==null)
                visitor = new Visitor();
            visitor.Name = NewName;
            visitor.Address = NewAddress;
            visitor.Number = NewNumber;
            visitor.Save();

            new Visit()
            {
                VisitorId = visitor.Id,
                Purpose = NewPurpose,
                Rfid = NewRfid,
            }.Save();

            Log.Add("VISITOR", $"A visitor has entered the campus. Name: {visitor.Name} Purpose: {NewPurpose}");
            
            ShowNewDialog = false;
        }, d =>
        {
            if (string.IsNullOrWhiteSpace(NewName)) return false;
            if (string.IsNullOrWhiteSpace(NewRfid)) return false;
            if (string.IsNullOrWhiteSpace(NewPurpose)) return false;

            var visitor = Visitor.Cache.FirstOrDefault(x => x.Name.ToLower() == NewName.ToLower());
            if (visitor != null)
            {
                var v = Visit.Cache.OrderByDescending(x => x.TimeIn).FirstOrDefault(x => x.VisitorId == visitor.Id);
                if (v != null)
                {
                    if (!v.HasLeft) return false;
                }
            }

            var visit = Visit.Cache.FirstOrDefault(x => !x.HasLeft && x.Rfid.ToLower() == NewRfid.ToLower());
            if (visit == null) return true;
            
            return visit.TimeOut != null;
        }));

        private string _NewPurpose;

        public string NewPurpose
        {
            get => _NewPurpose;
            set
            {
                if(value == _NewPurpose)
                    return;
                _NewPurpose = value;
                OnPropertyChanged(nameof(NewPurpose));
            }
        }
        
        public List<string> VisitorNames => Visitor.Cache.Select(x => x.Name).ToList();

        private string _NewName;

        public string NewName
        {
            get => _NewName;
            set
            {
                if(value == _NewName)
                    return;
                _NewName = value;
                OnPropertyChanged(nameof(NewName));
                RefreshNewVisitor();
            }
        }

        private string _NewNumber;

        public string NewNumber
        {
            get => _NewNumber;
            set
            {
                if(value == _NewNumber)
                    return;
                _NewNumber = value;
                OnPropertyChanged(nameof(NewNumber));
            }
        }

        private string _NewAddress;

        public string NewAddress
        {
            get => _NewAddress;
            set
            {
                if(value == _NewAddress)
                    return;
                _NewAddress = value;
                OnPropertyChanged(nameof(NewAddress));
            }
        }

        private string _NewRfid;

        public string NewRfid
        {
            get => _NewRfid;
            set
            {
                if(value == _NewRfid)
                    return;
                _NewRfid = value;
                OnPropertyChanged(nameof(NewRfid));
            }
        }

        private Visitor _selectedVisitor;
        public void RefreshNewVisitor()
        {
            var visitor = Visitor.Cache.FirstOrDefault(x => x.Name?.ToLower() == NewName?.ToLower());
            if (_selectedVisitor != null)
            {
                NewAddress = "";
                NewNumber = "";
            }
            
            _selectedVisitor = visitor;
            if (visitor == null || NewName.Trim()=="") return;
            
            NewAddress = visitor.Address;
            NewNumber = visitor.Number;
        }

        string IDataErrorInfo.this[string columnName] => GetDataError(columnName);

        private string GetDataError(string columnName)
        {
            if (columnName == nameof(NewName) && string.IsNullOrWhiteSpace(NewName))
                return "Name is required";
            if (columnName == nameof(NewPurpose) && string.IsNullOrWhiteSpace(NewPurpose))
                return "Provide purpose of visit";

            var rfid = "";
            var rfidColumn = false;
            if (columnName == nameof(NewRfid))
            {
                rfid = NewRfid;
                rfidColumn = true;
                if (string.IsNullOrWhiteSpace(rfid))
                    return "RFID is required";
                var stud = Student.Cache.FirstOrDefault(x => x.Rfid == rfid);
                if (stud != null)
                    return $"This is {stud.Fullname}'s ID";
                if (Visit.Cache.Any(x => !x.HasLeft && x.Rfid.ToLower() == rfid?.ToLower()))
                    return "Card is in use";
            }
            else if (columnName == nameof(ReturnRfid))
            {
                rfid = ReturnRfid;
                rfidColumn = true;
                if (string.IsNullOrWhiteSpace(rfid))
                    return "RFID is required";
                var stud = Student.Cache.FirstOrDefault(x => x.Rfid == rfid);
                if (stud != null)
                    return $"This is {stud.Fullname}'s ID";
                if (!Visit.Cache.Any(x => !x.HasLeft && x.Rfid.ToLower() == rfid?.ToLower()))
                    return "Card is not in use";
            }

            if (rfidColumn)
            {
                
            }


            return null;
        }

        string IDataErrorInfo.Error => null;

        private bool _ShowReturnDialog;

        public bool ShowReturnDialog
        {
            get => _ShowReturnDialog;
            set
            {
                if(value == _ShowReturnDialog)
                    return;
                _ShowReturnDialog = value;
                OnPropertyChanged(nameof(ShowReturnDialog));
                IsReturningCard = value;
            }
        }

        private string _ReturnRfid;

        public string ReturnRfid
        {
            get => _ReturnRfid;
            set
            {
                if(value == _ReturnRfid)
                    return;
                _ReturnRfid = value;
                OnPropertyChanged(nameof(ReturnRfid));
                OnPropertyChanged(nameof(HasReturnCardError));
                OnPropertyChanged(nameof(ReturnErrorMessage));
                ReturnVisit = Visit.Cache.FirstOrDefault(x => !x.HasLeft && x.Rfid.ToLower() == value?.ToLower());
            }
        }
        
        private ICommand _ShowReturnDialogCommand;

        public ICommand ReturnCardCommand =>
            _ShowReturnDialogCommand ?? (_ShowReturnDialogCommand = new DelegateCommand(
                d =>
                {
                    ReturnRfid = "";
                    ShowReturnDialog = true;
                    IsReturningCard = true;
                }));

        private ICommand _ReturnCancelCommand;

        public ICommand ReturnCancelCommand => _ReturnCancelCommand ?? (_ReturnCancelCommand = new DelegateCommand(d =>
        {
            ShowReturnDialog = false;
        }));

        private ICommand _ReturnAcceptCommand;

        public ICommand ReturnAcceptCommand => _ReturnAcceptCommand ?? (_ReturnAcceptCommand = new DelegateCommand(d =>
        {
            var visit = Visit.Cache.FirstOrDefault(x => !x.HasLeft && x.Rfid.ToLower() == ReturnRfid.ToLower());
            if (visit == null) return;
            visit.Update(nameof(Visit.TimeOut),DateTime.Now);
            Log.Add("VISITOR LEFT", $"{visit.Visitor.Name} has left.");
            ShowReturnDialog = false;
        },d=>string.IsNullOrEmpty(GetDataError(nameof(ReturnRfid)))));

        public bool HasReturnCardError
        {
            get
            {
                if (string.IsNullOrEmpty(ReturnRfid)) return false;
                return !string.IsNullOrEmpty(GetDataError(nameof(ReturnRfid)));
            }
        }

        public string ReturnErrorMessage => GetDataError(nameof(ReturnRfid));
    }
}
