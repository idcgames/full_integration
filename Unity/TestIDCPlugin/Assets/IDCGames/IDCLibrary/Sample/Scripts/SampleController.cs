using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;


public class SampleController : IDCLibrary {
    public string GameID;
    public string GameSecret;
    public UserDataController userDataController;

    // Start is called before the first frame update
    public void InitIDCServices() {
        InitServices(GameID, GameSecret, (res,userData) => {
            userDataController.SetData(userData);
        }, (purchaseRequestData) => {
            userDataController.ProcessPurchase(purchaseRequestData);
        }, (d) => {
            Debug.Log($"ERROR: {d.status} {d.description}");
        });
    }
    public void OpenShop() {
        Purchase("", true);
    }
    public void ClosePurchase() {
        ClosePurchase("", "");
    }

   
}
