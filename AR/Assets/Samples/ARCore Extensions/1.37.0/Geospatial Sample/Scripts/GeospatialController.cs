// <copyright file="GeospatialController.cs" company="Google LLC">
//
// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.Geospatial
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
#if UNITY_ANDROID

    using UnityEngine.Android;
#endif

    /// <summary>
    /// Controller for Geospatial sample.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines",
        Justification = "Bypass source check.")]
    public class GeospatialController : MonoBehaviour
    {
        [Header("AR Components")]

        public bool anchorEnable=false;

        /// <summary>
        /// The ARSessionOrigin used in the sample.
        /// </summary>
        public ARSessionOrigin SessionOrigin;

        /// <summary>
        /// The ARSession used in the sample.
        /// </summary>
        public ARSession Session;

        /// <summary>
        /// The ARAnchorManager used in the sample.
        /// </summary>
        public ARAnchorManager AnchorManager;

        /// <summary>
        /// The ARRaycastManager used in the sample.
        /// </summary>
        public ARRaycastManager RaycastManager;

        /// <summary>
        /// The AREarthManager used in the sample.
        /// </summary>
        public AREarthManager EarthManager;

        /// <summary>
        /// The ARStreetscapeGeometryManager used in the sample.
        /// </summary>
        public ARStreetscapeGeometryManager StreetscapeGeometryManager;

        /// <summary>
        /// The ARCoreExtensions used in the sample.
        /// </summary>
        public ARCoreExtensions ARCoreExtensions;

        /// <summary>
        /// The StreetscapeGeometry materials for rendering geometry building meshes.
        /// </summary>
        public List<Material> StreetscapeGeometryMaterialBuilding;

        /// <summary>
        /// The StreetscapeGeometry material for rendering geometry terrain meshes.
        /// </summary>
        public Material StreetscapeGeometryMaterialTerrain;

        [Header("UI Elements")]

        /// <summary>
        /// A 3D object that presents a Geospatial Anchor.
        /// </summary>
        public GameObject GeospatialPrefab;

        /// <summary>
        /// A 3D object that presents a Geospatial Terrain anchor.
        /// </summary>
        public GameObject TerrainPrefab;

        /// <summary>
        /// UI element showing privacy prompt.
        /// </summary>
        public GameObject PrivacyPromptCanvas;

        /// <summary>
        /// UI element showing VPS availability notification.
        /// </summary>
        public GameObject VPSCheckCanvas;

        /// <summary>
        /// UI element containing all AR view contents.
        /// </summary>
        public GameObject ARViewCanvas;

        /// <summary>
        /// UI element for clearing all anchors, including history.
        /// </summary>
        public Button ClearAllButton;

        /// <summary>
        /// UI element that enables streetscape geometry visibility.
        /// </summary>
        public Toggle GeometryToggle;


        public Button ResetCesiumOrigin;

        /// <summary>
        /// UI element to display or hide the Anchor Settings panel.
        /// </summary>
        public Button AnchorSettingButton;

        /// <summary>
        /// UI element for the Anchor Settings panel.
        /// </summary>
        public GameObject AnchorSettingPanel;

        /// <summary>
        /// UI element that toggles anchor type to Geometry.
        /// </summary>
        public Toggle GeospatialAnchorToggle;

        /// <summary>
        /// UI element that toggles anchor type to Terrain.
        /// </summary>
        public Toggle TerrainAnchorToggle;

        /// <summary>
        /// UI element that toggles anchor type to Rooftop.
        /// </summary>
        public Toggle RooftopAnchorToggle;

        /// <summary>
        /// UI element to display information at runtime.
        /// </summary>
        public GameObject InfoPanel;

        /// <summary>
        /// Text displaying <see cref="GeospatialPose"/> information at runtime.
        /// </summary>
        public Text InfoText;

        /// <summary>
        /// Text displaying in a snack bar at the bottom of the screen.
        /// </summary>
        public Text SnackBarText;

        /// <summary>
        /// Text displaying debug information, only activated in debug build.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// Help message shown while localizing.
        /// </summary>
        private const string _localizingMessage = "Localizing your device to set anchor.";

        /// <summary>
        /// Help message shown while initializing Geospatial functionalities.
        /// </summary>
        private const string _localizationInitializingMessage =
            "Initializing Geospatial functionalities.";

        /// <summary>
        /// Help message shown when <see cref="AREarthManager.EarthTrackingState"/> is not tracking
        /// or the pose accuracies are beyond thresholds.
        /// </summary>
        private const string _localizationInstructionMessage =
            "Point your camera at buildings, stores, and signs near you.";

        /// <summary>
        /// Help message shown when location fails or hits timeout.
        /// </summary>
        private const string _localizationFailureMessage =
            "Localization not possible.\n" +
            "Close and open the app to restart the session.";

        /// <summary>
        /// Help message shown when localization is completed.
        /// </summary>
        private const string _localizationSuccessMessage = "Localization completed.";

        /// <summary>
        /// Help message shown when resolving takes too long.
        /// </summary>
        private const string _resolvingTimeoutMessage =
            "Still resolving the terrain anchor.\n" +
            "Please make sure you're in an area that has VPS coverage.";

        /// <summary>
        /// The timeout period waiting for localization to be completed.
        /// </summary>
        private const float _timeoutSeconds = 180;

        /// <summary>
        /// Indicates how long a information text will display on the screen before terminating.
        /// </summary>
        private const float _errorDisplaySeconds = 3;

        /// <summary>
        /// The key name used in PlayerPrefs which indicates whether the privacy prompt has
        /// displayed at least one time.
        /// </summary>
        private const string _hasDisplayedPrivacyPromptKey = "HasDisplayedGeospatialPrivacyPrompt";

        /// <summary>
        /// The key name used in PlayerPrefs which stores geospatial anchor history data.
        /// The earliest one will be deleted once it hits storage limit.
        /// </summary>
        private const string _persistentGeospatialAnchorsStorageKey = "PersistentGeospatialAnchors";

        /// <summary>
        /// The limitation of how many Geospatial Anchors can be stored in local storage.
        /// </summary>
        private const int _storageLimit = 20;

        /// <summary>
        /// Accuracy threshold for orientation yaw accuracy in degrees that can be treated as
        /// localization completed.
        /// </summary>
        private const double _orientationYawAccuracyThreshold = 25;

        /// <summary>
        /// Accuracy threshold for heading degree that can be treated as localization completed.
        /// </summary>
        private const double _headingAccuracyThreshold = 25;

        /// <summary>
        /// Accuracy threshold for altitude and longitude that can be treated as localization
        /// completed.
        /// </summary>
        private const double _horizontalAccuracyThreshold = 20;

        /// <summary>
        /// Determines if the anchor settings panel is visible in the UI.
        /// </summary>
        private bool _showAnchorSettingsPanel = false;

        /// <summary>
        /// Determines if streetscape geometry is rendered in the scene.
        /// </summary>
        private bool _streetscapeGeometryVisibility = false;

        /// <summary>
        /// Represents the current anchor type of the anchor being placed in the scene.
        /// </summary>
        private AnchorType _anchorType = AnchorType.Geospatial;

        /// <summary>
        /// Determines which building material will be used for the current building mesh.
        /// </summary>
        private int _buildingMatIndex = 0;

        /// <summary>
        /// Dictionary of streetscapegeometry handles to render objects for rendering
        /// streetscapegeometry meshes.
        /// </summary>
        private Dictionary<TrackableId, GameObject> _streetscapegeometryGOs =
            new Dictionary<TrackableId, GameObject>();

        /// <summary>
        /// ARStreetscapeGeometries added in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _addedStreetscapeGeometrys = new List<ARStreetscapeGeometry>();

        /// <summary>
        /// ARStreetscapeGeometries updated in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _updatedStreetscapeGeometrys =
            new List<ARStreetscapeGeometry>();

        /// <summary>
        /// ARStreetscapeGeometries removed in the last Unity Update.
        /// </summary>
        List<ARStreetscapeGeometry> _removedStreetscapeGeometrys =
            new List<ARStreetscapeGeometry>();

        /// <summary>
        /// Determines if streetscape geometry should be removed from the scene.
        /// </summary>
        private bool _clearStreetscapeGeometryRenderObjects = false;

        private bool _waitingForLocationService = false;
        private bool _isInARView = false;
        private bool _isReturning = false;
        private bool _isLocalizing = false;
        private bool _enablingGeospatial = false;
        private bool _shouldResolvingHistory = false;
        private float _localizationPassedTime = 0f;
        private float _configurePrepareTime = 3f;
        private GeospatialAnchorHistoryCollection _historyCollection = null;
        private List<GameObject> _anchorObjects = new List<GameObject>();
        private IEnumerator _startLocationService = null;
        private IEnumerator _asyncCheck = null;

        /// <summary>
        /// Callback handling "Get Started" button click event in Privacy Prompt.
        /// </summary>
        public void OnGetStartedClicked()
        {
            PlayerPrefs.SetInt(_hasDisplayedPrivacyPromptKey, 1);
            PlayerPrefs.Save();
            SwitchToARView(true);
        }

        /// <summary>
        /// Callback handling "Learn More" Button click event in Privacy Prompt.
        /// </summary>
        public void OnLearnMoreClicked()
        {
            Application.OpenURL(
                "https://developers.google.com/ar/data-privacy");
        }

        /// <summary>
        /// Callback handling "Clear All" button click event in AR View.
        /// </summary>
        public void OnClearAllClicked()
        {
            foreach (var anchor in _anchorObjects)
            {
                Destroy(anchor);
            }

            _anchorObjects.Clear();
            _historyCollection.Collection.Clear();
            SnackBarText.text = "Anchor(s) cleared!";
            ClearAllButton.gameObject.SetActive(false);
            SaveGeospatialAnchorHistory();
        }

        /// <summary>
        /// Callback handling "Continue" button click event in AR View.
        /// </summary>
        public void OnContinueClicked()
        {
            VPSCheckCanvas.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Geometry" toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Streetscape Geometry visibility.</param>
        public void OnGeometryToggled(bool enabled)
        {
            _streetscapeGeometryVisibility = enabled;
            if (!_streetscapeGeometryVisibility)
            {
                _clearStreetscapeGeometryRenderObjects = true;
            }
        }

        /// <summary>
        /// Callback handling "Reset Cesium Origin"  button click event in AR View.
        /// Align the Cesium Origin with a Google Geospatial Anchor, with the same lat/lon with the supposed to be origin.
        /// </summary>
        public void OnResetCesiumOriginClicked()
        {
            //VPSCheckCanvas.SetActive(false);
            //Transform sourceTransform = sourceObject.transform;
            GameObject foundObject = GameObject.Find("AR Geospatial Creator Anchor 1");
            if (foundObject != null)
            {
                // The GameObject was found, do something with it
                //foundObject.transform.position = Vector3.zero; // Set the position of the foundObject to Vector3.zero (0, 0, 0)

                Transform sourceTransform = foundObject.transform;

                GameObject Cesium3DTilesetObject = GameObject.Find("Cesium3DTileset");
                if (foundObject != null)
                {

                    Cesium3DTilesetObject.transform.rotation = sourceTransform.rotation;
                    Cesium3DTilesetObject.transform.position = sourceTransform.position;

                    //Material mat = new Material(Shader.Find("Standard"));
                    //mat.SetFloat("_Mode", 3);
                    //mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    //mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    //mat.SetInt("_ZWrite", 0);
                    //mat.DisableKeyword("_ALPHATEST_ON");
                    //mat.DisableKeyword("_ALPHABLEND_ON");
                    //mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    //mat.renderQueue = 3000;

                    //Cesium3DTileset targetScript = Cesium3DTilesetObject.GetComponent<Cesium3DTileset>();
                    //targetScript.ChangeOpaqueMaterial(mat);


                }
                else
                {
                    // The GameObject was not found
                    Debug.Log("Cesium3DTileset");
                }

                }
                else
            {
                // The GameObject was not found
                Debug.Log("AR Geospatial Creator Anchor 1");
            }


        }

        public void OnAlignOriginWithVR()
        {
            //GameObject offsetObject = new GameObject("TrackingErrorOffset");
            //Transform offset = offsetObject.transform;
            //// Insert the offset between us and our current parent. i.e., the
            //// offset's parent is our current parent, and our parent is the offset
            //GameObject ARCamera = GameObject.Find("AR Camera");

            //offsetObject.parent = ARCamera.parent;
            //ARCamera.parent = offset;


           GameObject emptyGameObject; // Reference to the empty GameObject to be added
           Transform mainCameraTransform; // Reference to the main camera's transform
           Transform mainCameraParent; // Reference to the main camera's parent transform

        
            mainCameraTransform = Camera.main.transform;
            mainCameraParent = mainCameraTransform.parent;

            // Create the empty GameObject
            emptyGameObject = new GameObject("EmptyObjectBetweenCamera");




        //VPSCheckCanvas.SetActive(false);
        //Transform sourceTransform = sourceObject.transform;
        GameObject foundObject = GameObject.Find("AR Geospatial Creator Anchor 1");
            if (foundObject != null)
            {
                // The GameObject was found, do something with it
                //foundObject.transform.position = Vector3.zero; // Set the position of the foundObject to Vector3.zero (0, 0, 0)

                Transform sourceTransform = foundObject.transform;

                Quaternion inverseRotation = Quaternion.Inverse(sourceTransform.rotation);
                Vector3 inversePosition = inverseRotation * -sourceTransform.position;


                // Set its position to the midpoint between the main camera and its parent
                emptyGameObject.transform.position = inversePosition;

                // Set its rotation to the average of the main camera and its parent rotation
                emptyGameObject.transform.rotation = inverseRotation;

                // Make it the parent of the main camera
                mainCameraTransform.SetParent(emptyGameObject.transform);


            }
            else
            {
                // The GameObject was not found
                Debug.Log("AR Geospatial Creator Anchor 1");
            }


        }


        //public void OnAlignOriginWithVR()
        //{
        //    //VPSCheckCanvas.SetActive(false);
        //    //Transform sourceTransform = sourceObject.transform;
        //    GameObject foundObject = GameObject.Find("AR Geospatial Creator Anchor 1");
        //    if (foundObject != null)
        //    {
        //        // The GameObject was found, do something with it
        //        //foundObject.transform.position = Vector3.zero; // Set the position of the foundObject to Vector3.zero (0, 0, 0)

        //        Transform sourceTransform = foundObject.transform;

        //        Quaternion inverseRotation = Quaternion.Inverse(sourceTransform.rotation);
        //        Vector3 inversePosition = inverseRotation * -sourceTransform.position;

        //        // Apply the inverse transformation to the main camera
        //        Camera.main.transform.rotation = inverseRotation;
        //        Camera.main.transform.position = inversePosition;

        //    }
        //    else
        //    {
        //        // The GameObject was not found
        //        Debug.Log("AR Geospatial Creator Anchor 1");
        //    }


        //}




        /// <summary>
        /// Callback handling the  "Anchor Setting" panel display or hide event in AR View.
        /// </summary>
        public void OnAnchorSettingButtonClicked()
        {
            _showAnchorSettingsPanel = !_showAnchorSettingsPanel;
            if (_showAnchorSettingsPanel)
            {
                SetAnchorPanelState(true);
            }
            else
            {
                SetAnchorPanelState(false);
            }
        }

        /// <summary>
        /// Callback handling Geospatial anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Geospatial anchors.</param>
        public void OnGeospatialAnchorToggled(bool enabled)
        {
            // GeospatialAnchorToggle.GetComponent<Toggle>().isOn = true;;
            _anchorType = AnchorType.Geospatial;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Callback handling Terrain anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Terrain anchors.</param>
        public void OnTerrainAnchorToggled(bool enabled)
        {
            // TerrainAnchorToggle.GetComponent<Toggle>().isOn = true;
            _anchorType = AnchorType.Terrain;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Callback handling Rooftop anchor toggle event in AR View.
        /// </summary>
        /// <param name="enabled">Whether to enable Rooftop anchors.</param>
        public void OnRooftopAnchorToggled(bool enabled)
        {
            // RooftopAnchorToggle.GetComponent<Toggle>().isOn = true;
            _anchorType = AnchorType.Rooftop;
            SetAnchorPanelState(false);
        }

        /// <summary>
        /// Unity's Awake() method.
        /// </summary>
        public void Awake()
        {
            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;

            // Enable geospatial sample to target 60fps camera capture frame rate
            // on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;

            if (SessionOrigin == null)
            {
                Debug.LogError("Cannot find ARSessionOrigin.");
            }

            if (Session == null)
            {
                Debug.LogError("Cannot find ARSession.");
            }

            if (ARCoreExtensions == null)
            {
                Debug.LogError("Cannot find ARCoreExtensions.");
            }
        }

        /// <summary>
        /// Unity's OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);

            _isReturning = false;
            _enablingGeospatial = false;
            InfoPanel.SetActive(false);
            GeometryToggle.gameObject.SetActive(false);
            AnchorSettingButton.gameObject.SetActive(false);
            AnchorSettingPanel.gameObject.SetActive(false);
            GeospatialAnchorToggle.gameObject.SetActive(false);
            TerrainAnchorToggle.gameObject.SetActive(false);
            RooftopAnchorToggle.gameObject.SetActive(false);
            ClearAllButton.gameObject.SetActive(false);
            DebugText.gameObject.SetActive(Debug.isDebugBuild && EarthManager != null);
            GeometryToggle.onValueChanged.AddListener(OnGeometryToggled);
            AnchorSettingButton.onClick.AddListener(OnAnchorSettingButtonClicked);
            GeospatialAnchorToggle.onValueChanged.AddListener(OnGeospatialAnchorToggled);
            TerrainAnchorToggle.onValueChanged.AddListener(OnTerrainAnchorToggled);
            RooftopAnchorToggle.onValueChanged.AddListener(OnRooftopAnchorToggled);

            _localizationPassedTime = 0f;
            _isLocalizing = true;
            SnackBarText.text = _localizingMessage;

            LoadGeospatialAnchorHistory();
            _shouldResolvingHistory = _historyCollection.Collection.Count > 0;

            SwitchToARView(PlayerPrefs.HasKey(_hasDisplayedPrivacyPromptKey));

            if (StreetscapeGeometryManager == null)
            {
                Debug.LogWarning("StreetscapeGeometryManager must be set in the " +
                    "GeospatialController Inspector to render StreetscapeGeometry.");
            }

            if (StreetscapeGeometryMaterialBuilding.Count == 0)
            {
                Debug.LogWarning("StreetscapeGeometryMaterialBuilding in the " +
                    "GeospatialController Inspector must contain at least one material " +
                    "to render StreetscapeGeometry.");
                return;
            }

            if (StreetscapeGeometryMaterialTerrain == null)
            {
                Debug.LogWarning("StreetscapeGeometryMaterialTerrain must be set in the " +
                    "GeospatialController Inspector to render StreetscapeGeometry.");
                return;
            }
        }

        /// <summary>
        /// Unity's OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            StopCoroutine(_asyncCheck);
            _asyncCheck = null;
            StopCoroutine(_startLocationService);
            _startLocationService = null;
            Debug.Log("Stop location services.");
            Input.location.Stop();

            foreach (var anchor in _anchorObjects)
            {
                Destroy(anchor);
            }

            _anchorObjects.Clear();
            SaveGeospatialAnchorHistory();
        }

        /// <summary>
        /// Unity's Update() method.
        /// </summary>
        public void Update()
        {
            if (!_isInARView)
            {
                return;
            }

            UpdateDebugInfo();

            // Check session error status.
            LifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            // Check feature support and enable Geospatial API when it's supported.
            var featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    return;
                case FeatureSupported.Unsupported:
                    ReturnWithReason("The Geospatial API is not supported by this device.");
                    return;
                case FeatureSupported.Supported:
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode ==
                        GeospatialMode.Disabled)
                    {
                        Debug.Log("Geospatial sample switched to GeospatialMode.Enabled.");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        ARCoreExtensions.ARCoreExtensionsConfig.StreetscapeGeometryMode =
                            StreetscapeGeometryMode.Enabled;
                        _configurePrepareTime = 3.0f;
                        _enablingGeospatial = true;
                        return;
                    }

                    break;
            }

            // Waiting for new configuration to take effect.
            if (_enablingGeospatial)
            {
                _configurePrepareTime -= Time.deltaTime;
                if (_configurePrepareTime < 0)
                {
                    _enablingGeospatial = false;
                }
                else
                {
                    return;
                }
            }

            // Check earth state.
            var earthState = EarthManager.EarthState;
            if (earthState == EarthState.ErrorEarthNotReady)
            {
                SnackBarText.text = _localizationInitializingMessage;
                return;
            }
            else if (earthState != EarthState.Enabled)
            {
                string errorMessage =
                    "Geospatial sample encountered an EarthState error: " + earthState;
                Debug.LogWarning(errorMessage);
                SnackBarText.text = errorMessage;
                return;
            }

            // Check earth localization.
            bool isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
                Input.location.status == LocationServiceStatus.Running;
            var earthTrackingState = EarthManager.EarthTrackingState;
            var pose = earthTrackingState == TrackingState.Tracking ?
                EarthManager.CameraGeospatialPose : new GeospatialPose();
            if (!isSessionReady || earthTrackingState != TrackingState.Tracking ||
                pose.OrientationYawAccuracy > _orientationYawAccuracyThreshold ||
                pose.HorizontalAccuracy > _horizontalAccuracyThreshold)
            {
                // Lost localization during the session.
                if (!_isLocalizing)
                {
                    _isLocalizing = true;
                    _localizationPassedTime = 0f;
                    GeometryToggle.gameObject.SetActive(false);
                    AnchorSettingButton.gameObject.SetActive(false);
                    AnchorSettingPanel.gameObject.SetActive(false);
                    GeospatialAnchorToggle.gameObject.SetActive(false);
                    TerrainAnchorToggle.gameObject.SetActive(false);
                    RooftopAnchorToggle.gameObject.SetActive(false);
                    ClearAllButton.gameObject.SetActive(false);
                    foreach (var go in _anchorObjects)
                    {
                        go.SetActive(false);
                    }
                }

                if (_localizationPassedTime > _timeoutSeconds)
                {
                    Debug.LogError("Geospatial sample localization timed out.");
                    ReturnWithReason(_localizationFailureMessage);
                }
                else
                {
                    _localizationPassedTime += Time.deltaTime;
                    SnackBarText.text = _localizationInstructionMessage;
                }
            }
            else if (_isLocalizing)
            {
                // Finished localization.
                _isLocalizing = false;
                _localizationPassedTime = 0f;
                GeometryToggle.gameObject.SetActive(true);

                if (anchorEnable)
                { 
                    AnchorSettingButton.gameObject.SetActive(true);
                    ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                    foreach (var go in _anchorObjects)
                    {

                        go.SetActive(true);
                    }

                    ResolveHistory();
                }
                
                
                SnackBarText.text = _localizationSuccessMessage;

            }
            else
            {
                if (_streetscapeGeometryVisibility)
                {
                    // get access to ARstreetscapeGeometries in ARStreetscapeGeometryManager
                    if (StreetscapeGeometryManager)
                    {
                        StreetscapeGeometryManager.StreetscapeGeometriesChanged
                            += (ARStreetscapeGeometrysChangedEventArgs) =>
                        {
                            _addedStreetscapeGeometrys =
                                ARStreetscapeGeometrysChangedEventArgs.Added;
                            _updatedStreetscapeGeometrys =
                                ARStreetscapeGeometrysChangedEventArgs.Updated;
                            _removedStreetscapeGeometrys =
                                ARStreetscapeGeometrysChangedEventArgs.Removed;
                        };
                    }

                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _addedStreetscapeGeometrys)
                    {
                        InstantiateRenderObject(streetscapegeometry);
                    }

                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _updatedStreetscapeGeometrys)
                    {
                        // This second call to instantiate is required if geometry is toggled on
                        // or off after the app has started.
                        InstantiateRenderObject(streetscapegeometry);
                        UpdateRenderObject(streetscapegeometry);
                    }

                    foreach (
                        ARStreetscapeGeometry streetscapegeometry in _removedStreetscapeGeometrys)
                    {
                        DestroyRenderObject(streetscapegeometry);
                    }
                }
                else if (_clearStreetscapeGeometryRenderObjects)
                {
                    DestroyAllRenderObjects();
                    _clearStreetscapeGeometryRenderObjects = false;
                }

                if (anchorEnable)
                { 
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began
                    && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                    && _anchorObjects.Count < _storageLimit)
                {
                    // Set anchor on screen tap.
                    PlaceAnchorByScreenTap(Input.GetTouch(0).position);
                }

                // Hide anchor settings and toggles if the storage limit has been reached.
                if (_anchorObjects.Count >= _storageLimit)
                {
                    AnchorSettingButton.gameObject.SetActive(false);
                    AnchorSettingPanel.gameObject.SetActive(false);
                    GeospatialAnchorToggle.gameObject.SetActive(false);
                    TerrainAnchorToggle.gameObject.SetActive(false);
                    RooftopAnchorToggle.gameObject.SetActive(false);
                }
                else
                {
                    AnchorSettingButton.gameObject.SetActive(true);
                }
            }
            }
            InfoPanel.SetActive(true);
            if (earthTrackingState == TrackingState.Tracking)
            {
                InfoText.text = string.Format(
                "Latitude/Longitude: {1}°, {2}°{0}" +
                "Horizontal Accuracy: {3}m{0}" +
                "Altitude: {4}m{0}" +
                "Vertical Accuracy: {5}m{0}" +
                "Eun Rotation: {6}{0}" +
                "Orientation Yaw Accuracy: {7}°"
                + "Unity Camera Pos: {8}°",
                Environment.NewLine,
                pose.Latitude.ToString("F6"),
                pose.Longitude.ToString("F6"),
                pose.HorizontalAccuracy.ToString("F6"),
                pose.Altitude.ToString("F2"),
                pose.VerticalAccuracy.ToString("F2"),
                pose.EunRotation.ToString("F1"),
                pose.OrientationYawAccuracy.ToString("F1"),
                Camera.main.transform.position.ToString("F4"));
            }
            else
            {
                InfoText.text = "GEOSPATIAL POSE: not tracking";
            }
        }

        /// <summary>
        /// Sets up a render object for this <c>ARStreetscapeGeometry</c>.
        /// </summary>
        /// <param name="streetscapegeometry">The
        /// <c><see cref="ARStreetscapeGeometry"/></c> object containing the mesh
        /// to be rendered.</param>
        private void InstantiateRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (streetscapegeometry.mesh == null)
            {
                return;
            }

            // Check if a render object already exists for this streetscapegeometry and
            // create one if not.
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                return;
            }

            GameObject renderObject = new GameObject(
                "StreetscapeGeometryMesh", typeof(MeshFilter), typeof(MeshRenderer));

            if (renderObject)
            {
                renderObject.transform.position = new Vector3(0, 0.5f, 0);
                renderObject.GetComponent<MeshFilter>().mesh = streetscapegeometry.mesh;

                // Add a material with transparent diffuse shader.
                if (streetscapegeometry.streetscapeGeometryType ==
                    StreetscapeGeometryType.Building)
                {
                    renderObject.GetComponent<MeshRenderer>().material =
                        StreetscapeGeometryMaterialBuilding[_buildingMatIndex];
                    _buildingMatIndex =
                        (_buildingMatIndex + 1) % StreetscapeGeometryMaterialBuilding.Count;
                }
                else
                {
                    renderObject.GetComponent<MeshRenderer>().material =
                        StreetscapeGeometryMaterialTerrain;
                }

                renderObject.transform.position = streetscapegeometry.pose.position;
                renderObject.transform.rotation = streetscapegeometry.pose.rotation;

                _streetscapegeometryGOs.Add(streetscapegeometry.trackableId, renderObject);
            }
        }

        /// <summary>
        /// Updates the render object transform based on this streetscapegeometrys pose.
        /// It must be called every frame to update the mesh.
        /// </summary>
        /// <param name="streetscapegeometry">The <c><see cref="ARStreetscapeGeometry"/></c>
        /// object containing the mesh to be rendered.</param>
        private void UpdateRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                GameObject renderObject = _streetscapegeometryGOs[streetscapegeometry.trackableId];
                renderObject.transform.position = streetscapegeometry.pose.position;
                renderObject.transform.rotation = streetscapegeometry.pose.rotation;
            }
        }

        /// <summary>
        /// Destroys the render object associated with the
        /// <c><see cref="ARStreetscapeGeometry"/></c>.
        /// </summary>
        /// <param name="streetscapegeometry">The <c><see cref="ARStreetscapeGeometry"/></c>
        /// containing the render object to be destroyed.</param>
        private void DestroyRenderObject(ARStreetscapeGeometry streetscapegeometry)
        {
            if (_streetscapegeometryGOs.ContainsKey(streetscapegeometry.trackableId))
            {
                var geometry = _streetscapegeometryGOs[streetscapegeometry.trackableId];
                _streetscapegeometryGOs.Remove(streetscapegeometry.trackableId);
                Destroy(geometry);
            }
        }

        /// <summary>
        /// Destroys all stored <c><see cref="ARStreetscapeGeometry"/></c> render objects.
        /// </summary>
        private void DestroyAllRenderObjects()
        {
            var keys = _streetscapegeometryGOs.Keys;
            foreach (var key in keys)
            {
                var renderObject = _streetscapegeometryGOs[key];
                Destroy(renderObject);
            }

            _streetscapegeometryGOs.Clear();
        }

        /// <summary>
        /// Activate or deactivate all UI elements on the anchor setting Panel.
        /// </summary>
        /// <param name="state">A bool value to determine if the anchor settings panel is visible.
        private void SetAnchorPanelState(bool state)
        {
            AnchorSettingPanel.gameObject.SetActive(state);
            GeospatialAnchorToggle.gameObject.SetActive(state);
            TerrainAnchorToggle.gameObject.SetActive(state);
            RooftopAnchorToggle.gameObject.SetActive(state);
        }

        private IEnumerator CheckRooftopPromise(ResolveAnchorOnRooftopPromise promise,
            GeospatialAnchorHistory history)
        {
            var retry = 0;
            while (promise.State == PromiseState.Pending)
            {
                if (retry == 100)
                {
                    SnackBarText.text = _resolvingTimeoutMessage;
                }

                yield return new WaitForSeconds(0.1f);
                retry = Math.Min(retry + 1, 100);
            }

            var result = promise.Result;
            if (result.RooftopAnchorState == RooftopAnchorState.Success &&
                result.Anchor != null)
            {
                // Adjust the scale of the prefab anchor object to maintain visibility when it is
                // far away.
                result.Anchor.gameObject.transform.localScale *= GetRooftopAnchorScale(
                    result.Anchor.gameObject.transform.position,
                    Camera.main.transform.position);
                GameObject anchorGO = Instantiate(TerrainPrefab,
                    result.Anchor.gameObject.transform);
                anchorGO.transform.parent = result.Anchor.gameObject.transform;

                _anchorObjects.Add(result.Anchor.gameObject);
                _historyCollection.Collection.Add(history);

                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();
                if (anchorEnable)
                {
                    ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                }
                SaveGeospatialAnchorHistory();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            yield break;
        }

        private IEnumerator CheckTerrainPromise(ResolveAnchorOnTerrainPromise promise,
            GeospatialAnchorHistory history)
        {
            var retry = 0;
            while (promise.State == PromiseState.Pending)
            {
                if (retry == 100)
                {
                    SnackBarText.text = _resolvingTimeoutMessage;
                }

                yield return new WaitForSeconds(0.1f);
                retry = Math.Min(retry + 1, 100);
            }

            var result = promise.Result;
            if (result.TerrainAnchorState == TerrainAnchorState.Success &&
                result.Anchor != null)
            {
                GameObject anchorGO = Instantiate(TerrainPrefab,
                    result.Anchor.gameObject.transform);
                anchorGO.transform.parent = result.Anchor.gameObject.transform;

                _anchorObjects.Add(result.Anchor.gameObject);
                _historyCollection.Collection.Add(history);

                SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();

                ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                SaveGeospatialAnchorHistory();
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            yield break;
        }

        private float GetRooftopAnchorScale(Vector3 anchor, Vector3 camera)
        {
            // Return the scale in range [1, 2] after mapping a distance between camera and anchor
            // to [2, 20].
            float distance =
                Mathf.Sqrt(
                    Mathf.Pow(anchor.x - camera.x, 2.0f)
                    + Mathf.Pow(anchor.y - camera.y, 2.0f)
                    + Mathf.Pow(anchor.z - camera.z, 2.0f));
            float mapDistance = Mathf.Min(Mathf.Max(2.0f, distance), 20.0f);
            return (mapDistance - 2.0f) / (20.0f - 2.0f) + 1.0f;
        }

        private void PlaceAnchorByScreenTap(Vector2 position)
        {
            if (anchorEnable)
            {
                if (_streetscapeGeometryVisibility)
                {
                    // Raycast against streetscapeGeometry.
                    List<XRRaycastHit> hitResults = new List<XRRaycastHit>();
                    if (RaycastManager.RaycastStreetscapeGeometry(position, ref hitResults))
                    {
                        if (_anchorType == AnchorType.Rooftop || _anchorType == AnchorType.Terrain)
                        {
                            var streetscapeGeometry =
                                StreetscapeGeometryManager.GetStreetscapeGeometry(
                                    hitResults[0].trackableId);
                            if (streetscapeGeometry == null)
                            {
                                return;
                            }

                            if (_streetscapegeometryGOs.ContainsKey(streetscapeGeometry.trackableId))
                            {
                                Pose modifiedPose = new Pose(hitResults[0].pose.position,
                                    Quaternion.LookRotation(Vector3.right, Vector3.up));

                                GeospatialAnchorHistory history =
                                    CreateHistory(modifiedPose, _anchorType);

                                // Anchor returned will be null, the coroutine will handle creating
                                // the anchor when the promise is done.
                                PlaceARAnchor(history, modifiedPose, hitResults[0].trackableId);
                            }
                        }
                        else
                        {
                            GeospatialAnchorHistory history = CreateHistory(hitResults[0].pose,
                                _anchorType);
                            var anchor = PlaceARAnchor(history, hitResults[0].pose,
                                hitResults[0].trackableId);
                            if (anchor != null)
                            {
                                _historyCollection.Collection.Add(history);
                            }

                            ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                            SaveGeospatialAnchorHistory();
                        }
                    }

                    return;
                }

                // Raycast against detected planes.
                List<ARRaycastHit> planeHitResults = new List<ARRaycastHit>();
                RaycastManager.Raycast(
                    position, planeHitResults, TrackableType.Planes | TrackableType.FeaturePoint);
                if (planeHitResults.Count > 0)
                {
                    GeospatialAnchorHistory history = CreateHistory(planeHitResults[0].pose,
                        _anchorType);

                    if (_anchorType == AnchorType.Rooftop)
                    {
                        // The coroutine will create the anchor when the promise is done.
                        Quaternion eunRotation = CreateRotation(history);
                        ResolveAnchorOnRooftopPromise rooftopPromise =
                            AnchorManager.ResolveAnchorOnRooftopAsync(
                                history.Latitude, history.Longitude,
                                0, eunRotation);

                        StartCoroutine(CheckRooftopPromise(rooftopPromise, history));
                        return;
                    }

                    var anchor = PlaceGeospatialAnchor(history);
                    if (anchor != null)
                    {
                        _historyCollection.Collection.Add(history);
                    }

                    ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                    SaveGeospatialAnchorHistory();
                }
            }
        }

        private GeospatialAnchorHistory CreateHistory(Pose pose, AnchorType anchorType)
        {
            GeospatialPose geospatialPose = EarthManager.Convert(pose);

            GeospatialAnchorHistory history = new GeospatialAnchorHistory(
                geospatialPose.Latitude, geospatialPose.Longitude, geospatialPose.Altitude,
                anchorType, geospatialPose.EunRotation);
            return history;
        }

        private Quaternion CreateRotation(GeospatialAnchorHistory history)
        {
            Quaternion eunRotation = history.EunRotation;
            if (eunRotation == Quaternion.identity)
            {
                // This history is from a previous app version and EunRotation was not used.
                eunRotation =
                    Quaternion.AngleAxis(180f - (float)history.Heading, Vector3.up);
            }
            return eunRotation;
        }

        private ARAnchor PlaceARAnchor(GeospatialAnchorHistory history, Pose pose = new Pose(),
            TrackableId trackableId = new TrackableId())
        {
            Quaternion eunRotation = CreateRotation(history);
            ARAnchor anchor = null;
            switch (history.AnchorType)
            {
                case AnchorType.Rooftop:
                    ResolveAnchorOnRooftopPromise rooftopPromise =
                        AnchorManager.ResolveAnchorOnRooftopAsync(
                            history.Latitude, history.Longitude,
                            0, eunRotation);

                    StartCoroutine(CheckRooftopPromise(rooftopPromise, history));

                    return null;
                case AnchorType.Terrain:
                    ResolveAnchorOnTerrainPromise terrainPromise =
                        AnchorManager.ResolveAnchorOnTerrainAsync(
                            history.Latitude, history.Longitude,
                            0, eunRotation);

                    StartCoroutine(CheckTerrainPromise(terrainPromise, history));

                    return null;
                case AnchorType.Geospatial:
                    ARStreetscapeGeometry streetscapegeometry =
                        StreetscapeGeometryManager.GetStreetscapeGeometry(trackableId);
                    if (streetscapegeometry != null)
                    {
                        anchor = StreetscapeGeometryManager.AttachAnchor(
                            streetscapegeometry, pose);
                    }

                    if (anchor != null)
                    {
                        _anchorObjects.Add(anchor.gameObject);
                        _historyCollection.Collection.Add(history);
                        ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
                        SaveGeospatialAnchorHistory();

                        SnackBarText.text = GetDisplayStringForAnchorPlacedSuccess();
                    }
                    else
                    {
                        SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
                    }

                    break;
            }

            return anchor;
        }

        private ARGeospatialAnchor PlaceGeospatialAnchor(
            GeospatialAnchorHistory history)
        {
            bool terrain = history.AnchorType == AnchorType.Terrain;
            Quaternion eunRotation = CreateRotation(history);
            ARGeospatialAnchor anchor = null;

            if (terrain)
            {
                // Anchor returned will be null, the coroutine will handle creating the
                // anchor when the promise is done.
                ResolveAnchorOnTerrainPromise promise =
                    AnchorManager.ResolveAnchorOnTerrainAsync(
                        history.Latitude, history.Longitude,
                        0, eunRotation);

                StartCoroutine(CheckTerrainPromise(promise, history));
                return null;
            }
            else
            {
                anchor = AnchorManager.AddAnchor(
                    history.Latitude, history.Longitude, history.Altitude, eunRotation);
            }

            if (anchor != null)
            {
                GameObject anchorGO = history.AnchorType == AnchorType.Geospatial ?
                    Instantiate(GeospatialPrefab, anchor.transform) :
                    Instantiate(TerrainPrefab, anchor.transform);
                anchor.gameObject.SetActive(!terrain);
                anchorGO.transform.parent = anchor.gameObject.transform;
                _anchorObjects.Add(anchor.gameObject);
            }
            else
            {
                SnackBarText.text = GetDisplayStringForAnchorPlacedFailure();
            }

            return anchor;
        }

        private void ResolveHistory()
        {
            if (!_shouldResolvingHistory)
            {
                return;
            }

            _shouldResolvingHistory = false;
            foreach (var history in _historyCollection.Collection)
            {
                switch (history.AnchorType)
                {
                    case AnchorType.Rooftop:
                        PlaceARAnchor(history);
                        break;
                    case AnchorType.Terrain:
                        PlaceARAnchor(history);
                        break;
                    default:
                        PlaceGeospatialAnchor(history);
                        break;
                }
            }

            ClearAllButton.gameObject.SetActive(_anchorObjects.Count > 0);
            SnackBarText.text = string.Format("{0} anchor(s) set from history.",
                _anchorObjects.Count);
        }

        private void LoadGeospatialAnchorHistory()
        {
            if (PlayerPrefs.HasKey(_persistentGeospatialAnchorsStorageKey))
            {
                _historyCollection = JsonUtility.FromJson<GeospatialAnchorHistoryCollection>(
                    PlayerPrefs.GetString(_persistentGeospatialAnchorsStorageKey));

                // Remove all records created more than 24 hours and update stored history.
                DateTime current = DateTime.Now;
                _historyCollection.Collection.RemoveAll(
                    data => current.Subtract(data.CreatedTime).Days > 0);
                PlayerPrefs.SetString(_persistentGeospatialAnchorsStorageKey,
                    JsonUtility.ToJson(_historyCollection));
                PlayerPrefs.Save();
            }
            else
            {
                _historyCollection = new GeospatialAnchorHistoryCollection();
            }
        }

        private void SaveGeospatialAnchorHistory()
        {
            // Sort the data from latest record to earliest record.
            _historyCollection.Collection.Sort((left, right) =>
                right.CreatedTime.CompareTo(left.CreatedTime));

            // Remove the earliest data if the capacity exceeds storage limit.
            if (_historyCollection.Collection.Count > _storageLimit)
            {
                _historyCollection.Collection.RemoveRange(
                    _storageLimit, _historyCollection.Collection.Count - _storageLimit);
            }

            PlayerPrefs.SetString(
                _persistentGeospatialAnchorsStorageKey, JsonUtility.ToJson(_historyCollection));
            PlayerPrefs.Save();
        }

        private void SwitchToARView(bool enable)
        {
            _isInARView = enable;
            SessionOrigin.gameObject.SetActive(enable);
            Session.gameObject.SetActive(enable);
            ARCoreExtensions.gameObject.SetActive(enable);
            ARViewCanvas.SetActive(enable);
            PrivacyPromptCanvas.SetActive(!enable);
            VPSCheckCanvas.SetActive(false);
            if (enable && _asyncCheck == null)
            {
                _asyncCheck = AvailabilityCheck();
                StartCoroutine(_asyncCheck);
            }
        }

        private IEnumerator AvailabilityCheck()
        {
            if (ARSession.state == ARSessionState.None)
            {
                yield return ARSession.CheckAvailability();
            }

            // Waiting for ARSessionState.CheckingAvailability.
            yield return null;

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            // Waiting for ARSessionState.Installing.
            yield return null;
#if UNITY_ANDROID

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Requesting camera permission.");
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(3.0f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // User has denied the request.
                Debug.LogWarning(
                    "Failed to get the camera permission. VPS availability check isn't available.");
                yield break;
            }
#endif

            while (_waitingForLocationService)
            {
                yield return null;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning(
                    "Location services aren't running. VPS availability check is not available.");
                yield break;
            }

            // Update event is executed before coroutines so it checks the latest error states.
            if (_isReturning)
            {
                yield break;
            }

            var location = Input.location.lastData;
            var vpsAvailabilityPromise =
                AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude);
            yield return vpsAvailabilityPromise;

            Debug.LogFormat("VPS Availability at ({0}, {1}): {2}",
                location.latitude, location.longitude, vpsAvailabilityPromise.Result);
            VPSCheckCanvas.SetActive(vpsAvailabilityPromise.Result != VpsAvailability.Available);
        }

        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting the fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                yield return new WaitForSeconds(3.0f);
            }
