using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels
{
    /// <summary>
    /// Manages lists of the GPIOPinViewModels in MicrocontrollerViewModel.
    /// Contains functions to filter and attach GPIOPinViewModels to Printer equipment.
    /// </summary>
    public class GPIOPinListsViewModel : ViewModel
    {
        #region Fields and Properties
        //List of all GPIO Pins categorized by their PinSettingList property.
        //At some index x, all Pins that can possess the PinSetting value are in the same list.
        private List<PinSetting> _gPIOPinSettingCategories = new List<PinSetting>();
        private ObservableCollection<ObservableCollection<GPIOPinViewModel>> _gPIOPinBySettingList = new ObservableCollection<ObservableCollection<GPIOPinViewModel>>();
        #endregion

        #region Constructor
        public GPIOPinListsViewModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a PinViewModel into the PinSettingsList.
        /// </summary>
        /// <param name="pinName"></param>
        /// <param name="pinSetting"></param>
        public void AppendPinSettingList(GPIOPinViewModel pinViewModel)
        {
            foreach (PinSetting pinSetting in pinViewModel.GPIOPinModel.PossiblePinSettingList)
            {
                int categoryIndex;
                for (categoryIndex = 0; categoryIndex < _gPIOPinSettingCategories.Count; categoryIndex++)
                {
                    if (_gPIOPinSettingCategories[categoryIndex] == pinSetting)
                    { break; }
                }
                //Creates a new PinSetting category if applicable.
                if (categoryIndex == _gPIOPinSettingCategories.Count)
                {
                    _gPIOPinSettingCategories.Add(pinSetting);
                    _gPIOPinBySettingList.Add(new ObservableCollection<GPIOPinViewModel>());
                }
                _gPIOPinBySettingList[categoryIndex].Add(pinViewModel);
            }
            OnPropertyChanged("GPIOPinSettingCategoryList");
            OnPropertyChanged("GPIOPinSettingList");
        }

        /// <summary>
        /// Returns an Observable Collection of GPIOPinModels that have the specified PinSetting property.
        /// Called by the GetUnattachedPinsByThisPinSetting property.
        /// </summary>
        /// <param name="pinSetting"></param>
        /// <returns></returns>
        public ObservableCollection<GPIOPinViewModel> GetUnattachedPinsByPinSetting(PinSetting pinSetting)
        {
            ObservableCollection<GPIOPinViewModel> pinList = new ObservableCollection<GPIOPinViewModel>();
            for (int categoryIndex = 0; categoryIndex < _gPIOPinSettingCategories.Count; categoryIndex++)
            {
                if (_gPIOPinSettingCategories[categoryIndex] == pinSetting)
                {
                    foreach (GPIOPinViewModel gPIOPinViewModel in _gPIOPinBySettingList[categoryIndex])
                    {
                        if (gPIOPinViewModel.IsAttached == false)
                        { pinList.Add(gPIOPinViewModel); }
                    }
                }
            }
            return pinList;
        }

        /// <summary>
        /// Called by this class' GPIOPinViewModelList property setters.
        /// </summary>
        /// <param name="pinViewModelField"></param>
        /// <param name="possiblePinViewModelListField"></param>
        /// <param name="pinSetting"></param>
        public void RefreshGPIOPinViewModelList(ref GPIOPinViewModel pinViewModelField, ref ObservableCollection<GPIOPinViewModel> possiblePinViewModelListField, PinSetting pinSetting)
        {
            possiblePinViewModelListField = GetUnattachedPinsByPinSetting(pinSetting);
            possiblePinViewModelListField.Insert(0, pinViewModelField); //The first option of the list is to keep the same GPIO Pin.
            //The rest of the list is populated by unattached GPIO Pins.
        }
        #endregion

    }
}
