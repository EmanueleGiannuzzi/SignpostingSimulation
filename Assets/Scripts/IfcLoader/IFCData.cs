using System.Collections.Generic;
using UnityEngine;

public class IFCData : MonoBehaviour {

    public string IFCClass;
    public string STEPName;
    public string STEPId;
    public string STEPIndex;
    public string IFCLayer;

    public List<IFCPropertySet> propertySets;
    public List<IFCPropertySet> quantitySets;
}