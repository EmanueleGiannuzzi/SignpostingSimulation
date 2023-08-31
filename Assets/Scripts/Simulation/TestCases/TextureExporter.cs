
using System;
using System.IO;
using UnityEngine;
using File = UnityEngine.Windows.File;

[Serializable]
public class TextureExporter {
    private readonly Environment environment;

    [SerializeField] private string pathToFiles;

    public TextureExporter(Environment e) {
        this.environment = e;
    }

    public void ExportTexture(string pathToFile) {
        const string OUTPUT_FILE_NAME = "OutputTexture VisPlaneId={0} agentTypeId={1}";
        
        int visibilityPlanesSize = environment.visibilityHandler.GetVisibilityPlaneSize();
        int agentTypesSize = environment.visibilityHandler.agentTypes.Length;

        for (int visPlaneId = 0; visPlaneId < visibilityPlanesSize; visPlaneId++) {
            for (int agentTypeID = 0; agentTypeID < agentTypesSize; agentTypeID++) {
                Texture2D coverageTexture = environment.visibilityHandler.GetResultTexture(visPlaneId, agentTypeID);
                Texture2D bestSignboardTexture = environment.visibilityHandler.GetResultTexture(visPlaneId, agentTypeID);
                exportTexture(coverageTexture, string.Format(OUTPUT_FILE_NAME, visPlaneId, agentTypeID));
                exportTexture(bestSignboardTexture, string.Format(OUTPUT_FILE_NAME, visPlaneId, agentTypeID));
            }
        }
    }

    private void exportTexture(Texture2D texture, string name) {
        File.WriteAllBytes(Path.Combine(pathToFiles, name), texture.EncodeToPNG());//TODO
    }
}