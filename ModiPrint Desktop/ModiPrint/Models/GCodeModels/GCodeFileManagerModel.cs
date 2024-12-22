using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.GCodeModels
{
    public enum GCodeType
    {
        RepRap,
        ModiPrint,
        NotUploaded
    }

    public class GCodeFileManagerModel
    {
        #region Fields and Properties
        //References to the two GCode classes that this will interact with.
        private GCodeModel _uploadedGCodeModel;
        public GCodeModel UploadedGCodeModel
        {
            get { return _uploadedGCodeModel; }
        }

        //The type of g-code file that is uploaded.
        private GCodeType _uploadedGCodeType = GCodeType.NotUploaded;
        public GCodeType UploadedGCodeType
        {
            get { return _uploadedGCodeType; }
            set { _uploadedGCodeType = value; }
        }

        //Contains functions to display errors to the GUI.
        private ErrorListViewModel _errorListViewModel;
        #endregion

        #region Constructor
        public GCodeFileManagerModel(GCodeModel UploadedGCodeModel, ErrorListViewModel ErrorListViewModel)
        {
            _uploadedGCodeModel = UploadedGCodeModel;
            _errorListViewModel = ErrorListViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads a .gcode file into the program with OpenFileDialog.
        /// Determines which g-code file to load into depending on the file extension.
        /// Returns the name of the file.
        /// </summary>
        public string UploadGCodeFile()
        {
            try
            {
                //Open file.
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "G-code Files (.gcode, .mdpt)|*.gcode; *.mdpt|All files (*.*)|*.*";

                //Reads file.
                if (openFileDialog.ShowDialog() == true)
                {
                    string extenstion = Path.GetExtension(openFileDialog.FileName);
                    if (extenstion == ".gcode")
                    {
                        _uploadedGCodeModel.GCodeStr = File.ReadAllText(openFileDialog.FileName);
                        _uploadedGCodeType = GCodeType.RepRap;
                    }
                    else if (extenstion == ".mdpt")
                    {
                        _uploadedGCodeModel.GCodeStr = File.ReadAllText(openFileDialog.FileName);
                        _uploadedGCodeType = GCodeType.ModiPrint;
                    }
                    else
                    {
                        _errorListViewModel.AddError("GCode File Manager", "Unrecognized file extension. File extension must be .gcode or .mdpt.");
                    }

                    return Path.GetFileName(openFileDialog.FileName);
                }
            }
            catch
            {
                _errorListViewModel.AddError("GCode File Manager", "Unknown error, unable to upload g-code file.");
            }

            return "";
        }

        /// <summary>
        /// Saves the g-code into a file with SaveFileDialog.
        /// </summary>
        /// <param name="modiPrintGCode"></param>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        public void SaveGCodeFile(string modiPrintGCode, string fileType, string fileFilter, ref string fileName)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = fileType;
                saveFileDialog.Filter = fileFilter;

                if (saveFileDialog.ShowDialog() == true)
                {
                    fileName = saveFileDialog.FileName;
                    File.WriteAllText(fileName, modiPrintGCode);
                }
            }
            catch
            {
                _errorListViewModel.AddError("GCode File Manager", "Unknown error. Unable to save .mdpt file.");
            }
        }
        #endregion
    }
}
