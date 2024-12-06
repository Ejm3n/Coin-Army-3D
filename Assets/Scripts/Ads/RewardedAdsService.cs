using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class RewardedAdsService : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static RewardedAdsService Default;

    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null;

    bool _watchingAd;
    public bool AdsAreAvailable { get; private set; }

    Action _onComplete;

    void Awake()
    {   
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        Default = this;
    }
 
    public void LoadAd()
    {
        AdsAreAvailable = false;
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }
 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        AdsAreAvailable = true;
        Debug.Log("Ad Loaded: " + adUnitId);
    }
 
    public void ShowAd(Action onComplete)
    {
        if (_watchingAd)
        {
            return;
        }

        _watchingAd = true;

        _onComplete = onComplete;

        Advertisement.Show(_adUnitId, this);

#if UNITY_EDITOR
        OnUnityAdsShowComplete(_adUnitId, UnityAdsShowCompletionState.COMPLETED);
#endif
    }
 
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (!_watchingAd)
        {
            return;
        }

        _watchingAd = false;

        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");

            _onComplete?.Invoke();
        }

        LoadAd();
    }
 
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }
 
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        _watchingAd = false;

        Debug.LogWarning($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }
 
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
