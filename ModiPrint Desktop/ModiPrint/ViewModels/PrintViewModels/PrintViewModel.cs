using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.GCodeModels;
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;
using ModiPrint.ViewModels.SerialCommunicationViewModels;

namespace ModiPrint.ViewModels.PrintViewModels
{
    //Fires when a RepRapID from any Material is changed.
    public delegate void PrintViewModelRepRapIDChanged(object sender);

    public class PrintViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains parameters related to printing.
        private PrintModel _printModel;
        public PrintModel PrintModel
        {
            get { return _printModel; }
        }

        //Manages the messages that are sent and received through the serial port.
        private SerialMessageDisplayViewModel _serialMessageDisplayViewModel;

        //Contains a list of MaterialViewModels.
        private ObservableCollection<MaterialViewModel> _materialViewModelList = new ObservableCollection<MaterialViewModel>();
        public ObservableCollection<MaterialViewModel> MaterialViewModelList
        {
            get { return _materialViewModelList; }
        }

        //Keeps track of the number of materials that have been created.
        //Does not decrease when materials are removed.
        //Used such that material names do not overlap.
        public int MaterialsCreatedCount
        {
            get { return _printModel.MaterialsCreatedCount; }
            set
            {
                _printModel.MaterialsCreatedCount = value;
                OnPropertyChanged("MaterialsCreatedCount");
                OnPropertyChanged("CanRemoveMaterial");
            }
        }

        //List of all T commands in the uploaded g-code file that is not in use.
        //Used to populate possible option for the RepRapID parameter in Materials.
        //If a RepRapID uses one of these values, 
        private ObservableCollection<string> _availibleRepRapIDList = new ObservableCollection<string>();
        public ObservableCollection<string> AvailibleRepRapIDList
        {
            get { return _availibleRepRapIDList; }
            set
            {
                _availibleRepRapIDList = value;
                OnPropertyChanged("AvailibleRepRapIDList");
            }
        }

        //Number of all T commands in the uploaded g-code file.
        private int _repRapIDCount = 0;
        public int RepRapIDCount
        {
            get { return _repRapIDCount; }
            set { _repRapIDCount = value; }
        }

        //Returns true if there are two or more Materials.
        public bool CanRemoveMaterial
        {
            get { return (_materialViewModelList.Count > 1) ? true : false; }
        }
        #endregion

        #region Events
        //Events that is fired when a RepRapID from any Material is changed.
        public event PrintViewModelRepRapIDChanged PrintViewModelRepRapIDChanged;
        private void OnPrintViewModelRepRapIDChanged(object sender)
        {
            if (PrintViewModelRepRapIDChanged != null)
            { PrintViewModelRepRapIDChanged(this); }
        }
        #endregion

