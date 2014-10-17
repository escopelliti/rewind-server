using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace ServerTest
{
    static class XMLFactory
    {

        public static XDocument CreateXMLDocument(string type, List<ProtocolUtils.FileStruct> filesList)
        {
            XElement root = new XElement(ProtocolUtils.REQUEST);
            XDocument xmlDoc = new XDocument(root);
            root.Add(new XElement(ProtocolUtils.TYPE, type));
            XElement contentElement = new XElement(ProtocolUtils.CONTENT);
            contentElement = SetContentWithFiles(filesList, contentElement);
            root.Add(contentElement);
            return xmlDoc;
        }

        public static XDocument CreateXMLDocument(string type, object content)
        {
            XElement root = new XElement(ProtocolUtils.REQUEST);
            XDocument xmlDoc = new XDocument(root);
            root.Add(new XElement(ProtocolUtils.TYPE, type));
            XElement contentElement = new XElement(ProtocolUtils.CONTENT);
            content = SetContent(content, contentElement);
            root.Add(content);
            return xmlDoc;
        }

        private static XElement SetContent(object content, XElement contentElement)
        {
            contentElement.Value = content.ToString();
            return contentElement;
        }

        private static XElement SetContentWithFiles(List<ProtocolUtils.FileStruct> fileNameList, XElement contentElement)
        {  
            foreach (ProtocolUtils.FileStruct file in fileNameList)
            {
                XElement fileElement = new XElement(ProtocolUtils.FILE);
                XElement sizeElement = new XElement(ProtocolUtils.SIZE);
                sizeElement.Value = file.size.ToString();
                fileElement.SetAttributeValue(ProtocolUtils.NAME, file.name);
                fileElement.Add(sizeElement);
                contentElement.Add(fileElement);      
            }
            return contentElement;
        }

    }
}
