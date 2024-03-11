#pragma once  

#if defined(DIST_DLL)
#define DLL_EXPORT __declspec(dllexport)
#define STDCALL __stdcall
#else
#define DLL_EXPORT
//#define STDCALL
#endif

enum Command : unsigned int {
	Error = 0xfbb65363,
	InitLib = 0x89d4951f,
	EndLib = 0x87a3628f,
	PurchaseInit = 0xffeecce6,
	PurchaseEnd = 0xa9bbb04b,
	UserData = 0x3275c4a, //DTINF
	BadUserInitData = 0xaba83a28, //BDINT
	PurchaseNotification = 0x733f2850, //PPNOT
	NewPurchaseCode = 0xfa4967c3, //NWPRC
	PurchaseOk = 0x8218d08c, //OKPRC
	PurchaseError = 0xbb485d88, //BDPRC
	BadPurchaseCode = 0x6a6ac964, // BDCPR
	PurchaseClosed = 0x6c1ccf80 // PRCHC
	/*
/*
3275c4aa
bb485d88
37e5121c
834e0e5e
ca01d6b2
fb090158
c6c45313
8421e045
c0a53a66
c13492e6
4008e557
52cfcbc1
6af6f4c4
96c4729f
685a4808
aa80c175
e0e156c3
a9ace67b
b931c6b8
98210d80
*/
};


typedef void(STDCALL* NotifyCallBack)(Command command, const wchar_t* data, int dataLen);
typedef void(STDCALL* LogCallBack)(const char* string, int dataLen);

int DLL_EXPORT _InitIDCLib(const wchar_t* gameId, const wchar_t* token, unsigned long ProcessId, NotifyCallBack notifyCallBack, LogCallBack logCallback, bool useQueue);
int DLL_EXPORT _EndIDCLib();
int DLL_EXPORT _OpenShop(const wchar_t* transactionId, const wchar_t* extra, int userLevel, bool sandbox);
int DLL_EXPORT _ClosePurchase(const wchar_t* transactionId, const wchar_t* idcPaymentId);
int DLL_EXPORT _PullMessage();

