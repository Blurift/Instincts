using UnityEngine;
using System.Collections;
using System.IO;

public class Logger{

	private static Logger Instance
	{
		get
		{
			if(instance == null)
				instance = new Logger();
			return instance;
		}
	}
	private static Logger instance;

	public static void Error(string error)
	{

	}

	public static string Path()
	{
		string dataPath = Application.dataPath;
		dataPath = dataPath.Substring (0, dataPath.LastIndexOf ("/")+1);
		return dataPath;
	}

	string fileName = "";

	public Logger()
	{
		return;
		string date = System.DateTime.UtcNow.ToString ();

		date = date.Replace ("/", "-");
		date = date.Replace (":", "-");
		Debug.Log (date);

		string file = "Log_" + date + ".log";

		if(Network.isServer)
			file = "s" + file;
		fileName = Path() + "\\" + file;

		StreamWriter writer = new StreamWriter (fileName);
		writer.Close ();
	}

	public static void Write(string text)
	{
		return;
		StreamWriter writer = new StreamWriter (Instance.fileName,true);

		writer.WriteLine (text);

		writer.Close ();

	}
}
