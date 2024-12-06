using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class IAPService : MonoBehaviour, IStoreListener
{
    #region Singleton
    private static IAPService _default;
    public static IAPService Default => _default;
    #endregion

    public bool IAPIsAvailable { get; private set; }

    public string environment = "production";

    private IStoreController _controller;
    private IExtensionProvider _extensions;

    private Action _onSuccessfulPurchase;

    private bool _purchasing;

    public void DoPurchase(string productName, Action onSuccessfulPurchase)
    {
        if (_purchasing || !IAPIsAvailable)
        {
            return;
        }

#if UNITY_EDITOR
        onSuccessfulPurchase();
#else
        foreach (var product in _controller.products.all)
        {
            if (product.definition.id == productName)
            {
                _purchasing = true;
                _onSuccessfulPurchase = onSuccessfulPurchase;

                _controller.InitiatePurchase(product);

                return;
            }
        }
#endif
    }

    private void Awake()
    {
        _default = this;
    }

    async void Start()
    {
        Debug.Log("Initializing Unity Gaming Services");

        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);
 
            await UnityServices.InitializeAsync(options);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error initializing UGS: " + e);
        }

        Debug.Log("Initializating IAP");

#if UNITY_EDITOR
        IAPIsAvailable = true;

        Debug.Log("IAP is ready");
#else
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct("starter_pack", ProductType.Consumable);

        if (!GameData.Default.DisableShop)
        {
            foreach (var item in StoreDB.Default.Items)
            {
                if (item.BuyWith == StoreDB.Item.PurchaseCurrency.Money)
                {
                    builder.AddProduct(item.ProductName, ProductType.Consumable);
                }
            }
        }

        UnityPurchasing.Initialize(this, builder);
#endif
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _controller = controller;
        _extensions = extensions;
        IAPIsAvailable = true;

        Debug.Log("IAP is ready");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        IAPIsAvailable = false;

        Debug.LogWarning("IAP init failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string reason)
    {
        IAPIsAvailable = false;

        Debug.LogWarning("IAP init failed: " + error + " (" + reason + ")");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        _purchasing = false;

        Debug.Log("IAP purchase was successful");

        _onSuccessfulPurchase();

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        _purchasing = false;

        Debug.LogWarning("IAP purchase has failed: " + p);
    }
}
