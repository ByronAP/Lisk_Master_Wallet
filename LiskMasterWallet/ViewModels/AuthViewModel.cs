using System;
using System.ComponentModel;

namespace LiskMasterWallet.ViewModels
{
    // Using disposable only to mix up the mem pointers that is all otherwise this is bad practive and doesn't need a dispose
    public class AuthViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _accepted;

        private string _actiondescription = "";
        private string _pwd = "";

        public string ActionDescription
        {
            get { return _actiondescription; }
            set
            {
                _actiondescription = value;
                RaisePropertyChanged("ActionDescription");
            }
        }

        public string Password
        {
            get { return _pwd; }
            set
            {
                _pwd = value;
                RaisePropertyChanged("Password");
            }
        }

        public bool Accepted
        {
            get { return _accepted; }
            set
            {
                _accepted = value;
                RaisePropertyChanged("Accepted");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _actiondescription = "";
                _actiondescription = null;
                _accepted = false;
                _pwd = "";
                _pwd = null;
            }
        }
    }
}