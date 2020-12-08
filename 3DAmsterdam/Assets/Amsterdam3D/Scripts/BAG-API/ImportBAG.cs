﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ImportBAG
{
    public const string buildingUrl = "https://api.data.amsterdam.nl/bag/v1.1/pand/";
    public const string numberIndicatorURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/?pand=";
    public const string numberIndicatorInstanceURL = "https://api.data.amsterdam.nl/bag/v1.1/nummeraanduiding/";
    public const string monumentURL = "https://api.data.amsterdam.nl/monumenten/monumenten/?betreft_pand=";

    public const string format = "json";

    public const string requestFailureMessage = "Sorry, door een probleem met de server kan de BAG informatie tijdelijk niet worden geladen.";

    /// <summary>
    /// Returns a building data object
    /// </summary>
    /// <param name="bagId">The unique building BAG id</param>
    /// <param name="callback">The callback action containing the building data object</param>
    public static IEnumerator GetBuildingData(string bagId, Action<Pand.Rootobject> callback)
    {
        // adds data id and url in one string
        string url = buildingUrl + bagId + "/?format=" + format;

        Debug.Log(url);
        // send http request
        var request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
        }
        else
        {
            callback?.Invoke(JsonUtility.FromJson<Pand.Rootobject>(request.downloadHandler.text));
        }
    }

    public static IEnumerator GetBuildingAdresses(string bagId, Action<Pand.Rootobject> callback)
    {
        // adds data id and url in one string
        string url = numberIndicatorURL + bagId + "&format=" + format;

        Debug.Log(url);
        // send http request
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            WarningDialogs.Instance.ShowNewDialog(requestFailureMessage);
        }
        else
        {
            callback?.Invoke(JsonUtility.FromJson<Pand.Rootobject>(request.downloadHandler.text));
        }
    }
}

