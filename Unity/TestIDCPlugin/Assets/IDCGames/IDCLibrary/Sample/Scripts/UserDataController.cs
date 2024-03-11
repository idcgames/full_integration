using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserDataController : MonoBehaviour {
    public Transform purchases;
    public PurchaseDataController PurchasePrefab;

    public TMPro.TextMeshProUGUI nick;
    public TMPro.TextMeshProUGUI email;
    public TMPro.TextMeshProUGUI language;
    public TMPro.TextMeshProUGUI country;
    public TMPro.TextMeshProUGUI currency;

    // Start is called before the first frame update
    public void SetData( IDCLibrary.IDCUserData data) {
        nick.text = data.nick;
        email.text = data.email;
        language.text = data.language;
        country.text = data.country;
        currency.text = data.currency;
    }

    public void ProcessPurchase(IDCLibrary.PurchaseData data) {
        Instantiate<PurchaseDataController>(PurchasePrefab, purchases).Process(data);
    }
}
