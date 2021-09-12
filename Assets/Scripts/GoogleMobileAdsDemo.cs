using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleMobileAdsDemo: MonoBehaviour {

    private BannerView _bannerView;
    private int _tryLoadAgainCount = 5;



    public void Start() {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        RequestBanner();

        _bannerView.OnAdLoaded += HandleOnAdLoaded;
        _bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
    }


    public void ShowAd() {
        _bannerView.Show();
    }


    private void RequestBanner() {
        //test: ca-app-pub-3940256099942544/6300978111
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        RequestConfiguration requestConfiguration = new RequestConfiguration.Builder()
        .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.True)
        .build();
        MobileAds.SetRequestConfiguration(requestConfiguration);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        _bannerView.LoadAd(request);
    }



    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        if (_tryLoadAgainCount > 0) {
            AdRequest request = new AdRequest.Builder().Build();
            _bannerView.LoadAd(request);
            _tryLoadAgainCount--;
        }
    }

    public void HandleOnAdLoaded(object sender, EventArgs args) {
        _tryLoadAgainCount = 5;
    }


}