        #region Constructor
        public PrintViewModel(PrintModel PrintModel, SerialMessageDisplayViewModel SerialMessageDisplayViewModel)
        {
            _printModel = PrintModel;
            _serialMessageDisplayViewModel = SerialMessageDisplayViewModel;

            //Populates MaterialViewModelList with all Materials within MaterialModel's MaterialModelList.
            foreach (MaterialModel materialModel in _printModel.MaterialModelList)
            {
                int newIndex = _printModel.MaterialModelList.Count - 1;
                _materialViewModelList.Add(new MaterialViewModel(_printModel.MaterialModelList[newIndex], this));
            }

            //Subscribe to all events in each materialViewModel.
            foreach (MaterialViewModel materialViewModel in _materialViewModelList)
            {
                materialViewModel.RepRapIDSelected += new RepRapIDSelectedEventHandler(RemoveAvailibleRepRapID);
                materialViewModel.RepRapIDCleared += new RepRapIDClearedEventHandler(AddAvailibleRepRapID);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a Material to the PrintModel and PrintViewModel.
        /// </summary>
        public void AddMaterial()
        {
            _printModel.AddMaterial();
            int newIndex = _printModel.MaterialModelList.Count - 1;
            _materialViewModelList.Add(new MaterialViewModel(_printModel.MaterialModelList[newIndex], this));
            _materialViewModelList[_materialViewModelList.Count - 1].RepRapIDSelected += new RepRapIDSelectedEventHandler(RemoveAvailibleRepRapID);
            _materialViewModelList[_materialViewModelList.Count - 1].RepRapIDCleared += new RepRapIDClearedEventHandler(AddAvailibleRepRapID);
            OnPropertyChanged("MaterialsCreatedCount");
            OnPropertyChanged("CanRemoveMaterial");
        }

        /// <summary>
        /// Adds a Material with to the Print with the given Name.
        /// </summary>
        /// <param name="materialName"></param>
        public MaterialViewModel AddMaterial(string materialName)
        {
            _printModel.AddMaterial(materialName);
            int newIndex = _printModel.MaterialModelList.Count - 1;
            _materialViewModelList.Add(new MaterialViewModel(_printModel.MaterialModelList[newIndex], this));
            _materialViewModelList[_materialViewModelList.Count - 1].RepRapIDSelected += new RepRapIDSelectedEventHandler(RemoveAvailibleRepRapID);
            _materialViewModelList[_materialViewModelList.Count - 1].RepRapIDCleared += new RepRapIDClearedEventHandler(AddAvailibleRepRapID);
            OnPropertyChanged("MaterialViewModelList");
            OnPropertyChanged("MaterialsCreatedCount");
            OnPropertyChanged("CanRemoveMaterial");
            return _materialViewModelList[_materialViewModelList.Count - 1];
        }

        /// <summary>
        /// Removes one Material with the specified name from the Print parameters.
        /// </summary>
        /// <param name="materialName"></param>
        public bool RemoveMaterial(string materialName)
        {
            if (_materialViewModelList.Count > 1) //The Print Parameters must have at least 1 Material.
            {
                for (int index = 0; index < _materialViewModelList.Count; index++)
                {
                    if (materialName == _materialViewModelList[index].Name
                     && _printModel.RemoveMaterial(materialName))
                    {
                        _materialViewModelList.RemoveAt(index);
                        OnPropertyChanged("MaterialViewModelList");
                        OnPropertyChanged("MaterialsCreatedCount");
                        OnPropertyChanged("CanRemoveMaterial");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference to an MaterialViewModel with the matching Name property.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public MaterialViewModel FindMaterial(string materialName)
        {
            foreach (MaterialViewModel materialViewModel in _materialViewModelList)
            {
                if (materialViewModel.Name == materialName)
                {
                    return materialViewModel;
                }
            }
            return null;
        }

        /// <summary>
        /// Add an entry to AvailibleRepRapIDList.
        /// </summary>
        /// <param name="repRapID"></param>
        private void AddAvailibleRepRapID(string repRapID)
        {
            if (!String.IsNullOrWhiteSpace(repRapID))
            {
                if (!_availibleRepRapIDList.Contains(repRapID))
                {
                    _availibleRepRapIDList.Add(repRapID);
                }
            }
            OnPropertyChanged("AvailibleRepRapIDList");
            OnPrintViewModelRepRapIDChanged(this);
        }

        /// <summary>
        /// Remove an entry from AvailibleRepRapIDList.
        /// </summary>
        /// <param name="repRapID"></param>
        private void RemoveAvailibleRepRapID(string repRapID)
        {
            _availibleRepRapIDList.Remove(repRapID);
            OnPropertyChanged("AvailibleRepRapIDList");
            OnPrintViewModelRepRapIDChanged(this);
        }

        /// <summary>
        /// Calls OnPropertyChanged on AvailibleRepRapIDList.
        /// </summary>
        public void UpdateAvailibleRepRapIDList()
        {
            OnPropertyChanged("AvailibleRepRapIDList");
        }

        /// <summary>
        /// Clears RepRap ID properties on all Materials.
        /// </summary>
        public void ClearAllRepRapIDs()
        {
            foreach (MaterialViewModel materialViewModel in _materialViewModelList)
            {
                materialViewModel.RepRapID = "";
            }
        }

        /// <summary>
        /// Returns true if all availible RepRapIDs from the gcode file is set in Materials.
        /// </summary>
        public bool AvailibleRepRapIDSet()
        {
            int setRepRapIDs = 0;
            foreach (MaterialViewModel material in _materialViewModelList)
            {
                if (!String.IsNullOrWhiteSpace(material.RepRapID))
                {
                    setRepRapIDs++;
                }
            }

            return (setRepRapIDs == _repRapIDCount) ? true : false;
        }

        /// <summary>
        /// Are all of the parameters set correctly such that printing can occur?
        /// </summary>
        /// <returns></returns>
        public bool ReadyToPrint()
        {
            return _printModel.ReadyToPrint();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Adds a new Material to the Print parameters.
        /// </summary>
        private RelayCommand<object> _addMaterialCommand;
        public ICommand AddMaterialCommand
        {
            get
            {
                if (_addMaterialCommand == null)
                { _addMaterialCommand = new RelayCommand<object>(ExecuteAddMaterialCommand, CanExecuteAddMaterialCommand); }
                return _addMaterialCommand;
            }
        }

        public bool CanExecuteAddMaterialCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteAddMaterialCommand(object notUsed)
        {
            AddMaterial();
        }

        /// <summary>
        /// Removes a Material from the Print parameters.
        /// </summary>
        /// <param>
        /// String Name is the Name property of the Material to be removed.
        /// </param>
        private RelayCommand<string> _removeMaterialCommand;
        public ICommand RemoveMaterialCommand
        {
            get
            {
                if (_removeMaterialCommand == null)
                { _removeMaterialCommand = new RelayCommand<string>(ExecuteRemoveMaterialCommand, CanExecuteRemoveMaterialCommand); }
                return _removeMaterialCommand;
            }
        }

        public bool CanExecuteRemoveMaterialCommand(string materialName)
        {
            return ((!String.IsNullOrWhiteSpace(materialName))
                 && (_materialViewModelList.Count > 1)) ? true : false;
        }

        public void ExecuteRemoveMaterialCommand(string materialName)
        {
            FindMaterial(materialName).RepRapID = "";
            RemoveMaterial(materialName);
        }
        #endregion
    }
}
