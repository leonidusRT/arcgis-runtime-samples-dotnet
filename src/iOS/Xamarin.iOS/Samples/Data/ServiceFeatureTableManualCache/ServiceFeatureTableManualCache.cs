// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.ServiceFeatureTableManualCache
{
    [Register("ServiceFeatureTableManualCache")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Service feature table (manual cache)",
        "Data",
        "This sample demonstrates how to use a feature service in manual cache mode.",
        "")]
    public class ServiceFeatureTableManualCache : UIViewController
    {
        // Create and hold a reference to the MapView
        private MapView _myMapView;

        private ServiceFeatureTable _incidentsFeatureTable;

        public ServiceFeatureTableManualCache()
        {
            Title = "Service feature table (manual cache)";
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location.
            MapPoint initialLocation = new MapPoint(-13630484, 4545415, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 500000);

            // Create URL to the used feature service.
            Uri serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Create feature table for the incident feature service.
            _incidentsFeatureTable = new ServiceFeatureTable(serviceUri)
            {
                // Define the request mode.
                FeatureRequestMode = FeatureRequestMode.ManualCache
            };

            // When feature table is loaded, populate data.
            _incidentsFeatureTable.LoadStatusChanged += OnLoadedPopulateData;

            // Create FeatureLayer that uses the created table.
            FeatureLayer incidentsFeatureLayer = new FeatureLayer(_incidentsFeatureTable);

            // Add created layer to the map.
            myMap.OperationalLayers.Add(incidentsFeatureLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        private async void OnLoadedPopulateData(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // If layer isn't loaded, do nothing.
            if (e.Status != Esri.ArcGISRuntime.LoadStatus.Loaded)
                return;

            // Create new query object that contains parameters to query specific request types.
            QueryParameters queryParameters = new QueryParameters
            {
                WhereClause = "req_Type = 'Tree Maintenance or Damage'"
            };

            // Create list of the fields that are returned from the service.
            string[] outputFields = {"*"};

            // Populate feature table with the data based on query.
            await _incidentsFeatureTable.PopulateFromServiceAsync(queryParameters, true, outputFields);
        }
    }
}