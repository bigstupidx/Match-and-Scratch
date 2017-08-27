using System;
using System.Collections.Generic;
using UnityEngine;

public static class ExtendedMethods
{
	public static List<GameObject> ToList(this GameObject[] list) {
		List<GameObject> newList = new List<GameObject>();
		for (int i = 0; i < list.Length; i++) {
			newList.Add(list[i]);
		}
		return newList;
	}

	public static string ListaAsString(this GameObject[] list, string enunciado = "") {
		string ret = enunciado + " ";
		for (int i = 0; i < list.Length; i++) {
			ret += list[i] == null ? "<null>" : list[i].name;
			if (i < list.Length -1) ret += ", ";
		}
		return ret;
	}

	public static string ListaAsString(this List<GameObject> list, string enunciado = "") {
		string ret = enunciado + " ";
		for (int i = 0; i < list.Count; i++) {
			ret += list[i].name;
			if (i < list.Count -1) ret += ", ";
		}
		return ret;
	}

	public static string ListaAsString(this List<Circumference> list, string enunciado = "") {
		string ret = enunciado + " ";
		for (int i = 0; i < list.Count; i++) {
			ret += list[i].gameObject.name;
			if (i < list.Count -1) ret += ", ";
		}
		return ret;
	}

	public static string ToSingleString (this List<ScoreEntry> list) {
		string result = "";
		for (int i = 0; i < list.Count; i++) {
			if (i > 0) result += "\n";

			result += list [i].ToContatString ();
		}
		return result;
	}

	public static string parseToDateTime(this string s) {
		string result;
		result = s.Substring (0, 2) + "/" + s.Substring (2, 2) + "/" + s.Substring (4, 4) + " " + s.Substring (8, 2) + ":" + s.Substring (10, 2) + ":" + s.Substring (12);
		return result;
	}

	public static float RoundToNearest(this float angle, float snapAngle) {
		if (snapAngle == 0) {
			return angle;
		}
		float times = Mathf.CeilToInt(angle / snapAngle);
		return (times * snapAngle);
	}
}

