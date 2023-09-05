
using System;
using System.IO;
using UnityEngine;
using File = UnityEngine.Windows.File;

[Serializable]
public class TextureExporter {
    private readonly Environment environment;

    public TextureExporter(Environment e) {
        this.environment = e;
    }

    public bool CanExport() {
        return environment.visibilityHandler.IsCoverageReady() ||
               environment.GetBestSignboardPosition().isVisibilityReady();
    }

    public void ExportTexture(string pathToFile) {
        const string OUTPUT_FILE_NAME = "OutputTexture VisPlaneId={0} agentTypeId={1}";
        
        int visibilityPlanesSize = environment.visibilityHandler.GetVisibilityPlaneSize();
        int agentTypesSize = environment.visibilityHandler.agentTypes.Length;

        if (environment.visibilityHandler.IsCoverageReady()) {
            for (int visPlaneId = 0; visPlaneId < visibilityPlanesSize; visPlaneId++) {
                for (int agentTypeID = 0; agentTypeID < agentTypesSize; agentTypeID++) {
                    Texture2D coverageTexture = environment.visibilityHandler.GetResultTexture(visPlaneId, agentTypeID);//TODO: Check if available
                    exportTexture(coverageTexture, pathToFile, string.Format("Coverage " + OUTPUT_FILE_NAME, visPlaneId, agentTypeID));
                }
            }
        }
        if(environment.GetBestSignboardPosition().isVisibilityReady()) {
            for (int visPlaneId = 0; visPlaneId < visibilityPlanesSize; visPlaneId++) {
                for (int agentTypeID = 0; agentTypeID < agentTypesSize; agentTypeID++) {
                    Texture2D bestSignboardTexture = environment.GetBestSignboardPosition().GetResultTexture(visPlaneId, agentTypeID);//TODO: Check if available
                    exportTexture(bestSignboardTexture, pathToFile, string.Format("BestSignboard " + OUTPUT_FILE_NAME, visPlaneId, agentTypeID));
                }
            }
        }
        
    }

    private void exportTexture(Texture2D texture, string path, string name) {
        string completePath = Path.Combine(path, name + ".png");
        Debug.Log(completePath);
        File.WriteAllBytes(completePath, texture.EncodeToPNG());//TODO
    }
}