using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;

public class ObjFromStream : MonoBehaviour {
	void Start () {
        //make www
#pragma warning disable CS0618 // Type or member is obsolete
        var www = new WWW("https://people.sc.fsu.edu/~jburkardt/data/obj/lamp.obj");
#pragma warning restore CS0618 // Type or member is obsolete
        while (!www.isDone)
            System.Threading.Thread.Sleep(1);
        
        //create stream and load
        var textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.text));
        var loadedObj = new OBJLoader().Load(textStream);
	}
}
