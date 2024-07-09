using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Serializable]
    public class DayStateVisuals
    {
        public Vector3 SunRotation = new(0,0,0);
        [Range(0f, 3f)]
        public float SunIntensity = 2f;
        [Range(-100f, 100f)]
        public float Temperature = 0f;
        [Range(-100f, 100f)]
        public float Tint = 0f;
        [Range(-2f, 2f)]
        public float Brightness = 0f;
        [Range(-100f, 100f)]
        public float Saturation = 0f;
        public Color Filter = Color.white;
    }

    [SerializeField]
    private List<DayStateVisuals> _states;

    private Light _mainLight;
    #region Managed PostProcessing Components
    private WhiteBalance _whiteBalance;
    private ColorAdjustments _colorAdjustments;
    #endregion

    private float _skipCounter = 0f;
    private float _skipTarget = 0f;

    private void Start()
    {
        RefreshComponents();
        SetRefreshTo(true);
    }
    private void RefreshComponents()
    {
        _mainLight = GameObject.FindGameObjectWithTag("MainLight").GetComponent<Light>();
        _whiteBalance = VolumeManager.instance.stack.GetComponent<WhiteBalance>();
        _colorAdjustments = VolumeManager.instance.stack.GetComponent<ColorAdjustments>();
    }

    private void SetRefreshTo(bool state)
    {
        if (state)
            TimeManager.OnGameSecond += OnTimeUpdatedHandler;
        else
            TimeManager.OnGameSecond -= OnTimeUpdatedHandler;
    }

    private void OnTimeUpdatedHandler()
    {
        //GameSeconds Skip
        _skipCounter++;
        if (_skipCounter < _skipTarget)
            return;
        _skipCounter = 0f;
        //GameSeconds Skip

        float globalFactor = TimeManager.Instance.GameTime.GetValue01();
        float factor = Mathf.Repeat(globalFactor / (1f / _states.Count), 1f);

        DayStateVisuals a = _states[Mathf.FloorToInt(globalFactor * _states.Count)];
        DayStateVisuals b = _states[(Mathf.FloorToInt(globalFactor * _states.Count) + 1) % _states.Count];

        _mainLight.intensity = Mathf.Lerp(a.SunIntensity, b.SunIntensity, factor);
        _mainLight.transform.rotation = Quaternion.Euler(Vector3.Lerp(a.SunRotation,b.SunRotation, factor));

        _whiteBalance.temperature = new ClampedFloatParameter(Mathf.Lerp(a.Temperature, b.Temperature, factor), -100f, 100f, true);
        _whiteBalance.tint = new ClampedFloatParameter(Mathf.Lerp(a.Tint, b.Tint, factor), -100f, 100f, true);

        _colorAdjustments.postExposure = new FloatParameter(Mathf.Lerp(a.Brightness, b.Brightness, factor), true);
        _colorAdjustments.saturation = new ClampedFloatParameter(Mathf.Lerp(a.Saturation, b.Saturation, factor), -100f, 100f, true);
        _colorAdjustments.colorFilter = new ColorParameter(Color.Lerp(a.Filter, b.Filter, factor), true);
    }

    private void OnDestroy() => SetRefreshTo(false);
}
