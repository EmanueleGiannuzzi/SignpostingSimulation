
using UnityEngine;
using UnityEngine.Windows;

public class TextureExporter {
    private readonly Environment environment;
    public TextureExporter(Environment e) {
        this.environment = e;
    }
    
    
    public void ExportTexture(string pathToFile) {
        int agentTypes = environment.visibilityHandler.agentTypes.Length;

        for (int i = 0; i < agentTypes; i++) {
            GameObject visibilityPlane = environment.visibilityHandler.GetVisibilityPlane(i);
            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            Texture2D coverageTexture = meshRenderer.sharedMaterial.mainTexture as Texture2D;
            byte[] pngByteArray = coverageTexture.EncodeToPNG();
        
            File.WriteAllBytes(pathToFile + "i", pngByteArray);
            
            environment.GetBestSignboardPosition().ShowVisibilityPlane();
        }
    }
}