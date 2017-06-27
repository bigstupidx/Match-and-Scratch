#import "TAPPXUnityBanner.h"

extern UIViewController* UnityGetGLViewController();
extern UIView* UnityGetGLView();

@implementation TAPPXUnityBanner

@synthesize bannerView;
@synthesize position;

static TAPPXUnityBanner *instance = nil;

+ (void) trackInstall:(NSString *)tappxID{
    [TappxFramework addTappxKey:tappxID fromNonNative:@"unity_ios"];
}

+ (void) createBanner:(int)position{
    if(instance != nil) return;
    
    // Init
    TAPPXUnityBanner *tappxUnityBanner = [[TAPPXUnityBanner alloc] init];
    instance = tappxUnityBanner;
    
    if (position==1) {
        instance.position = true;
    }else{
        instance.position = false;
    }
    TappxBannerViewController *bannerView = [[ TappxBannerViewController alloc] initWithDelegate:instance andSize:TappxBannerSmartBanner andPosition:(position == 0) ? TappxBannerPositionTop : TappxBannerPositionBottom];
    tappxUnityBanner.bannerView = bannerView;
    [tappxUnityBanner.bannerView load];
}


- (id)init {
    self = [super init];
    if (self != nil) {
        
    }
    return self;
}

- (void) showAd{
    if(self.bannerView==nil)
        self.bannerView = [[ TappxBannerViewController alloc] initWithDelegate:self andSize:TappxBannerSmartBanner andPosition: (instance.position == 0 ) ? TappxBannerPositionTop : TappxBannerPositionBottom];
    [self.bannerView load];
    
}

- (void) hideAd{
    if(self.bannerView!=nil){
        [self.bannerView removeBanner];
        self.bannerView = nil;
    }
}

- (UIViewController*)presentViewController{
    return UnityGetGLViewController();
}

-(void) tappxBannerViewControllerDidFinishLoad:(TappxBannerViewController*) vc{
    
    NSLog(@"BANNER: DIDAPPEAR");
    UnitySendMessage("TappxManagerUnity", "tappxBannerDidReceiveAd", "");
}

-(void) tappxBannerViewControllerDidPress:(TappxBannerViewController*) vc{
    
    NSLog(@"BANNER: DIDPRESS");
    UnitySendMessage("TappxManagerUnity", "tappxViewWillLeaveApplication", "");
    
}
-(void) tappxBannerViewControllerDidClose:(TappxBannerViewController*) vc{
    NSLog(@"BANNER: DIDCLOSE");
}

-(void) tappxBannerViewControllerDidFail:(TappxBannerViewController*) vc withError:(TappxErrorAd*) error{
    NSLog(@"BANNER: DIDFAIL %@", error.descriptionError);
    UnitySendMessage("TappxManagerUnity", "tappxBannerFailedToLoad", [[NSString stringWithFormat:@"%@",error.descriptionError] UTF8String]);
}




- (void)dealloc {
    self.view = nil;
    [self.bannerView removeBanner];
    instance = nil;
    //  [super dealloc];
}

@end

extern "C" {
    void trackInstallIOS_(char *tappxID);
    void createBannerIOS_(int positionBanner);
    void hideAdIOS_();
    void showAdIOS_(int positionBanner);
    void releaseTappxIOS_();
}

void trackInstallIOS_(char *tappxID){
    [TAPPXUnityBanner trackInstall:[NSString stringWithCString:tappxID encoding:NSASCIIStringEncoding]];
}

void createBannerIOS_(int positionBanner){
    [TAPPXUnityBanner createBanner:positionBanner];
}

void hideAdIOS_(){
    if(instance != nil){
        [instance hideAd];
    }
}

void showAdIOS_(int positionBanner){
    if(instance != nil){
        [instance showAd];
    }else{
        [TAPPXUnityBanner createBanner:positionBanner];
    }
}

void releaseTappxIOS_(){
    if(instance != nil){
        instance = nil;
    }
}