#endif

            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by the user.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Starting location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return null;
            }

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ended with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }

        private void LifecycleUpdate()
        {
            // Pressing 'back' button quits the app.
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (_isReturning)
            {
                return;
            }

            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            // Quit the app if ARSession is in an error status.
            string returningReason = string.Empty;
            if (ARSession.state != ARSessionState.CheckingAvailability &&
                ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                returningReason = string.Format(
                    "Geospatial sample encountered an ARSession error state {0}.\n" +
                    "Please restart the app.",
                    ARSession.state);
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                returningReason =
                    "Geospatial sample failed to start location service.\n" +
                    "Please restart the app and grant the fine location permission.";
            }
            else if (SessionOrigin == null || Session == null || ARCoreExtensions == null)
            {
                returningReason = string.Format(
                    "Geospatial sample failed due to missing AR Components.");
            }

            ReturnWithReason(returningReason);
        }

        private void ReturnWithReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return;
            }

            GeometryToggle.gameObject.SetActive(false);
            AnchorSettingButton.gameObject.SetActive(false);
            AnchorSettingPanel.gameObject.SetActive(false);
            GeospatialAnchorToggle.gameObject.SetActive(false);
            TerrainAnchorToggle.gameObject.SetActive(false);
            RooftopAnchorToggle.gameObject.SetActive(false);
            ClearAllButton.gameObject.SetActive(false);
            InfoPanel.SetActive(false);

            Debug.LogError(reason);
            SnackBarText.text = reason;
            _isReturning = true;
            Invoke(nameof(QuitApplication), _errorDisplaySeconds);
        }

        private void QuitApplication()
        {
            Application.Quit();
        }

        private void UpdateDebugInfo()
        {
            if (!Debug.isDebugBuild || EarthManager == null)
            {
                return;
            }

            var pose = EarthManager.EarthState == EarthState.Enabled &&
                EarthManager.EarthTrackingState == TrackingState.Tracking ?
                EarthManager.CameraGeospatialPose : new GeospatialPose();
            var supported = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            DebugText.text =
                $"IsReturning: {_isReturning}\n" +
                $"IsLocalizing: {_isLocalizing}\n" +
                $"SessionState: {ARSession.state}\n" +
                $"LocationServiceStatus: {Input.location.status}\n" +
                $"FeatureSupported: {supported}\n" +
                $"EarthState: {EarthManager.EarthState}\n" +
                $"EarthTrackingState: {EarthManager.EarthTrackingState}\n" +
                $"  LAT/LNG: {pose.Latitude:F6}, {pose.Longitude:F6}\n" +
                $"  HorizontalAcc: {pose.HorizontalAccuracy:F6}\n" +
                $"  ALT: {pose.Altitude:F2}\n" +
                $"  VerticalAcc: {pose.VerticalAccuracy:F2}\n" +
                $". EunRotation: {pose.EunRotation:F2}\n" +
                $"  OrientationYawAcc: {pose.OrientationYawAccuracy:F2}";
        }

        /// <summary>
        /// Generates the placed anchor success string for the UI display.
        /// </summary>
        /// <returns> The string for the UI display for successful anchor placement.</returns>
        private string GetDisplayStringForAnchorPlacedSuccess()
        {
            return string.Format(
                    "{0} / {1} Anchor(s) Set!", _anchorObjects.Count, _storageLimit);
        }

        /// <summary>
        /// Generates the placed anchor failure string for the UI display.
        /// </summary>
        /// <returns> The string for the UI display for a failed anchor placement.</returns>
         private string GetDisplayStringForAnchorPlacedFailure()
        {
            return string.Format(
                    "Failed to set a {0} anchor!", _anchorType);
        }
    }
}