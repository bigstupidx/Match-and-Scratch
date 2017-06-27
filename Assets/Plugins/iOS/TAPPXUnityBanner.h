#import <UIKit/UIKit.h>
extern "C" {
#import <TappxFramework/TappxAds.h>
}
@interface TAPPXUnityBanner : UIViewController  <TappxBannerViewControllerDelegate>{
    TappxBannerViewController *bannerView;
    BOOL position;
    BOOL isBannerVisible;
}

+ (void) trackInstall:(NSString *) tappxID;
+ (void) createBanner:(int)position;
- (void) hideAd;
- (void) showAd;

@property(nonatomic, strong) TappxBannerViewController *bannerView;
@property(assign) BOOL position;

@end
