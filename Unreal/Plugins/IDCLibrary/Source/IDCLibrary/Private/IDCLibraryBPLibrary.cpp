// Copyright Epic Games, Inc. All Rights Reserved.
#include "IDCLibraryBPLibrary.h"
#include "IDCLibrary.h"

#include "IDCLib.h"
#pragma comment(lib, "IDCLib2.lib")

#include "Windows/WindowsHWrapper.h"
#include <processthreadsapi.h>

#include "Async/Async.h"
#include "Async/TaskGraphInterfaces.h"

#include "JsonObjectConverter.h"

FPurchaseRequestCallback UIDCLibraryBPLibrary::OnPurchaseRequestCallback;
FPurchaseCloseCallback UIDCLibraryBPLibrary::OnPurchaseCloseCallback;
FPurchaseCallback UIDCLibraryBPLibrary::OnPurchaseCallback;
FUserDataCallback UIDCLibraryBPLibrary::OnUserDataCallback;
FErrorCallback UIDCLibraryBPLibrary::OnErrorCallback;
bool UIDCLibraryBPLibrary::running = false;

Notifier* UIDCLibraryBPLibrary::notifier;

UIDCLibraryBPLibrary::UIDCLibraryBPLibrary(const FObjectInitializer& ObjectInitializer)
: Super(ObjectInitializer)
{
	UIDCLibraryBPLibrary::running = false;
}

void _Log(const char * string, int dataLen) {
	UE_LOG(LogTemp, Warning, TEXT("[IDC] LOG: %s"), *FString(string));
}

void _OnNotifyCallBack(Command command, const wchar_t* data, int dataLen)
{
	FString fdata = FString(data);
	UE_LOG(LogTemp, Warning, TEXT("[IDC] MSG: %d, %s"), command, *fdata);
	switch ((unsigned int)command) {
		case Command::UserData:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FIDCUserDataResponse userDataResponse;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &userDataResponse, 0, 0);
				UIDCLibraryBPLibrary::OnUserDataCallback.ExecuteIfBound(true, userDataResponse.extraParams);
			});
			break;
		case Command::BadUserInitData:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FIDCUserDataResponse userDataResponse;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &userDataResponse, 0, 0);
				UIDCLibraryBPLibrary::OnUserDataCallback.ExecuteIfBound(false, userDataResponse.extraParams);
			});
			break;

		case Command::PurchaseNotification:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FPurchaseData purchaseData;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &purchaseData, 0, 0);
				UIDCLibraryBPLibrary::OnPurchaseCallback.ExecuteIfBound(true, purchaseData);
			});
			break;
		case Command::NewPurchaseCode:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FPurchaseRequestData purchaseRequestData;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &purchaseRequestData, 0, 0);
				UIDCLibraryBPLibrary::OnPurchaseRequestCallback.ExecuteIfBound(true, purchaseRequestData);
				UIDCLibraryBPLibrary::OnPurchaseRequestCallback.Clear();
			});
			break;
		case Command::PurchaseOk:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FPurchaseCloseData purchaseCloseData;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &purchaseCloseData, 0, 0);
				UIDCLibraryBPLibrary::OnPurchaseCloseCallback.ExecuteIfBound(purchaseCloseData);
				UIDCLibraryBPLibrary::OnPurchaseCloseCallback.Clear();
			});
			break;
		case Command::PurchaseClosed:
			UIDCLibraryBPLibrary::OnPurchaseRequestCallback.Clear();
			UIDCLibraryBPLibrary::OnPurchaseCloseCallback.Clear();
			break;
		case Command::BadPurchaseCode:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FPurchaseCloseData purchaseCloseData;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &purchaseCloseData, 0, 0);
				//UIDCLibraryBPLibrary::OnPurchaseRequestDataCallback.ExecuteIfBound(false, purchaseRequestData);
			});
			break;
		case Command::Error:
			AsyncTask(ENamedThreads::GameThread, [fdata]() {
				FErrorResponse errorResponse;
				FJsonObjectConverter::JsonObjectStringToUStruct(fdata, &errorResponse, 0, 0);
				UIDCLibraryBPLibrary::OnErrorCallback.ExecuteIfBound(errorResponse);
			});
			break;
	}
}

int UIDCLibraryBPLibrary::InitServices(FString gameID, FString gameSecret, FUserDataCallback onUserData, FPurchaseCallback onPurchase, FErrorCallback onError) {
	OnPurchaseCallback = onPurchase;
	OnUserDataCallback = onUserData;
	
	OnPurchaseRequestCallback.Clear();
	OnPurchaseCloseCallback.Clear();

	OnErrorCallback = onError;
	int ret = _InitIDCLib(TCHAR_TO_WCHAR(*gameID), TCHAR_TO_WCHAR(*gameSecret), GetCurrentProcessId(), &_OnNotifyCallBack, &_Log, false);
	UIDCLibraryBPLibrary::running = ret==0;
	return ret;
}

int UIDCLibraryBPLibrary::EndServices() {
	UIDCLibraryBPLibrary::running = false;
	return _EndIDCLib();
}

int UIDCLibraryBPLibrary::Purchase(FString transactionId, FString extra, int userLevel, bool sandbox, FPurchaseRequestCallback onPurchaseRequestCallback){
	if (OnPurchaseRequestCallback.IsBound() ) return -1;
	OnPurchaseRequestCallback = onPurchaseRequestCallback;
	return _OpenShop(TCHAR_TO_WCHAR(*transactionId), TCHAR_TO_WCHAR(*extra), userLevel, sandbox);
}

int UIDCLibraryBPLibrary::ClosePurchase(FString transactionId, FString paymentId, FPurchaseCloseCallback onPurchaseRequestCallback) {
	if (OnPurchaseCloseCallback.IsBound()) return -1;
	OnPurchaseCloseCallback = onPurchaseRequestCallback;
	return _ClosePurchase(TCHAR_TO_WCHAR(*transactionId), TCHAR_TO_WCHAR(*paymentId) );
}

int UIDCLibraryBPLibrary::Tick() {
	if(!UIDCLibraryBPLibrary::running) return 0;
	return _PullMessage();
}