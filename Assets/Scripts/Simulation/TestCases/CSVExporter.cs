using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

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
            string name = signboard.name + "(" + signboard.transform.parent.name + ")";
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
        using(StreamWriter writer = new StreamWriter(pathToFile))
        using(CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
            csv.Configuration.RegisterClassMap(new SignboardDataMap());
            csv.Configuration.HasHeaderRecord = false;

            csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
            csv.NextRecord();
            string header = "Name";
            for(int i = 0; i < environment.GetVisibilityHandler().agentTypes.Length; i++) {
                header += csv.Configuration.Delimiter + "Coverage[" + environment.GetVisibilityHandler().agentTypes[i].Key + "]";
            }
            for(int i = 0; i < environment.GetVisibilityHandler().agentTypes.Length; i++) {
                header += csv.Configuration.Delimiter + "Visibility[" + environment.GetVisibilityHandler().agentTypes[i].Key + "]";
            }
            csv.WriteField(header, false);
            csv.NextRecord();
            csv.WriteRecords(createSignboardsData());
        }
    }

    class SignboardData {
        public readonly string signageboardName;
        public readonly float[] coverage;
        public readonly float[] visibility;

        public SignboardData(string signageboardName, float[] coverage, float[] visibility) {
            this.signageboardName = signageboardName;
            this.coverage = coverage;
            this.visibility = visibility;
        }
    }
    class SignboardDataMap : ClassMap<SignboardData> {
        public SignboardDataMap() {
            Map(m => m.signageboardName).Name("Name");
            Map(m => m.coverage).Name("Coverage");
            Map(m => m.visibility).Name("Visibility");
        }
    }
}
