using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.ViewModels.PrintViewModels.MaterialViewModels
{
    //Fires when a RepRap ID is selected.
    public delegate void RepRapIDSelectedEventHandler(string repRapID);

    //Fires when a RepRap ID is cleared.
    public delegate void RepRapIDClearedEventHandler(string repRapID);

    /// <summary>
    /// ViewModel that interfaces MaterialModel with the GUI.
    /// </summary>
    public class MaterialViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains parameters for the Material that will be printed.
        private MaterialModel _materialModel;
        public MaterialModel MaterialModel
        {
            get { return _materialModel; }
        }
        private PrintViewModel _printViewModel;

        //Name of this Material.
        public string Name
        {
            get { return _materialModel.Name; }
        }

        //How RepRap identifies this material (e.g. "T0" for the first material).
        public string RepRapID
        {
            get { return _materialModel.RepRapID; }
            set
            {
                string previousRepRapID = _materialModel.RepRapID;
                _materialModel.RepRapID = value;
                OnRepRapIDSelected(value);
                OnRepRapIDCleared(previousRepRapID);
                OnPropertyChanged("RepRapID");
            }
        }

        //List of availible RepRapIDs.
        private ObservableCollection<string> _availibleRepRapIDList;
        public ObservableCollection<string> AvailibleRepRapIDList
        {
            get
            {
                _availibleRepRapIDList = new ObservableCollection<string>(_printViewModel.AvailibleRepRapIDList);
                if (!String.IsNullOrWhiteSpace(RepRapID)) { _availibleRepRapIDList.Add(RepRapID); }
                return _availibleRepRapIDList;
            }
        }

        //Which printhead will be dispensing this material?
        private PrintheadViewModel _printheadViewModel;
        public PrintheadViewModel PrintheadViewModel
        {
            get { return _printheadViewModel; }
            set
            {
                if (value != null)
                {
                    _materialModel.PrintheadModel = value.PrintheadModel;
                    _printheadViewModel = value;
                    OnPropertyChanged("PrintheadViewModel");
                }
            }
        }

        //How is this Material being printed?
        //Type specific properties are contained in the PrintStyle object.
        private PrintStyleViewModel _printStyleViewModel;
        public PrintStyleViewModel PrintStyleViewModel
        {
            get { return _printStyleViewModel; }
            set { _printStyleViewModel = value; }
        }

        //How is this Material being printed?
        public PrintStyle PrintStyle
        {
            get { return _materialModel.PrintStyle; }
            set
            {
                _materialModel.PrintStyle = value;
                switch (value)
                {
                    case PrintStyle.Continuous:
                        _printStyleViewModel = new ContinuousPrintStyleViewModel((ContinuousPrintStyleModel)_materialModel.PrintStyleModel);
                        break;
                    case PrintStyle.Droplet:
                        _printStyleViewModel = new DropletPrintStyleViewModel((DropletPrintStyleModel)_materialModel.PrintStyleModel);
                        break;
                    case PrintStyle.Unset:
                        _printStyleViewModel = null;
                        break;
                }
                OnPropertyChanged("PrintStyle");
            }
        }

        //Returns values of PrintStyle that are bindable to a control's Itemsource property.
        public IEnumerable<PrintStyle> PrintStyleValues
        {
            get { return Enum.GetValues(typeof(PrintStyle)).Cast<PrintStyle>(); }
        }

        //Printing speed for each axis.
        public double XYPrintSpeed
        {
            get { return _materialModel.XYPrintSpeed; }
            set
            {
                _materialModel.XYPrintSpeed = value;
                OnPropertyChanged("XYPrintSpeed");
            }
        }

        public double ZPrintSpeed
        {
            get { return _materialModel.ZPrintSpeed; }
            set
            {
                _materialModel.ZPrintSpeed = value;
                OnPropertyChanged("ZPrintSpeed");
            }
        }

        //Printing acceleration for each axis.
        public double XYPrintAcceleration
        {
            get { return _materialModel.XYPrintAcceleration; }
            set
            {
                _materialModel.XYPrintAcceleration = value;
                OnPropertyChanged("XYPrintAcceleration");
            }
        }

        public double ZPrintAcceleration
        {
            get { return _materialModel.ZPrintAcceleration; }
            set
            {
                _materialModel.ZPrintAcceleration = value;
                OnPropertyChanged("ZPrintAcceleration");
            }
        }

        //Junction deviation determines junction speed of movemements.
        //The higher the value, the faster the junction speeds.
        //However, high values can cause harsh transitions in between movements and missed steps.
        //Value must be between 0 to 1 and is usually on the very low end (< 0.1).
        public double JunctionDeviation
        {
            get { return _materialModel.JunctionDeviation; }
            set { _materialModel.JunctionDeviation = value; }
        }

        //Pause a print sequence before switching to or switching from this Material.
        //Manual actions will still fire during this pause.
        public bool PauseAfterActivating
        {
            get { return _materialModel.PauseAfterActivating; }
            set { _materialModel.PauseAfterActivating = value; }
        }

        public bool PauseBeforeDeactivating
        {
            get { return _materialModel.PauseBeforeDeactivating; }
            set { _materialModel.PauseBeforeDeactivating = value; }
        }

        //Are all of the parameters set correctly such that printing can occur?
        public bool ReadyToPrint()
        {
            return _materialModel.ReadyToPrint();
        }
        #endregion

        #region Events
        //Event that is fired when the RepRapID is selected.
        public event RepRapIDSelectedEventHandler RepRapIDSelected;
        private void OnRepRapIDSelected(string repRapID)
        {
            if (RepRapIDSelected != null)
            { RepRapIDSelected(repRapID); }
        }

        //Event that is fired when the RepRapID is cleared.
        public event RepRapIDClearedEventHandler RepRapIDCleared;
        private void OnRepRapIDCleared(string repRapID)
        {
            if (RepRapIDCleared != null)
            { RepRapIDCleared(repRapID); }
        }
        #endregion

        #region Constructor
        public MaterialViewModel(MaterialModel MaterialModel, PrintViewModel PrintViewModel)
        {
            _materialModel = MaterialModel;
            _printViewModel = PrintViewModel;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Clears the RepRap ID field for this Material.
        /// </summary>
        private RelayCommand<string> _clearRepRapIDCommand;
        public ICommand ClearRepRapIDCommand
        {
            get
            {
                if (_clearRepRapIDCommand == null)
                { _clearRepRapIDCommand = new RelayCommand<string>(ExecuteClearRepRapIDCommand, CanExecuteClearRepRapIDCommand); }
                return _clearRepRapIDCommand;
            }
        }

        public bool CanExecuteClearRepRapIDCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteClearRepRapIDCommand(object notUsed)
        {
            RepRapID = "";
        }
        #endregion
    }
}
