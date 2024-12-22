using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels
{
    /// <summary>
    /// Interface between ValvePrintheadModel and the GUI.
    /// </summary>
    public class ValvePrintheadTypeViewModel : PrintheadTypeViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        private ValvePrintheadTypeModel _valvePrintheadTypeModel;
        public ValvePrintheadTypeModel ValvePrintheadTypeModel
        {
            get { return _valvePrintheadTypeModel; }
        }

        //GPIOPin that signals this Printhead's valve.
        private GPIOPinViewModel _attachedValveGPIOPinViewModel;
        public GPIOPinViewModel AttachedValveGPIOPinViewModel
        {
            get { return _attachedValveGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _valvePrintheadTypeModel.AttachedValveGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedValveGPIOPinViewModel, PinSetting.Input, base.AttachedName + " Limit");
                OnPropertyChanged("AttachedValveGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _valveGPIOPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> ValveGPIOPinViewModelList
        {
            get { return _valveGPIOPinViewModelList; }
        }
        #endregion

        #region Constructor
        public ValvePrintheadTypeViewModel(ValvePrintheadTypeModel ValvePrintheadTypeModel, GPIOPinListsViewModel GPIOPinListsViewModel) : base(ValvePrintheadTypeModel, GPIOPinListsViewModel)
        {
            _valvePrintheadTypeModel = ValvePrintheadTypeModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public override void UnattachGPIOPins()
        {
            if (_attachedValveGPIOPinViewModel != null)
            { _attachedValveGPIOPinViewModel.ResetPinParameters(); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Updates UnattachedPinsByPinSetting with specified PinSetting.
        /// </summary>
        private RelayCommand<string> _refreshPinBySettingListCommand;
        public ICommand RefreshPinBySettingListCommand
        {
            get
            {
                if (_refreshPinBySettingListCommand == null)
                { _refreshPinBySettingListCommand = new RelayCommand<string>(ExecuteRefreshPinBySettingListCommand, CanExecuteRefreshPinBySettingListCommand); }
                return _refreshPinBySettingListCommand;
            }
        }

        public bool CanExecuteRefreshPinBySettingListCommand(string propertyName)
        {
            return (!String.IsNullOrWhiteSpace(propertyName)) ? true : false;
        }

        public void ExecuteRefreshPinBySettingListCommand(string propertyName)
        {
            switch (propertyName)
            {
                case "ValveGPIOPinViewModelList":
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedValveGPIOPinViewModel, ref _valveGPIOPinViewModelList, PinSetting.Input);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Unattaches a GPIO Pin from this Printhead.
        /// </summary>
        private RelayCommand<string> _clearGPIOPinCommand;
        public ICommand ClearGPIOPinCommand
        {
            get
            {
                if (_clearGPIOPinCommand == null)
                { _clearGPIOPinCommand = new RelayCommand<string>(ExecuteClearGPIOPinCommand, CanExecuteClearGPIOPinCommand); }
                return _clearGPIOPinCommand;
            }
        }

        public bool CanExecuteClearGPIOPinCommand(string propertyName)
        {
            return true;
        }

        public void ExecuteClearGPIOPinCommand(string propertyName)
        {
            switch (propertyName)
            {
                case "AttachedValveGPIOPinViewModel":
                    AttachedValveGPIOPinViewModel = null;
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedValveGPIOPinViewModel, ref _valveGPIOPinViewModelList, PinSetting.Output);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}
