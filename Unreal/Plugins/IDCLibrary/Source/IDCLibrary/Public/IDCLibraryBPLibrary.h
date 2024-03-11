#pragma once

#include "Kismet/BlueprintFunctionLibrary.h"
#include "IDCLibraryBPLibrary.generated.h"

USTRUCT(BlueprintType)
struct FIDCUserData {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    int32 userID;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString nick;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString email;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString language;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString country;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString currency;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString tokenUserGameId;

    FIDCUserData() : userID(0) { }
};

USTRUCT(BlueprintType)
struct FIDCUserDataResponse {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    int32 status;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString description;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FIDCUserData extraParams;

    FIDCUserDataResponse() : status(0) { }
};


USTRUCT(BlueprintType)
struct FErrorResponse {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    int32 status;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString description;

    FErrorResponse(): status(0) { }
};

USTRUCT(BlueprintType)
struct FPurchaseData {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString dllTransaction;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString idcpaymentTransaction;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString extra;
};

USTRUCT(BlueprintType)
struct FPurchaseRequestData {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString transactionId;
};

USTRUCT(BlueprintType)
struct FPurchaseCloseData {
    GENERATED_BODY()
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    int32 status;
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IDCLibrary")
    FString description;

    FPurchaseCloseData() : status(0) { }
};

DECLARE_DYNAMIC_DELEGATE_TwoParams(FUserDataCallback, bool, bSuccess, FIDCUserData, userData);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FPurchaseCallback, bool, bSuccess, FPurchaseData, purchaseData);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FPurchaseRequestCallback, bool, bSuccess, FPurchaseRequestData, purchaseRequestData);
DECLARE_DYNAMIC_DELEGATE_OneParam(FPurchaseCloseCallback, FPurchaseCloseData, purchaseCloseData);
DECLARE_DYNAMIC_DELEGATE_OneParam(FErrorCallback, FErrorResponse, errorData);


UCLASS()
class UIDCLibraryBPLibrary : public UBlueprintFunctionLibrary {
	GENERATED_UCLASS_BODY()

    UFUNCTION(BlueprintCallable, meta = (DisplayName = "Init IDC Services"), Category = "IDCLibrary")
    static int InitServices(FString gameID, FString gameSecret, FUserDataCallback onUserData, FPurchaseCallback onPurchase, FErrorCallback onError);

    UFUNCTION(BlueprintCallable, meta = (DisplayName = "End IDC Services"), Category = "IDCLibrary")
    static int EndServices();

	UFUNCTION(BlueprintCallable, meta = (DisplayName = "Shop Purchase"), Category = "IDCLibrary")
	static int Purchase(FString transactionID, FString extra, int userLevel, bool sandbox, FPurchaseRequestCallback purchaseRequestCallback);

    UFUNCTION(BlueprintCallable, meta = (DisplayName = "Close Purchase"), Category = "IDCLibrary")
    static int ClosePurchase(FString transactionId, FString idcPaymentId, FPurchaseCloseCallback purchaseCloseCallback);

    UFUNCTION(BlueprintCallable, meta = (DisplayName = "Tick"), Category = "IDCLibrary")
    static int Tick();

    static FPurchaseCloseCallback OnPurchaseCloseCallback;
    static FPurchaseRequestCallback OnPurchaseRequestCallback;
    static FPurchaseCallback OnPurchaseCallback;
    static FUserDataCallback OnUserDataCallback;
    static FErrorCallback OnErrorCallback;
    static class Notifier* notifier;
    static bool running;
};

