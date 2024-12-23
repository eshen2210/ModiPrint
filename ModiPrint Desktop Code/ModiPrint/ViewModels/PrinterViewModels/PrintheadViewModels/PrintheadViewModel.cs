using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels
{
    //Associated with the PrintheadViewModel class.
    //States the status of the Printhead during operation.
    public enum PrintheadStatus
    {
        Idle, //Default value. This Printhead is not in use.
        BeingSet, //The command to set this Printhead has been sent to the microcontroller.
        Active //The microcontroller is currently using this Printhead.
    }

    /// <summary>
    /// A ViewModel that interfaces PrintheadModel with the GUI.
    /// </summary>
    public class PrintheadViewModel : ViewModel
    {
        #region Fields and Properties
        //That Model this ViewModel is interfacing with the GUI.
        private PrintheadModel _printheadModel;
        public PrintheadModel PrintheadModel
        {
            get { return _printheadModel; }
        }

        //How ModiPrint identifies this printhead in the GUI.
        public string Name
        {
            get { return _printheadModel.Name; }
        }

        //Provides lists of GPIOPins sorted by function.
        private GPIOPinListsViewModel _gPIOPinListsViewModel;

        //Name of the Z Axis that this printhead is attached to.
        private AxisViewModel _attachedZAxisViewModel;
        public AxisViewModel AttachedZAxisViewModel
        {
            get { return _attachedZAxisViewModel; }
            set
            {
                _printheadModel.AttachedZAxisModel = value.AxisModel;
                _attachedZAxisViewModel = value;
                OnPropertyChanged("AttachedZAxisViewModel");
            }
        }

        //Image that represents this Printhead.
        private string _imageSource = "";
        public string ImageSource
        {
            get { return _imageSource; }
        }

        //What type of dispensing mechanism does this printhead have?
        //Type specific properties are contained in PrintheadType class.
        private PrintheadTypeViewModel _printheadTypeViewModel;
        public PrintheadTypeViewModel PrintheadTypeViewModel
        {
            get { return _printheadTypeViewModel; }
        }

        public PrintheadType PrintheadType
        {
            get { return _printheadModel.PrintheadType; }
            set
            {
                _printheadModel.PrintheadType = value;
                switch (_printheadModel.PrintheadType)
                {
                    case PrintheadType.Motorized:
                        _printheadTypeViewModel = new MotorizedPrintheadTypeViewModel((MotorizedPrintheadTypeModel)_printheadModel.PrintheadTypeModel, _gPIOPinListsViewModel);
                        _imageSource = "/Resources/General/MotorizedPrinthead.png";
                        break;
                    case PrintheadType.Valve:
                        _printheadTypeViewModel = new ValvePrintheadTypeViewModel((ValvePrintheadTypeModel)_printheadModel.PrintheadTypeModel, _gPIOPinListsViewModel);
                        _imageSource = "/Resources/General/ValvePrinthead.png";
                        break;
                    case PrintheadType.Custom:
                        _printheadTypeViewModel = new CustomPrintheadTypeViewModel((CustomPrintheadTypeModel)_printheadModel.PrintheadTypeModel, _gPIOPinListsViewModel);
                        break;
                }
                OnPropertyChanged("PrintheadType");
            }
        }

        //Returns values of PrintheadType that are bindable to a control's Itemsource property.
        public IEnumerable<PrintheadType> PrintheadTypeValues
        {
            get { return Enum.GetValues(typeof(PrintheadType)).Cast<PrintheadType>(); }
        }

        //Offset of Printhead.
        //In milimeters.
        //Note: These values are relative and do not specify a specific position on the print surface.
        //Offset values are compared to other offset values to determine new positioning when switching Printheads.

        public double XOffset
        {
            get { return _printheadModel.XOffset; }
            set
            {
                _printheadModel.XOffset = value;
                OnPropertyChanged("XOffset");
            }
        }

        public double YOffset
        {
            get { return _printheadModel.YOffset; }
            set
            {
                _printheadModel.YOffset = value;
                OnPropertyChanged("YOffset");
            }
        }

        //States the current status of the Printhead during operation.
        private PrintheadStatus _printheadStatus = PrintheadStatus.Idle;
        public PrintheadStatus PrintheadStatus
        {
            get { return _printheadStatus; }
            set
            {
                _printheadStatus = value;
                OnPropertyChanged("PrintheadStatus");
            }
        }
        #endregion

        #region Constructors
        public PrintheadViewModel(PrintheadModel PrintheadModel, GPIOPinListsViewModel GPIOPinListsViewModel)
        {
            _printheadModel = PrintheadModel;
            _gPIOPinListsViewModel = GPIOPinListsViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public void UnattachGPIOPins()
        {
            if (_printheadTypeViewModel != null)
            { _printheadTypeViewModel.UnattachGPIOPins(); }
        }
        #endregion
    }
}
