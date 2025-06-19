using Oculus.Avatar2;
using UnityEngine;

/// <summary>
/// Allows manual control of avatar tracking by setting transforms.
/// Attach this to the same GameObject as OvrAvatarEntity.
/// </summary>
public class ManualTrackingInputManager : OvrAvatarInputManager
{
    [Header("Manual Tracking Transforms")]
    public Transform manualHeadTransform;
    public Transform manualLeftHandTransform;
    public Transform manualRightHandTransform;
    public bool controllersVisible = true;

    // These can be toggled at runtime to enable/disable manual tracking
    public bool useManualHeadTracking = true;
    public bool useManualLeftHandTracking = true;
    public bool useManualRightHandTracking = true;

    protected override void OnTrackingInitialized()
    {
        var trackingDelegate = new ManualInputTrackingDelegate(this);
        var controlDelegate = new TrackingTransformsInputControlDelegate();
        _inputTrackingProvider = new OvrAvatarInputTrackingDelegatedProvider(trackingDelegate);
        _inputControlProvider = new OvrAvatarInputControlDelegatedProvider(controlDelegate);
    }

    // Helper methods for external scripts/UI
    public void SetHeadPose(Vector3 pos, Quaternion rot)
    {
        if (manualHeadTransform != null)
        {
            manualHeadTransform.position = pos;
            manualHeadTransform.rotation = rot;
        }
    }
    public void SetLeftHandPose(Vector3 pos, Quaternion rot)
    {
        if (manualLeftHandTransform != null)
        {
            manualLeftHandTransform.position = pos;
            manualLeftHandTransform.rotation = rot;
        }
    }
    public void SetRightHandPose(Vector3 pos, Quaternion rot)
    {
        if (manualRightHandTransform != null)
        {
            manualRightHandTransform.position = pos;
            manualRightHandTransform.rotation = rot;
        }
    }
}

/// <summary>
/// Delegate that provides manual tracking data to the avatar system.
/// </summary>
public class ManualInputTrackingDelegate : OvrAvatarInputTrackingDelegate
{
    private readonly ManualTrackingInputManager _manager;

    public ManualInputTrackingDelegate(ManualTrackingInputManager manager)
    {
        _manager = manager;
    }

    public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState state)
    {
        state = default;

        if (_manager.useManualHeadTracking && _manager.manualHeadTransform != null)
        {
            state.headset = (CAPI.ovrAvatar2Transform)_manager.manualHeadTransform;
            state.headsetActive = true;
        }

        if (_manager.useManualLeftHandTracking && _manager.manualLeftHandTransform != null)
        {
            state.leftController = (CAPI.ovrAvatar2Transform)_manager.manualLeftHandTransform;
            state.leftControllerActive = true;
            state.leftControllerVisible = _manager.controllersVisible;
        }

        if (_manager.useManualRightHandTracking && _manager.manualRightHandTransform != null)
        {
            state.rightController = (CAPI.ovrAvatar2Transform)_manager.manualRightHandTransform;
            state.rightControllerActive = true;
            state.rightControllerVisible = _manager.controllersVisible;
        }

        return true;
    }
}

/// <summary>
/// Simple control delegate (can be extended as needed).
/// </summary>
public class TrackingTransformsInputControlDelegate : OvrAvatarInputControlDelegate
{
    public CAPI.ovrAvatar2ControllerType controllerType = CAPI.ovrAvatar2ControllerType.Invalid;

    public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
    {
        inputControlState = default;
        inputControlState.type = controllerType;
        return true;
    }
}
