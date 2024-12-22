using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.Models.GCodeModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;

namespace ModiPrint.Models.XMLSerializerModels.GCodeXMLSerializerModels
{
    public class GCodeManagerXMLDeserializerModel : XMLConverterModel
    {
        #region Constructor
        public GCodeManagerXMLDeserializerModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads GCodeManagerViewModel properties from XML and loads a GCodeManagerViewModel with said properties.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="gCodeManagerViewModel"></param>
        public void DeserializeGCode(XmlReader xmlReader, GCodeManagerViewModel gCodeManagerViewModel)
        {
            //Read through each element of the XML string and populate each property of the GCodeManager.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "GCodeManager" element is reached.
                    if ((xmlReader.Name == "GCodeManager") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "UploadedGCode":
                            gCodeManagerViewModel.UploadedGCodeModel.GCodeStr = xmlReader.ReadElementContentAsString();
                            gCodeManagerViewModel.UpdateRepRapIDList();
                            gCodeManagerViewModel.OnGCodeFileUploaded();
                            break;
                        case "UploadedGCodeType":
                            gCodeManagerViewModel.UploadedGCodeType = (GCodeType)Enum.Parse(typeof(GCodeType), xmlReader.ReadElementContentAsString(), false);
                            break;
                        case "GCodeFileName":
                            gCodeManagerViewModel.GCodeFileName = xmlReader.ReadElementContentAsString();
                            break;
                        default:
                            base.ReportErrorUnrecognizedElement(xmlReader);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
