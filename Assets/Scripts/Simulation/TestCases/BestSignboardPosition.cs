using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BestSignboardPosition {
    public Gradient Gradient;

    private readonly Environment environment;
    private readonly CSVExporter csvExporter;

    private bool done = false;

    public BestSignboardPosition(Environment e) {
        environment = e;
        csvExporter = new CSVExporter(environment);
    }

    public void StartEvalutation() {
        environment.GenerateVisibilityPlanes();

        environment.GetSignboardGridGenerator().GenerateGrid();

        environment.InitVisibilityHandlerData();
        environment.GetVisibilityHandler().ShowVisibilityPlane(0);

        environment.RunSimulationForInspectorDuration();

        done = true;
        //environment.GetBestSignboardPosition().ShowVisibilityPlane(0);
    }

    public void ShowVisibilityPlane(int agentTypeID) {
        if(!isVisibilityReady()) {
            return;
        }

        GameObject signboardGridGroup = environment.signboardGridGenerator.GetSignboardGridGroup();

        float minVisibility = float.PositiveInfinity;
        float maxVisibility = 0f;
        for(int visPlaneId = 0; visPlaneId < environment.visibilityHandler.GetVisibilityPlaneSize(); visPlaneId++) {
            foreach(Transform child in signboardGridGroup.transform.GetChild(visPlaneId)) {
                SignageBoard signboard = child.gameObject.GetComponent<SignageBoard>();

                float visibility = signboard.GetVisiblityForHeatmap()[agentTypeID];
                if(visibility > maxVisibility) {
                    maxVisibility = visibility;
                }
                if(visibility < minVisibility) {
                    minVisibility = visibility;
                }
            }
        }

        for(int visPlaneId = 0; visPlaneId < environment.visibilityHandler.GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = environment.visibilityHandler.GetVisibilityPlane(visPlaneId);
            //Dictionary<Vector2Int, VisibilityInfo>[] visInfos = environment.visibilityHandler.visibilityInfos[visPlaneId];

            //Vector3 position = visibilityPlane.transform.position;
            //VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            //float originalFloorHeight = planeData.OriginalFloorHeight;
            //position[1] = originalFloorHeight + environment.visibilityHandler.agentTypes[agentTypeID].Value; // the Y value
            //visibilityPlane.transform.position = position;

            float signboardGridResolution = environment.signboardGridGenerator.resolution;

            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            float planeWidth = meshRendererBounds.extents.x * 2;
            float planeHeight = meshRendererBounds.extents.z * 2;

            int widthResolution = (int)Mathf.Floor((float)planeWidth * signboardGridResolution);
            int heightResolution = (int)Mathf.Floor((float)planeHeight * signboardGridResolution);


            Vector3[] meshVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector2[] uvs = new Vector2[meshVertices.Length];

            Vector3 localMin = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.min);
            Vector3 localMax = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;

            for(int i = 0; i < meshVertices.Length; i++) {
                Vector3 normVertex = meshVertices[i] - localMin;
                uvs[i] = new Vector2(1f - (float)(normVertex.x / localMax.x), 1f - (float)(normVertex.z / localMax.z));
            }
            visibilityPlane.GetComponent<MeshFilter>().sharedMesh.uv = uvs;

            Texture2D texture = VisibilityTextureGenerator.BestSignboardTexture(signboardGridGroup, agentTypeID, visPlaneId, 
                widthResolution, heightResolution, minVisibility, maxVisibility, this.Gradient);
            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            //meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
            meshRenderer.sharedMaterial.mainTexture = texture;
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
