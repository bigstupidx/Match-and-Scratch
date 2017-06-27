using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TappxSDK {
	[CustomEditor(typeof(TappxSettings))]
	public class TappxSettingEditor : Editor {
		
		GUIContent iOSAppIdLabel = new GUIContent("Tappx Key [?]:", "Tappx App Keys can be found at http://www.tappx.com/en/admin/apps/");
		GUIContent androidAppIdLabel = new GUIContent("Tappx Key [?]:", "Tappx App Keys can be found at http://www.tappx.com/en/admin/apps/");
		GUIContent iOSLabel = new GUIContent("iOS");
		GUIContent androidLabel = new GUIContent("Android");

		// minimum version of the Google Play Services library project
		private long MinGmsCoreVersionCode = 4030530;
		private string googlePlayServicesVersion = "9.0.0";

		private string sError = "Error";
	    private string sOk = "OK";
	    private string sCancel = "Cancel";
	    private string sSuccess = "Success";
	    private string sWarning = "Warning";

        private string sSdkNotFound = "Android SDK Not found";
        private string sSdkNotFoundBlurb = "The Android SDK path was not found. " +
                "Please configure it in the Unity preferences window (under External Tools).";

        private string sLibProjNotFound = "Google Play Services Library Project Not Found";
        private string sLibProjNotFoundBlurb = "Google Play Services library project " +
                "could not be found your SDK installation. Make sure it is installed (open " +
                "the SDK manager and go to Extras, and select Google Play Services).";
                
        private string sLibProjVerNotFound = "The version of your copy of the Google Play " +
                "Services Library Project could not be determined. Please make sure it is " +
                "at least version {0}. Continue?";
                
        private string sLibProjVerTooOld = "Your copy of the Google Play " +
            "Services Library Project is out of date. Please launch the Android SDK manager " +
            "and upgrade your Google Play Services bundle to the latest version (your version: " +
            "{0}; required version: {1}). Proceeding may cause problems. Proceed anyway?";
        
        private string sSetupComplete = "Tappx configured successfully.";



	    private Vector2 scrollPos;
	    bool showPosition = true;
		private TappxSettings instance;


		public override void OnInspectorGUI() {
			instance = (TappxSettings)target;

			SetupUI();
		}



	    private void SetupUI() {
//			EditorGUILayout.HelpBox("Add the Chartboost App Id and App Secret associated with this game", MessageType.None);


	        if (instance && instance.sceneIndex.Length <= 0)
	        {
	            int numScenes = EditorSceneManager.sceneCountInBuildSettings;
	            instance.bannerSceneIndex = new bool[numScenes];
	            instance.interstitialSceneIndex = new bool[numScenes];
	            instance.sceneIndex = new bool[numScenes];
	            instance.interstitialAutoShow = new bool[numScenes];

	            instance.positionSceneIndex = new TappxSettings.POSITION_BANNER[numScenes];
	            for (int i = 0; i < instance.positionSceneIndex.Length; i++)
	            {
	                instance.positionSceneIndex[i] = TappxSettings.POSITION_BANNER.BOTTOM;
	            }
	        }


	        // iOS
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(iOSLabel);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
	        EditorGUILayout.LabelField(iOSAppIdLabel);
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
            instance.SetIOSAppId(EditorGUILayout.TextField(instance.iOSTappxID));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Android
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(androidLabel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(androidAppIdLabel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			instance.SetAndroidAppId(EditorGUILayout.TextField(instance.androidTappxID));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

		    EditorGUILayout.Space();
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();


		    instance.sceneListEnabled = EditorGUILayout.Toggle( "Select Scenes", instance.sceneListEnabled );

		    if (instance.sceneListEnabled)
		    {

			    
		        EditorGUILayout.HelpBox("Scene Manager", MessageType.Info);

		        int numberScenes = EditorSceneManager.sceneCountInBuildSettings;
		        for (int i = 0; i < numberScenes; i++)
		        {

		            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

			        if (instance.sceneIndex.Length != scenes.Length)
			        {
						instance.sceneIndex = new bool[scenes.Length];
				        instance.bannerSceneIndex = new bool[scenes.Length];
				        instance.interstitialSceneIndex = new bool[scenes.Length];
				        instance.positionSceneIndex = new TappxSettings.POSITION_BANNER[scenes.Length];
				        instance.interstitialAutoShow = new bool[scenes.Length];
			        }
			        
			        instance.sceneIndex[i] = EditorGUILayout.Foldout(instance.sceneIndex[i], scenes[i].path, EditorStyles.foldoutPreDrop );

		            if (instance.sceneIndex[i])
		            {
		                instance.bannerSceneIndex[i] = EditorGUILayout.Toggle("Banner", instance.bannerSceneIndex[i]);

		                if (instance.bannerSceneIndex[i])
		                {
		                    GUILayout.BeginHorizontal();
		                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

		                    instance.positionSceneIndex[i] = (TappxSettings.POSITION_BANNER) EditorGUILayout.EnumPopup("Position ", instance.positionSceneIndex[i]);

		                    GUILayout.EndVertical();
		                    GUILayout.EndHorizontal();
		                }

		                instance.interstitialSceneIndex[i] = EditorGUILayout.Toggle("Interstitial", instance.interstitialSceneIndex[i]);
		                if (instance.interstitialSceneIndex[i])
		                {
		                    instance.interstitialAutoShow[i] = EditorGUILayout.Toggle("---> Auto Show", instance.interstitialAutoShow[i]);
		                }

		            }

		        }

		    }


		}
		
    
	
	    private void EnsureDirExists(string dir) {
	        dir = dir.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
	        if (!System.IO.Directory.Exists(dir)) {
	            System.IO.Directory.CreateDirectory(dir);
	        }
	    }
	
	    private void DeleteDirIfExists(string dir) {
	        if (System.IO.Directory.Exists(dir)) {
	            System.IO.Directory.Delete(dir, true);
	        }
	    }

		private void DeleteFileIfExists(string file) {
			if (System.IO.File.Exists (file)) {
				System.IO.File.Delete (file);
			}
		}
		private string FixSlashes(string path) {
	        return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
	    }
		
	    private string ReadFile(string filePath) {
	        filePath = FixSlashes(filePath);
	        if (!File.Exists(filePath)) {
	            EditorUtility.DisplayDialog(sError, "Plugin error: file not found: " + filePath, sOk);
	            return null;
	        }
	        StreamReader sr = new StreamReader(filePath);
	        string body = sr.ReadToEnd();
	        sr.Close();
	        return body;
	    }
		
	    private string GetAndroidSdkPath() {
	        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
	        if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\"))) {
	            sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
	        }
	        return sdkPath;
	    }
	
	    private bool HasAndroidSdk() {
	        string sdkPath = GetAndroidSdkPath();
	        return sdkPath != null && sdkPath.Trim() != "" && System.IO.Directory.Exists(sdkPath);
	    }
	}
}
