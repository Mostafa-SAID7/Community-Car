# Friends Module - Localization Keys Reference

This document lists all localization keys used in the Friends module views.

## Resource File Location
- **Path**: `src/CommunityCar.Mvc/Resources/Views/Friends/`
- **Files**: 
  - `Index.en.resx` (English)
  - `Index.ar.resx` (Arabic)

## Common Navigation Keys
Used across all Friends pages for navigation tabs.

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Friends` | Friends | الأصدقاء | Navigation tab |
| `Received` | Received | المستلمة | Navigation tab |
| `Sent` | Sent | المرسلة | Navigation tab |
| `Suggestions` | Suggestions | الاقتراحات | Navigation tab |
| `Blocked` | Blocked | المحظورون | Navigation tab |

## Index Page (My Friends)
**View**: `Views/Friends/Index.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `My Friends` | My Friends | أصدقائي | Page title |
| `Connect with your community` | Connect with your community | تواصل مع مجتمعك | Page description |
| `No Friends Yet` | No Friends Yet | لا يوجد أصدقاء بعد | Empty state heading |
| `Start connecting with people in the community.` | Start connecting with people in the community. | ابدأ بالتواصل مع الأشخاص في المجتمع. | Empty state message |
| `Find Friends` | Find Friends | البحث عن أصدقاء | Button text |
| `Friends since` | Friends since | أصدقاء منذ | Friend card meta |
| `Message` | Message | رسالة | Button tooltip |
| `View Profile` | View Profile | عرض الملف الشخصي | Button text/tooltip |
| `Remove Friend` | Remove Friend | إزالة صديق | Dropdown menu item |
| `Block User` | Block User | حظر المستخدم | Dropdown menu item |

## Requests Page (Received Friend Requests)
**View**: `Views/Friends/Requests.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Friend Requests` | Friend Requests | طلبات الصداقة | Page title |
| `Manage your pending friend requests` | Manage your pending friend requests | إدارة طلبات الصداقة المعلقة | Page description |
| `No Pending Requests` | No Pending Requests | لا توجد طلبات معلقة | Empty state heading |
| `You don't have any friend requests at the moment.` | You don't have any friend requests at the moment. | ليس لديك أي طلبات صداقة في الوقت الحالي. | Empty state message |
| `You have {0} pending friend request(s).` | You have {0} pending friend request(s). | لديك {0} طلب صداقة معلق. | Info alert (formatted) |
| `Accept` | Accept | قبول | Button text |
| `Reject` | Reject | رفض | Button text |

## Sent Requests Page
**View**: `Views/Friends/SentRequests.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Sent Friend Requests` | Sent Friend Requests | طلبات الصداقة المرسلة | Page title |
| `View friend requests you've sent` | View friend requests you've sent | عرض طلبات الصداقة التي أرسلتها | Page description |
| `You haven't sent any friend requests yet.` | You haven't sent any friend requests yet. | لم ترسل أي طلبات صداقة بعد. | Empty state message |
| `Cancel Request` | Cancel Request | إلغاء الطلب | Button text |
| `Pending` | Pending | معلق | Status label |

## Suggestions Page
**View**: `Views/Friends/Suggestions.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Friend Suggestions` | Friend Suggestions | اقتراحات الأصدقاء | Page title |
| `Discover people you may know` | Discover people you may know | اكتشف أشخاصًا قد تعرفهم | Page description |
| `No Suggestions Available` | No Suggestions Available | لا توجد اقتراحات متاحة | Empty state heading |
| `Check back later for new friend suggestions.` | Check back later for new friend suggestions. | تحقق لاحقًا للحصول على اقتراحات أصدقاء جديدة. | Empty state message |
| `Search for Friends` | Search for Friends | البحث عن أصدقاء | Button/heading text |
| `Profile` | Profile | الملف الشخصي | Button text |
| `Add` | Add | إضافة | Button text |

