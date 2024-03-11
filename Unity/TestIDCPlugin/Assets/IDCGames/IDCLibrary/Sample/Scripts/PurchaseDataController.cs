using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseDataController : MonoBehaviour
{
    public TMPro.TextMeshProUGUI dllTransaction;
    public TMPro.TextMeshProUGUI idcpaymentTransaction;
    public void Process(IDCLibrary.PurchaseData data) {
        dllTransaction.text = data.dllTransaction;
        idcpaymentTransaction.text = data.idcpaymentTransaction;
    }

    public void OnClosePurchase() {
        
    }
}
