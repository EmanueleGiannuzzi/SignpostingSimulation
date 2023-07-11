using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System.Globalization;

public class IfcOpenShellParser : MonoBehaviour {
    //private string fileName;
    [Header("Walkable Areas")]
    [SerializeField] private string[] walkableAreas = { "IfcSlab", "IfcStair", "IfcStairFlight", "IfcWallStandardCase" };
    [SerializeField] private string[] navMeshIgnoredAreas = { "IfcDoor" };

    [Header("IFCSignboard Standard")]
    [SerializeField] private string ifcSignTag = "IfcSign";
    [SerializeField] private string ifcSignboardPropertiesName = "Pset_SignboardCommon";
    [SerializeField] private string ifcSignViewingDistanceProperty = "ViewingDistance";
    [SerializeField] private string ifcSignViewingAngleProperty = "ViewingAngle";
    [SerializeField] private string ifcSignMinimumReadingTimeProperty = "MinimumReadingTime";


    [Header("Loader")]
    public bool deleteTemporaryFiles = true;

    private GameObject loadedOBJ;
    private XmlDocument loadedXML;

    private static readonly int NAVMESH_NOTWALKABLE_AREATYPE = 1;

    private readonly Dummiesman.OBJLoader objLoader = new Dummiesman.OBJLoader {
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

        if(this.deleteTemporaryFiles) {
            Utility.DeleteFile(objPath);
            Utility.DeleteFile(mtlPath);
            Utility.DeleteFile(xmlPath);
        }
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

    public void LoadXML(string path) {

        loadedXML = new XmlDocument();
        loadedXML.Load(path);

        string basePath = @"//ifc/decomposition";

        GameObject root = new GameObject {
            name = Path.GetFileNameWithoutExtension(path)
        };

        foreach(XmlNode node in loadedXML.SelectNodes(basePath + "/IfcProject")) {
            AddElements(node, root);
        }

        DestroyImmediate(loadedOBJ);

        Debug.Log("Loaded XML");
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
                HandleSpecialObjects(ref goElement, node);

                foreach(XmlNode child in node.ChildNodes) {
                    AddElements(child, goElement);
                }
            }
        }
    }

    private bool IsIfcWalkableArea(string ifcClass) {
        return this.walkableAreas.Contains(ifcClass);
    }

    private bool IsIfcIgnoreFromBuildArea(string ifcClass) {
        return this.navMeshIgnoredAreas.Contains(ifcClass);
    }

    private bool IsIfcSign(string ifcClass) {
        return ifcClass.Equals(ifcSignTag);
    }

    private void HandleSpecialObjects(ref GameObject goElement, XmlNode node) {
        MeshFilter goMeshFilter = goElement.GetComponent<MeshFilter>();
        if(goMeshFilter != null && goMeshFilter.sharedMesh != null) {
            goElement.AddComponent<MeshCollider>();
            if(IsIfcIgnoreFromBuildArea(node.Name)) {
                NavMeshModifier navmeshModifier = goElement.AddComponent<NavMeshModifier>();
                navmeshModifier.ignoreFromBuild = true;
            }
            else if(!IsIfcWalkableArea(node.Name)) {
                NavMeshModifier navmeshModifier = goElement.AddComponent<NavMeshModifier>();
                navmeshModifier.overrideArea = true;
                navmeshModifier.area = NAVMESH_NOTWALKABLE_AREATYPE;
            }
            else if(IsIfcSign(node.Name)) {
                LoadSignboardData(ref goElement);
            }
        }
    }

    private void LoadSignboardData(ref GameObject goElement) {
        if(goElement.TryGetComponent<IFCData>(out IFCData signboardIFCData)) {
            List<IFCProperty> signboardProperties = signboardIFCData.propertySets.Find(property => property.propSetName == ifcSignboardPropertiesName).properties;
            if(signboardProperties != null) {
                SignageBoard signBoard = goElement.AddComponent<SignageBoard>();
                try {
                    signBoard.ViewingDistance = StringToFloat(signboardProperties.Find(property => property.propName == ifcSignViewingDistanceProperty).propValue);
                    signBoard.ViewingAngle = StringToFloat(signboardProperties.Find(property => property.propName == ifcSignViewingAngleProperty).propValue);
                    signBoard.MinimumReadingTime = StringToFloat(signboardProperties.Find(property => property.propName == ifcSignMinimumReadingTimeProperty).propValue);
                }
                catch(Exception e) {
                    Debug.LogError($"Error parsing properties from Signboard {goElement.name} ({e.Message}).");
                }
            }
            else {
                Debug.LogError($"No IFCPropertySet named {ifcSignboardPropertiesName} in Signboard {goElement.name}.");
            }
        }
        else {
            Debug.LogError($"No IFCData in Signboard {goElement.name}.");
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

    private float StringToFloat(string str) {
        return float.Parse(str, CultureInfo.InvariantCulture.NumberFormat);
    }
}
