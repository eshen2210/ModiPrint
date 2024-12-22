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
using ModiPrint.Models.PrinterModels.MicrocontrollerModels;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels
{

    /// <summary>
    /// Interfaces MicrocontrollerModel with the GUI.
    /// </summary>
    public class MicrocontrollerViewModel : ViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        private MicrocontrollerModel _microcontrollerModel;

        //Keeps lists of GPIOPinViewModels as sorted by their possible settings.
        private GPIOPinListsViewModel _gPIOPinListsViewModel;
        public GPIOPinListsViewModel GPIOPinListsViewModel
        {
            get { return _gPIOPinListsViewModel; }
        }

        //List of all GPIO Pins on the microcontroller.
        private ObservableCollection<GPIOPinViewModel> _gPIOPinViewModelList = new ObservableCollection<GPIOPinViewModel>();
        public ObservableCollection<GPIOPinViewModel> GPIOPinViewModelList
        {
            get { return _gPIOPinViewModelList; }
        }
        #endregion

        #region Constructor
        public MicrocontrollerViewModel(MicrocontrollerModel MicrocontrollerModel, GPIOPinListsViewModel GPIOPinListsViewModel)
        {
            _microcontrollerModel = MicrocontrollerModel;
            _gPIOPinListsViewModel = GPIOPinListsViewModel;

            //Populates the GPIOPinViewModelList based on its model counterpart.
            foreach (GPIOPinModel gPIOPinModel in _microcontrollerModel.GPIOPinModelList)
            {
                GPIOPinViewModel pinViewModel = null;

                if (gPIOPinModel.GetType() == typeof(PWMPinModel))
                {
                    pinViewModel = new PWMPinViewModel((PWMPinModel)gPIOPinModel);
                }
                else if (gPIOPinModel.GetType() == typeof(CommunicationPinModel))
                {
                    pinViewModel = new CommunicationPinViewModel((CommunicationPinModel)gPIOPinModel);
                }
                else if (gPIOPinModel.GetType() == typeof(AnalogInPinModel))
                {
                    pinViewModel = new AnalogInPinViewModel((AnalogInPinModel)gPIOPinModel);
                }
                else if (gPIOPinModel.GetType() == typeof(DigitalPinModel))
                {
                    pinViewModel = new DigitalPinViewModel((DigitalPinModel)gPIOPinModel);
                }

                if (pinViewModel != null)
                {
                    _gPIOPinViewModelList.Add(pinViewModel);
                    //Populates the lists in GPIOPinListsViewModel.
                    _gPIOPinListsViewModel.AppendPinSettingList(pinViewModel);
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds and returns a reference to a GPIOPin with the matching Name property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GPIOPinViewModel FindPin(string name)
        {
            foreach (GPIOPinViewModel gPIOPinViewModel in GPIOPinViewModelList)
            {
                if (gPIOPinViewModel.Name == name)
                {
                    return gPIOPinViewModel;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns a reference to a GPIOPin with the matching PinID property.
        /// </summary>
        /// <param name="pinID"></param>
        /// <returns></returns>
        public GPIOPinViewModel FindPin(int pinID)
        {
            foreach (GPIOPinViewModel gPIOPinViewModel in GPIOPinViewModelList)
            {
                if (gPIOPinViewModel.PinID == pinID)
                {
                    return gPIOPinViewModel;
                }
            }
            return null;
        }
        #endregion
    }
}
