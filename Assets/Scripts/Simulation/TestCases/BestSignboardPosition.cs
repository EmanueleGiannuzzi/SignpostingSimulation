using System.Collections;
using UnityEngine;

[System.Serializable]
public class BestSignboardPosition {
    public Gradient Gradient;

    private readonly Environment environment;
    private readonly CSVExporter csvExporter;

    private bool done = false;

    private Texture2D[,] resultTextures;

    public BestSignboardPosition(Environment e) {
        environment = e;
        csvExporter = new CSVExporter(environment);
    }

    public IEnumerator CoroutineWarmupAndSimulate() {
        yield return environment.CoroutineSimulationWarmup(environment.WarmupDurationSeconds);
        yield return environment.CoroutineRunSimulationForSeconds(environment.SimulationDurationSeconds);
        environment.StopSpawnAgents();

        generateTextures();
        
        done = true;

        ShowVisibilityPlane(0);
        Debug.Log("Warmup and Sim done");
    }

    public void WarmupAndSimulate() {
        environment.StartCoroutine(CoroutineWarmupAndSimulate());
    }

    public void StartEvalutation() {
        environment.GenerateVisibilityPlanes();

        environment.GetSignboardGridGenerator().GenerateGrid();

        environment.InitVisibilityHandlerData();
        environment.GetVisibilityHandler().ShowVisibilityPlane(0);

        WarmupAndSimulate();
    }
    
    private int getVisibilityPlaneSize() {
        return environment.visibilityHandler.GetVisibilityPlaneSize();
    }

    private int getAgentTypesSize() {
        return environment.visibilityHandler.agentTypes.Length;
    }

    private void generateTextures() {
        resultTextures = new Texture2D[getVisibilityPlaneSize(), getAgentTypesSize()];
        
        GameObject signboardGridGroup = environment.signboardGridGenerator.GetSignboardGridGroup();
        float signboardGridResolution = environment.signboardGridGenerator.resolution;

        float[] minVisibility = new float[getAgentTypesSize()];
        float[] maxVisibility = new float[getAgentTypesSize()];
        for (int agentTypeID = 0; agentTypeID < getAgentTypesSize(); agentTypeID++) {
            minVisibility[agentTypeID] = float.PositiveInfinity;
            maxVisibility[agentTypeID] = 0f;
        }

        for (int agentTypeID = 0; agentTypeID < getAgentTypesSize(); agentTypeID++) {
            for (int visPlaneId = 0; visPlaneId < getVisibilityPlaneSize(); visPlaneId++) {
                foreach (Transform child in signboardGridGroup.transform.GetChild(visPlaneId)) {
                    SignBoard signboard = child.gameObject.GetComponent<SignBoard>();

                    float visibility = signboard.GetVisiblityForHeatmap()[agentTypeID];
                    if (visibility > maxVisibility[agentTypeID]) {
                        maxVisibility[agentTypeID] = visibility;
                    }

                    if (visibility < minVisibility[agentTypeID]) {
                        minVisibility[agentTypeID] = visibility;
                    }
                }
            }
        }

        for (int visPlaneId = 0; visPlaneId < getVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = environment.visibilityHandler.GetVisibilityPlane(visPlaneId);
            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;
            int widthResolution = (int)Mathf.Floor(planeWidth * signboardGridResolution);
            int heightResolution = (int)Mathf.Floor(planeHeight * signboardGridResolution);
            
            for (int agentTypeID = 0; agentTypeID < getAgentTypesSize(); agentTypeID++) {
                resultTextures[visPlaneId, agentTypeID] = VisibilityTextureGenerator.BestSignboardTexture(signboardGridGroup, agentTypeID, visPlaneId, 
                    widthResolution, heightResolution, minVisibility[agentTypeID], maxVisibility[agentTypeID], this.Gradient);
            }
        }
    }

    public void ShowVisibilityPlane(int agentTypeID) {
        if(!isVisibilityReady()) {
            return;
        }

        for(int visPlaneId = 0; visPlaneId < getVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = environment.visibilityHandler.GetVisibilityPlane(visPlaneId);

            Vector3 position = visibilityPlane.transform.position;
            VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            float originalFloorHeight = planeData.OriginalFloorHeight;
            position[1] = originalFloorHeight; // the Y value
            visibilityPlane.transform.position = position;

            Vector3[] meshVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector2[] uvs = new Vector2[meshVertices.Length];

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            Vector3 localMin = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.min);
            Vector3 localMax = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;

            for(int i = 0; i < meshVertices.Length; i++) {
                Vector3 normVertex = meshVertices[i] - localMin;
                uvs[i] = new Vector2(1f - normVertex.x / localMax.x, 1f - normVertex.z / localMax.z);
            }
            visibilityPlane.GetComponent<MeshFilter>().sharedMesh.uv = uvs;

            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.mainTexture = resultTextures[visPlaneId, agentTypeID];
        }
    }

    public bool isVisibilityReady() {
        return done;
    }

    public void ExportCSV(string pathToFile) {
        if(isVisibilityReady()) {
            csvExporter.ExportCSV(pathToFile);
        }
    }

}
