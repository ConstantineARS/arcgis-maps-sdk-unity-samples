// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Net;
using System.Collections.Generic;


public class ArcGISRaycast : MonoBehaviour
{
    [SerializeField] private InputAction inputAction;
    private const float offSet = 200f;
	
    public ArcGISMapComponent arcGISMapComponent;
    public ArcGISCameraComponent arcGISCamera;
    public Canvas canvas;
    public TextMeshProUGUI featureText;

    private void Start()
    {
	    canvas.enabled = false;
    }

    private void OnEnable()
    {
	    inputAction.Enable();
    }

    private void OnDisable()
    {
	    inputAction.Disable();
    }

    private void Update()
    {
	    if (inputAction.triggered)
	    {
		    if (!canvas.enabled)
		    {
			    canvas.enabled = true;
		    }
		    RaycastHit hit;
		    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		    if (Physics.Raycast(ray, out hit))
		    {
			    var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
			    var layer = arcGISRaycastHit.layer;

				var featureId = arcGISRaycastHit.featureId;
				StartCoroutine(GetBuildingName(featureId));
				
				//var name = arcGISRaycastHit.NAME;

				/*
				if (layer != null && featureId != -1)
			    {
				    featureText.text = featureId.ToString();

				    var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
				    var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + offSet, geoPosition.SpatialReference);

				    var rotation = arcGISCamera.GetComponent<ArcGISLocationComponent>().Rotation;
				    var location = canvas.GetComponent<ArcGISLocationComponent>();
				    location.Position = offsetPosition;
				    location.Rotation = rotation;
			    }*/
		    }
	    }
    }
	IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
		{
			new KeyValuePair<string, string>("token", arcGISMapComponent.APIKey),
			new KeyValuePair<string, string>("f", "json"),
		};
	IEnumerator GetBuildingName(long featureId)
	{
		string url = $"https://basemaps3d.arcgis.com/arcgis/rest/services/OpenStreetMap3D_Buildings_v1/SceneServer/0/query?where=OBJECTID={featureId}&outFields=NAME&f=json";
		using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
		{
			// Request and wait for the desired page
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				Debug.Log(": Error: " + webRequest.error);
			}
			else
			{
				ProcessBuildingNameResponse(webRequest.downloadHandler.text);
				Debug.Log("Raw response: " + webRequest.downloadHandler.text);
			}
		}
	}

	// Process the JSON response to extract the building's name
	void ProcessBuildingNameResponse(string jsonResponse)
	{
		// Parse the JSON response to find the building name
		// This is a simplified example; you'll need to use a JSON parsing method appropriate for your setup
		// Unity's JsonUtility or a third-party library like Newtonsoft.Json can be used for more complex JSON parsing
		Debug.Log("Building name response: " + jsonResponse);
		

		// After parsing, you might extract the NAME attribute and update your UI or logic accordingly
		// Example: featureText.text = extractedName;
	}
}