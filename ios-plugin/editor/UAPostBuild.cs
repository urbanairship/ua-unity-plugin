using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;

public class UAPostBuild {

	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		if (target == BuildTarget.iOS)
		{
			
			UnityEngine.Debug.Log("Running Urban Airship iOS post build script.");

			ProcessStartInfo start = new ProcessStartInfo();
			start.FileName = "python";
			start.WorkingDirectory = Application.dataPath + "/Editor/";
			start.Arguments = System.String.Format("{0} \"{1}\"", "ua_ios_post_build.py", pathToBuiltProject);
			start.UseShellExecute = false;
			start.RedirectStandardOutput = true;
			try {
				using (Process process = Process.Start(start))
				{
					using (StreamReader reader = process.StandardOutput)
					{
						UnityEngine.Debug.Log(reader.ReadToEnd());
					}
					process.WaitForExit ();
				}
			} catch (System.Exception ex) {
				UnityEngine.Debug.Log(ex.Message);
			}

			UnityEngine.Debug.Log("Finished Urban Airship iOS post build script.");
		}
	}
		
}