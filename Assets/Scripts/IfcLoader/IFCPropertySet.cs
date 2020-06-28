using System.Collections.Generic;

[System.Serializable]
public class IFCPropertySet {
    public string propSetName = "";
    public string propSetId = "";

    public List<IFCProperty> properties;
}
