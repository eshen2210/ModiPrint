using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels
{
    /// <summary>
    /// Interface between PrinterModel to the GUI.
    /// </summary>
    public class PrinterViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains parameters for the Printer.
        private PrinterModel _printerModel;
        public PrinterModel PrinterModel
        {
            get { return _printerModel; }
        }

        //Contains events that updates the Min and Max Position properties of Printer equipment.
        SerialCommunicationCommandSetsModel _serialCommunicationCommandSetsModel;

        //Microcontroller and its GPIO Pins.
        private MicrocontrollerViewModel _microcontrollerViewModel;
        public MicrocontrollerViewModel MicrocontrollerViewModel
        {
            get { return _microcontrollerViewModel; }
        }

        //Provides lists of GPIOPins sorted by function.
        private GPIOPinListsViewModel _gPIOPinListsViewModel;
        public GPIOPinListsViewModel GPIOPinListsViewModel
        {
            get { return _gPIOPinListsViewModel; }
        }

        //List of all PrintheadViewModels.
        private ObservableCollection<PrintheadViewModel> _printheadViewModelList = new ObservableCollection<PrintheadViewModel>();
        public ObservableCollection<PrintheadViewModel> PrintheadViewModelList
        {
            get { return _printheadViewModelList; }
        }

        //An observable collection of only one Printhead with empty parameters.
        //Used to trick comboboxes into having a blank space.
        private ObservableCollection<PrintheadViewModel> _emptyPrintheadViewModelList = new ObservableCollection<PrintheadViewModel>();
        public ObservableCollection<PrintheadViewModel> EmptyPrintheadViewModelList
        {
            get { return _emptyPrintheadViewModelList; }
        }

        //List of all AxisViewModels.
        private ObservableCollection<AxisViewModel> _axisViewModelList = new ObservableCollection<AxisViewModel>();
        public ObservableCollection<AxisViewModel> AxisViewModelList
        {
            get { return _axisViewModelList; }
        }

        //List of all Z AxisViewModels.
        private ObservableCollection<AxisViewModel> _zAxisViewModelList = new ObservableCollection<AxisViewModel>();
        public ObservableCollection<AxisViewModel> ZAxisViewModelList
        {
            get { return _zAxisViewModelList; }
        }

        //Keeps track of the number of Z Axes that have been created.
        //Does not decrease when Printheads are removed.
        //Used such that Z Axes Names do not overlap.
        public int ZAxesCreatedCount
        {
            get { return _printerModel.ZAxesCreatedCount; }
            set
            {
                _printerModel.ZAxesCreatedCount = value;
                OnPropertyChanged("ZAxesCreatedCount");
            }
        }

        //Keeps track of the number of Printheads that have been created.
        //Does not decrease when Printheads are removed.
        //Used such that Printhead Names do not overlap.
        public int PrintheadsCreatedCount
        {
            get { return _printerModel.PrintheadsCreatedCount; }
            set
            {
                _printerModel.PrintheadsCreatedCount = value;
                OnPropertyChanged("PrintheadsCreatedCount");
                OnPropertyChanged("CanRemovePrinthead");
            }
        }

        //Returns true if there are more than 2 Printheads.
        public bool CanRemovePrinthead
        {
            get { return (_printheadViewModelList.Count > 1) ? true : false; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a PrinterViewModel with one X Axis ViewModel, one Y Axis ViewModel, one Z Axis ViewModel, and one PrintheadViewModel.
        /// </summary>
        /// <param name="PrinterModel"></param>
        public PrinterViewModel(PrinterModel PrinterModel, SerialCommunicationCommandSetsModel SerialCommunicationCommandSetsModel)
        {
            _printerModel = PrinterModel;
            _serialCommunicationCommandSetsModel = SerialCommunicationCommandSetsModel;
            _gPIOPinListsViewModel = new GPIOPinListsViewModel();

            _microcontrollerViewModel = new MicrocontrollerViewModel(_printerModel.MicroControllerModel, _gPIOPinListsViewModel);

            //Populates the Axis ViewModel lists with their Axis Model counterparts.
            foreach (AxisModel axisModel in _printerModel.AxisModelList)
            {
                AxisViewModel newAxisViewModel = new AxisViewModel(axisModel, _gPIOPinListsViewModel);

                _axisViewModelList.Add(newAxisViewModel);
                if (newAxisViewModel.Name.Contains('Z'))
                { _zAxisViewModelList.Add(newAxisViewModel); }
            }

            //Populates the Printhead ViewModel lists with their Printhead Model counterparts.
            foreach (PrintheadModel printheadModel in _printerModel.PrintheadModelList)
            {
                _printheadViewModelList.Add(new PrintheadViewModel(printheadModel, _gPIOPinListsViewModel));
            }

            //Populates the empty PrintheadViewModel list with an empty Printhead.
            _emptyPrintheadViewModelList.Add(new PrintheadViewModel(new PrintheadModel(""), _gPIOPinListsViewModel));

            //Subscribe to events.
            SerialCommunicationCommandSetsModel.RealTimeStatusDataModel.RecordLimitExecuted += new RecordLimitExecutedEventHandler(UpdateMinMaxPositions);
            SerialCommunicationCommandSetsModel.CommandSetMinMaxPositionChanged += new CommandSetMinMaxPositionChangedEventHandler(UpdateMinMaxPositions);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a Printhead to the Printer.
        /// </summary>
        private void AddPrinthead()
        {
            _printerModel.AddPrinthead();
            int newIndex = _printerModel.PrintheadModelList.Count - 1;
            _printheadViewModelList.Add(new PrintheadViewModel(_printerModel.PrintheadModelList[newIndex], _gPIOPinListsViewModel));
            OnPropertyChanged("PrintheadViewModelList");
            OnPropertyChanged("CanRemovePrinthead");
        }

        /// <summary>
        /// Adds a Printhead with a set Name to the Printer.
        /// </summary>
        /// <param name="printheadName"></param>
        /// <remarks>
        /// This method was created for the XML Serilializers.
        /// </remarks>
        public PrintheadViewModel AddPrinthead(string printheadName)
        {
            _printerModel.AddPrinthead(printheadName);
            int newIndex = _printerModel.PrintheadModelList.Count - 1;
            _printheadViewModelList.Add(new PrintheadViewModel(_printerModel.PrintheadModelList[newIndex], _gPIOPinListsViewModel));
            OnPropertyChanged("PrintheadViewModelList");
            OnPropertyChanged("CanRemovePrinthead");
            return _printheadViewModelList[_printheadViewModelList.Count - 1];
        }

        /// <summary>
        /// Removes one Printhead with the specified name from the Printer.
        /// </summary>
        /// <param name="printheadViewModelName"></param>
        public bool RemovePrinthead(string printheadName)
        {
            if (_printheadViewModelList.Count > 1) //The Printer must have at least 1 Printhead.
            {
                for (int index = 0; index < _printheadViewModelList.Count; index++)
                {
                    if ((printheadName == _printheadViewModelList[index].Name)
                     && (_printerModel.RemovePrinthead(printheadName) == true))
                    {
                        _printheadViewModelList[index].UnattachGPIOPins();
                        _printheadViewModelList.RemoveAt(index);

                        OnPropertyChanged("PrintheadViewModelList");
                        OnPropertyChanged("CanRemovePrinthead");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference to an PrintheadViewModel with the matching Name property.
        /// </summary>
        /// <param name="printheadName"></param>
        /// <returns></returns>
        public PrintheadViewModel FindPrinthead(string printheadName)
        {
            foreach (PrintheadViewModel printheadViewModel in _printheadViewModelList)
            {
                if (printheadViewModel.Name == printheadName)
                {
                    return printheadViewModel;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds an Axis to the Printer.
        /// </summary>
        private void AddZAxis()
        {
            _printerModel.AddZAxis();
            AxisViewModel newZAxis = new AxisViewModel(_printerModel.AxisModelList[_printerModel.AxisModelList.Count - 1], _gPIOPinListsViewModel);
            _axisViewModelList.Add(newZAxis);
            _zAxisViewModelList.Add(newZAxis);
            OnPropertyChanged("AxisViewModelList");
            OnPropertyChanged("ZAxisViewModelList");
            OnPropertyChanged("ZAxesCreatedCount");
        }

        /// <summary>
        /// Adds an Axis with a Name to the Printer.
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method was created for XML Serializers.
        /// </remarks>
        public AxisViewModel AddZAxis(string axisName)
        {
            _printerModel.AddZAxis(axisName);
            AxisViewModel newZAxis = new AxisViewModel(_printerModel.AxisModelList[_printerModel.AxisModelList.Count - 1], _gPIOPinListsViewModel);
            _axisViewModelList.Add(newZAxis);
            _zAxisViewModelList.Add(newZAxis);
            OnPropertyChanged("AxisViewModelList");
            OnPropertyChanged("ZAxisViewModelList");
            OnPropertyChanged("ZAxesCreatedCount");
            return _axisViewModelList[_axisViewModelList.Count - 1];
        }

        /// <summary>
        /// Removes one Z Axis with the specified name from the printer.
        /// </summary>
        /// <param name="axisName"></param>
        public bool RemoveZAxis(string axisName)
        {
            if (_axisViewModelList.Count > 3) //The printer must have at least 3 axes.
            {
                for (int index = 0; index < _axisViewModelList.Count; index++)
                {
                    if (axisName == _axisViewModelList[index].Name
                        && _axisViewModelList[index].IsRemovable == true
                        && _printerModel.RemoveZAxis(axisName) == true)
                    {
                        _axisViewModelList[index].UnattachGPIOPins();
                        _axisViewModelList.RemoveAt(index);
                        _zAxisViewModelList.RemoveAt(index - 2);

                        OnPropertyChanged("AxisViewModelList");
                        OnPropertyChanged("ZAxisViewModelList");
                        OnPropertyChanged("ZAxesCreatedCount");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference to an AxisViewModel with the matching Name property.
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public AxisViewModel FindAxis(string axisName)
        {
            foreach (AxisViewModel axisViewModel in _axisViewModelList)
            {
                if (axisViewModel.Name == axisName)
                {
                    return axisViewModel;
                }
            }
            return null;
        }

        /// <summary>
        /// Calls OnPropertyChanged on the Min and Max Position properties for every Axis and Motorized Printhead.
        /// Typically called during automated calibration.
        /// </summary>
        public void UpdateMinMaxPositions()
        {
            foreach (AxisViewModel axisViewModel in _axisViewModelList)
            {
                axisViewModel.UpdateMinMaxPositions();
            }

            foreach (PrintheadViewModel printheadViewModel in _printheadViewModelList)
            {
                if (printheadViewModel.PrintheadType == PrintheadType.Motorized)
                {
                    MotorizedPrintheadTypeViewModel motorizedPrintheadTypeViewModel = (MotorizedPrintheadTypeViewModel)printheadViewModel.PrintheadTypeViewModel;
                    motorizedPrintheadTypeViewModel.UpdateMinMaxPositions();
                }
            }
        }

        /// <summary>
        /// Are all of the parameters set correctly such that printing can occur?
        /// </summary>
        /// <returns></returns>
        public bool ReadyToPrint()
        {
            return _printerModel.ReadyToPrint();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Adds a new Printhead to the Printer.
        /// </summary>
        private RelayCommand<object> _addPrintheadCommand;
        public ICommand AddPrintheadCommand
        {
            get
            {
                if (_addPrintheadCommand == null)
                { _addPrintheadCommand = new RelayCommand<object>(ExecuteAddPrintheadCommand, CanExecuteAddPrintheadCommand); }
                return _addPrintheadCommand;
            }
        }

        public bool CanExecuteAddPrintheadCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteAddPrintheadCommand(object notUsed)
        {
            AddPrinthead();
        }

        /// <summary>
        /// Removes a Printhead from the Printer.
        /// </summary>
        private RelayCommand<string> _removePrintheadCommand;
        public ICommand RemovePrintheadCommand
        {
            get
            {
                if (_removePrintheadCommand == null)
                { _removePrintheadCommand = new RelayCommand<string>(ExecuteRemovePrintheadCommand, CanExecuteRemovePrintheadCommand); }
                return _removePrintheadCommand;
            }
        }

        public bool CanExecuteRemovePrintheadCommand(string printheadName)
        {
            return (!String.IsNullOrWhiteSpace(printheadName)
                && _printheadViewModelList.Count > 1) ? true : false;
        }

        public void ExecuteRemovePrintheadCommand(string printheadName)
        {
            RemovePrinthead(printheadName);
        }

        /// <summary>
        /// Add a new Z Axis to the printer.
        /// </summary>
        private RelayCommand<object> _addZAxisCommand;
        public ICommand AddZAxisCommand
        {
            get
            {
                if (_addZAxisCommand == null)
                { _addZAxisCommand = new RelayCommand<object>(ExecuteAddZAxisCommand, CanExecuteAddPrintheadCommand); }
                return _addZAxisCommand;
            }
        }

        public bool CanExecuteAddZAxisCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteAddZAxisCommand(object notUsed)
        {
            AddZAxis();
        }

        /// <summary>
        /// Remove a Z Axis at the specified index.
        /// </summary>
        /// <param>
        /// string zName is the Name property of the printhead to be removed.
        /// </param>
        private RelayCommand<string> _removeZAxisCommand;
        public ICommand RemoveZAxisCommand
        {
            get
            {
                if (_removeZAxisCommand == null)
                { _removeZAxisCommand = new RelayCommand<string>(ExecuteRemoveZAxisCommand, CanExecuteRemoveZAxisCommand); }
                return _removeZAxisCommand;
            }
        }

        public bool CanExecuteRemoveZAxisCommand(string axisName)
        {
            return (!String.IsNullOrWhiteSpace(axisName)
                && _axisViewModelList.Count > 3) ? true : false;
        }

        public void ExecuteRemoveZAxisCommand(string axisName)
        {
            RemoveZAxis(axisName);
        }
        #endregion
    }
}
