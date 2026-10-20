using System.Collections;
using Google.XR.Cardboard;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR;
using UnityEngine.XR.Management;

using InputSystemTouchPhase = UnityEngine.InputSystem.TouchPhase;

public class VrModeController : MonoBehaviour
{

    private const float _defaultFieldOfView = 60.0f;

    // Main camera from the scene.
    private Camera _mainCamera;


    private bool _isScreenTouched
    {
        get
        {
            TouchControl touch = GetFirstTouchIfExists();
            return touch != null && touch.phase.ReadValue() == InputSystemTouchPhase.Began;
        }
    }

    private bool _isVrModeEnabled
    {
        get
        {
            return XRGeneralSettings.Instance.Manager.isInitializationComplete;
        }
    }


    public void Start()
    {

        _mainCamera = Camera.main;


        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;


        if (!Api.HasDeviceParams())
        {
            Api.ScanDeviceParams();
        }
    }


    public void Update()
    {
        if (_isVrModeEnabled)
        {
            if (Api.IsCloseButtonPressed)
            {
                ExitVR();
            }

            if (Api.IsGearButtonPressed)
            {
                Api.ScanDeviceParams();
            }

            Api.UpdateScreenParams();
        }
        else
        {

            if (_isScreenTouched)
            {
                EnterVR();
            }
        }
    }


    private static TouchControl GetFirstTouchIfExists()
    {
        Touchscreen touchScreen = Touchscreen.current;
        if (touchScreen == null)
        {
            return null;
        }

        if (!touchScreen.enabled)
        {
            InputSystem.EnableDevice(touchScreen);
        }

        ReadOnlyArray<TouchControl> touches = touchScreen.touches;
        if (touches.Count == 0)
        {
            return null;
        }

        return touches[0];
    }

    private void EnterVR()
    {
        StartCoroutine(StartXR());
        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }
    }

    private void ExitVR()
    {
        StopXR();
    }

    /// <summary>
    /// Initializes and starts the Cardboard XR plugin.
    /// See https://docs.unity3d.com/Packages/com.unity.xr.management@3.2/manual/index.html.
    /// </summary>
    ///
    /// <returns>
    /// Returns result value of <c>InitializeLoader</c> method from the XR General Settings Manager.
    /// </returns>
    private IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed.");
        }
        else
        {
            Debug.Log("XR initialized.");

            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            Debug.Log("XR started.");
        }
    }


    private void StopXR()
    {
        Debug.Log("Stopping XR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        Debug.Log("XR stopped.");

        Debug.Log("Deinitializing XR...");
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR deinitialized.");

        _mainCamera.ResetAspect();
        _mainCamera.fieldOfView = _defaultFieldOfView;
    }
}