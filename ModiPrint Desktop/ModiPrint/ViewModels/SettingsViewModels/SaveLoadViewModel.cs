using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrintViewModels;
using System.IO;
using Microsoft.Win32;

namespace ModiPrint.ViewModels.SettingsViewModels
{
    public delegate void SettingsLoadedEventHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Saves and loads this program's Print-related elements to and from XML.
    /// </summary>
    public class SaveLoadViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains functions to serialize and deserialize the program's Printer, Print, and GCode Manager classes to / from XML.
        ModiPrintXMLSerializerModel _modiPrintXMLSerializerModel;
        ModiPrintXMLDeserializerModel _modiPrintXMLDeserializerModel;

        //Classes required by the Serializer and Deserializer.
        GCodeManagerViewModel _gCodeManagerViewModel;
        PrinterViewModel _printerViewModel;
        PrintViewModel _printViewModel;
        ErrorListViewModel _errorListViewModel;

        //Event triggers when Print-related elements are loaded from XML.
        public event SettingsLoadedEventHandler SettingsLoaded;
        private void OnSettingsLoaded()
        {
            if (SettingsLoaded != null)
            { SettingsLoaded(this, new EventArgs()); }
        }
        #endregion

        #region Constructor
        public SaveLoadViewModel(GCodeManagerViewModel GCodeManagerViewModel, PrinterViewModel PrinterViewModel, PrintViewModel PrintViewModel, ErrorListViewModel ErrorListViewModel)
        {
            _gCodeManagerViewModel = GCodeManagerViewModel;
            _printerViewModel = PrinterViewModel;
            _printViewModel = PrintViewModel;
            _errorListViewModel = ErrorListViewModel;

            _modiPrintXMLSerializerModel = new ModiPrintXMLSerializerModel(_gCodeManagerViewModel, _printerViewModel, _printViewModel, _errorListViewModel);
            _modiPrintXMLDeserializerModel = new ModiPrintXMLDeserializerModel(_gCodeManagerViewModel, _printerViewModel, _printerViewModel.GPIOPinListsViewModel, _printViewModel, _errorListViewModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Save Print-related elements to XML.
        /// </summary>
        private void SaveModiPrintSettings()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = ".xml";
                saveFileDialog.Filter = "XML Save Files (.xml)|*.xml|Text Files (.txt)|*.txt|All Files (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string serializedModiPrint = _modiPrintXMLSerializerModel.SerializeModiPrint(_printerViewModel, _printViewModel, _gCodeManagerViewModel);
                    File.WriteAllText(saveFileDialog.FileName, serializedModiPrint);
                }
            }
            catch
            {
                _errorListViewModel.AddError("Print Settings Save", "Unable to Save Print Settings");
            }
        }

        /// <summary>
        /// Load Print-related elements from XML.
        /// </summary>
        private void LoadModiPrintSettings()
        {
            try
            {
                //Open file.
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML Save Files (.xml)|*.xml|Text Files (.txt)|*.txt|All Files (*.*)|*.*";

                //Reads file.
                if (openFileDialog.ShowDialog() == true)
                {
                    string serializedModiPrint = File.ReadAllText(openFileDialog.FileName);
                    _modiPrintXMLDeserializerModel.DeserializeModiPrint(serializedModiPrint, _printerViewModel, _printViewModel, _gCodeManagerViewModel);

                    //Fire event.
                    OnSettingsLoaded();
                }
            }
            catch
            {
                _errorListViewModel.AddError("GCode File Manager", "Unable to load XML file");
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Saves a .xml file with Print-related settings.
        /// </summary>
        private RelayCommand<object> _saveModiPrintXMLFileCommand;
        public ICommand SaveModiPrintXMLFileCommand
        {
            get
            {
                if (_saveModiPrintXMLFileCommand == null)
                { _saveModiPrintXMLFileCommand = new RelayCommand<object>(ExecuteSaveModiPrintXMLFileCommand, CanExecuteSaveModiPrintXMLFileCommand); }
                return _saveModiPrintXMLFileCommand;
            }
        }

        public bool CanExecuteSaveModiPrintXMLFileCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteSaveModiPrintXMLFileCommand(object notUsed)
        {
            SaveModiPrintSettings();
        }

        /// <summary>
        /// Loads a .xml file with Print-related settings.
        /// </summary>
        private RelayCommand<object> _loadModiPrintXMLFileCommand;
        public ICommand LoadModiPrintXMLFileCommand
        {
            get
            {
                if (_loadModiPrintXMLFileCommand == null)
                { _loadModiPrintXMLFileCommand = new RelayCommand<object>(ExecuteLoadModiPrintXMLFileCommand, CanExecuteLoadModiPrintXMLFileCommand); }
                return _loadModiPrintXMLFileCommand;
            }
        }

        public bool CanExecuteLoadModiPrintXMLFileCommand(object notUsed)
        {
            //To Do: Do not allow this if Printer is Printing.
            return true;
        }

        public void ExecuteLoadModiPrintXMLFileCommand(object notUsed)
        {
            LoadModiPrintSettings();
        }

        #endregion
    }
}
