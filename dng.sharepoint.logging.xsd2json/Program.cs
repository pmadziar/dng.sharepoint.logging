using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace dng.sharepoint.logging.xsd2json
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, object> allElements = new Dictionary<string, object>();

            string xmlString = File.ReadAllText("NLog.xsd");
            XmlDocument nlogXsd = new XmlDocument();
            nlogXsd.LoadXml(xmlString);

            // targetNamespace="http://www.nlog-project.org/schemas/NLog.xsd" elementFormDefault="qualified" 
            // xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns="http://www.nlog-project.org/schemas/NLog.xsd">

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nlogXsd.NameTable);
            nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
            nsmgr.AddNamespace("targetNamespace", "http://www.nlog-project.org/schemas/NLog.xsd");
            nsmgr.AddNamespace("default", "http://www.nlog-project.org/schemas/NLog.xsd");

            XmlNodeList elementNodes = nlogXsd.SelectNodes("/xs:schema/xs:element", nsmgr);
            foreach (var elementNode in elementNodes)
                if (elementNode is XmlElement) processXmlNode(nsmgr, (XmlElement)elementNode, allElements);

            StringWriter sw = new StringWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.Serialize(sw, allElements);
            string json = sw.ToString();
            File.WriteAllText("NlogConfig.json", json, Encoding.UTF8);
        }

        private static void processXmlNode(XmlNamespaceManager nsmgr, XmlElement element, Dictionary<string, object> allElements)
        {
            Dictionary<string, object> currentElementEntry = new Dictionary<string, object>();

            string elementName = element.Attributes["name"].Value;
            string elementType = element.Attributes["type"].Value;

            if (!allElements.ContainsKey(elementName))// && !elementType.StartsWith("xs:"))
            {

                allElements.Add(elementName, currentElementEntry);

                // find type def
                string typeDefsXpath = @"//*[@name=""" + elementType + @"""]";
                List<XmlElement> typeDefs = element.OwnerDocument.SelectNodes(typeDefsXpath, nsmgr).Cast<XmlElement>().ToList();
                XmlElement typeDef = typeDefs.Single(x => x.Name.EndsWith("Type"));

                // get all attributes
                {
                    List<XmlElement> typeAttributes = typeDef.SelectNodes(".//xs:attribute", nsmgr).Cast<XmlElement>().ToList();
                    if (typeAttributes != null && typeAttributes.Count > 0)
                    {
                        Dictionary<string, object> attrs = new Dictionary<string, object>();
                        currentElementEntry.Add("attrs", attrs);
                        foreach (XmlElement typeAttribute in typeAttributes)
                        {
                            if (typeAttribute.Attributes["type"].Value.StartsWith("xs:"))
                            {
                                string attrName = typeAttribute.Attributes["name"].Value;
                                attrs.Add(attrName, null);
                            }
                            else
                            {
                                string attrName = typeAttribute.Attributes["name"].Value;
                                List<string> values = findAttrValueList(typeAttribute, nsmgr);
                                attrs.Add(attrName, values);

                            }
                        }
                    }
                }
                // get all elements
                {
                    List<XmlElement> childElements = typeDef.SelectNodes(".//xs:choice/xs:element", nsmgr).Cast<XmlElement>().ToList();
                    if (childElements != null && childElements.Count > 0)
                    {
                        foreach (XmlElement childElement in childElements)
                        {
                            string chldType = childElement.GetAttribute("type");
                            string chldName = childElement.GetAttribute("name");

                            if (!chldType.StartsWith("xs:"))
                            {
                                processXmlNode(nsmgr, childElement, allElements);
                            }
                            else
                            {
                                if (!allElements.ContainsKey(chldName))
                                {
                                    allElements.Add(chldName, new Dictionary<string, object>());
                                }
                            }
                        }
                        List<string> childElementNames = childElements.Select(x => x.GetAttribute("name")).ToList();
                        currentElementEntry.Add("children", childElementNames);
                    }
                }

                // get all implementations for abstract element
                {
                    string typeDefName = typeDef.GetAttribute("name");
                    bool isAbstract = typeDef.HasAttribute("abstract") && typeDef.GetAttribute("abstract").Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    if (isAbstract)
                    {
                        List<XmlElement> abs = element.OwnerDocument.SelectNodes(@"//xs:extension[@base=""" + elementType + @"""]", nsmgr).Cast<XmlElement>().ToList();
                        if (abs != null && abs.Count > 0)
                        {
                            foreach (XmlElement ab in abs)
                            {
                                List<XmlElement> childElements = ab.SelectNodes(".//xs:choice/xs:element", nsmgr).Cast<XmlElement>().ToList();
                                if (childElements != null && childElements.Count > 0)
                                {
                                    foreach (XmlElement childElement in childElements)
                                    {
                                        List<string> children;
                                        Dictionary<string, object> attributes;

                                        if (!currentElementEntry.ContainsKey("children"))
                                        {
                                            children = new List<string>();
                                            currentElementEntry.Add("children", children);
                                        }
                                        else
                                        {
                                            children = currentElementEntry["children"] as List<string>;
                                        }

                                        if (!currentElementEntry.ContainsKey("attrs"))
                                        {
                                            attributes = new Dictionary<string, object>();
                                            currentElementEntry.Add("attrs", attributes);
                                        }
                                        else
                                        {
                                            attributes = currentElementEntry["attrs"] as Dictionary<string, object>;
                                        }


                                        string chlName = childElement.GetAttribute("name");
                                        if (!children.Contains(chlName))
                                        {
                                            string chldType = childElement.GetAttribute("type");
                                            if (!allElements.ContainsKey(chlName))
                                            {
                                                if (!chldType.StartsWith("xs:"))
                                                {
                                                    processXmlNode(nsmgr, childElement, allElements);
                                                }
                                                else
                                                {
                                                    allElements.Add(chlName, new Dictionary<string, object>());
                                                }
                                            }
                                            if (!attributes.ContainsKey(chlName))
                                            {
                                                if (!chldType.StartsWith("xs:"))
                                                {
                                                    List<string> values = findAttrValueList(childElement, nsmgr);
                                                    attributes.Add(chlName, values);
                                                }
                                                else
                                                {
                                                    attributes.Add(chlName, null);
                                                }
                                            }
                                            children.Add(chlName);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }
        }
        private static List<string> findAttrValueList(XmlElement typeAttribute, XmlNamespaceManager nsmgr)
        {
            List<string> attrValueList = null;
            string elementType = typeAttribute.Attributes["type"].Value;
            string typeDefsXpath = @"//*[@name=""" + elementType + @"""]";
            List<XmlElement> typeDefs = typeAttribute.OwnerDocument.SelectNodes(typeDefsXpath, nsmgr).Cast<XmlElement>().ToList();
            XmlElement typeDef = typeDefs.Single(x => x.Name.EndsWith("Type"));

            List<XmlElement> valueElements = typeDef.SelectNodes(".//xs:enumeration", nsmgr).Cast<XmlElement>().ToList();

            if (valueElements.Count > 0)
            {
                attrValueList = valueElements.Select(x => x.GetAttribute("value")).ToList();
            }

            return attrValueList;
        }


    }
}
