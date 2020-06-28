using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;


public class IfcOpenShellParser : MonoBehaviour {
    //private string fileName;
    private GameObject loadedOBJ;
    private XmlDocument loadedXML;

    private Dummiesman.OBJLoader objLoader = new Dummiesman.OBJLoader {
        SplitMode = Dummiesman.SplitMode.Object,
    };

    public void LoadIFC() {
        string ifcPath = EditorUtility.OpenFilePanel("Import IFC", "", "ifc");
        string objPath = Path.ChangeExtension(ifcPath, "obj");
        string mtlPath = Path.ChangeExtension(ifcPath, "mtl");
        string xmlPath = Path.ChangeExtension(ifcPath, "xml");

        string ifcConveter = Path.GetFullPath("IFC/IfcConvert.exe");
        System.Diagnostics.Process processOBJ = Utility.RunCommand(ifcConveter, "\"" + ifcPath + "\"" + " " + "\"" + objPath + "\"" + " --use-element-guids -y", true);
        System.Diagnostics.Process processXML = Utility.RunCommand(ifcConveter, "\"" + ifcPath + "\"" + " " + "\"" + xmlPath + "\"" + " --use-element-guids -y", true);

        processOBJ.WaitForExit();
        processXML.WaitForExit();

        if(File.Exists(objPath) && File.Exists(mtlPath) && File.Exists(xmlPath)) {
            LoadOBJ(objPath, mtlPath);
            LoadXML(xmlPath);
        }
        else {
            Debug.LogError("Error during parse");
        }

        Utility.DeleteFile(objPath);
        Utility.DeleteFile(mtlPath);
        Utility.DeleteFile(xmlPath);
    }

    public void LoadOBJMTLXMLFile() {
        string objPath = EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
        if(!string.IsNullOrEmpty(objPath)) {
            string mtlPath = EditorUtility.OpenFilePanel("Import MTL", "", "mtl");
            if(!string.IsNullOrEmpty(mtlPath)) {
                string xmlPath = EditorUtility.OpenFilePanel("Import XML", "", "xml");

                if(!string.IsNullOrEmpty(xmlPath)) {
                    LoadOBJ(objPath, mtlPath);
                    LoadXML(xmlPath);
                }
            }
        }
    }

    private void LoadOBJ(string objPath, string mtlPath) {
        loadedOBJ = objLoader.Load(objPath, mtlPath);

        if(loadedOBJ != null) {
            // turn -90 on the X-Axis (CAD/BIM uses Z up)
            loadedOBJ.transform.Rotate(-90, 0, 0);
        }
    }

    private void LoadXML(string path) {
        loadedXML = new XmlDocument();
        loadedXML.Load(path);

        string basePath = @"//ifc/decomposition";

        GameObject root = new GameObject {
            name = Path.GetFileNameWithoutExtension(path) + " (IFC)"
        };

        foreach(XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject")) {
            AddElements(node, root);
        }

        DestroyImmediate(loadedOBJ);
    }

    private void AddElements(XmlNode node, GameObject parent) {
        if(node.Attributes.GetNamedItem("id") != null) {
            string id = node.Attributes.GetNamedItem("id").Value;
            string name = "";
            if(node.Attributes.GetNamedItem("Name") != null) {
                name = node.Attributes.GetNamedItem("Name").Value;
            }
            name += "[" + node.Name + "]";

            string searchPath = /*fileName + "/" +*/ id;
            GameObject goElement = GameObject.Find(searchPath);

            if(goElement == null) {
                goElement = new GameObject();
            }

            if(goElement != null) {
                goElement.name = name;
                if(parent != null) {
                    goElement.transform.SetParent(parent.transform);
                }

                AddProperties(node, goElement);

                foreach(XmlNode child in node.ChildNodes) {
                    AddElements(child, goElement);
                }
            }
        }
    }
    private void AddProperties(XmlNode node, GameObject go) {
        IFCData ifcData = go.AddComponent(typeof(IFCData)) as IFCData;

        ifcData.IFCClass = node.Name;
        ifcData.STEPId = node.Attributes.GetNamedItem("id").Value;

        string nameProperty = "";
        if(node.Attributes.GetNamedItem("Name") != null) {
            nameProperty = node.Attributes.GetNamedItem("Name").Value;
        }
        ifcData.STEPName = nameProperty;
        // Initialize PropertySets and QuantitySets
        if(ifcData.propertySets == null)
            ifcData.propertySets = new List<IFCPropertySet>();
        if(ifcData.quantitySets == null)
            ifcData.quantitySets = new List<IFCPropertySet>();


        // Go through Properties (and Quantities and ...)
        foreach(XmlNode child in node.ChildNodes) {
            switch(child.Name) {
                case "IfcPropertySet":
                case "IfcElementQuantity":
                    // we only receive a link beware of # character
                    string link = child.Attributes.GetNamedItem("xlink:href").Value.TrimStart('#');
                    string path = String.Format("//ifc/properties/IfcPropertySet[@id='{0}']", link);
                    if(child.Name == "IfcElementQuantity")
                        path = String.Format("//ifc/quantities/IfcElementQuantity[@id='{0}']", link);
                    XmlNode propertySet = loadedXML.SelectSingleNode(path);
                    if(propertySet != null) {

                        // initialize this propertyset (Name, Id)
                        IFCPropertySet myPropertySet = new IFCPropertySet();
                        myPropertySet.propSetName = propertySet.Attributes.GetNamedItem("Name").Value;
                        myPropertySet.propSetId = propertySet.Attributes.GetNamedItem("id").Value;
                        if(myPropertySet.properties == null)
                            myPropertySet.properties = new List<IFCProperty>();

                        // run through property values
                        foreach(XmlNode property in propertySet.ChildNodes) {
                            string propName, propValue = "";
                            IFCProperty myProp = new IFCProperty();
                            propName = property.Attributes.GetNamedItem("Name").Value;

                            if(property.Name == "IfcPropertySingleValue")
                                propValue = property.Attributes.GetNamedItem("NominalValue").Value;
                            if(property.Name == "IfcQuantityLength")
                                propValue = property.Attributes.GetNamedItem("LengthValue").Value;
                            if(property.Name == "IfcQuantityArea")
                                propValue = property.Attributes.GetNamedItem("AreaValue").Value;
                            if(property.Name == "IfcQuantityVolume")
                                propValue = property.Attributes.GetNamedItem("VolumeValue").Value;
                            // Write property (name & value)
                            myProp.propName = propName;
                            myProp.propValue = propValue;
                            myPropertySet.properties.Add(myProp);
                        }

                        // add propertyset to IFCData
                        if(child.Name == "IfcPropertySet")
                            ifcData.propertySets.Add(myPropertySet);
                        if(child.Name == "IfcElementQuantity")
                            ifcData.quantitySets.Add(myPropertySet);

                    }
                    break;
                default:
                    break;
            } 
        }
    }
}