## Search Page
**View**: `Views/Friends/Search.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Find Friends` | Find Friends | البحث عن أصدقاء | Page title |
| `Search for people to connect with` | Search for people to connect with | ابحث عن أشخاص للتواصل معهم | Page description |
| `Search by name or username...` | Search by name or username... | البحث بالاسم أو اسم المستخدم... | Input placeholder |
| `No Results Found` | No Results Found | لم يتم العثور على نتائج | Empty state heading |
| `Try searching with different keywords.` | Try searching with different keywords. | حاول البحث بكلمات مفتاحية مختلفة. | Empty state message |
| `Enter a name or username to find people you know.` | Enter a name or username to find people you know. | أدخل اسمًا أو اسم مستخدم للعثور على أشخاص تعرفهم. | Initial state message |
| `View Suggestions` | View Suggestions | عرض الاقتراحات | Button text |
| `Friend` | Friend | صديق | Status button |

## Blocked Page
**View**: `Views/Friends/Blocked.cshtml`

| Key | English Value | Arabic Value | Usage |
|-----|---------------|--------------|-------|
| `Blocked Users` | Blocked Users | المستخدمون المحظورون | Page title |
| `Manage users you've blocked` | Manage users you've blocked | إدارة المستخدمين الذين حظرتهم | Page description |
| `No Blocked Users` | No Blocked Users | لا يوجد مستخدمون محظورون | Empty state heading |
| `You haven't blocked anyone yet.` | You haven't blocked anyone yet. | لم تحظر أي شخص بعد. | Empty state message |
| `View My Friends` | View My Friends | عرض أصدقائي | Button text |
| `Blocked users cannot send you friend requests or interact with your content.` | Blocked users cannot send you friend requests or interact with your content. | لا يمكن للمستخدمين المحظورين إرسال طلبات صداقة أو التفاعل مع محتواك. | Info alert |
| `Unblock User` | Unblock User | إلغاء حظر المستخدم | Button text |

## Shared Partial (_FriendCard.cshtml)
**View**: `Views/Friends/_FriendCard.cshtml`

All keys from Index page are reused in the friend card partial:
- `Friends since`
- `Message`
- `View Profile`
- `Remove Friend`
- `Block User`

## Implementation Notes

### Usage in Views
All views now use `@Localizer["Key"]` to access localized strings:

```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer

<h3>@Localizer["My Friends"]</h3>
```

### ViewData Assignments
For ViewData assignments, use `.Value` to get the string:

```razor
ViewData["Title"] = Localizer["My Friends"].Value;
ViewData["HeaderTitle"] = Localizer["My Friends"].Value;
```

### Formatted Strings
For strings with placeholders, use `string.Format`:

```razor
@string.Format(Localizer["You have {0} pending friend request(s)."].Value, ViewBag.RequestCount)
```

## Adding New Languages

To add support for a new language:

1. Create a new resource file: `Index.[language-code].resx`
2. Copy all `<data>` entries from `Index.en.resx`
3. Translate the `<value>` content for each entry
4. Keep the `name` attribute unchanged

Example for French:
- File: `Index.fr.resx`
- Key: `Friends`
- Value: `Amis`

## Total Keys Count
**Total unique localization keys**: 47

## Files Updated
1. ✅ `Views/Friends/Index.cshtml` - Already had localizer
2. ✅ `Views/Friends/Requests.cshtml` - Already had localizer
3. ✅ `Views/Friends/SentRequests.cshtml` - Updated with all keys
4. ✅ `Views/Friends/Suggestions.cshtml` - Updated with all keys
5. ✅ `Views/Friends/Search.cshtml` - Updated with all keys
6. ✅ `Views/Friends/Blocked.cshtml` - Added localizer and updated all keys
7. ✅ `Views/Friends/_FriendCard.cshtml` - Already had localizer

## Resource Files Created
1. ✅ `Resources/Views/Friends/Index.en.resx` - English translations
2. ✅ `Resources/Views/Friends/Index.ar.resx` - Arabic translations
