using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrintModels.MaterialModels;

namespace ModiPrint.Models.PrintModels
{
    /// <summary>
    /// Contains parameters related to printing.
    /// </summary>
    public class PrintModel
    {
        #region Fields and Properties
        //Contains parameters regarding the printer.
        private PrinterModel _printerModel;

        //A list of all the different materials that will be printed.
        private List<MaterialModel> _materialModelList = new List<MaterialModel>();
        public List<MaterialModel> MaterialModelList
        {
            get { return _materialModelList; }
            set { _materialModelList = value; }
        }

        //Keeps track of the number of materials that have been created.
        //Does not decrease when materials are removed.
        //Used such that material names do not overlap.
        private int _materialsCreatedCount = 0;
        public int MaterialsCreatedCount
        {
            get { return _materialsCreatedCount; }
            set { _materialsCreatedCount = value; }
        }
        #endregion

        #region Constructors
        public PrintModel(PrinterModel PrinterModel)
        {
            _printerModel = PrinterModel;
            
            //One material by default.
            AddMaterial();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add one Material to the Print.
        /// </summary>
        public void AddMaterial()
        {
            string newName = "Material " + ++_materialsCreatedCount;
            _materialModelList.Add(new MaterialModel(newName, _printerModel));
        }

        /// <summary>
        /// Adds one Material to the Print with the given Name.
        /// </summary>
        /// <param name="newMaterialName"></param>
        /// <remarks>
        /// This method was created for the XML Serializers.
        /// </remarks>
        public void AddMaterial(string newMaterialName)
        {
            _materialModelList.Add(new MaterialModel(newMaterialName, _printerModel));
        }

        /// <summary>
        /// Removes one Material with the specified name from the Print parameters.
        /// </summary>
        /// <param name="materialName"></param>
        public bool RemoveMaterial(string materialName)
        {
            if (_materialModelList.Count > 1) //The Print Parameters must have at least 1 Material.
            {
                for (int index = 0; index < _materialModelList.Count; index++)
                {
                    if (materialName == _materialModelList[index].Name)
                    {
                        _materialModelList.RemoveAt(index);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference of a material with the matching RepRapID.
        /// </summary>
        /// <param name="repRapID"></param>
        /// <returns></returns>
        public MaterialModel FindMaterial(string repRapID)
        {
            foreach(MaterialModel materialModel in _materialModelList)
            {
                if (materialModel.RepRapID == repRapID)
                { return materialModel; }
            }
            return null;
        }

        /// <summary>
        /// Finds and returns a reference of a material with the matching Name.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public MaterialModel FindMaterialByName(string materialName)
        {
            foreach (MaterialModel materialModel in _materialModelList)
            {
                if (materialModel.Name == materialName)
                { return materialModel; }
            }
            return null;
        }

        /// <summary>
        /// Are all of the parameters set correctly such that printing can occur?
        /// </summary>
        /// <returns></returns>
        public bool ReadyToPrint()
        {
            foreach(MaterialModel materialModel in _materialModelList)
            {
                if (materialModel.ReadyToPrint() == false)
                { return false; }
            }
            return true;
        }
        #endregion
    }
}
