using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;


namespace ModiPrint.ViewModels
{
    public class UnsetMainViewModel : ViewModel
    {
        #region Fields and Properties

        #endregion

        #region Constructor
        public UnsetMainViewModel()
        {

        }
        #endregion

        #region Methods

        #endregion

        #region Commands
        /// <summary>
        /// Hyperlinks to the McCloskey Lab homepage.
        /// </summary>
        private RelayCommand<object> _mcCloskeyLabHyperlinkCommand;
        public ICommand McCloskeyLabHyperlinkCommand
        {
            get
            {
                if (_mcCloskeyLabHyperlinkCommand == null)
                { _mcCloskeyLabHyperlinkCommand = new RelayCommand<object>(ExecuteMcCloskeyLabHyperlinkCommand, CanExecuteMcCloskeyLabHyperlinkCommand); }
                return _mcCloskeyLabHyperlinkCommand;
            }
        }

        public bool CanExecuteMcCloskeyLabHyperlinkCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteMcCloskeyLabHyperlinkCommand(object notUsed)
        {
            Process.Start("http://www.kara-mccloskey.squarespace.com/");
        }

        /// <summary>
        /// Hyperlinks to the ModiPrint Github repository.
        /// </summary>
        private RelayCommand<object> _githubHyperlinkCommand;
        public ICommand GithubHyperlinkCommand
        {
            get
            {
                if (_githubHyperlinkCommand == null)
                { _githubHyperlinkCommand = new RelayCommand<object>(ExecuteGithubHyperlinkCommand, CanExecuteGithubHyperlinkCommand); }
                return _githubHyperlinkCommand;
            }
        }

        public bool CanExecuteGithubHyperlinkCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteGithubHyperlinkCommand(object notUsed)
        {
            Process.Start("https://github.com/eshen2210/ModiPrint-Desktop/");
        }
        #endregion
    }
}
