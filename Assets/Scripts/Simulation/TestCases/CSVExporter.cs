using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

public class CSVExporter
{
    private readonly Environment environment;
    public CSVExporter(Environment e) {
        this.environment = e;
    }

    private List<SignboardData> createSignboardsData() {
        List<SignboardData> signboardData = new List<SignboardData>();
        SignageBoard[] signboards = environment.signageBoards;
        int agentTypeSize = environment.GetVisibilityHandler().agentTypes.Length;

        foreach(SignageBoard signboard in signboards) {
            string name = signboard.name;
            float[] coverage = new float[agentTypeSize];
            float[] visibility = new float[agentTypeSize];

            for(int i = 0; i < agentTypeSize; i++) {
                coverage[i] = signboard.coveragePerAgentType[i];
                visibility[i] = signboard.visibilityPerAgentType[i];
            }
            signboardData.Add(new SignboardData(name, coverage, visibility));
        }
        return signboardData;
    }

    public void ExportCSV(string pathToFile) {
        using(var writer = new StreamWriter(pathToFile))
        using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
            csv.WriteRecords(createSignboardsData());
        }
    }

    class SignboardData {
        string signageboardName;
        float[] coverage;
        float[] visibility;

        public SignboardData(string signageboardName, float[] coverage, float[] visibility) {
            this.signageboardName = signageboardName;
            this.coverage = coverage;
            this.visibility = visibility;
        }
    }
}
