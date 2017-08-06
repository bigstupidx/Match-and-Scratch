using System;
using System.Collections.Generic;
using UnityEngine.Analytics;

public static class AnalyticsSender {

	public static void SendCustomAnalitycs(string name, Dictionary<string, object> parammeters) {
		Analytics.CustomEvent(name, parammeters);
	} 

}

