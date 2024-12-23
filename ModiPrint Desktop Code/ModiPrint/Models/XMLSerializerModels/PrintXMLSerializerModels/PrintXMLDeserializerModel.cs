using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.MaterialXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrintViewModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels
{
    public class PrintXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Contains functions to convert XML to Print class properties.
        MaterialXMLDeserializerModel _materialXMLDeserializerModel;

        //A list of the MaterialViewModels that were deserialized.
        private List<MaterialViewModel> _deserializedMaterialViewModelsList = new List<MaterialViewModel>();
        #endregion

        #region Constructor
        public PrintXMLDeserializerModel(PrinterViewModel PrinterViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _materialXMLDeserializerModel = new MaterialXMLDeserializerModel(PrinterViewModel, ErrorListViewModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads Print properties from XML and sets a Print with said properties.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="materialViewModel"></param>
        public void DeserializePrint(XmlReader xmlReader, PrintViewModel printViewModel)
        {
            //Read through each element of the XML string and populate each property of the Print.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "Print" element is reached.
                    if ((xmlReader.Name == "Print") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        //Remove all Print elements that were not just deserialized.
                        RemoveNonDeserializedMaterials(printViewModel);
                        return;
                    }

                    //Deserialize and populate each property of the Print.
                    switch (xmlReader.Name)
                    {
                        case "Material":
                            LoadMaterial(xmlReader, printViewModel);
                            break;
                        case "MaterialsCreatedCount":
                            printViewModel.MaterialsCreatedCount = xmlReader.ReadElementContentAsInt();
                            break;
                        default:
                            base.ReportErrorUnrecognizedElement(xmlReader);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize a Material.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printViewModel"></param>
        private void LoadMaterial(XmlReader xmlReader, PrintViewModel printViewModel)
        {
            //Name of the new Material.
            string materialName = xmlReader.GetAttribute("Name");

            //Reference of the new Material that will be deserialized.
            MaterialViewModel newMaterial = null;

            //See if there is a matching Material.
            bool matchingMaterial = false;
            foreach (MaterialViewModel materialViewModel in printViewModel.MaterialViewModelList)
            {
                if (materialViewModel.Name == materialName)
                {
                    matchingMaterial = true;
                    newMaterial = materialViewModel;
                }
            }

            //If there is no matching Material, then create a new Material.
            if (matchingMaterial == false)
            {
                newMaterial = printViewModel.AddMaterial(materialName);
            }

            //Loads the new material with properties from XML and remembers the newly deserialized Material.
            if (newMaterial != null)
            {
                _materialXMLDeserializerModel.DeserializerMaterial(xmlReader, newMaterial);
                _deserializedMaterialViewModelsList.Add(newMaterial);
            }
        }

        /// <summary>
        /// Remove all Materials that were not just deserialized.
        /// </summary>
        /// <param name="printViewModel"></param>
        private void RemoveNonDeserializedMaterials(PrintViewModel printViewModel)
        {
            //List of Materials that need to be removed.
            //Materials need to be removed if they are a part of the Print but were not just deserialized.
            List<MaterialViewModel> nonDeserializedMaterialViewModelsList = new List<MaterialViewModel>();

            //Check all Materials and mark appropriate ones for removal.
            foreach (MaterialViewModel existingMaterialViewModel in printViewModel.MaterialViewModelList)
            {
                //Check if the existing Material is a deserialized Material.
                bool wasDeserialized = false;
                foreach(MaterialViewModel deserializedMaterialViewModel in _deserializedMaterialViewModelsList)
                {
                    if (existingMaterialViewModel.Name == deserializedMaterialViewModel.Name)
                    {
                        wasDeserialized = true;
                        break;
                    }
                }

                //If existing Material was not a deserialized Material, then mark it for removal.
                if (wasDeserialized == false)
                {
                    nonDeserializedMaterialViewModelsList.Add(existingMaterialViewModel);
                }
            }

            //Remove all non-deserialized Materials.
            foreach (MaterialViewModel nonDeserializedMaterial in nonDeserializedMaterialViewModelsList)
            {
                printViewModel.RemoveMaterial(nonDeserializedMaterial.Name);
            }
        }
        #endregion
    }
}
